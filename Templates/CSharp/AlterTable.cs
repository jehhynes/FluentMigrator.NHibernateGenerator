using System;
using System.IO;
using FluentMigrator.Model;

namespace FluentMigrator.NHibernateGenerator.Templates.CSharp
{
    internal class AlterTableExpressionTemplate : ExpressionTemplate<Expressions.AlterTableExpression>
    {
        public override void WriteTo(TextWriter tw)
        {
            throw new NotImplementedException("FluentMigrator.Expressions.AlterTableExpression");
        }
    }
}

