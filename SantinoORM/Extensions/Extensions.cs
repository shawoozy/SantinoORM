using System;
using System.Collections.Generic;
using System.Linq;
using SantinoORM.Builder;
using SantinoORM.Configuration;
using SantinoORM.ORM;

namespace SantinoORM.Extensions
{
    public static class Extensions
    {
        /// <summary>
        /// Update all models in database
        /// </summary>
        /// <param name="models"></param>
        /// <typeparam name="T"></typeparam>
        /// <exception cref="Exception"></exception>
        public static void BulkUpdate<T>(this IEnumerable<T> models)
        {
            if (!typeof(T).IsSubclassOf(typeof(BaseModel)))
                throw new Exception("Class must extend BaseModel to call BulkUpdate");
       
            new UpdateClause(new Execute(MysqlConnectionString.GetConnectionString())).BulkUpdate(models as IList<BaseModel>);
        }

        /// <summary>
        /// Save all models to database
        /// </summary>
        /// <param name="models"></param>
        /// <typeparam name="T"></typeparam>
        /// <exception cref="Exception"></exception>
        public static void BulkSave<T>(this IEnumerable<T> models)
        {
            if (!typeof(T).IsSubclassOf(typeof(BaseModel)))
                throw new Exception("Class must extend BaseModel to call BulkUpdate");

            new InsertClause(new Execute( MysqlConnectionString.GetConnectionString())).BulkSave(models as IList<BaseModel>);
        }

        /// <summary>
        /// Lazyload a relation object 
        /// </summary>
        /// <param name="models"></param>
        /// <param name="column">The column of the orignal table</param>
        /// <param name="joinColumn">The column of the table to join on</param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TRelation"></typeparam>
        /// <exception cref="Exception"></exception>
        public static void LazyLoad<T, TRelation>(this IEnumerable<T> models, string column, string joinColumn) where TRelation : BaseModel, new() where T : BaseModel
        {
            if (!models.Any())
                return;
            
            if (!models.First().GetType().IsSubclassOf(typeof(BaseModel)))
                throw new Exception("Class must extend BaseModel to call LazyLoad");
            
            if (!typeof(TRelation).IsSubclassOf(typeof(BaseModel)))
                throw new Exception("Class must extend BaseModel to call LazyLoad");
                       
            ORM.LazyLoad.LoadModels<T, TRelation>(models, column, joinColumn);
        }
    }
}