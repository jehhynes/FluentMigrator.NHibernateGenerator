using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

#if NETCOREAPP
using System.Runtime.Loader;
#endif

namespace FluentMigrator.NHibernateGenerator
{
    public static class NugetTooling
    {
        public static object Generate(string assemblyPath, string migrationName)
        {
            var appDomain = AppDomain.CurrentDomain;

            var proxy = (MigrationProxy)appDomain.CreateInstanceAndUnwrap(typeof(MigrationProxy).Assembly.FullName, typeof(MigrationProxy).FullName);

            var migration = proxy.Generate(assemblyPath, migrationName);

            return migration;
        }

        public class MigrationProxy : MarshalByRefObject
        {
            public GeneratedMigration Generate(string assemblyPath, string migrationName)
            {
                var assembly = Assembly.LoadFrom(assemblyPath);

                var migrationConfigTypes = assembly.GetTypes().Where(x => x.IsClass && !x.IsAbstract
                    && typeof(MigrationConfigurationBase).IsAssignableFrom(x)).ToList();

                if (migrationConfigTypes.Count() > 1)
                    throw new Exception("Multiple migrations configurations exist in selected project");

                if (migrationConfigTypes.Count() == 0)
                    throw new Exception("Migrations configurations not found in selected project");

                var migrationConfig = Activator.CreateInstance(migrationConfigTypes[0]) as MigrationConfigurationBase;

                var migration = migrationConfig.Generate(migrationName);

                return migration;
            }
        }
    }
}
