using System;
using System.IO;
using FluentMigrator.Model;

namespace FluentMigrator.NHibernateGenerator.Templates.CSharp
{
    public class RenameIndexExpressionTemplate : ExpressionTemplate<RenameIndexExpression>
    {
        public override void WriteTo(TextWriter tw)
        {
            tw.Write($@"
            Execute.Sql(""{Expression.SqlStatement}"")");
        }
    }
}