using System.IO;

namespace FluentMigrator.NHibernateGenerator.Templates
{
    public interface ITemplate
    {
        void WriteTo(TextWriter tw);
    }
}