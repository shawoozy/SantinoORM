using System.Collections.Generic;
using Dapper;

namespace SantinoORM.ORM.Interfaces
{
    public interface IMapper
    {
        object GetAsObject(string query, DynamicParameters dynamicParameters);

        IList<TFirst> Get<TFirst, TSecond>(string query, DynamicParameters dynamicParameters)
            where TFirst : BaseModel where TSecond : BaseModel;

        IList<TFirst> Get<TFirst, TSecond, TThird>(string query, DynamicParameters dynamicParameters)
            where TFirst : BaseModel where TSecond : BaseModel where TThird : BaseModel;

        IList<TFirst> Get<TFirst, TSecond, TThird, TFourth>(string query, DynamicParameters dynamicParameters)
            where TFirst : BaseModel where TSecond : BaseModel where TThird : BaseModel where TFourth : BaseModel;

        IList<TFirst> Get<TFirst, TSecond, TThird, TFourth, TFifth>(string query, DynamicParameters dynamicParameters)
            where TFirst : BaseModel
            where TSecond : BaseModel
            where TThird : BaseModel
            where TFourth : BaseModel
            where TFifth : BaseModel;
        
        IList<TFirst> Get<TFirst, TSecond, TThird, TFourth, TFifth, TSixth>(string query, DynamicParameters dynamicParameters)
            where TFirst : BaseModel
            where TSecond : BaseModel
            where TThird : BaseModel
            where TFourth : BaseModel
            where TSixth :  BaseModel
            where TFifth :  BaseModel;
        
        IList<TFirst> Get<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh>(string query, DynamicParameters dynamicParameters)
            where TFirst : BaseModel
            where TSecond : BaseModel
            where TThird : BaseModel
            where TFourth : BaseModel
            where TSixth :  BaseModel
            where TFifth :  BaseModel
            where TSeventh :  BaseModel;

        List<T> Get<T>(string query, DynamicParameters dynamicParameters) where T : IBaseModel;

    }
}