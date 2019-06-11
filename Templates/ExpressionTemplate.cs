using System.IO;

namespace FluentMigrator.NHibernateGenerator.Templates
{
    internal abstract class ExpressionTemplate<T> : ITemplate
    {
        public virtual T Expression { get; set; }
        public abstract void WriteTo(TextWriter tw);
    }
}