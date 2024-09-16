using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentMigrator.Expressions;

namespace FluentMigrator.NHibernateGenerator.Templates.CSharp
{
    public class MigrationCodeFile : ITemplate
    {
        public virtual string MigrationBaseClassName { get; set; }
        public virtual string Namespace { get; set; }
        public virtual string Name { get; set; }
        public virtual long Version { get; set; }
        public virtual IEnumerable<DifferentialExpression> Expressions { get; set; }
        public virtual ITemplateFromExpressionFactory TemplateFactory { get; set; }
        public virtual string AdditionalUsings { get; set; }

        public void WriteTo(TextWriter tw)
        {
            var upExpressions = Expressions.Select(x => x.Up).ToList();
            var reverseExpressions = Expressions.Where(x => x.Down != null).Select(x => x.Down).ToList();
            reverseExpressions.Reverse();

            tw.Write(
$@"using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Linq.Expressions;
using FluentMigrator;
{(AdditionalUsings == null ? null : (AdditionalUsings + Environment.NewLine))}
namespace {Namespace}
{{
    [Migration({Version})]
    public partial class {Name} : {MigrationBaseClassName}
    {{
        public override void Up()
        {{");
            bool isFirst = true;
            foreach (var templ in upExpressions.Select(e => TemplateFactory.GetTemplate(e)))
            {
                if (!isFirst) tw.WriteLine();
                templ.WriteTo(tw);
                tw.Write(";");
                isFirst = false;
            }

            tw.Write($@"
        }}

        public override void Down()
        {{");
            isFirst = true;
            foreach (var templ in reverseExpressions.Select(e => TemplateFactory.GetTemplate(e)))
            {
                if (!isFirst) tw.WriteLine();
                templ.WriteTo(tw);
                tw.Write(";");
                isFirst = false;
            }

            tw.Write($@"
        }}
    }}
}}");
        }
    }
}