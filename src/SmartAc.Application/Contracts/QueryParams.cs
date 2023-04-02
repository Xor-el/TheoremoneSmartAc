namespace SmartAc.Application.Contracts;

public sealed record QueryParams
{
    private const int MaxPageSize = 50;

    private int _pageSize = 50;

    public int PageNumber { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }

    public FilterType Filter { get; init; } = FilterType.New;
}