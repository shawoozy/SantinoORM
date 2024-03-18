using System.Reflection;

namespace SantinoORM.ORM.Interfaces
{
    public interface IBaseModel
    {
        object GetPrimaryKey();

        void SetProperty(string property, object value);
        
        PropertyInfo GetPropertyType(string property);

        object GetPropertyValue(string property);

        void SetOriginal(object original);
        
        IBaseModel GetOriginal();

        object Clone();

    }
}