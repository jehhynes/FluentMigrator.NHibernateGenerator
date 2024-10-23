using FluentMigrator.Expressions;
using FluentMigrator.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentMigrator.NHibernateGenerator
{
    public class RenameIndexExpression : ExecuteSqlStatementExpression
    {
        public RenameIndexExpression(string schema, string table, string indexName, string newName)
        {
            SqlStatement = $"EXEC sp_rename '{(schema == null ? null : (schema + "."))}{table}.{indexName}', '{newName}', 'INDEX'";
        }
    }
}
