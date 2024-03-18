using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using SantinoORM.Configuration;
using SantinoORM.ORM.Interfaces;

namespace SantinoORM.ORM
{
    public class LazyLoad : Builder.Builder
    {
        private delegate object ObjectActivator();

        public static void LoadModels<T, TRelation>(IEnumerable<T> models, string column, string joinColumn)
            where TRelation : BaseModel, new() where T : BaseModel
        {            
            // the property infos from the original model
            var properties = models.First().GetType().GetProperties();

            // the values of the column to join on
            var values = models.Select(model => properties.First(x => x.Name.Equals(column)).GetValue(model));

            if (values.All(x => x == null))
                return;
            
            // the relation models fetch from db
            var relations = new TRelation()
                .WhereIn(joinColumn, values.Where(x => x != null).ToList())
                .Get<TRelation>();    
            
            var propertyInfo = Array.Find(properties, (x => x.Name.Equals(typeof(TRelation).Name)));
            
            foreach (var relation in relations)
            {
                if (propertyInfo.PropertyType.IsGenericType)
                    OneToMany(relation, column, joinColumn, new List<T>(models));
                else
                    OneToOne(relation, column, joinColumn, new List<T>(models));
            }
        }

        private static void OneToOne<T>(IBaseModel relation, string column, string joinColumn, List<T> models) where T : BaseModel
        {
            var relationValue = relation.GetPropertyValue(joinColumn);
                foreach (var mo in models)
                {
                    var parentValue = mo.GetPropertyValue(column);
                        if (parentValue == null || !parentValue.Equals(relationValue))
                            continue;
                        mo.SetProperty(relation.GetType().Name, relation);
                        break;
                }
        }

        private static void OneToMany<T>(IBaseModel relation, string column, string joinColumn,
            List<T> models) where T : BaseModel
        {
          
            var relationName = relation.GetType().Name;
            var model = models.Find(x => x.GetPropertyValue(column).Equals(relation.GetPropertyValue(joinColumn)));
            if (model == null) return;

            var relationList = (IList) model.GetPropertyValue(relationName);

            if (relationList == null)
            {
                var listType = typeof(List<>).MakeGenericType(relation.GetType());
                var creator = CreateCtor(listType);
                var newList = (IList) creator();
                newList.Add(relation);
                model.SetProperty(relationName, newList);
            }
            else
                relationList.Add(relation);
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
    }
}