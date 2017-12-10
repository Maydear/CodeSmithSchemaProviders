using System.Data;

namespace SchemaExplorer
{
    internal static class NativeTypeExtension
    {
        /// <summary>
        /// nativeType转换为DbType
        /// </summary>
        /// <param name="nativeType">nativeType显示的字符串</param>
        /// <returns></returns>
        internal static DbType ToDbType(this string nativeType, bool isUnsigned)
        {
            string switchSring = nativeType.Trim().ToLower();

            if (string.IsNullOrEmpty(switchSring))
                return DbType.Object;

            switch (switchSring)
            {

                case "tinyint":
                    return isUnsigned ? DbType.SByte : DbType.Byte;

                case "bit":
                    return DbType.Boolean;

                case "smallint":
                    return isUnsigned ? DbType.UInt16 : DbType.Int16;

                case "int":
                case "year":
                    return isUnsigned ? DbType.UInt32 : DbType.Int32;

                case "bigint":
                    return isUnsigned ? DbType.UInt64 : DbType.Int64;
                case "float":
                    return DbType.Single;
                case "double":
                    return DbType.Double;

                case "numeric":
                case "decimal":
                    return DbType.Decimal;

                case "char":
                case "nvarchar":
                case "ntext":
                case "varchar":
                case "text":
                case "longtext":
                case "tinytext":
                case "mediumtext":
                    return DbType.String;

                case "time":
                    return DbType.Time;

                case "date":
                    return DbType.Date;

                case "datetime":
                case "timestamp":
                    return DbType.DateTime;

                case "binary":
                case "varbinary":
                case "blob":
                case "longblob":
                case "tinyblob":
                    return DbType.Binary;
                default:
                    return DbType.Object;
            }
        }
    }
}
