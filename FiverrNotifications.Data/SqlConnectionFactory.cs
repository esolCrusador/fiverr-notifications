using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace FiverrTelegramNotifications.Data
{
    public class SqlConnectionFactory
    {
        private readonly string _sqlString;
        public SqlConnectionFactory(string sqlString) => _sqlString = sqlString;

        public async Task<SqlConnection> Create()
        {
            var connection = new SqlConnection(_sqlString);
            await connection.OpenAsync();
            
            return connection;
        }
    }
}
