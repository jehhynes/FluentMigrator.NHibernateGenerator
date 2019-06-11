using System;
using System.IO;
using FluentMigrator.Model;

namespace FluentMigrator.NHibernateGenerator.Templates.CSharp
{
    internal class DeleteColumnExpressionTemplate : ExpressionTemplate<Expressions.DeleteColumnExpression>
    {
        public override void WriteTo(TextWriter tw)
        {
            foreach (var cn in Expression.ColumnNames)
            {
                tw.Write($@"
            Delete.Column(""{cn}"").FromTable(""{Expression.TableName}"")");

                if (!string.IsNullOrEmpty(Expression.SchemaName))
                    tw.Write($@".InSchema(""{Expression.SchemaName}"")");
            }
        }
    }
}