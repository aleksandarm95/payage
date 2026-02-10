using Npgsql;
using Payage.Application.Abstractions;
using System.Data;

namespace Payage.Infrastructure.Db
{
    public class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public DbConnectionFactory(string connectionString)
        {
            _connectionString = connectionString ?? throw new InvalidOperationException("Missing connection string: ConnectionStrings:PayageDb");
        }

        public IDbConnection Create() => new NpgsqlConnection(_connectionString);
    }
}
