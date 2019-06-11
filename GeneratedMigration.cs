using System;

namespace FluentMigrator.NHibernateGenerator
{
    [Serializable]
    public class GeneratedMigration
    {
        public long Version { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Designer { get; set; }
        public string MigrationsDirectory { get; set; }
        public string FileNamePrefix { get; set; }
    }
}