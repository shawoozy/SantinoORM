using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using SantinoORM.Builder.Interfaces;
using SantinoORM.ORM;
using SantinoORM.ORM.Interfaces;

namespace SantinoORM.Builder
{
    public class UpdateClause
    {
        private static IExecute _execute;

        public UpdateClause(IExecute execute)
        {
            _execute = execute;
        }
        
        /// <summary>
        /// Directly update a model
        /// </summary>
        /// <returns></returns>
        public void UpdateModel(string table, BaseModel model)
        {
            // if the model has no primary key, it means it is not in database yet.
            if (model.GetPrimaryKey() == null || (int)model.GetPrimaryKey() == 0)
            {
                new InsertClause(_execute).Save(table, model);
                return;
            }

            // build the query
            var query = BuildUpdateQuery(table, model);

            // run the query
            if(!string.IsNullOrEmpty(query))
             _execute.RunQuery(query, model);
        }

        /// <summary>
        /// Builds the query to update a single model
        /// </summary>
        /// <param name="table"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public static string BuildUpdateQuery(string table, BaseModel model)
        {
            // new string builder for the query
            var sb = new StringBuilder();
            sb.AppendFormat("update {0} set ", table);

            // columns of the table
            var columns = Fields.FieldsWithoutPrimaryKey(_execute.ColumnsFromTable(table));

            var dirtyFields = OnlyDirtyProperties(model, columns, model.GetType().GetProperties());
            if (dirtyFields.Count < 1)
                return "";
            
            // primary key(s) of the table
            var primaryKey = Fields.PrimaryKeyProperties(model);

            // create the part where fields are mapped
            for (var i = 0; i < dirtyFields.Count; i++)
            {
                sb.AppendFormat("`{0}` = @{0}", dirtyFields[i]);
                if (i < dirtyFields.Count - 1)
                    sb.Append(", ");
            }

            // create the where part
            sb.Append(" where ");
            for (var i = 0; i < primaryKey.Count; i++)
            {
                sb.AppendFormat("{0} = {1}", primaryKey[i].Name, primaryKey[i].GetValue(model));
                if (primaryKey.Count > 1 && i < primaryKey.Count - 1)
                    sb.Append(" and ");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Bulk update a list of the same models in the database
        /// </summary>
        /// <returns></returns>
        public IList<BaseModel> BulkUpdate(IList<BaseModel> models)
        {
            // build the query
            var query = BuildBulkUpdateQuery(models);

            // run the query
            _execute.RunQuery(query);

            return models;
        }

        /// <summary>
        /// Builds a query to update multiple models
        /// </summary>
        /// <param name="models"></param>
        /// <param name="table"></param>
        /// <typeparam name="BaseModel"></typeparam>
        /// <returns></returns>
        public static string BuildBulkUpdateQuery(IList<BaseModel> models)
        {
            // the model used to fetch table information
            var model = models.First();

            var table = model.Table;

            // new string builder for the query
            var sb = new StringBuilder();

            // columns of the table
            var columns = _execute.ColumnsFromTable(table);

            // primary key(s) of the table 
            var primaryKeys = Fields.PrimaryKeyFields(columns);

            // columns without primary keys of the table
            columns = columns.Select(x => x.Field).ToList();
            
            
            // the statement is an insert statment, not an update statement, explained later in the method.
            sb.AppendFormat("insert into {0} ({1}) values ", table,
                string.Join(", ", columns.Select(x => BackTicks.AddBackTicks(x))));

            // the properties of the model
            var properties = model.GetType().GetProperties();

            // create the part where fields are mapped for each model 
            for (var x = 0; x < models.Count; x++)
            {
                sb.Append("(");

                // add each column to the statement
                for (var i = 0; i < columns.Count; i++)
                {
                    var prop = properties.Where(y =>
                        string.Equals(y.Name, columns[i], StringComparison.CurrentCultureIgnoreCase));

                    // if poperty is not a column of the table, do not add to statement
                    if (!prop.Any())
                        continue;

                    // set the value for the column
                    var value = prop.First().GetValue(models[x]);
                    sb.AppendFormat(@"""{0}""", value);

                    if (i < columns.Count - 1)
                        sb.Append(", ");
                }

                sb.Append(")");
                if (x < models.Count - 1)
                    sb.Append(", ");
            }

            // the statement is an insert because there is no easier way to do bulk updates on sql
            // if the primary key already exists, do not insert but update the row, pretty sweet
            sb.AppendFormat(" on duplicate key update ");
            for (var a = 0; a < columns.Count; a++)
            {
                // ignore the primary key columns in the statement
                if (primaryKeys.Any(x => x.ToLower() == columns[a].ToLower()))
                    continue;

                sb.AppendFormat("{0} = VALUES({0})", BackTicks.AddBackTicks(columns[a]));
                if (a < columns.Count - 1)
                    sb.Append(", ");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Returns a list of dirty properties
        /// </summary>
        /// <param name="model"></param>
        /// <param name="columns"></param>
        /// <param name="propertyInfos"></param>
        /// <returns></returns>
        private static IList<string> OnlyDirtyProperties(IBaseModel model, IList<dynamic> columns, IList<PropertyInfo> propertyInfos)
        {
            var dirtyFields = new List<string>();
            var originalModel = model.GetOriginal();

            if (originalModel == null)
            {
                dirtyFields.AddRange(columns.Cast<string>());
                return dirtyFields;
            }

            var originalProperties = originalModel.GetType().GetProperties();
            
            for (var i = 0; i < columns.Count; i++)
            {
                // the property of the model
                var property = propertyInfos.First(y => string.Equals(y.Name, columns[i], StringComparison.CurrentCultureIgnoreCase));
                
                // the property of the unchanged original model
                var original = originalProperties.FirstOrDefault(y => string.Equals(y.Name, property.Name));
                var originalValue = original.GetValue(originalModel);
                var changedValue = property.GetValue(model);
                // if the value is not equal, then the property is dirty
                if(originalValue != changedValue)
                    dirtyFields.Add(columns[i]);
            }

            return dirtyFields;
        }
    }
}