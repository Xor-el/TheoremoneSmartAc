using ErrorOr;
using MediatR;
using SmartAc.Application.Contracts;

namespace SmartAc.Application.Features.Devices.GetAlertLogs;

public sealed record GetAlertLogsQuery(string SerialNumber, FilterType FilterType) : IRequest<ErrorOr<IEnumerable<LogResult>>>;