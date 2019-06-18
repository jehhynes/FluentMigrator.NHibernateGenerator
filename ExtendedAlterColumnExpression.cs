using FluentMigrator.Expressions;
using FluentMigrator.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentMigrator.NHibernateGenerator
{
    public class ExtendedAlterColumnExpression : AlterColumnExpression
    {
        public virtual ColumnDefinition Previous { get; set; }
    }
}
