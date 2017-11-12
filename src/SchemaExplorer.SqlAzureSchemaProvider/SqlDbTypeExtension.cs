using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Data
{
    internal static class SqlDbTypeExtension
    {
        /// <summary>
        /// SqlDbType 转换为NativeType
        /// </summary>
        /// <param name="sqlDbType"></param>
        /// <returns></returns>
        internal static string ToNativeType(this SqlDbType sqlDbType)
        {

            switch (sqlDbType)
            {
                case SqlDbType.BigInt:
                    return "bigint";
                case SqlDbType.Binary:
                    return "binary";
                case SqlDbType.Bit:
                    return "bit";
                case SqlDbType.Char:
                    return "char";
                case SqlDbType.DateTime:
                    return "datetime";
                case SqlDbType.Decimal:
                    return "decimal";
                case SqlDbType.Float:
                    return "float";
                case SqlDbType.Image:
                    return "image";
                case SqlDbType.Int:
                    return "int";
                case SqlDbType.Money:
                    return "money";
                case SqlDbType.NChar:
                    return "nchar";
                case SqlDbType.NText:
                    return "ntext";
                case SqlDbType.NVarChar:
                    return "nvarchar";
                case SqlDbType.Real:
                    return "real";
                case SqlDbType.UniqueIdentifier:
                    return "uniqueidentifier";
                case SqlDbType.SmallDateTime:
                    return "smalldatetime";
                case SqlDbType.SmallInt:
                    return "smallint";
                case SqlDbType.SmallMoney:
                    return "smallmoney";
                case SqlDbType.Text:
                    return "text";
                case SqlDbType.Timestamp:
                    return "timestamp";
                case SqlDbType.TinyInt:
                    return "tinyint";
                case SqlDbType.VarBinary:
                    return "varbinary";
                case SqlDbType.VarChar:
                    return "varchar";
                case SqlDbType.Variant:
                    return "sql_variant";
                case SqlDbType.Xml:
                    return "xml";
                case SqlDbType.Date:
                    return "date";
                case SqlDbType.Time:
                    return "time";
                case SqlDbType.DateTime2:
                    return "datetime2";
                case SqlDbType.DateTimeOffset:
                    return "datetimeoffset";
                default:
                    return "sql_variant";
            }
        }
    }
}
