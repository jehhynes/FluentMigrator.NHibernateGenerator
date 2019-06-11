using System;
using System.IO;
using FluentMigrator.Model;

namespace FluentMigrator.NHibernateGenerator.Templates.CSharp
{
    internal class DeleteForeignKeyExpressionTemplate : ExpressionTemplate<Expressions.DeleteForeignKeyExpression>
    {
        public override void WriteTo(TextWriter tw)
        {
            tw.Write($@"
            Delete.ForeignKey(");

            if (!string.IsNullOrEmpty(Expression.ForeignKey.Name))
                tw.Write($@"""{Expression.ForeignKey.Name}""");

            tw.Write($@").OnTable(""{Expression.ForeignKey.ForeignTable}"")");

            if (!string.IsNullOrEmpty(Expression.ForeignKey.ForeignTableSchema))
                tw.Write($@".InSchema(""{Expression.ForeignKey.ForeignTableSchema}"")");
        }
    }
}