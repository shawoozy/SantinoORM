namespace SantinoORM.Configuration
{
    public class MysqlConnectionString
    {
        private static string _connectionString;

        public static string GetConnectionString()
        {
            return _connectionString;
        }

        public static void SetConnectionString(string connectionString)
        {
            _connectionString = connectionString;
        }
    }
}