using FluentMigrator.Expressions;

namespace FluentMigrator.NHibernateGenerator.Templates
{
    public interface ITemplateFromExpressionFactory
    {
        ITemplate GetTemplate(MigrationExpressionBase expr);
    }
}