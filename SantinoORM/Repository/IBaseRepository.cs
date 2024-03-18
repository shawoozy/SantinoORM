using System.Collections.Generic;
using SantinoORM.ORM.Interfaces;

namespace SantinoORM.Repository
{
    public interface IBaseRepository<T> where T : IBaseModel
    {            
        IList<T> FetchAll(); 
        
        T Find(int id);

        T Insert(T model);

        T Update(T model);

        bool Delete(T model);

        IList<T> BulkUpdate(IList<T> models);

        IList<T> BulkInsert(IList<T> models);
        
        IList<T> GetCollectionFromOneWhere(string column, object operatoR, object value = null);

        T GetOneFromOneWhere(string column, object operatoR, object value = null);

    }
}