using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchemaExplorer
{
    internal static class NativeTypeExtension
    {
        /// <summary>
        /// nativeType转换为DbType
        /// </summary>
        /// <param name="nativeType">nativeType显示的字符串</param>
        /// <returns></returns>
        internal static DbType ToDbType(this string nativeType)
        {
            string switchSring = nativeType.Trim().ToLower();

            if (string.IsNullOrEmpty(switchSring))
                return DbType.Object;

            switch (switchSring)
            {

                case "tinyint":
                    return DbType.Byte;

                case "bit":
                    return DbType.Boolean;

                case "smallint":
                    return DbType.Int16;
                case "int":
                    return DbType.Int32;
                case "bigint":
                    return DbType.Int64;
                case "real":
                    return DbType.Single;
                case "float":
                    return DbType.Double;

                case "numeric":
                case "decimal":
                    return DbType.Decimal;

                case "smallmoney":
                case "money":
                    return DbType.Currency;

                case "nvarchar":
                case "ntext":
                    return DbType.String;

                case "varchar":
                case "text":
                    return DbType.AnsiString;

                case "sysname":
                case "nchar":
                    return DbType.StringFixedLength;

                case "char":
                    return DbType.AnsiStringFixedLength;

                case "time":
                    return DbType.Time;

                case "date":
                    return DbType.Date;

                case "datetime":
                case "smalldatetime":
                    return DbType.DateTime;

                case "datetimeoffset":
                    return DbType.DateTimeOffset;

                case "datetime2":
                    return DbType.DateTime2;

                case "image":
                case "timestamp":
                case "binary":
                case "varbinary":
                    return DbType.Binary;

                case "xml":
                    return DbType.Xml;

                case "uniqueidentifier":
                    return DbType.Guid;

                case "sql_variant":
                default:
                    return DbType.Object;
            }
        }

        /// <summary>
        /// nativeType转换为SqlDbType
        /// </summary>
        /// <param name="nativeType">nativeType显示的字符串</param>
        /// <returns></returns>
        internal static SqlDbType ToSqlDbType(this string nativeType)
        {
            string switchSring = nativeType.Trim().ToLower();
            if (string.IsNullOrEmpty(switchSring))
                return SqlDbType.Variant;

            switch (switchSring)
            {

                case "tinyint":
                    return SqlDbType.TinyInt;

                case "bit":
                    return SqlDbType.Bit;

                case "smallint":
                    return SqlDbType.SmallInt;
                case "int":
                    return SqlDbType.Int;
                case "bigint":
                    return SqlDbType.BigInt;
                case "real":
                    return SqlDbType.Real;
                case "float":
                    return SqlDbType.Float;

                case "numeric":
                case "decimal":
                    return SqlDbType.Decimal;

                case "smallmoney":
                    return SqlDbType.SmallMoney;
                case "money":
                    return SqlDbType.Money;

                case "nvarchar":
                    return SqlDbType.NVarChar;
                case "ntext":
                    return SqlDbType.NText;

                case "varchar":
                    return SqlDbType.VarChar;
                case "text":
                    return SqlDbType.Text;

                case "sysname":
                case "nchar":
                    return SqlDbType.NChar;

                case "char":
                    return SqlDbType.Char;

                case "time":
                    return SqlDbType.Time;

                case "date":
                    return SqlDbType.Date;

                case "datetime":
                    return SqlDbType.DateTime;
                case "smalldatetime":
                    return SqlDbType.SmallDateTime;

                case "datetimeoffset":
                    return SqlDbType.DateTimeOffset;

                case "datetime2":
                    return SqlDbType.DateTime2;

                case "image":
                    return SqlDbType.Image;
                case "timestamp":
                    return SqlDbType.Timestamp;
                case "binary":
                    return SqlDbType.Binary;
                case "varbinary":
                    return SqlDbType.VarBinary;

                case "xml":
                    return SqlDbType.Xml;

                case "uniqueidentifier":
                    return SqlDbType.UniqueIdentifier;

                case "sql_variant":
                    return SqlDbType.Variant;
                default:
                    return SqlDbType.Variant;
            }
        }

    }
}
