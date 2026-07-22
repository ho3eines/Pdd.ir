using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Pdd.ir.Data
{
    public class DbService : IDbService
    {
        private readonly string _connectionString;

        public DbService(IConfiguration configuration)
        {
            _connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                ?? Environment.GetEnvironmentVariable("ConnectionString")
                ?? configuration.GetConnectionString("DefaultConnection")
                ?? "workstation id=support;password=123456;packet size=4096;user id=sa;data source=.;persist security info=false;initial catalog=pdd;Encrypt=False";
        }

        public IDbConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? parameters = null)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            return await connection.QueryFirstOrDefaultAsync<T>(sql, parameters);
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            return await connection.QueryAsync<T>(sql, parameters);
        }

        public async Task<int> ExecuteAsync(string sql, object? parameters = null)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            return await connection.ExecuteAsync(sql, parameters);
        }

        public async Task<T> ExecuteScalarAsync<T>(string sql, object? parameters = null)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            return (await connection.ExecuteScalarAsync<T>(sql, parameters))!;
        }
    }
}
