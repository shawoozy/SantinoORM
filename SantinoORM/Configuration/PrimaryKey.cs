using System;

namespace SantinoORM.Configuration
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public sealed class PrimaryKey : Attribute
    {
    }
}