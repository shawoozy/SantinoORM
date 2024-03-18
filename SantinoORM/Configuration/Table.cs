using System;

namespace SantinoORM.Configuration
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class Table : Attribute
    {
        private readonly string _table;

        public Table(string table)
        {
            _table = table;
        }

        public virtual string TableName => _table;
    }
}