using System;
using System.IO;
using FluentMigrator.Model;

namespace FluentMigrator.NHibernateGenerator.Templates.CSharp
{
    public class AlterSchemaExpressionTemplate : ExpressionTemplate<Expressions.AlterSchemaExpression>
    {
        public override void WriteTo(TextWriter tw)
        {
            throw new NotImplementedException("FluentMigrator.Expressions.AlterSchemaExpression");
        }
    }
}

