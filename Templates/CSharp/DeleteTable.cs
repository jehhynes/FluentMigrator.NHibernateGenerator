using System;
using System.IO;
using FluentMigrator.Model;

namespace FluentMigrator.NHibernateGenerator.Templates.CSharp
{
    internal class DeleteTableExpressionTemplate : ExpressionTemplate<Expressions.DeleteTableExpression>
    {
        public override void WriteTo(TextWriter tw)
        {
            tw.Write($@"
            Delete.Table(""{Expression.TableName}"")");

            if (!string.IsNullOrEmpty(Expression.SchemaName))
                tw.Write($@".InSchema(""{Expression.SchemaName}"")");
        }
    }
}