using System.Data.Common;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Tantalus.Entities;
using Tantalus.Models;
using Tantalus.Services;

namespace Controllers; 

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class WeightController: TantalusController {
    private readonly IWeightService _weightService;

    public WeightController(IWeightService weightService) {
        _weightService = weightService;
    }
    
    [HttpGet]
    public async Task<ActionResult<AllWeightsResponse>> GetWeightMeasurements([FromQuery] WeightStatRequest parameters) {
        return Ok(await _weightService.GetWeightMeasurements(UserGuid, parameters));
    }

    [HttpPut]
    public async Task<ActionResult> UpdateWeightMeasurement(WeightUpdateRequest request) {
        return await _weightService.UpdateWeightMeasurement(UserGuid, request) == 1 ? NoContent() : BadRequest();
    }
    
    [HttpDelete("{measuredOn}")]
    public async Task<ActionResult> DeleteMeasurement(DateTimeOffset measuredOn) {
        return await _weightService.DeleteWeightMeasurement(UserGuid, measuredOn) == 1 ? Ok() : NotFound();
    }

    [HttpGet("duplicates")]
    public async Task<ActionResult> GetDuplicates([FromQuery] WeightStatRequest request) {
        var duplicates = (await _weightService.FindDuplicates(UserGuid, request)).ToList();
        return Ok( new {
            duplicates,
            total = duplicates.FirstOrDefault()?.Total ?? 0
        });
    }

    [HttpPost("import")]
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

        return Ok(new { Imported = await _weightService.ImportWeightMeasurements(measurements, data.Overwrite) });
    }
}