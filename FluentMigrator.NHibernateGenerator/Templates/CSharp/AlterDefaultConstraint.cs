using System;
using System.IO;

namespace FluentMigrator.NHibernateGenerator.Templates.CSharp
{
    public class AlterDefaultConstraint : ExpressionTemplate<Expressions.AlterDefaultConstraintExpression>
    {
        public override void WriteTo(TextWriter tw)
        {
            throw new NotImplementedException("FluentMigrator.Expressions.AlterDefaultConstraintExpression");
        }
    }
}