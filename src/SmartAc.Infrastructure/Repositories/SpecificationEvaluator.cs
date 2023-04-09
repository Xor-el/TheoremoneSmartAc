using Microsoft.EntityFrameworkCore;
using SmartAc.Application.Specifications.Shared;
using SmartAc.Domain;

namespace SmartAc.Infrastructure.Repositories;

public class SpecificationEvaluator<TEntity> where TEntity : EntityBase
{
    public static IQueryable<TEntity> GetQuery(IQueryable<TEntity> inputQuery, ISpecification<TEntity> specification)
    {
        var query = specification.IsQuerySplittingEnabled ? inputQuery.AsSplitQuery() : inputQuery;

        if (specification.Criteria is not null)
        {
            query = query.Where(specification.Criteria);
        }

        query = specification.Includes.Aggregate(query, (current, expression)
            => current.Include(expression));

        query = specification.IncludeStrings.Aggregate(query, (current, expression)
            => current.Include(expression));

        if (specification.OrderBy is not null)
        {
            query = query.OrderBy(specification.OrderBy);
        }
        else if (specification.OrderByDescending is not null)
        {
            query = query.OrderByDescending(specification.OrderByDescending);
        }

        if (specification.GroupBy is not null)
        {
            query = query.GroupBy(specification.GroupBy).SelectMany(x => x);
        }

        if (specification.IsPagingEnabled)
        {
            query = query.Skip(specification.Skip).Take(specification.Take);
        }

        return query;
    }
}