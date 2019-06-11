using System;
using System.IO;
using FluentMigrator.Model;

namespace FluentMigrator.NHibernateGenerator.Templates.CSharp
{
    internal class DeleteSequenceExpressionTemplate : ExpressionTemplate<Expressions.DeleteSequenceExpression>
    {
        public override void WriteTo(TextWriter tw)
        {
            tw.Write($@"
            Delete.Sequence(""{Expression.SequenceName}"")");

            if (!string.IsNullOrEmpty(Expression.SchemaName))
                tw.Write($@".InSchema(""{Expression.SchemaName}"")");
        }
    }
}