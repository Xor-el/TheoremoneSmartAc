using System.Linq.Expressions;

namespace SmartAc.Application.Specifications.Shared;

public abstract class BaseSpecification<T> : ISpecification<T>
{
    protected BaseSpecification(Expression<Func<T, bool>>? criteria = null) => Criteria = criteria;

    protected BaseSpecification()
    {
    }

    public Expression<Func<T, bool>>? Criteria { get; }
    public List<Expression<Func<T, object>>> Includes { get; } = new();
    public List<string> IncludeStrings { get; } = new();
    public Expression<Func<T, object>>? OrderBy { get; private set; }
    public Expression<Func<T, object>>? OrderByDescending { get; private set; }
    public Expression<Func<T, object>>? GroupBy { get; private set; }
    public bool IsPagingEnabled { get; private set; }
    public int Take { get; private set; }
    public int Skip { get; private set; }

    protected virtual void AddInclude(Expression<Func<T, object>> include) => Includes.Add(include);
    protected virtual void AddInclude(string include) => IncludeStrings.Add(include);
    protected virtual void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }
    protected virtual void ApplyOrderBy(Expression<Func<T, object>> orderBy) => OrderBy = orderBy;
    protected virtual void ApplyOrderByDescending(Expression<Func<T, object>> orderByDesc)
        => OrderByDescending = orderByDesc;
    protected virtual void ApplyGroupBy(Expression<Func<T, object>> groupBy) => GroupBy = groupBy;
}