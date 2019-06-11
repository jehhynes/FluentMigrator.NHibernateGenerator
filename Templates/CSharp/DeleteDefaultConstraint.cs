using System;
using System.IO;
using FluentMigrator.Model;

namespace FluentMigrator.NHibernateGenerator.Templates.CSharp
{
    internal class DeleteDefaultConstraintExpressionTemplate : ExpressionTemplate<Expressions.DeleteDefaultConstraintExpression>
    {
        public override void WriteTo(TextWriter tw)
        {
            tw.Write($@"
            Delete.DefaultConstraint().OnTable(""{Expression.TableName}"")");

            if (!string.IsNullOrEmpty(Expression.SchemaName))
                tw.Write($@".InSchema(""{Expression.SchemaName}"")");

            tw.Write($@".OnColumn(""{Expression.ColumnName}"")");
        }
    }
}