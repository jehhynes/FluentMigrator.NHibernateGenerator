using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FluentMigrator.NHibernateGenerator
{
    public static class NugetTooling
    {
        public static object Generate(string targetPath, string assemblyName, string migrationName)
        {
            string targetFileName = Path.GetFileName(targetPath);
            string targetDir = Path.GetDirectoryName(targetPath);
            
            AppDomainSetup info = new AppDomainSetup
            {
                ShadowCopyFiles = "true",
                ApplicationBase = targetDir,
                ConfigurationFile = targetFileName + ".config"
            };

            var appDomain = AppDomain.CreateDomain("NHMigrations" + Convert.ToBase64String(Guid.NewGuid().ToByteArray()), null, info);

            var proxy = (MigrationProxy)appDomain.CreateInstanceAndUnwrap(typeof(MigrationProxy).Assembly.FullName, typeof(MigrationProxy).FullName);

            var migration = proxy.Generate(targetDir, assemblyName, migrationName);

            AppDomain.Unload(appDomain);

            return migration;
        }

        public class MigrationProxy : MarshalByRefObject
        {
            public GeneratedMigration Generate(string projectRoot, string migrationAssemblyName, string migrationName)
            {
                var assembly = Assembly.Load(migrationAssemblyName);
           
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
