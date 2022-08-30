using System;
using System.IO;
using System.Linq;
using FluentMigrator.Model;

namespace FluentMigrator.NHibernateGenerator.Templates.CSharp
{
    public class CreateForeignKeyExpressionTemplate : ExpressionTemplate<Expressions.CreateForeignKeyExpression>
    {
        public override void WriteTo(TextWriter tw)
        {
            tw.Write($@"
            Create.ForeignKey(");
            if (!string.IsNullOrEmpty(Expression.ForeignKey.Name))
                tw.Write($@"""{Expression.ForeignKey.Name}""");

            tw.Write($@").FromTable(""{Expression.ForeignKey.ForeignTable}"")");

            if (!string.IsNullOrEmpty(Expression.ForeignKey.ForeignTableSchema))
                tw.Write($@".InSchema(""{Expression.ForeignKey.ForeignTableSchema}"")");

            tw.Write($@".ForeignColumns({string.Join(", ", Expression.ForeignKey.ForeignColumns.Select(c => $@"""{c}"""))})");

            tw.Write($@"
                .ToTable(""{Expression.ForeignKey.PrimaryTable}"")");

            if (!string.IsNullOrEmpty(Expression.ForeignKey.PrimaryTableSchema))
                tw.Write($@".InSchema(""{Expression.ForeignKey.PrimaryTableSchema}"")");

            tw.Write($@".PrimaryColumns({string.Join(",", Expression.ForeignKey.PrimaryColumns.Select(c => $@"""{c}"""))})");

            if (Expression.ForeignKey.OnDelete != System.Data.Rule.None)
                tw.Write($@".OnDelete(Rule.{Expression.ForeignKey.OnDelete.ToString()})");
            if (Expression.ForeignKey.OnDelete != System.Data.Rule.None)
                tw.Write($@".OnUpdate(Rule.{Expression.ForeignKey.OnUpdate.ToString()})");
        }
    }
}

