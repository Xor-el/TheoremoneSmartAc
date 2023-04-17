using MediatR;
using SmartAc.Application.Contracts;

namespace SmartAc.Application.Features.Devices.AlertLogs;

public sealed record GetAlertLogsQuery(string SerialNumber, QueryParams Params) : IRequest<PagedList<LogItem>>;