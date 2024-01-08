using Microsoft.EntityFrameworkCore;
using MonitorsPanel.Core.Manager.Primitives;

namespace MonitorsPanel.Web.ManagerApi.Infra.Repositories
{
  public abstract class EfCrudRepository<T>
    : EfCrudRepository<T, long>
    where T : class, IEntity<long>
  {
    protected EfCrudRepository(DbContext context, IUnitOfWork unitOfWork)
      : base(context, unitOfWork)
    {
    }
  }
}