using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentMigrator.Expressions;

namespace FluentMigrator.NHibernateGenerator.Templates.CSharp
{
    internal class MigrationDesignerFile : ITemplate
    {
        public virtual string Namespace { get; set; }
        public virtual string Name { get; set; }
        public virtual long Version { get; set; }
        public virtual string SerializedConfiguration { get; set; }
        public virtual string AdditionalUsings { get; set; }

        public void WriteTo(TextWriter tw)
        {
            tw.Write(
$@"using System;
using FluentMigrator;
{(AdditionalUsings == null ? null : (AdditionalUsings + Environment.NewLine))}
namespace {Namespace}
{{
    public partial class {Name}
    {{
        public const string ConfigurationData = ""{SerializedConfiguration}"";
    }}
}}");
        }
    }
}