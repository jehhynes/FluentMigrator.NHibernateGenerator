using FluentMigrator.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentMigrator.NHibernateGenerator
{
    public class DifferentialExpression
    {
        public DifferentialExpression(IMigrationExpression upReversible)
        {
            Up = upReversible;
            Down = upReversible.Reverse();
        }
        public DifferentialExpression(IMigrationExpression up, IMigrationExpression down)
        {
            Up = up;
            Down = down;
        }

        public IMigrationExpression Up { get; set; }
        public IMigrationExpression Down { get; set; }
    }
}
