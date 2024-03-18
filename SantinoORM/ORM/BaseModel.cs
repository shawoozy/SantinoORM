using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using SantinoORM.ORM.Interfaces;

namespace SantinoORM.ORM
{
    public abstract class BaseModel : Builder.Builder, IBaseModel, ICloneable
    {
        /// <summary>
        /// The primary key
        /// </summary>
        [JsonIgnore]
        public virtual int Id { get; set; }

        private object _original;

        /// <summary>
        /// Returns the primary key
        /// </summary>
        /// <returns></returns>
        public virtual object GetPrimaryKey()
        {
            return Id;
        }

        /// <summary>
        /// Set property through Reflection
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        public void SetProperty(string property, object value)
        {
            var type = GetType();
            var prop = type.GetProperty(property);

            if (prop != null)
                prop.SetValue(this, value);
        }

        /// <summary>
        /// Get the property type through Reflection
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public PropertyInfo GetPropertyType(string property)
        {
            var type = GetType();
            return type.GetProperty(property);
        }


        /// <summary>
        /// Get property value through Reflection
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public object GetPropertyValue(string property)
        {
            var prop = GetPropertyType(property);
            return prop != null ? prop.GetValue(this) : null;
        }

        public void SetOriginal(object original)
        {
            _original = original;
        }

        public IBaseModel GetOriginal()
        {
            return _original as IBaseModel;
        }

        /// <summary>
        /// Returns a list of dirty properties.
        /// </summary>
        /// <returns></returns>
        public IList<PropertyInfo> DirtyProperties()
        {
            if (_original == null)
                return new List<PropertyInfo>();
            
            var originalProperties = _original.GetType().GetProperties();
            return GetType()
                .GetProperties()
                .Where(property => property.GetValue(this)
                    .Equals(originalProperties.First(x => x.Name.Equals(property.Name)).GetValue(_original)))
                .ToList();
        }

        public object Clone()
        {
            return MemberwiseClone();
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
        public void LazyLoad<T>(string column, string joinColumn) where T : BaseModel, new()
        {
            if (!typeof(T).IsSubclassOf(typeof(BaseModel)))
                throw new Exception("Relation class must extend BaseModel to call lazy load the relation");

            var models = new List<BaseModel> { this };
            ORM.LazyLoad.LoadModels<BaseModel, T>(models, column, joinColumn);
        }

        public BaseModel() { }
    }
}