using System;
using System.IO;
using FluentMigrator.Model;

namespace FluentMigrator.NHibernateGenerator.Templates.CSharp
{
    public class CreateIndexExpressionTemplate : ExpressionTemplate<Expressions.CreateIndexExpression>
    {
        public override void WriteTo(TextWriter tw)
        {
            tw.Write($@"
            Create.Index(");

            if (!string.IsNullOrEmpty(Expression.Index.Name))
                tw.Write($@"""{Expression.Index.Name}""");

            tw.Write($@").OnTable(""{Expression.Index.TableName}"")");

            if (!string.IsNullOrEmpty(Expression.Index.SchemaName))
                tw.Write($@".InSchema(""{Expression.Index.SchemaName}"")");

            tw.Write($@".WithOptions().{(!Expression.Index.IsClustered ? "Non" : null)}Clustered()");

            if (Expression.Index.IsUnique)
                tw.Write(".WithOptions().Unique()");

            tw.Write($@"
                ");

            foreach (var c in Expression.Index.Columns)
                tw.Write($@".OnColumn(""{c.Name}"").{c.Direction.ToString()}()");
        }
    }
}