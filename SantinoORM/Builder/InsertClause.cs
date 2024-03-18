using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SantinoORM.Builder.Interfaces;
using SantinoORM.ORM;
using SantinoORM.ORM.Interfaces;

namespace SantinoORM.Builder
{
    public class InsertClause
    {
        private static IExecute _execute;

        public InsertClause(IExecute execute)
        {
            _execute = execute;
        }
        
        /// <summary>
        /// Save a model in the database
        /// </summary>
        /// <returns></returns>
        public IBaseModel Save(string table, IBaseModel model)
        {
            // build the query
            var query = BuildSaveQuery(table);
                         
            // add this to retrieve the id 
            query += "; select last_insert_id() as primaryKey;";
            
            // run the query
            var id = _execute.RunQuery(query, model) as IList<dynamic>;

            var primaryKeys = Fields.PrimaryKeyProperties(model);
            
            // set the primary key only if table has one primary key column
            if(primaryKeys.Count <= 1)
                model.SetProperty(primaryKeys[0].Name, Convert.ToInt32(id.First().primaryKey));

            return model;
        }

        /// <summary>
        /// Build the query to save a model in the database
        /// </summary>
        /// <param name="table"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public static string BuildSaveQuery(string table)
        {
            // new string builder for the query
            var sb = new StringBuilder();
            
            // columns of the table
            var columns = _execute.ColumnsFromTable(table);
            
            columns = Fields.FieldsWithoutPrimaryKey(columns);
            
            // the insert statement
            sb.AppendFormat("insert into {0} ({1}) values (", table, string.Join(", ", columns.Select(x => BackTicks.AddBackTicks(x))));

            // the column value mapping part
            for (var i = 0; i < columns.Count; i++)
            {
                sb.AppendFormat("@{0}", columns[i]);
                if (i < columns.Count - 1)
                    sb.Append(", ");
            }

            sb.Append(")");

            return sb.ToString();
        }

        /// <summary>
        /// Bulk save a list of the same models in the database
        /// </summary>
        /// <returns></returns>
        public IList<BaseModel> BulkSave(IList<BaseModel> models)
        {
            // build the query
            var query = BuildBulkSaveQuery(models);

            // run the query
            _execute.RunQuery(query);

            return models;
        }

        /// <summary>
        /// Build the query to save a model in the database
        /// </summary>
        /// <param name="models"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public static string BuildBulkSaveQuery(IList<BaseModel> models)
        {         
            // the model used to fetch table information
            var model = models.First();

            var table = model.Table;
            
            // new string builder for the query
            var sb = new StringBuilder();

            // columns of the table
            var columns = Fields.FieldsWithoutPrimaryKey(_execute.ColumnsFromTable(table));

            // the properties of the model
            var properties = model.GetType().GetProperties();

            // the insert statement
            sb.AppendFormat("insert into {0} ({1}) values ", table, string.Join(", ", columns.Select(x => BackTicks.AddBackTicks(x))));

            // map the values for each model
            for (var x = 0; x < models.Count; x++)
            {
                // the column value mapping part
                sb.Append("(");
                for (var i = 0; i < columns.Count; i++)
                {
                    // if poperty is not a column of the table, do not add to statement
                    var prop = properties.Where(y =>
                        string.Equals(y.Name, columns[i], StringComparison.CurrentCultureIgnoreCase)).ToList();
                    if (!prop.Any()) continue;

                    var value = prop.First().GetValue(models[x]);
                    sb.AppendFormat(@"""{0}""", value);
                    if (i < columns.Count - 1)
                        sb.Append(", ");
                }

                sb.Append(")");
                if (x < models.Count - 1)
                    sb.Append(", ");
            }

            return sb.ToString();
        }
        
    }
}