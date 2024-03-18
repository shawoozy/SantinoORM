using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Dapper;
using MySql.Data.MySqlClient;
using SantinoORM.ORM.Interfaces;

namespace SantinoORM.ORM

{
    public class Mapper : IMapper
    {
        private readonly string _mysqlConnectionString;

        /// <summary>
        /// A dictionary containing properties. key = model type and value = properties
        /// </summary>
        private readonly Dictionary<Type, PropertyInfo[]> _propertyCache = new Dictionary<Type, PropertyInfo[]>();

        /// <summary>
        /// A dictionary containing creator objects. key = model type and value = a creator object
        /// </summary>
        private readonly Dictionary<Type, ObjectActivator> _creatorCache = new Dictionary<Type, ObjectActivator>();

        private delegate object ObjectActivator();

        public Mapper(string mysqlConnectionString)
        {
            _mysqlConnectionString = mysqlConnectionString;
        }

        /// <summary>
        /// Return query result as an object
        /// </summary>
        /// <param name="query"></param>
        /// <param name="dynamicParameters"></param>
        /// <returns></returns>
        public object GetAsObject(string query, DynamicParameters dynamicParameters)
        {
            using (var dbConnection = new MySqlConnection(_mysqlConnectionString))
            {
                dbConnection.Open();
                var result = dbConnection.Query<dynamic>(query, dynamicParameters);
                var asObject = result.ToList();
                return asObject.Count() > 1 ? asObject : asObject.First();
            }
        }

        /// <summary>
        /// Fetch objects from database and map them to their corresponding models
        /// </summary>
        /// <param name="query"></param>
        /// <param name="dynamicParameters"></param>
        /// <typeparam name="TFirst"></typeparam>
        /// <typeparam name="TSecond"></typeparam>
        /// <returns></returns>
        public IList<TFirst> Get<TFirst, TSecond>(string query, DynamicParameters dynamicParameters)
            where TFirst : BaseModel where TSecond : BaseModel
        {
            using (var dbConnection = new MySqlConnection(_mysqlConnectionString))
            {
                // the collection which holds the objects as dictionary to avoid duplicates
                // key is an int most of the time, except when table has multiple primary keys
                var collection = new Dictionary<object, TFirst>();

                dbConnection.Open();
                dbConnection.Query<TFirst, TSecond, TFirst>(query,
                    (first, second) =>
                    {
                        CopyModel(first);
                        CopyModel(second);

                        if (collection.TryGetValue(first.GetPrimaryKey(), out var f))
                            return MapRelations(new IBaseModel[] {f, second}) as TFirst;

                        collection.Add(first.GetPrimaryKey(), first);
                        return MapRelations(new IBaseModel[] {first, second}) as TFirst;
                    },
                    dynamicParameters, splitOn: ":");

                ClearCache();
                return collection.Values.ToList();
            }
        }

        /// <summary>
        ///Fetch objects from database and map them to their corresponding models
        /// </summary>
        /// <param name="query"></param>
        /// <param name="dynamicParameters"></param>
        /// <typeparam name="TFirst"></typeparam>
        /// <typeparam name="TSecond"></typeparam>
        /// <typeparam name="TThird"></typeparam>
        /// <returns></returns>
        public IList<TFirst> Get<TFirst, TSecond, TThird>(string query, DynamicParameters dynamicParameters)
            where TFirst : BaseModel where TSecond : BaseModel where TThird : BaseModel
        {
            using (var dbConnection = new MySqlConnection(_mysqlConnectionString))
            {
                dbConnection.Open();

                var collection = new Dictionary<object, TFirst>();

                dbConnection.Query<TFirst, TSecond, TThird, TFirst>(query,
                    (first, second, third) =>
                    {
                        CopyModel(first);
                        CopyModel(second);
                        CopyModel(third);
                        if (collection.TryGetValue(first.GetPrimaryKey(), out var f))
                            return MapRelations(new IBaseModel[] {f, second, third}) as TFirst;

                        collection.Add(first.GetPrimaryKey(), first);
                        return MapRelations(new IBaseModel[] {first, second, third}) as TFirst;
                    },
                    dynamicParameters, splitOn: ":");
                ClearCache();
                return collection.Values.ToList();
            }
        }

        /// <summary>
        /// Fetch objects from database and map them to their corresponding models
        /// </summary>
        /// <param name="query"></param>
        /// <param name="dynamicParameters"></param>
        /// <typeparam name="TFirst"></typeparam>
        /// <typeparam name="TSecond"></typeparam>
        /// <typeparam name="TThird"></typeparam>
        /// <typeparam name="TFourth"></typeparam>
        /// <returns></returns>
        public IList<TFirst> Get<TFirst, TSecond, TThird, TFourth>(string query, DynamicParameters dynamicParameters)
            where TFirst : BaseModel where TSecond : BaseModel where TThird : BaseModel where TFourth : BaseModel
        {
            using (var dbConnection = new MySqlConnection(_mysqlConnectionString))
            {
                var collection = new Dictionary<object, TFirst>();
                dbConnection.Open();
                dbConnection.Query<TFirst, TSecond, TThird, TFourth, TFirst>(query,
                    (first, second, third, fourth) =>
                    {
                        CopyModel(first);
                        CopyModel(second);
                        CopyModel(third);
                        CopyModel(fourth);
                        if (collection.TryGetValue(first.GetPrimaryKey(), out var f))
                            return MapRelations(new IBaseModel[] {f, second, third, fourth}) as TFirst;

                        collection.Add(first.GetPrimaryKey(), first);
                        return MapRelations(new IBaseModel[] {first, second, third, fourth}) as TFirst;
                    },
                    dynamicParameters, splitOn: ":").ToList();
                ClearCache();
                return collection.Values.ToList();
            }
        }

        /// <summary>
        /// Fetch objects from database and map them to their corresponding models
        /// </summary>
        /// <param name="query"></param>
        /// <param name="dynamicParameters"></param>
        /// <typeparam name="TFirst"></typeparam>
        /// <typeparam name="TSecond"></typeparam>
        /// <typeparam name="TThird"></typeparam>
        /// <typeparam name="TFourth"></typeparam>
        /// <typeparam name="TFifth"></typeparam>
        /// <returns></returns>
        public IList<TFirst> Get<TFirst, TSecond, TThird, TFourth, TFifth>(string query,
            DynamicParameters dynamicParameters)
            where TFirst : BaseModel
            where TSecond : BaseModel
            where TThird : BaseModel
            where TFourth : BaseModel
            where TFifth : BaseModel
        {
            using (var dbConnection = new MySqlConnection(_mysqlConnectionString))
            {
                var collection = new Dictionary<object, TFirst>();
                dbConnection.Open();
                dbConnection.Query<TFirst, TSecond, TThird, TFourth, TFifth, TFirst>(query,
                    (first, second, third, fourth, fifth) =>
                    {
                        CopyModel(first);
                        CopyModel(second);
                        CopyModel(third);
                        CopyModel(fifth);
                        if (collection.TryGetValue(first.GetPrimaryKey(), out var f))
                            return MapRelations(new IBaseModel[] {f, second, third, fourth, fifth}) as TFirst;

                        collection.Add(first.GetPrimaryKey(), first);
                        return MapRelations(new IBaseModel[] {first, second, third, fourth, fifth}) as TFirst;
                    },
                    dynamicParameters, splitOn: ":", buffered: true, commandTimeout: 120);

                return collection.Values.ToList();
            }
        }
        
        
        /// <summary>
        /// Fetch objects from database and map them to their corresponding models
        /// </summary>
        /// <param name="query"></param>
        /// <param name="dynamicParameters"></param>
        /// <typeparam name="TFirst"></typeparam>
        /// <typeparam name="TSecond"></typeparam>
        /// <typeparam name="TThird"></typeparam>
        /// <typeparam name="TFourth"></typeparam>
        /// <typeparam name="TFifth"></typeparam>
        /// <typeparam name="TSixth"></typeparam>
        /// <returns></returns>
        public IList<TFirst> Get<TFirst, TSecond, TThird, TFourth, TFifth, TSixth>(string query,
            DynamicParameters dynamicParameters)
            where TFirst : BaseModel
            where TSecond : BaseModel
            where TThird : BaseModel
            where TFourth : BaseModel
            where TFifth : BaseModel
            where TSixth : BaseModel
        {
            using (var dbConnection = new MySqlConnection(_mysqlConnectionString))
            {
                var collection = new Dictionary<object, TFirst>();
                dbConnection.Open();
                dbConnection.Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TFirst>(query,
                    (first, second, third, fourth, fifth, sixth) =>
                    {
                        CopyModel(first);
                        CopyModel(second);
                        CopyModel(third);
                        CopyModel(fifth);
                        CopyModel(sixth);
                        if (collection.TryGetValue(first.GetPrimaryKey(), out var f))
                            return MapRelations(new IBaseModel[] {f, second, third, fourth, fifth, sixth}) as TFirst;

                        collection.Add(first.GetPrimaryKey(), first);
                        return MapRelations(new IBaseModel[] {first, second, third, fourth, fifth, sixth}) as TFirst;
                    },
                    dynamicParameters, splitOn: ":").ToList();

                return collection.Values.ToList();
            }
        }

        /// <summary>
        /// Fetch objects from database and map them to their corresponding models
        /// </summary>
        /// <param name="query"></param>
        /// <param name="dynamicParameters"></param>
        /// <typeparam name="TFirst"></typeparam>
        /// <typeparam name="TSecond"></typeparam>
        /// <typeparam name="TThird"></typeparam>
        /// <typeparam name="TFourth"></typeparam>
        /// <typeparam name="TFifth"></typeparam>
        /// <typeparam name="TSixth"></typeparam>
        /// <typeparam name="TSeventh"></typeparam>
        /// <returns></returns>
        public IList<TFirst> Get<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh>(string query,
            DynamicParameters dynamicParameters)
            where TFirst : BaseModel
            where TSecond : BaseModel
            where TThird : BaseModel
            where TFourth : BaseModel
            where TFifth : BaseModel
            where TSixth : BaseModel
            where TSeventh : BaseModel
        {
            using (var dbConnection = new MySqlConnection(_mysqlConnectionString))
            {
                var collection = new Dictionary<object, TFirst>();
                dbConnection.Open();
                dbConnection.Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth,TSeventh, TFirst>(query,
                    (first, second, third, fourth, fifth, sixth, seventh) =>
                    {
                        CopyModel(first);
                        CopyModel(second);
                        CopyModel(third);
                        CopyModel(fifth);
                        CopyModel(sixth);
                        CopyModel(seventh);
                        if (collection.TryGetValue(first.GetPrimaryKey(), out var f))
                            return MapRelations(new IBaseModel[] {f, second, third, fourth, fifth, sixth, seventh}) as TFirst;

                        collection.Add(first.GetPrimaryKey(), first);
                        return MapRelations(new IBaseModel[] {first, second, third, fourth, fifth, sixth, seventh}) as TFirst;
                    },
                    dynamicParameters, splitOn: ":").ToList();

                return collection.Values.ToList();
            }
        }

        public List<T> Get<T>(string query, DynamicParameters dynamicParameters) where T : IBaseModel
        {
            using (var dbConnection = new MySqlConnection(_mysqlConnectionString))
            {
                dbConnection.Open();
                var result = dbConnection.Query<T>(query, dynamicParameters);
                foreach (var model in result)
                    CopyModel(model);


                return (List<T>) result;
            }
        }


        /// <summary>
        /// this function loops through a row of models and adds the relation to each other
        /// </summary>
        /// <param name="row"> an array of models from a single row</param>
        /// <returns></returns>
        public object MapRelations(IBaseModel[] row)
        {
            for (var i = 0; i < row.Length; i++)
            {
                var item = row[i];
                for (var y = 0; y < row.Length; y++)
                {
                    var relation = row[y];
                    var properties = GetPropertyInfos(item.GetType());

                    foreach (var propertyInfo in properties)
                    {
                        if (propertyInfo.PropertyType != relation.GetType())
                        {
                            if (propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                            {
                                if(propertyInfo.PropertyType.GetGenericArguments()[0] != relation.GetType())
                                    continue;
                            }else
                                continue;
                        }
                        
                        if(relation.GetPrimaryKey() == null || long.Parse(relation.GetPrimaryKey().ToString()) == 0)
                            continue;
                        
                        // check if relation is one-to-many or one-to-one
                        var original = propertyInfo.PropertyType.IsGenericType
                            ? OneToMany(item, relation, propertyInfo.Name)
                            : OneToOne(item, relation, propertyInfo.Name);

                        // the reference to the original model
                        row[y] = original;
                        break;
                    }
                }
            }

            return row[0];
        }

        /// <summary>
        /// Sets the one-to-one relation
        /// </summary>
        /// <param name="model"></param>
        /// <param name="relation"></param>
        /// <param name="relationName"></param>
        private static IBaseModel OneToOne(IBaseModel model, IBaseModel relation, string relationName)
        {
            model.SetProperty(relationName, relation);
            return relation;
        }

        /// <summary>
        /// Sets the one-to-many relation
        /// </summary>
        /// <param name="model"></param>
        /// <param name="relation"></param>
        /// <param name="relationName"></param>
        private IBaseModel OneToMany(IBaseModel model, IBaseModel relation, string relationName)
        {
            // check if relation object already exists
            var relationList = (IList) model.GetPropertyValue(relationName);

            // return if relation with the same key already exists
            if (relationList != null && relationList.Cast<IBaseModel>()
                .Any(x => x.GetPrimaryKey().Equals(relation.GetPrimaryKey())))
                return relationList.Cast<IBaseModel>()
                    .First(x => x.GetPrimaryKey().Equals(relation.GetPrimaryKey()));

            // if the list is initialized
            if (relationList != null)
            {
                // if the key does not exists, add the relation model to the list
                if (!relationList.Cast<IBaseModel>().Any(x => x.GetPrimaryKey().Equals(relation.GetPrimaryKey())))
                    relationList.Add(relation);
            }

            // create new list and add this relation as property to object
            else
                CreateRelationList(relation, model, relationName);

            return relation;
        }

        /// <summary>
        /// Creates and adds the relation model to a list
        /// </summary>
        /// <param name="relation"></param>
        /// <param name="model"></param>
        /// <param name="relationName"></param>
        private void CreateRelationList(IBaseModel relation, IBaseModel model, string relationName)
        {
            var listType = typeof(List<>).MakeGenericType(relation.GetType());
            var creator = GetCreator(listType);
            var newList = (IList) creator();
            newList.Add(relation);
            model.SetProperty(relationName, newList);
        }


        /// <summary>
        /// Creates an object creator through IL emitter
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static ObjectActivator CreateCtor(Type type)
        {
            var emptyConstructor = type.GetConstructor(Type.EmptyTypes);
            var dynamicMethod = new DynamicMethod("CreateInstance", type, Type.EmptyTypes, true);
            var ilGenerator = dynamicMethod.GetILGenerator();
            ilGenerator.Emit(OpCodes.Nop);
            ilGenerator.Emit(OpCodes.Newobj, emptyConstructor);
            ilGenerator.Emit(OpCodes.Ret);
            return (ObjectActivator) dynamicMethod.CreateDelegate(typeof(ObjectActivator));
        }

        /// <summary>
        /// Get properties of a type from cache
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private IEnumerable<PropertyInfo> GetPropertyInfos(Type type)
        {
            if (_propertyCache.ContainsKey(type))
                return _propertyCache[type];

            const BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance;
            var propertyInfos = type.GetProperties(bindingAttr);
            _propertyCache[type] = propertyInfos;
            return propertyInfos;
        }


        /// <summary>
        /// Get a creator object from cache
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private ObjectActivator GetCreator(Type t)
        {
            if (_creatorCache.ContainsKey(t))
                return _creatorCache[t];

            var creator = CreateCtor(t);
            _creatorCache[t] = creator;
            return creator;
        }

        /// <summary>
        /// Empty the dictionaries
        /// </summary>
        private void ClearCache()
        {
            _creatorCache.Clear();
            _propertyCache.Clear();
        }

        /// <summary>
        /// Copies a model with its original properties
        /// </summary>
        /// <param name="model"></param>
        private static void CopyModel(IBaseModel model)
        {
            model.SetOriginal(model.Clone());
        }
    }
}