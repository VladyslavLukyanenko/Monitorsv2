using Microsoft.EntityFrameworkCore;
using MonitorsPanel.Core.Manager;
using MonitorsPanel.Core.Manager.Primitives;
using MonitorsPanel.Core.Manager.Services;

namespace MonitorsPanel.Web.ManagerApi.Infra.Repositories
{
  public class EfImageInfoRepository : EfCrudRepository<ImageInfo, string>, IImageInfoRepository
  {
    public EfImageInfoRepository(DbContext context, IUnitOfWork unitOfWork) : base(context, unitOfWork)
    {
    }
  }
}