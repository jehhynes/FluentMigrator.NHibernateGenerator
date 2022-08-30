using FluentMigrator.Infrastructure.Extensions;
using FluentMigrator.Model;
using FluentMigrator.SqlServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentMigrator.NHibernateGenerator
{
    public static class Extensions
    {
        public static ICollection<IndexIncludeDefinition> GetIncludes(this IndexDefinition index)
        {
            var includes = index.GetAdditionalFeature(SqlServerExtensions.IncludesList, () => new List<IndexIncludeDefinition>());
            return includes;
        }
    }
}
