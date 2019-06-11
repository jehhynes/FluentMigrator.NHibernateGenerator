using System;
using System.IO;
using FluentMigrator.Model;

namespace FluentMigrator.NHibernateGenerator.Templates.CSharp
{
    internal class CreateSchemaExpressionTemplate : ExpressionTemplate<Expressions.CreateSchemaExpression>
    {
        public override void WriteTo(TextWriter tw)
        {
            tw.Write($@"
            Create.Schema(""{Expression.SchemaName}"")");
        }
    }
}
