using System;
using System.Collections.Generic;
using System.IO;
using FluentMigrator.Expressions;
using FluentMigrator.NHibernateGenerator.Templates.CSharp;

namespace FluentMigrator.NHibernateGenerator.Templates
{
    internal class CSharpTemplateFromExpressionFactory : ITemplateFromExpressionFactory
    {
        private Dictionary<Type, Func<MigrationExpressionBase, ITemplate>> _templateLookup = InitTemplates();

        private static Dictionary<Type, Func<MigrationExpressionBase, ITemplate>> InitTemplates()
        {
            return new Dictionary<Type, Func<MigrationExpressionBase, ITemplate>>
            {
                {typeof(AlterColumnExpression), e => new AlterColumnExpressionTemplate { Expression = (AlterColumnExpression)e} },
                {typeof(AlterDefaultConstraintExpression), e => new AlterDefaultConstraint { Expression = (AlterDefaultConstraintExpression)e} },
                {typeof(AlterSchemaExpression), e => new AlterSchemaExpressionTemplate { Expression = (AlterSchemaExpression)e} },
                {typeof(AlterTableExpression), e => new AlterTableExpressionTemplate { Expression = (AlterTableExpression)e} },
                {typeof(CreateColumnExpression), e => new CreateColumnExpressionTemplate { Expression = (CreateColumnExpression)e} },
                {typeof(CreateConstraintExpression), e => new CreateConstraintExpressionTemplate { Expression = (CreateConstraintExpression)e} },
                {typeof(CreateForeignKeyExpression), e => new CreateForeignKeyExpressionTemplate { Expression = (CreateForeignKeyExpression)e} },
                {typeof(CreateIndexExpression), e => new CreateIndexExpressionTemplate { Expression = (CreateIndexExpression)e} },
                {typeof(CreateSchemaExpression), e => new CreateSchemaExpressionTemplate { Expression = (CreateSchemaExpression)e} },
                {typeof(CreateSequenceExpression), e => new CreateSequenceExpressionTemplate { Expression = (CreateSequenceExpression)e} },
                {typeof(CreateTableExpression), e => new CreateTableExpressionTemplate { Expression = (CreateTableExpression)e} },
                {typeof(DeleteColumnExpression), e => new DeleteColumnExpressionTemplate { Expression = (DeleteColumnExpression)e} },
                {typeof(DeleteConstraintExpression), e => new DeleteConstraintExpressionTemplate { Expression = (DeleteConstraintExpression)e} },
                {typeof(DeleteDefaultConstraintExpression), e => new DeleteDefaultConstraintExpressionTemplate { Expression = (DeleteDefaultConstraintExpression)e} },
                {typeof(DeleteForeignKeyExpression), e => new DeleteForeignKeyExpressionTemplate { Expression = (DeleteForeignKeyExpression)e} },
                {typeof(DeleteIndexExpression), e => new DeleteIndexExpressionTemplate { Expression = (DeleteIndexExpression)e} },
                {typeof(DeleteSchemaExpression), e => new DeleteSchemaExpressionTemplate { Expression = (DeleteSchemaExpression)e} },
                {typeof(DeleteSequenceExpression), e => new DeleteSequenceExpressionTemplate { Expression = (DeleteSequenceExpression)e} },
                {typeof(DeleteTableExpression), e => new DeleteTableExpressionTemplate { Expression = (DeleteTableExpression)e} },
                {typeof(ExecuteSqlStatementExpression), e => new ExecuteSqlStatementExpressionTemplate { Expression = (ExecuteSqlStatementExpression)e} },
            };
        }

        public ITemplate GetTemplate(MigrationExpressionBase expr)
        {
            var expressionType = expr.GetType();
            if (_templateLookup.ContainsKey(expressionType))
            {
                return _templateLookup[expressionType](expr);
            }
            return new BadExpressionTemplate(expressionType);
        }

        private class BadExpressionTemplate : ITemplate
        {
            private readonly Type _t;

            public BadExpressionTemplate(Type t)
            {
                _t = t;
            }

            public void WriteTo(TextWriter tw)
            {
                tw.Write("throw new NotImplementedException(\"No template implemented for {0}\");", _t.FullName);
            }
        }
    }
}
