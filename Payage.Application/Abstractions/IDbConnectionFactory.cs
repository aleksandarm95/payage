using System.Data;

namespace Payage.Application.Abstractions
{
    public interface IDbConnectionFactory
    {
        IDbConnection Create();
    }
}
