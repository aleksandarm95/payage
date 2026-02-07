using Npgsql;
using System.Data;

namespace Payage.Api.Infrastructure.Db
{
    public class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public DbConnectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("PayageDb")
                ?? throw new InvalidOperationException("Missing connection string: ConnectionStrings:PayageDb");
        }

        public IDbConnection Create() => new NpgsqlConnection(_connectionString);
    }
}
