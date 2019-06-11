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
        public DifferentialExpression(MigrationExpressionBase upReversible)
        {
            Up = upReversible;
            Down = (MigrationExpressionBase)upReversible.Reverse();
        }
        public DifferentialExpression(MigrationExpressionBase up, MigrationExpressionBase down)
        {
            Up = up;
            Down = down;
        }
        public DifferentialExpression(IMigrationExpression up, MigrationExpressionBase down) : this((MigrationExpressionBase)up, down) { }

        public MigrationExpressionBase Up { get; set; }
        public MigrationExpressionBase Down { get; set; }
    }
}
