using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAc.API.Bindings;
using SmartAc.API.Filters;
using SmartAc.Application.Abstractions.Repositories;
using SmartAc.Application.Abstractions.Services;
using SmartAc.Application.Contracts;
using SmartAc.Application.Features.Devices.Get;
using SmartAc.Application.Features.Devices.GetAlertLogs;
using SmartAc.Domain;
using SmartAc.Infrastructure.Services;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using SmartAc.API.Contracts;
using SmartAc.Application.Features.Devices.StoreReadings;

namespace SmartAc.API.Controllers;

[ApiController]
[Route("api/v1/device")]
[Authorize("DeviceIngestion")]
public class DeviceIngestionController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ISmartAcJwtService _jwtService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeviceIngestionController> _logger;

    public DeviceIngestionController(
        ISender sender,
        ISmartAcJwtService jwtService,
        IUnitOfWork unitOfWork,
        ILogger<DeviceIngestionController> logger)
    {
        _sender = sender;
        _jwtService = jwtService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Allow smart ac devices to register themselves  
    /// and get a jwt token for subsequent operations
    /// </summary>
    /// <param name="serialNumber">Unique device identifier burned into ROM</param>
    /// <param name="sharedSecret">Unique device shareable secret burned into ROM</param>
    /// <param name="firmwareVersion">Device firmware version at the moment of registering</param>
    /// <returns>A jwt token</returns>
    /// <response code="400">If any of the required data is not present or is invalid.</response>
    /// <response code="404">If there is no device int the database matching the parameters</response>
    /// <response code="200">If the registration has successfully generated a new jwt token.</response>
    [HttpPost("{serialNumber}/registration")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [AllowAnonymous]
    [ValidateFirmware]
    public async Task<IActionResult> RegisterDevice(
        [Required][FromRoute] string serialNumber,
        [Required][FromHeader(Name = "x-device-shared-secret")] string sharedSecret,
        [Required][FromQuery] string firmwareVersion)
    {
        var deviceResult =
            await _sender.Send(new GetDeviceQuery(serialNumber, sharedSecret));

        if (deviceResult.IsError)
        {
            return NotFound(new { deviceResult.FirstError.Code, deviceResult.FirstError.Description });
        }

        var device = deviceResult.Value;

        var (tokenId, jwtToken) =
            _jwtService.GenerateJwtFor(serialNumber, SmartAcJwtService.JwtScopeDeviceIngestionService);

        var newRegistrationDevice = new DeviceRegistration
        {
            DeviceSerialNumber = device.SerialNumber,
            TokenId = tokenId
        };

        device.AddRegistration(newRegistrationDevice, firmwareVersion);

        _unitOfWork.GetRepository<Device>().Update(device);

        await _unitOfWork.SaveChangesAsync();

        _logger.LogDebug(
            "A new registration record with tokenId '{tokenId}'  has been created for the device '{serialNumber}'",
            serialNumber, tokenId);

        return Ok(jwtToken);
    }

    /// <summary>
    /// Allow smart ac devices to send sensor readings in batch
    /// 
    /// This will additionally trigger analysis over the sensor readings
    /// to generate alerts based on it
    /// </summary>
    /// <param name="serialNumber">Unique device identifier burned into ROM.</param>
    /// <param name="sensorReadings">Collection of sensor readings send by a device.</param>
    /// <response code="400">If readings are not properly formatted, or required properties are missing.</response>
    /// <response code="401">If jwt token provided is invalid.</response>
    /// <response code="202">If sensor readings has successfully accepted.</response>
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
        await _sender
            .Send(new StoreReadingsCommand(serialNumber, sensorReadings))
            .ConfigureAwait(false);

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
    [HttpGet("alerts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AllowAnonymous]
    public async Task<IActionResult> GetSensorAlerts(
        [ModelBinder(BinderType = typeof(DeviceInfoBinder))] string serialNumber,
        [FromQuery] QueryParams parameters)
    {
        var alertResult =
            await _sender.Send(new GetAlertLogsQuery(serialNumber, parameters));

        if (alertResult.IsError)
        {
            return NotFound(new { alertResult.FirstError.Code, alertResult.FirstError.Description });
        }

        var res = PagedList<LogItem>.ToPagedList(
            alertResult.Value.AsQueryable(),
            parameters.PageNumber,
            parameters.PageSize);

        var metadata = new
        {
            res.TotalCount,
            res.PageSize,
            res.CurrentPage,
            res.TotalPages,
            res.HasNext,
            res.HasPrevious
        };

        Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(metadata));

        return Ok(res);
    }
}