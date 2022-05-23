using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Tantalus.Entities;

namespace Controllers;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class ImportController : TantalusController {
    private readonly string _connectionString;
    private NpgsqlConnection DbConnection => new(_connectionString);

    public ImportController(IConfiguration configuration) {
        _connectionString = configuration.GetConnectionString("Database") ?? throw new InvalidOperationException();
    }

    [HttpPost("weight")]
    public async Task<ActionResult> ImportWeightMeasurements([FromForm] WeightMeasurementImport data) {
        var file = data.Data;
        var userId = UserGuid;
        var measurements = new List<WeightMeasurement>();
        
        // parse CSV file before opening a DB connection and transaction
        using (var reader = new StreamReader(file.OpenReadStream()))
        using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) {
                   DetectDelimiter = true,
                   PrepareHeaderForMatch = prepareHeaderForMatchArgs => prepareHeaderForMatchArgs.Header.ToLower()
                   
               })) {
            await csv.ReadAsync();
            csv.ReadHeader();
            
            // prepareHeaderForMatchArgs doesn't apply to HeaderRecord
            var usesTimestamps = false;
            var usesImpedance = false;

            foreach (var headerRecord in csv.HeaderRecord) {
                var header = headerRecord.ToLower();
                switch (header) {
                    case "timestamp":
                        usesTimestamps = true;
                        break;
                    case "impedance":
                        usesImpedance = true;
                        break;
                }
            }
            
            while (await csv.ReadAsync()) {
                
                // read the UNIX Epoch timestamp or attempt to parse the date
                var measuredOn = usesTimestamps
                    ? DateTimeOffset.FromUnixTimeMilliseconds(csv.GetField<long>("Timestamp")).UtcDateTime
                    : DateTime.Parse(csv.GetField<string>("Datetime"));

                short? impedance = usesImpedance ? csv.GetField<short>("Impedance") : null;
                
                var measurement = new WeightMeasurement {
                    UserId = userId,
                    MeasuredOn = measuredOn,
                    Weight = Convert.ToInt32(csv.GetField<float>("Weight") * 1000),
                    Impedance = impedance,
                    Fat = csv.GetField<float>("Fat")
                };
                measurements.Add(measurement);
            }
        }

        const string skipExistingQuery = @"
            INSERT INTO weight_measurements (user_id, measured_on, weight, impedance, fat, note)
            VALUES (@userId, @measuredOn, @weight, @impedance, @fat, @note) ON CONFLICT DO NOTHING";

        const string overwriteQuery = @"
            INSERT INTO weight_measurements (user_id, measured_on, weight, impedance, fat, note)
            VALUES (@userId, @measuredOn, @weight, @impedance, @fat, @note) ON CONFLICT (user_id, measured_on) 
                DO UPDATE SET weight = @weight, impedance = @impedance, note = @note";

        await using var connection = DbConnection;

        // use transaction to process bulk inserts
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        var rows = await connection.ExecuteAsync(data.Overwrite ? overwriteQuery : skipExistingQuery, measurements,
            transaction: transaction);
        if (!data.Overwrite || (data.Overwrite && rows == measurements.Count))
            await transaction.CommitAsync();
        else {
            await transaction.RollbackAsync();
        }

        return Ok(new { Imported = rows });
    }
}

public sealed record WeightMeasurementImport {
    public bool Overwrite { get; init; }
    public IFormFile Data { get; init; }
}