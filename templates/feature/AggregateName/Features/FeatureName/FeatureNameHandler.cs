#if (IsQuery)
using Wolverine.Attributes;

#endif
namespace ModuleName.AggregateName.Features.FeatureName;

public static class FeatureNameHandler
{
#if (IsCommand)
    // Wolverine wraps this handler in a transaction and saves the DbContext afterwards: do not call
    // SaveChangesAsync. Return an Error for expected failures. Make it async once it awaits the database.
    public static Result<FeatureNameResult> Handle(FeatureNameCommand command, ModuleNameDbContext dbContext)
    {
        // Create or load the aggregate, e.g.:
        // var entity = FeatureNameMapper.ToEntity(command);
        // dbContext.Add(entity);
        // return FeatureNameMapper.ToResult(entity);
        return new FeatureNameResult(Guid.CreateVersion7());
    }
#else
    // Queries read with AsNoTracking and project to DTOs, e.g.:
    //   var result = await dbContext.Orders.AsNoTracking()
    //       .Where(order => order.Id == query.Id)
    //       .Select(FeatureNameMapper.Projection)
    //       .FirstOrDefaultAsync(cancellationToken);
    // They do not need a transaction.
    [NonTransactional]
    public static Result<FeatureNameResult> Handle(FeatureNameQuery query, ModuleNameDbContext dbContext) =>
        Error.NotFound("ModuleName.AggregateName.NotFound", $"'{query.Id}' was not found.");
#endif
}
