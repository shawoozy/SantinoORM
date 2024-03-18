using System.Collections.Generic;
using SantinoORM.Builder;
using SantinoORM.Configuration;
using SantinoORM.ORM;

namespace SantinoORM.Repository
{
    /// <summary>
    /// This is a abstract repository class and is responsible for a couple of simple CRUD function for every API
    /// The queries parameters are all dynamic
    /// </summary>
    public abstract class BaseRepository<T> : IBaseRepository<T> where T : BaseModel, new()
    {
        /// <summary>
        /// Fetches all rows from table
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IList<T> FetchAll()
        {
            return Model().Get<T>();
        }

        /// <summary>
        /// Fetches single row based on id
        /// </summary>
        /// <param name="id"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Find(int id)
        {
            return Model().Find<T>(id);
        }

        /// <summary>
        /// InsertClause row into table
        /// </summary>
        /// <param name="model"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Insert(T model)
        {
            return model.Insert() as T;
        }

        /// <summary>
        /// UpdateClause row from table
        /// </summary>
        /// <param name="model">
        /// </param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Update(T model)
        {
            return model.Update() as T;
        }

        /// <summary>
        /// Delete row from table
        /// </summary>
        /// <param name="model"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool Delete(T model)
        {
            return model.Delete();
        }

        /// <summary>
        /// InsertClause rows into table
        /// </summary>
        /// <param name="models"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IList<T> BulkInsert(IList<T> models)
        {
            new InsertClause(new Execute(MysqlConnectionString.GetConnectionString())).BulkSave(
                models as IList<BaseModel>);
            return models;
        }

        /// <summary>
        /// UpdateClause row from table
        /// </summary>
        /// <param name="models">
        /// </param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IList<T> BulkUpdate(IList<T> models)
        {
            new UpdateClause(new Execute(MysqlConnectionString.GetConnectionString())).BulkUpdate(
                models as IList<BaseModel>);
            return models;
        }

        /// <summary>
        /// Return a collection of models from a single where statement
        /// </summary>
        /// <param name="column"></param>
        /// <param name="operatoR"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public IList<T> GetCollectionFromOneWhere(string column, object operatoR, object value = null)
        {
            return Model().Where(column, operatoR, value).Get<T>();
        }

        /// <summary>
        /// Return a single model from a single where statement
        /// </summary>
        /// <param name="column"></param>
        /// <param name="operatoR"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public T GetOneFromOneWhere(string column, object operatoR, object value = null)
        {
            return Model().Where(column, operatoR, value).First<T>();
        }

        /// <summary>
        /// Create instance of the model used in this repository
        /// </summary>
        /// <returns></returns>
        protected abstract T Model();
    }
}