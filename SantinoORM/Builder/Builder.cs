using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using SantinoORM.Configuration;
using SantinoORM.Extensions;
using SantinoORM.ORM;
using SantinoORM.ORM.Interfaces;

namespace SantinoORM.Builder
{
    public class Builder : Query
    {

        [JsonIgnore] 
        private bool _lazyLoaders = false;
        
        private readonly IList<DoLazyLoad> _lazyLoadFuncs = new List<DoLazyLoad>();

        private delegate void DoLazyLoad(IEnumerable<BaseModel> list);

        /// <summary>
        /// Constructor
        /// </summary>
        public Builder()
        {
            var table = GetType().GetTypeInfo().GetCustomAttribute<Table>();

            if (table == null)
                throw new CustomAttributeFormatException($"Table attribute is missing of {GetType()}");

            Table = "`" + table.TableName + "`";
        }

        /// <summary>
        /// Returns results from query as a object (probably like a dictionary)
        /// </summary>
        /// <returns></returns>
        public object GetAsObject()
        {
            return Mapper().GetAsObject(BuildQuery(), ValueBindings);
        }


        /// <summary>
        /// Returns a single model without relations 
        /// </summary>
        public T First<T>() where T : BaseModel
        {
            NoRelations();
            ReplaceBinding(new List<string> { "1" }, LimitIndex, "limit");
            return Get<T>().FirstOrDefault();
        }

        /// <summary>
        /// Returns a single model with relations 
        /// </summary>
        public TFirst First<TFirst, TSecond>() where TFirst : BaseModel where TSecond : BaseModel
        {
            return Get<TFirst, TSecond>().FirstOrDefault();
        }

        /// <summary>
        /// Returns a single model with relations 
        /// </summary>
        public TFirst First<TFirst, TSecond, TThird>() where TFirst : BaseModel
            where TSecond : BaseModel
            where TThird : BaseModel
        {
            return Get<TFirst, TSecond, TThird>().FirstOrDefault();
        }

        /// <summary>
        /// Returns a single model with relations 
        /// </summary>
        public TFirst First<TFirst, TSecond, TThird, TFourth>() where TFirst : BaseModel
            where TSecond : BaseModel
            where TThird : BaseModel
            where TFourth : BaseModel
        {
            return Get<TFirst, TSecond, TThird, TFourth>().FirstOrDefault();
        }
        
        /// <summary>
        /// Returns a single model with relations 
        /// </summary>
        public TFirst First<TFirst, TSecond, TThird, TFourth, TFifth>() where TFirst : BaseModel
            where TSecond : BaseModel
            where TThird : BaseModel
            where TFourth : BaseModel
            where TFifth : BaseModel
        {
            return Get<TFirst, TSecond, TThird, TFourth, TFifth>().FirstOrDefault();
        }
        
        /// <summary>
        /// Returns a single model with relations 
        /// </summary>
        public TFirst First<TFirst, TSecond, TThird, TFourth, TFifth, TSixth>() where TFirst : BaseModel
            where TSecond : BaseModel
            where TThird : BaseModel
            where TFourth : BaseModel
            where TFifth : BaseModel
            where TSixth : BaseModel
        {
            return Get<TFirst, TSecond, TThird, TFourth, TFifth, TSixth>().FirstOrDefault();
        }
        
        /// <summary>
        /// Returns a single model with relations 
        /// </summary>
        public TFirst First<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh>() where TFirst : BaseModel
            where TSecond : BaseModel
            where TThird : BaseModel
            where TFourth : BaseModel
            where TFifth : BaseModel
            where TSixth : BaseModel
            where TSeventh : BaseModel
        {
            return Get<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh>().FirstOrDefault();
        }

        /// <summary>
        /// Returns a collection of models without relations
        /// </summary>
        /// <returns></returns>     
        public IList<T> Get<T>() where T : BaseModel
        {
            NoRelations();
            var result = Mapper().Get<T>(BuildQuery(), ValueBindings);
            if (!_lazyLoaders)
                return result;

            if (!result.Any())
                return result;
            
            foreach (var lazyLoadFunc in _lazyLoadFuncs)
                lazyLoadFunc.Invoke(result);


            return result;
        }

        /// <summary>
        /// Returns a collection of models with relations
        /// </summary>
        /// <typeparam name="TFirst"></typeparam>
        /// <typeparam name="TSecond"></typeparam>
        /// <returns></returns>
        public IList<TFirst> Get<TFirst, TSecond>() where TFirst : BaseModel where TSecond : BaseModel
        {
            return Mapper().Get<TFirst, TSecond>(BuildQuery(), ValueBindings);
        }

        /// <summary>
        /// Returns a collection of models with relations
        /// </summary>
        /// <typeparam name="TFirst"></typeparam>
        /// <typeparam name="TSecond"></typeparam>
        /// <typeparam name="TThird"></typeparam>
        /// <returns></returns>
        public IList<TFirst> Get<TFirst, TSecond, TThird>() where TFirst : BaseModel
            where TSecond : BaseModel
            where TThird : BaseModel
        {
            return Mapper().Get<TFirst, TSecond, TThird>(BuildQuery(), ValueBindings);
        }

        /// <summary>
        /// Returns a collection of models with relations
        /// </summary>
        /// <typeparam name="TFirst"></typeparam>
        /// <typeparam name="TSecond"></typeparam>
        /// <typeparam name="TThird"></typeparam>
        /// <typeparam name="TFourth"></typeparam>
        /// <returns></returns>
        public IList<TFirst> Get<TFirst, TSecond, TThird, TFourth>() where TFirst : BaseModel
            where TSecond : BaseModel
            where TThird : BaseModel
            where TFourth : BaseModel
        {
            return Mapper().Get<TFirst, TSecond, TThird, TFourth>(BuildQuery(), ValueBindings);
        }

        /// <summary>
        /// Returns a collection of models with relations
        /// </summary>
        /// <typeparam name="TFirst"></typeparam>
        /// <typeparam name="TSecond"></typeparam>
        /// <typeparam name="TThird"></typeparam>
        /// <typeparam name="TFourth"></typeparam>
        /// <typeparam name="TFifth"></typeparam>
        /// <returns></returns>
        public IList<TFirst> Get<TFirst, TSecond, TThird, TFourth, TFifth>() where TFirst : BaseModel
            where TSecond : BaseModel
            where TThird : BaseModel
            where TFourth : BaseModel
            where TFifth : BaseModel
        {
            return Mapper().Get<TFirst, TSecond, TThird, TFourth, TFifth>(BuildQuery(), ValueBindings);
        }
        
        /// <summary>
        /// Returns a collection of models with relations
        /// </summary>
        /// <typeparam name="TFirst"></typeparam>
        /// <typeparam name="TSecond"></typeparam>
        /// <typeparam name="TThird"></typeparam>
        /// <typeparam name="TFourth"></typeparam>
        /// <typeparam name="TFifth"></typeparam>
        /// <typeparam name="TSixth"></typeparam>
        /// <returns></returns>
        public IList<TFirst> Get<TFirst, TSecond, TThird, TFourth, TFifth, TSixth>() where TFirst : BaseModel
            where TSecond : BaseModel
            where TThird : BaseModel
            where TFourth : BaseModel
            where TFifth : BaseModel
            where TSixth : BaseModel
        {
            return Mapper().Get<TFirst, TSecond, TThird, TFourth, TFifth, TSixth>(BuildQuery(), ValueBindings);
        }
        
        /// <summary>
        /// Returns a collection of models with relations
        /// </summary>
        /// <typeparam name="TFirst"></typeparam>
        /// <typeparam name="TSecond"></typeparam>
        /// <typeparam name="TThird"></typeparam>
        /// <typeparam name="TFourth"></typeparam>
        /// <typeparam name="TFifth"></typeparam>
        /// <typeparam name="TSixth"></typeparam>
        /// <typeparam name="TSeventh"></typeparam>
        /// <returns></returns>
        public IList<TFirst> Get<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh>() where TFirst : BaseModel
            where TSecond : BaseModel
            where TThird : BaseModel
            where TFourth : BaseModel
            where TFifth : BaseModel
            where TSixth : BaseModel
            where TSeventh : BaseModel
        {
            return Mapper().Get<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh>(BuildQuery(), ValueBindings);
        }

        /// <summary>
        /// Lazy load the relation
        /// </summary>
        /// <param name="column"></param>
        /// <param name="joinColumn"></param>
        /// <typeparam name="TRelation"></typeparam>
        /// <returns></returns>
        public Builder With<TRelation>(string column, string joinColumn) where TRelation : BaseModel, new()
        {
            var delegateDoLazyLoad = new DoLazyLoad(list => list.LazyLoad<BaseModel, TRelation>(column, joinColumn));
            _lazyLoadFuncs.Add(delegateDoLazyLoad);
            _lazyLoaders = true;
            return this;
        }

        private static IMapper Mapper()
        {
            return new Mapper(MysqlConnectionString.GetConnectionString());
        }
    }
}
