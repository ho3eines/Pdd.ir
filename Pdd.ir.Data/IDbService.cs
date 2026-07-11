using System.Data;
using System.Data.SqlClient;

namespace Pdd.ir.Data
{
    public interface IDbService
    {
        IDbConnection GetConnection();
        Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? parameters = null);
        Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null);
        Task<int> ExecuteAsync(string sql, object? parameters = null);
        Task<T> ExecuteScalarAsync<T>(string sql, object? parameters = null);
    }
}
