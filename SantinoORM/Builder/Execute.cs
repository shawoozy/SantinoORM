using System.Collections.Generic;
using System.Linq;
using Dapper;
using MySql.Data.MySqlClient;
using SantinoORM.Builder.Interfaces;

namespace SantinoORM.Builder
{
    public class Execute : IExecute
    {
        private readonly string _mysqlConnectionString;

        public Execute(string mysqlConnectionString)
        {
            _mysqlConnectionString = mysqlConnectionString;
        }
        /// <summary>
        /// Execute a query and return results
        /// </summary>
        /// <param name="query"></param>
        /// <param name="dynamicParameters"></param>
        /// <returns></returns>
        public object RunQuery(string query,  object dynamicParameters = null)
        {
            using (var dbConnection = new MySqlConnection(_mysqlConnectionString))
            {
                dbConnection.Open();
                var result = dbConnection.Query(query, dynamicParameters).ToList();
                return result;
            }
        }

        /// <summary>
        /// Get columns of the table, it is virtual for mocking purposes
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public IList<dynamic> ColumnsFromTable(string table)
        {
            var query = "show columns from " + table;
            return RunQuery(query) as IList<dynamic>;
        }
    }
}