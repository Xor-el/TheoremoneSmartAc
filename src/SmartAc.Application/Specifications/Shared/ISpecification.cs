using System.Linq.Expressions;

namespace SmartAc.Application.Specifications.Shared;

public interface ISpecification<T>
{
    Expression<Func<T, bool>>? Criteria { get; }

    HashSet<Expression<Func<T, object>>> Includes { get; }

    HashSet<string> IncludeStrings { get; }

    Expression<Func<T, object>>? OrderBy { get; }

    Expression<Func<T, object>>? OrderByDescending { get; }

    Expression<Func<T, object>>? GroupBy { get; }

    bool IsPagingEnabled { get; }

    int Take { get; }

    int Skip { get; }
}