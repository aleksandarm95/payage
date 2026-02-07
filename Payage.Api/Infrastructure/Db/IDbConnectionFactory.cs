using System.Data;

namespace Payage.Api.Infrastructure.Db
{
    public interface IDbConnectionFactory
    {
        IDbConnection Create();
    }
}
