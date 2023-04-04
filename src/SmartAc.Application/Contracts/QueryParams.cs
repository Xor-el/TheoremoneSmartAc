namespace SmartAc.Application.Contracts;

public sealed record QueryParams
{
    private const int MaxPageSize = 50;

    private readonly int _pageSize = 50;

    public int PageNumber { get; init; } = 1;

    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }

    public FilterType Filter { get; init; } = FilterType.New;
}