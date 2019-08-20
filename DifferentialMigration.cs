using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using FluentMigrator.Builders.Alter;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure.Extensions;
using FluentMigrator.Model;
using FluentMigrator.SqlServer;
using NHibernate.Cfg;
using NHibernate.Dialect;

namespace FluentMigrator.NHibernateGenerator
{
    public class DifferentialMigration : IEnumerable<DifferentialExpression>
    {
        private readonly List<MigrationExpressionBase> _fromSchema;
        private readonly List<MigrationExpressionBase> _toSchema;

        public DifferentialMigration(List<MigrationExpressionBase> fromSchema, List<MigrationExpressionBase> toSchema)
        {
            _fromSchema = fromSchema;
            _toSchema = toSchema;
        }

        private static bool DefaultFilter(List<MigrationExpressionBase> a, List<MigrationExpressionBase> b,
            MigrationExpressionBase c)
        {
            return true;
        }

        public IEnumerator<DifferentialExpression> GetEnumerator()
        {
            if (_fromSchema == null || _fromSchema.Count == 0)
                return _toSchema.Select(x => new DifferentialExpression(x, null)).GetEnumerator();

            return
                GetRemovedIndexes()
                .Concat(GetRemovedPrimaryKeys())
                .Concat(GetRemovedForeignKeys())
                .Concat(GetRemovedTables())
                .Concat(GetRemovedSequences())
                .Concat(GetRemovedSchemas())

                .Concat(GetUpdatedTables())

                .Concat(GetNewSchemas())
                .Concat(GetNewSequences())
                .Concat(GetNewTables())
                .Concat(GetNewPrimaryKeys())
                .Concat(GetNewForeignKeys())
                .Concat(GetNewIndexes())
                .Concat(GetAuxObjects())
                .GetEnumerator();
        }

        private IEnumerable<DifferentialExpression> GetAuxObjects()
        {
            return _fromSchema.OfType<ExecuteSqlStatementExpression>()
                .Concat(_toSchema.OfType<ExecuteSqlStatementExpression>())
                .Select(x => new DifferentialExpression(x, null));
        }

        private IEnumerable<DifferentialExpression> GetNewSchemas()
        {
            var fromSchema = _fromSchema.OfType<CreateSchemaExpression>().ToList();
            var toSchema = _toSchema.OfType<CreateSchemaExpression>().ToList();
            return toSchema.Where(ts => !fromSchema.Any(fs => fs.SchemaName == ts.SchemaName))
                .Select(x => new DifferentialExpression(x));
        }

        private IEnumerable<DifferentialExpression> GetRemovedIndexes()
        {
            var fromIxs = _fromSchema.OfType<CreateIndexExpression>().ToList();

            var toIxs = _toSchema.OfType<CreateIndexExpression>().ToList();

            return fromIxs.Where(f => !toIxs.Any(t => AreSameIndexName(f.Index, t.Index)))
                .Select(f => new DifferentialExpression(f.Reverse(), f));
        }

        private IEnumerable<DifferentialExpression> GetNewTables()
        {
            var fromTables = _fromSchema.OfType<CreateTableExpression>().ToList();
            var toTables = _toSchema.OfType<CreateTableExpression>().ToList();
            var tablesDelta = toTables.Where(t => !fromTables.Any(f => AreSameTableName(f, t)))
                .Select(x => new DifferentialExpression(x));
            var fromPks = _fromSchema.OfType<CreateConstraintExpression>()
                .Where(c => c.Constraint.IsPrimaryKeyConstraint).ToList();
            var toPks = _toSchema.OfType<CreateConstraintExpression>()
                .Where(c => c.Constraint.IsPrimaryKeyConstraint).ToList();

            var pksDelta = toPks.Where(t => !fromPks.Any(f => AreSameTableName(f, t)))
                .Select(x => new DifferentialExpression(x));

            return tablesDelta.Concat(pksDelta);
        }

        private bool AreSameTableName(CreateConstraintExpression f, CreateConstraintExpression t)
        {
            return f.Constraint.SchemaName == t.Constraint.SchemaName && f.Constraint.TableName == t.Constraint.TableName && f.Constraint.ConstraintName == t.Constraint.ConstraintName;
        }

        private IEnumerable<DifferentialExpression> GetUpdatedTables()
        {
            var fromTables = _fromSchema.OfType<CreateTableExpression>().ToList();
            var toTables = _toSchema.OfType<CreateTableExpression>().ToList();

            var alteredTables = toTables.Select(t => new
            {
                To = t,
                From = fromTables.FirstOrDefault(f => AreSameTableName(f, t))
            })
                .Where(x => !AreSameTableDef(x.From, x.To))
                .SelectMany(x => GetAlters(x.From, x.To));

            var fromPks = _fromSchema.OfType<CreateConstraintExpression>()
                .Where(c => c.Constraint.IsPrimaryKeyConstraint).ToList();
            var toPks = _toSchema.OfType<CreateConstraintExpression>()
                .Where(c => c.Constraint.IsPrimaryKeyConstraint).ToList();

            var alteredPks = toPks.Select(t => new
            {
                To = t,
                From = fromPks.FirstOrDefault(f => AreSameTableName(f, t))
            })
                .Where(x => x.From != null && !AreSameKeyDef(x.From, x.To))
                .SelectMany(x => GetAlters(x.From, x.To));

            return alteredTables.Concat(alteredPks);
        }

        private IEnumerable<DifferentialExpression> GetAlters(CreateConstraintExpression from, CreateConstraintExpression to)
        {
            var deleteOld = new DeleteConstraintExpression(ConstraintType.PrimaryKey) { Constraint = from.Constraint };
            var createNew = to;

            var deleteNew = new DeleteConstraintExpression(ConstraintType.PrimaryKey) { Constraint = to.Constraint };
            var createOld = from;

            return new DifferentialExpression[]
            {
                new DifferentialExpression(deleteOld, createOld),
                new DifferentialExpression(createNew, deleteNew)
            };
        }

        private bool AreSameKeyDef(CreateConstraintExpression @from, CreateConstraintExpression to)
        {
            return AreSameTableName(@from, to) &&
            MatchStrings(from.Constraint.Columns, to.Constraint.Columns);
        }

        private IEnumerable<DifferentialExpression> GetRemovedTables()
        {
            var fromTables = _fromSchema.OfType<CreateTableExpression>().ToList();
            var toTables = _toSchema.OfType<CreateTableExpression>().ToList();

            return fromTables.Where(f => !toTables.Any(t => AreSameTableName(f, t)))
                .Select(t => new DifferentialExpression(t.Reverse(), t));
        }

        private IEnumerable<DifferentialExpression> GetNewForeignKeys()
        {
            var fromFks = _fromSchema.OfType<CreateForeignKeyExpression>().ToList();
            var toFks = _toSchema.OfType<CreateForeignKeyExpression>();

            return toFks.Where(tf => !fromFks.Any(ff => AreSameFk(tf.ForeignKey, ff.ForeignKey)))
                .Select(x => new DifferentialExpression(x));
        }

        private IEnumerable<DifferentialExpression> GetRemovedForeignKeys()
        {
            var fromFks = _fromSchema.OfType<CreateForeignKeyExpression>().ToList();
            var toFks = _toSchema.OfType<CreateForeignKeyExpression>();

            return fromFks.Where(ff => !toFks.Any(tf => AreSameFk(ff.ForeignKey, tf.ForeignKey)))
                .Select(x => new DifferentialExpression(x.Reverse(), x));
        }

        IEnumerable<CreateConstraintExpression> GetPrimaryKeys(IEnumerable<MigrationExpressionBase> schema)
        {
            return schema.OfType<CreateTableExpression>().Where(t => t.Columns.Any(c => c.IsPrimaryKey))
                .Select(t => new CreateConstraintExpression(ConstraintType.PrimaryKey)
                {
                    Constraint = new ConstraintDefinition(ConstraintType.PrimaryKey)
                    {
                        ConstraintName = t.Columns.First().PrimaryKeyName,
                        SchemaName = t.SchemaName,
                        TableName = t.TableName,
                        Columns = t.Columns.Where(x => x.IsPrimaryKey).Select(c => c.Name).ToList()
                    }
                });
        }

        private IEnumerable<DifferentialExpression> GetNewPrimaryKeys()
        {
            var fromPks = GetPrimaryKeys(_fromSchema);
            var toPks = GetPrimaryKeys(_toSchema);

            return toPks.Where(toPk => !fromPks.Any(fromPk => AreSameConstraint(toPk.Constraint, fromPk.Constraint)))
                .Where(x => _fromSchema.OfType<CreateTableExpression>().Any(t => t.TableName == x.Constraint.TableName)) //table previously existed
                .Select(x => new DifferentialExpression(x));
        }

        private IEnumerable<DifferentialExpression> GetRemovedPrimaryKeys()
        {
            var fromPks = GetPrimaryKeys(_fromSchema);
            var toPks = GetPrimaryKeys(_toSchema);

            return fromPks.Where(fromPk => !toPks.Any(toPk => AreSameConstraint(fromPk.Constraint, toPk.Constraint)))
                .Where(x => _toSchema.OfType<CreateTableExpression>().Any(t => t.TableName == x.Constraint.TableName)) //table still exists
                .Select(x => new DifferentialExpression(x.Reverse(), x));
        }

        private IEnumerable<DifferentialExpression> GetNewIndexes()
        {
            var fromIndexes = _fromSchema.OfType<CreateIndexExpression>().ToList();
            var toIndexes = _toSchema.OfType<CreateIndexExpression>().ToList();

            return toIndexes.Where(t => !fromIndexes.Any(f => AreSameIndexName(f.Index, t.Index)))
                .Select(x => new DifferentialExpression(x));
        }

        private IEnumerable<DifferentialExpression> GetRemovedSchemas()
        {
            var fromSchemata = _fromSchema.OfType<CreateSchemaExpression>().ToList();
            var toSchemata = _toSchema.OfType<CreateSchemaExpression>().ToList();

            return fromSchemata.Where(fs => !toSchemata.Any(ts => ts.SchemaName == fs.SchemaName))
                .Select(fs => new DifferentialExpression(fs.Reverse(), fs));
        }

        private IEnumerable<DifferentialExpression> GetNewSequences()
        {
            var toSequences = _toSchema.OfType<CreateSequenceExpression>();
            var fromSequences = _fromSchema.OfType<CreateSequenceExpression>();

            return toSequences.Where(t => !fromSequences.Any(f => HaveSameSequenceName(f, t)))
                .Select(x => new DifferentialExpression(x, Reverse(x)));
        }

        private static bool HaveSameSequenceName(CreateSequenceExpression f, CreateSequenceExpression t)
        {
            return f.Sequence.SchemaName == t.Sequence.SchemaName && f.Sequence.Name == t.Sequence.Name;
        }

        private IEnumerable<DifferentialExpression> GetRemovedSequences()
        {
            var toSequences = _toSchema.OfType<CreateSequenceExpression>();
            var fromSequences = _fromSchema.OfType<CreateSequenceExpression>();

            return fromSequences.Where(f => !toSequences.Any(t => HaveSameSequenceName(f, t)))
                .Select(x => new DifferentialExpression(Reverse(x), x));
        }

        private DeleteSequenceExpression Reverse(CreateSequenceExpression x)
        {
            return new DeleteSequenceExpression() { SchemaName = x.Sequence.SchemaName, SequenceName = x.Sequence.Name };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private bool AreSameTableName(CreateTableExpression fromTable, CreateTableExpression toTable)
        {
            return fromTable.SchemaName == toTable.SchemaName && fromTable.TableName == toTable.TableName;
        }

        private bool MatchIndexColumns(ICollection<IndexColumnDefinition> fromIx, ICollection<IndexColumnDefinition> toIx)
        {
            return MatchCollection(fromIx, toIx, (a, b) => a.Name == b.Name && a.Direction == b.Direction);
        }

        private bool MatchIndexIncludes(ICollection<IndexIncludeDefinition> fromIx, ICollection<IndexIncludeDefinition> toIx)
        {
            return MatchCollection(fromIx, toIx, (a, b) => a.Name == b.Name);
        }

        private bool MatchStrings(ICollection<string> from, ICollection<string> to)
        {
            return MatchCollection(from, to, (a, b) => a == b);
        }

        private bool MatchCollection<T>(ICollection<T> from, ICollection<T> to, Func<T, T, bool> comparer)
        {
            if (from.Count != to.Count) return false;
            var toList = to.ToList();
            return from.Select((f, i) => comparer(f, toList[i])).All(x => x);
        }

        private bool AreSameFk(ForeignKeyDefinition fromFk, ForeignKeyDefinition toFk)
        {
            return fromFk.Name == toFk.Name
                && fromFk.ForeignTableSchema == toFk.ForeignTableSchema
                && fromFk.ForeignTable == toFk.ForeignTable
                && fromFk.PrimaryTable == toFk.PrimaryTable
                && fromFk.PrimaryTableSchema == toFk.PrimaryTableSchema
                && fromFk.OnDelete == toFk.OnDelete
                && fromFk.OnUpdate == toFk.OnUpdate
                && MatchStrings(fromFk.ForeignColumns, toFk.ForeignColumns)
                && MatchStrings(fromFk.PrimaryColumns, toFk.PrimaryColumns);
        }
        private bool AreSameConstraint(ConstraintDefinition fromConstraint, ConstraintDefinition toConstraint)
        {
            return fromConstraint.ConstraintName == toConstraint.ConstraintName
                && fromConstraint.IsPrimaryKeyConstraint == toConstraint.IsPrimaryKeyConstraint
                && fromConstraint.IsUniqueConstraint == toConstraint.IsUniqueConstraint
                && fromConstraint.TableName == toConstraint.TableName
                && fromConstraint.SchemaName == toConstraint.SchemaName
                && MatchStrings(fromConstraint.Columns, toConstraint.Columns);
        }

        private bool AreSameIndexName(IndexDefinition fromIx, IndexDefinition toIx)
        {
            return fromIx.SchemaName == toIx.SchemaName && fromIx.TableName == toIx.TableName
                && fromIx.IsClustered == toIx.IsClustered && fromIx.IsUnique == toIx.IsUnique
                && MatchIndexColumns(fromIx.Columns, toIx.Columns) && MatchIndexIncludes(fromIx.GetIncludes(), toIx.GetIncludes());
        }

        private IEnumerable<DifferentialExpression> GetAlters(CreateTableExpression from, CreateTableExpression to)
        {
            if (from == null || to == null) return Enumerable.Empty<DifferentialExpression>();

            var removedCols = from.Columns.Where(f => !to.Columns.Any(t => AreSameColumnName(f, t)))
                .Select(c => new CreateColumnExpression
                {
                    TableName = to.TableName,
                    SchemaName = to.SchemaName,
                    Column = c
                }).Select(x => new DifferentialExpression(x.Reverse(), x));

            var addedCols = to.Columns.Where(t => !from.Columns.Any(f => AreSameColumnName(f, t)))
                .Select(c => new CreateColumnExpression
                {
                    TableName = to.TableName,
                    SchemaName = to.SchemaName,
                    Column = c
                }).Select(x => new DifferentialExpression(x));

            var matches = from.Columns
                .Select(f => new
                {
                    From = f,
                    To = to.Columns.FirstOrDefault(t => AreSameColumnName(f, t))
                }).Where(x => x.To != null) //if To == null, this is a dropped column
                .Where(x => !AreSameColumnDef(x.From, x.To)).ToList();

            var updatedCols = matches.Select(x => new DifferentialExpression(
                new ExtendedAlterColumnExpression
                {
                    SchemaName = to.SchemaName,
                    TableName = to.TableName,
                    Column = x.To,
                    Previous = x.From
                },
                new AlterColumnExpression
                {
                    SchemaName = to.SchemaName,
                    TableName = to.TableName,
                    Column = x.From
                }
            ));

            return addedCols.Concat(updatedCols).Concat(removedCols);
        }

        private bool AreSameColumnName(ColumnDefinition from, ColumnDefinition to)
        {
            return from.Name == to.Name;
        }

        private bool AreSameTableDef(CreateTableExpression from, CreateTableExpression to)
        {
            if (from == null || to == null || !AreSameTableName(from, to)) return false;
            return MatchCollection(from.Columns, to.Columns, AreSameColumnDef);
        }

        static readonly DbType[] _dbTypesWithMAX = new[] { DbType.AnsiString, DbType.Binary, DbType.Xml , DbType.String };
        
        private bool AreSameColumnDef(ColumnDefinition a, ColumnDefinition b)
        {
            //Special case to treat a string length of 1000000 as int.MaxValue
            int? aSize = a == null ? null : _dbTypesWithMAX.Contains(a.Type.Value) && a.Size == 1000000 ? int.MaxValue : a.Size;
            int? bSize = b == null ? null : _dbTypesWithMAX.Contains(b.Type.Value) && b.Size == 1000000 ? int.MaxValue : b.Size;

            bool sameName = a.Name == b.Name;
            bool sameType = a.Type == b.Type;
            bool sameSize = aSize == bSize;
            bool samePrecision = a.Precision == b.Precision;
            bool sameCustomType = a.CustomType == b.CustomType;
            bool sameDefaultValue = Equals(a.DefaultValue, b.DefaultValue) || (a.DefaultValue is ColumnDefinition.UndefinedDefaultValue) && (b.DefaultValue is FluentMigrator.Model.ColumnDefinition.UndefinedDefaultValue);
            bool sameIsForeignKey = a.IsForeignKey == b.IsForeignKey;
            bool sameIsIdentity = a.IsIdentity == b.IsIdentity;
            bool sameIsIndexed = a.IsIndexed == b.IsIndexed;
            //bool sameIsPrimaryKey = a.IsPrimaryKey == b.IsPrimaryKey;
            //bool samePrimaryKeyName = a.PrimaryKeyName == b.PrimaryKeyName;
            bool sameIsNullable = a.IsNullable == b.IsNullable;
            bool sameIsUnique = a.IsUnique == b.IsUnique;
            bool sameTableName = a.TableName == b.TableName;
            bool sameModificationType = a.ModificationType == b.ModificationType;
            bool sameColumnDescription = a.ColumnDescription == b.ColumnDescription;
            bool sameCollationName = a.CollationName == b.CollationName;

            bool isEverythingTheSame = sameName && sameType && sameSize && samePrecision && sameCustomType && sameDefaultValue && sameIsForeignKey &&
                sameIsIdentity && sameIsIndexed /*&& sameIsPrimaryKey && samePrimaryKeyName*/ && sameIsNullable && sameIsUnique &&
                sameTableName && sameModificationType && sameColumnDescription && sameCollationName;

            return isEverythingTheSame;
        }

    }
}