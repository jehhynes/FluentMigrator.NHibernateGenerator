using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using FluentMigrator.Expressions;
using FluentMigrator.NHibernateGenerator.Templates;
using Newtonsoft.Json;
using NHibernate.Cfg;
using NHibernate.Dialect;

namespace FluentMigrator.NHibernateGenerator
{
    public abstract class MigrationConfigurationBase
    {
        protected string MigrationNamespace { get; set; }
        protected Assembly MigrationAssembly { get; set; }

        protected virtual string MigrationsDirectory { get { return "Migrations"; } }

        public virtual string GetFileNamePrefix(long version)
        {
            return version.ToString().PadLeft(8, '0') + "_";
        }
        
        protected virtual bool FilterExpressions(List<MigrationExpressionBase> @from, List<MigrationExpressionBase> @to,
            MigrationExpressionBase expression)
        {
            return true;
        }

        public virtual GeneratedMigration Generate(string name)
        {
            var from = GetFromExpressions();
            var to = GetToExpressions();

            var diff = new DifferentialMigration(from, to)
                .Where(exp => FilterExpressions(from, to, exp.Up))
                .ToList();

            var tf = new CSharpTemplateFromExpressionFactory();
            var serializedConfiguration = SerializeConfiguration(to);
            var version = GenerateNextVersionNumber();

            var code = new Templates.CSharp.MigrationCodeFile
            {
                Expressions = diff,
                Name = name,
                Namespace = MigrationNamespace,
                TemplateFactory = tf,
                Version = version
            };

            var result = new GeneratedMigration()
            {
                Name = name,
                Version = version,
                MigrationsDirectory = MigrationsDirectory,
                FileNamePrefix = GetFileNamePrefix(version)
            };

            using (var sw = new StringWriter())
            {
                code.WriteTo(sw);
                sw.Flush();
                result.Code = sw.GetStringBuilder().ToString();
            }

            var designer = new Templates.CSharp.MigrationDesignerFile
            {
                Name = name,
                Namespace = MigrationNamespace,
                SerializedConfiguration = serializedConfiguration,
                Version = version
            };

            using (var sw = new StringWriter())
            {
                designer.WriteTo(sw);
                sw.Flush();
                result.Designer = sw.GetStringBuilder().ToString();
            }

            return result;
        }

        protected virtual long GenerateNextVersionNumber()
        {
            var lastMigration = MigrationAssembly.ExportedTypes.Where(t => t.BaseType == typeof(Migration))
               .Where(s => HasConfigurationData(s))
               .Select(t => GetVersion(t))
               .OrderBy(x => x)
               .LastOrDefault();
            return lastMigration + 1;
        }

        protected virtual List<MigrationExpressionBase> GetToExpressions()
        {
            var configuration = GetConfiguration();
            configuration.BuildMappings();
            Dialect dialect = Dialect.GetDialect(configuration.Properties);
            return new ExpressionExporter(configuration, dialect).ToList();
        }

        protected abstract Configuration GetConfiguration();

        protected virtual List<MigrationExpressionBase> GetFromExpressions()
        {
            return GetFromExpressionList(MigrationAssembly);
        }

        public static string SerializeConfiguration(List<MigrationExpressionBase> expressions)
        {
            var ms = new MemoryStream();
            using (var gs = new GZipStream(ms, CompressionLevel.Optimal, true))
            using (var sw = new StreamWriter(gs))
            {
                sw.Write(JsonConvert.SerializeObject(expressions, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All,
                    Formatting = Formatting.Indented
                }));
                sw.Flush();
            }
            return Convert.ToBase64String(ms.ToArray());
        }

        public static List<MigrationExpressionBase> DeserializeConfiguration(string source)
        {
            var bytes = Convert.FromBase64String(source);
            var ms = new MemoryStream(bytes);
            using (var gs = new GZipStream(ms, CompressionMode.Decompress))
            using (var sr = new StreamReader(gs))
            {
                var src = sr.ReadToEnd();
                return JsonConvert.DeserializeObject<List<MigrationExpressionBase>>(src, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All,
                    Formatting = Formatting.Indented
                });
            }
        }

        public static List<MigrationExpressionBase> GetFromExpressionList(Assembly migrationsAssembly)
        {
            var lastMigration = migrationsAssembly.ExportedTypes.Where(t => t.BaseType == typeof(Migration))
                .Where(s => HasConfigurationData(s))
                .OrderBy(t => GetVersion(t))
                .LastOrDefault();

            if (lastMigration == null)
            {
                return new List<MigrationExpressionBase>();
            }

            var f = lastMigration.GetField("ConfigurationData", BindingFlags.Public | BindingFlags.Static);
            var data = (string)f.GetValue(null);

            return DeserializeConfiguration(data);
        }

        private static long GetVersion(Type type)
        {
            return type.GetCustomAttributes(false)
                .OfType<MigrationAttribute>()
                .Select(x => x.Version)
                .FirstOrDefault();
        }

        private static bool HasConfigurationData(Type type)
        {
            return type.GetField("ConfigurationData", BindingFlags.Public | BindingFlags.Static) != null;
        }
    }
}