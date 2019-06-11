using System;
using System.IO;
using FluentMigrator.Model;

namespace FluentMigrator.NHibernateGenerator.Templates.CSharp
{
    internal class CreateTableExpressionTemplate : ExpressionTemplate<Expressions.CreateTableExpression>
    {
        public override void WriteTo(TextWriter tw)
        {
            tw.Write($@"
            Create.Table(""{Expression.TableName}"")");

            if (!string.IsNullOrEmpty(Expression.SchemaName))
                tw.Write($@".InSchema(""{Expression.SchemaName}"")");

            foreach (var c in Expression.Columns)
            {
                tw.Write($@"
                .WithColumn(""{c.Name}"")");

                new ColumnDefinitionTemplate { Expression = c }.WriteTo(tw);
            }

            if (!string.IsNullOrEmpty(Expression.TableDescription))
            {
                tw.Write($@"
                .WithDescription(""{Expression.TableDescription}"")");
            }
        }
    }
}

