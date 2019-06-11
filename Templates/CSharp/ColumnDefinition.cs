using System;
using System.Data;
using System.IO;
using FluentMigrator.Model;

namespace FluentMigrator.NHibernateGenerator.Templates.CSharp
{
    internal class ColumnDefinitionTemplate : ExpressionTemplate<ColumnDefinition>
    {
        public override void WriteTo(TextWriter tw)
        {
            var columnDefinition = Expression;
            WriteColumnValue(tw, columnDefinition);
        }

        public static void WriteColumnValue(TextWriter tw, ColumnDefinition columnDefinition)
        {
            var col = columnDefinition;
            var colType = columnDefinition.Type;

            if (!string.IsNullOrWhiteSpace(col.CustomType))
                tw.Write($@".AsCustom(""{columnDefinition.CustomType}"")");
            else if (colType == DbType.AnsiString || colType == DbType.Binary || colType == DbType.Xml || colType == DbType.String)
                tw.Write($@".As{colType.ToString()}({(col.Size == int.MaxValue || col.Size == 1000000 ? "int.MaxValue" : col.Size.ToString())})");
            else if (colType == DbType.Decimal && col.Size == 0)
                tw.Write(".AsDecimal()");
            else if (colType == DbType.Decimal && col.Size != 0)
                tw.Write($@".AsDecimal({col.Size}, {col.Precision})");
            else if (colType == DbType.Single)
                tw.Write(".AsFloat()");
            else if (colType == DbType.AnsiStringFixedLength)
                tw.Write($@".AsFixedLengthAnsiString({col.Size})");
            else if (colType == DbType.StringFixedLength)
                tw.Write($@".AsFixedLengthString({col.Size})");
            else
                tw.Write($@".As{colType.ToString()}()");

            if (col.IsNullable ?? true)
                tw.Write(".Nullable()");
            else
                tw.Write(".NotNullable()");

            if (col.IsIdentity)
                tw.Write(".Identity()");

            if (col.IsUnique)
                tw.Write(".Unique()");

            if (col.IsPrimaryKey)
                tw.Write($@".PrimaryKey({(!string.IsNullOrEmpty(col.PrimaryKeyName) ? $@"""{col.PrimaryKeyName}""" : null)})");

            if (!(col.DefaultValue is ColumnDefinition.UndefinedDefaultValue) && col.DefaultValue != null)
            {
                string strDefaultValue = col.DefaultValue.ToString(); //It's always a string from NH
                bool isNumeric = col.Type == DbType.Byte || col.Type == DbType.Decimal || col.Type == DbType.Double 
                    || col.Type == DbType.Int16 || col.Type == DbType.Int32 || col.Type == DbType.Int64 || col.Type == DbType.SByte 
                    || col.Type == DbType.Single || col.Type == DbType.UInt16 || col.Type == DbType.UInt32 || col.Type == DbType.UInt64;

                if (col.Type == DbType.Boolean)
                    strDefaultValue = Convert.ToBoolean(int.Parse(strDefaultValue)).ToString().ToLower();
                else if (!isNumeric)
                    strDefaultValue = "\"" + strDefaultValue + "\"";

                tw.Write($@".WithDefaultValue({strDefaultValue})");
            }

            if (!string.IsNullOrEmpty(col.ColumnDescription))
                tw.Write($@".WithColumnDescription(""{col.ColumnDescription}"")");
        }
    }
}