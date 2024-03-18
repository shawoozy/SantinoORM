using System.Collections.Generic;

namespace SantinoORM.Builder.Interfaces
{
    public interface IExecute
    {
        object RunQuery(string query, object dynamicParameters = null);

        IList<dynamic> ColumnsFromTable(string table);
    }
}