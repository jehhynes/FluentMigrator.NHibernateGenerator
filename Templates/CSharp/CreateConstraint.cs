using System;
using System.IO;
using System.Linq;
using FluentMigrator.Model;

namespace FluentMigrator.NHibernateGenerator.Templates.CSharp
{
    public class CreateConstraintExpressionTemplate : ExpressionTemplate<Expressions.CreateConstraintExpression>
    {
        public override void WriteTo(TextWriter tw)
        {
            tw.Write($@"
            Create.{(Expression.Constraint.IsPrimaryKeyConstraint ? "PrimaryKey" : "UniqueConstraint")}(");

            if (!string.IsNullOrEmpty(Expression.Constraint.ConstraintName))
                tw.Write($@"""{Expression.Constraint.ConstraintName}""");

            tw.Write($@").OnTable(""{Expression.Constraint.TableName}"")");

            if (!string.IsNullOrEmpty(Expression.Constraint.SchemaName))
                tw.Write($@".WithSchema(""{Expression.Constraint.SchemaName}"")");

            tw.Write($@".Columns({string.Join(", ", Expression.Constraint.Columns.Select(c => $@"""{c}"""))})");
        }
    }
}