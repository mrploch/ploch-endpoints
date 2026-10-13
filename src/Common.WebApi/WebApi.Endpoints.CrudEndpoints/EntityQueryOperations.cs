namespace Ploch.Common.WebApi.Endpoints.CrudEndpoints;

public class EntityQueryOperations<TEntity>
{
    //public Expression<Func<, object>>? OrderBy { get; set; }

    public Func<IQueryable<TEntity>, IQueryable<TEntity>>? GetListOperation { get; set; }

    public Func<IQueryable<TEntity>, IQueryable<TEntity>>? GetByIdOperation { get; set; }

    public Func<IQueryable<TEntity>, IQueryable<TEntity>>? UpdateOperation { get; set; }
}
