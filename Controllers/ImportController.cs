using System.Globalization;
using CsvHelper;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Tantalus.Data;
using Tantalus.Entities;

namespace Controllers;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class ImportController : TantalusController {

    private readonly DataContext _dataContext;
    private readonly string _connectionString;
    private NpgsqlConnection DbConnection => new(_connectionString);

    public ImportController(DataContext dataContext, IConfiguration configuration) {
        _dataContext = dataContext;
        _connectionString = configuration.GetConnectionString("Database") ?? throw new InvalidOperationException();
    }

    [HttpPost("weight")]
    public async Task<ActionResult> ImportWeightMeasurements([FromForm] WeightMeasurementImport data) {
        var file = data.Data;
        var userId = UserGuid;
        var measurements = new List<WeightMeasurement>();
        
        // parse CSV file before opening a DB connection and transaction
        using (var reader = new StreamReader(file.OpenReadStream()))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture)) {
            await csv.ReadAsync();
            csv.ReadHeader();
            while (await csv.ReadAsync()) {
                var measurement = new WeightMeasurement {
                    UserId = userId,
                    MeasuredOn = DateTimeOffset.FromUnixTimeMilliseconds(csv.GetField<long>("Timestamp")).UtcDateTime,
                    Weight = Convert.ToInt32(csv.GetField<float>("Weight") * 1000),
                    Impedance = csv.GetField<short>("Impedance")
                };
                measurements.Add(measurement);
            }
        }

        const string skipExistingQuery = @"
            INSERT INTO weight_measurements (user_id, measured_on, weight, impedance, note)
            VALUES (@userId, @measuredOn, @weight, @impedance, @note) ON CONFLICT DO NOTHING";
        
        const string overwriteQuery = @"
            INSERT INTO weight_measurements (user_id, measured_on, weight, impedance, note)
            VALUES (@userId, @measuredOn, @weight, @impedance, @note) ON CONFLICT (user_id, measured_on) 
                DO UPDATE SET weight = @weight, impedance = @impedance, note = @note";

        var selectedQuery = data.Overwrite ? overwriteQuery : skipExistingQuery;
                             
        await using var connection = DbConnection;
        
        // use transaction to process bulk inserts
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        var rows = await connection.ExecuteAsync(selectedQuery, measurements, transaction: transaction);
        if (!data.Overwrite || (data.Overwrite && rows == measurements.Count))
            await transaction.CommitAsync();
        else {
            await transaction.RollbackAsync();
        }

        return Ok(new {Imported = rows});
    }
}

public sealed record WeightMeasurementImport {
    public bool Overwrite { get; init; }
    public IFormFile Data { get; init; }
}
