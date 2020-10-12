using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace FiverrTelegramNotifications.Data
{
    public class SqlConnectionFactory
    {
        private readonly string _connectionString;
        public SqlConnectionFactory(string connectionString) => _connectionString = connectionString;

        public async Task<SqlConnection> Create()
        {
            var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            return connection;
        }
    }
}
