using System;
using System.IO;
using FluentMigrator.Model;

namespace FluentMigrator.NHibernateGenerator.Templates.CSharp
{
    public class AlterColumnExpressionTemplate
        : ExpressionTemplate<Expressions.AlterColumnExpression>
    {
        public override void WriteTo(TextWriter tw)
        {
            if (Expression is ExtendedAlterColumnExpression e && e.Previous != null
                && e.Previous.IsNullable == true && Expression.Column.IsNullable == false
                && e.Previous.DefaultValue == null && Expression.Column.DefaultValue != null)
            {
                //Update the column with the default value
                tw.Write($@"
            Update.Table(""{Expression.TableName}"").Set(new {{ {Expression.Column.Name} = {ColumnDefinitionTemplate.FormatDefaultValue(Expression.Column)} }}).Where(new {{ {Expression.Column.Name} = (object)null }});");
            }

            tw.Write($@"
            Alter.Column(""{Expression.Column.Name}"").OnTable(""{Expression.TableName}"")");

            if (!string.IsNullOrEmpty(Expression.SchemaName))
                tw.Write($@".InSchema(""{Expression.SchemaName}"")");

            new ColumnDefinitionTemplate { Expression = Expression.Column }.WriteTo(tw);
        }
    }
}

