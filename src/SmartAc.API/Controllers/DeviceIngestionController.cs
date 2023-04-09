using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAc.API.Bindings;
using SmartAc.API.Contracts;
using SmartAc.API.Filters;
using SmartAc.Application.Contracts;
using SmartAc.Application.Features.DeviceReadings.StoreReadings;
using SmartAc.Application.Features.Devices.AlertLogs;
using SmartAc.Application.Features.Devices.Registration;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace SmartAc.API.Controllers;

[ApiController]
[Route("api/v1/device")]
[Authorize("DeviceIngestion")]
public class DeviceIngestionController : ControllerBase
{
    private readonly ISender _sender;

    public DeviceIngestionController(ISender sender) => _sender = sender;

    /// <summary>
    /// Allow smart ac devices to register themselves  
    /// and get a jwt token for subsequent operations
    /// </summary>
    /// <param name="serialNumber">Unique device identifier burned into ROM</param>
    /// <param name="sharedSecret">Unique device shareable secret burned into ROM</param>
    /// <param name="firmwareVersion">Device firmware version at the moment of registering</param>
    /// <returns>A jwt token</returns>
    /// <response code="400">If any of the required data is not present or is invalid.</response>
    /// <response code="404">If there is no device in the database matching the parameters</response>
    /// <response code="200">If the registration has successfully generated a new jwt token.</response>
    [HttpPost("{serialNumber}/registration")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [AllowAnonymous, ValidateFirmware]
    public async Task<IActionResult> RegisterDevice(
        [Required][FromRoute] string serialNumber,
        [Required][FromHeader(Name = "x-device-shared-secret")] string sharedSecret,
        [Required][FromQuery] string firmwareVersion)
    {
        var registerResult = await
            _sender.Send(new RegisterDeviceCommand(serialNumber, sharedSecret, firmwareVersion))
                   .ConfigureAwait(false);

        if (registerResult.IsError)
        {
            return NotFound(new
            {
                registerResult.FirstError.Code,
                registerResult.FirstError.Description
            });
        }

        return Ok(registerResult.Value);
    }

    /// <summary>
    /// Allow smart ac devices to send sensor readings in batch
    /// 
    /// This will additionally trigger analysis over the sensor readings
    /// to generate alerts based on it
    /// </summary>
    /// <param name="serialNumber">Unique device identifier burned into ROM.</param>
    /// <param name="sensorReadings">Collection of sensor readings send by a device.</param>
    /// <response code="400">If readings are not properly formatted</response>
    /// <response code="401">If jwt token provided is invalid.</response>
    /// <response code="202">If sensor readings are successfully accepted.</response>
    /// <returns>No Content.</returns>
    [HttpPost("readings/batch")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ValidateReadings]
    [AllowAnonymous]
    public async Task<IActionResult> AddSensorReadings(
        [ModelBinder(BinderType = typeof(DeviceInfoBinder))] string serialNumber,
        [FromBody] IEnumerable<SensorReading> sensorReadings)
    {
        var readings =
            sensorReadings
                .Select(r => r.ToDeviceReading(serialNumber))
                .ToList();

        await _sender.Send(new StoreReadingsCommand(readings)).ConfigureAwait(false);

        return Accepted();
    }

    /// <summary>
    /// Allow smart ac devices to read their alerts
    /// 
    /// </summary>
    /// <param name="serialNumber">Unique device identifier burned into ROM.</param>
    /// <param name="parameters">Query parameters for data filtering and paging.</param>
    /// <response code="401">If something is wrong on the information provided.</response>
    /// <response code="404">If there is no device int the database matching the parameters</response>
    /// <response code="200">There is data to display</response>
    /// <returns>A List of alerts matching the request parameters, with paging information in the response header.</returns>
    [HttpGet("alerts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AllowAnonymous]
    public async Task<IActionResult> GetSensorAlerts(
        [ModelBinder(BinderType = typeof(DeviceInfoBinder))] string serialNumber,
        [FromQuery] QueryParams parameters)
    {
        var logResult = await
            _sender.Send(new GetAlertLogsQuery(serialNumber, parameters)).ConfigureAwait(false);

        if (logResult.IsError)
        {
            return NotFound(new
            {
                logResult.FirstError.Code,
                logResult.FirstError.Description
            });
        }

        var metadata = new
        {
            logResult.Value.TotalCount,
            logResult.Value.PageSize,
            logResult.Value.CurrentPage,
            logResult.Value.TotalPages,
            logResult.Value.HasNext,
            logResult.Value.HasPrevious
        };

        Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(metadata));

        return Ok(logResult.Value);
    }
}