using System;
using System.IO;
using FluentMigrator.Model;

namespace FluentMigrator.NHibernateGenerator.Templates.CSharp
{
    internal class DeleteSchemaExpressionTemplate : ExpressionTemplate<Expressions.DeleteSchemaExpression>
    {
        public override void WriteTo(TextWriter tw)
        {
            tw.Write($@"
            Delete.Schema(""{Expression.SchemaName}"")");
        }
    }
}