using Microsoft.Extensions.DependencyInjection;
using SantinoORM.Builder;
using SantinoORM.Builder.Interfaces;
using SantinoORM.Configuration;
using SantinoORM.ORM;
using SantinoORM.ORM.Interfaces;

namespace SantinoORM
{
    public static class ServiceRegistration
    {
        public static void AddSantinoOrm(this IServiceCollection services, string mysqlConnectionString)
        {         
            MysqlConnectionString.SetConnectionString(mysqlConnectionString);
            services.AddTransient<IMapper>(provider => new Mapper(mysqlConnectionString));
            services.AddTransient<IExecute>(provider => new Execute(mysqlConnectionString));
        }
    }
}