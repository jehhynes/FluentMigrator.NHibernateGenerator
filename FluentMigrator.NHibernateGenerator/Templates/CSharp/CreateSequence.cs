using System;
using System.IO;
using FluentMigrator.Model;

namespace FluentMigrator.NHibernateGenerator.Templates.CSharp
{
    public class CreateSequenceExpressionTemplate : ExpressionTemplate<Expressions.CreateSequenceExpression>
    {
        public override void WriteTo(TextWriter tw)
        {
            tw.Write($@"
            Create.Sequence(""{Expression.Sequence.Name}"")");

            if (!string.IsNullOrEmpty(Expression.Sequence.SchemaName))
                tw.Write($@".InSchema(""{Expression.Sequence.SchemaName}"")");
        
            if (Expression.Sequence.Increment.HasValue)
                tw.Write($@".Increment({Expression.Sequence.Increment.ToString()})");

            if (Expression.Sequence.MinValue.HasValue)
                tw.Write($@".MinValue({Expression.Sequence.MinValue.ToString()})");

            if (Expression.Sequence.MaxValue.HasValue)
                tw.Write($@".MaxValue({Expression.Sequence.MaxValue.ToString()})");

            if (Expression.Sequence.StartWith.HasValue)
                tw.Write($@".StartWith({Expression.Sequence.StartWith.ToString()})");

            if (Expression.Sequence.Cache.HasValue)
                tw.Write($@".Cache({Expression.Sequence.Cache.ToString()})");

            if (Expression.Sequence.Cycle)
                tw.Write(".Cycle()");
        }
    }
}