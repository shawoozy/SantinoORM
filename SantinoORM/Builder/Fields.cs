using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SantinoORM.Configuration;
using SantinoORM.ORM;
using SantinoORM.ORM.Interfaces;

namespace SantinoORM.Builder
{
    public static class Fields
    {
        /// <summary>
        /// Returns a list of field names of a table without the primary key
        /// </summary>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static List<dynamic> FieldsWithoutPrimaryKey(IList<dynamic> columns)
        {
            return columns.Where(x => x.Key != "PRI" && x.Field != "CreatedAt" && x.Field != "UpdatedAt").Select(x => x.Field).ToList();
        }

        /// <summary>
        /// Returns a list of primary key field names of a table
        /// </summary>
        /// <param name="colums"></param>
        /// <returns></returns>
        public static IList<dynamic> PrimaryKeyFields(IList<dynamic> colums)
        {
            return colums.Where(x => x.Key == "PRI").Select(x => x.Field).ToList();
        }

        /// <summary>
        /// Get the primary key(s) property of model
        /// </summary>
        /// <returns></returns>
        public static List<PropertyInfo> PrimaryKeyProperties(IBaseModel model)
        {
            var primaryKeys = model
                .GetType()
                .GetProperties()
                .Where(x => Attribute.IsDefined(x, typeof(PrimaryKey)))
                .ToList();
            if(primaryKeys.Count < 1)
                throw new Exception("Model is missing primary key attribute");

            return primaryKeys;
        }
    }
}