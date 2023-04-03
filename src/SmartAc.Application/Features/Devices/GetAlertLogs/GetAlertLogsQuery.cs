using ErrorOr;
using MediatR;
using SmartAc.Application.Contracts;

namespace SmartAc.Application.Features.Devices.GetAlertLogs;

public sealed record GetAlertLogsQuery(string SerialNumber, QueryParams Params) : IRequest<ErrorOr<IEnumerable<LogItem>>>;