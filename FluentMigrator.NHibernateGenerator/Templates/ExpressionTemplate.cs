using System.IO;

namespace FluentMigrator.NHibernateGenerator.Templates
{
    public abstract class ExpressionTemplate<T> : ITemplate
    {
        public virtual T Expression { get; set; }
        public abstract void WriteTo(TextWriter tw);
    }
}