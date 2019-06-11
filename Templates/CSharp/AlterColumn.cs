using System;
using System.IO;
using FluentMigrator.Model;

namespace FluentMigrator.NHibernateGenerator.Templates.CSharp
{
    internal class AlterColumnExpressionTemplate
        : ExpressionTemplate<Expressions.AlterColumnExpression>
    {
        public override void WriteTo(TextWriter tw)
        {
            tw.Write($@"
            Alter.Column(""{Expression.Column.Name}"").OnTable(""{Expression.TableName}"")");

            if (!string.IsNullOrEmpty(Expression.SchemaName))
                tw.Write($@".InSchema(""{Expression.SchemaName}"")");

            new ColumnDefinitionTemplate { Expression = Expression.Column }.WriteTo(tw);
        }
    }
}

