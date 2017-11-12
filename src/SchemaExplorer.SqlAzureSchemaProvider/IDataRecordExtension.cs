using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchemaExplorer
{
    public static class IDataRecordExtension
    {
        /// <summary>
        /// 以布尔值的形式获取指定列的值。
        /// </summary>
        /// <param name="record">提供对每行中的列值的访问 DataReader</param>
        /// <param name="columnName">列名</param>
        /// <returns>列的值</returns>
        public static bool GetBoolean(this IDataRecord record, string columnName)
        {
            return record.GetBoolean(record.GetOrdinal(columnName));
        }

        /// <summary>
        /// 获取指定列的 8 位无符号的整数值。
        /// </summary>
        /// <param name="record">提供对每行中的列值的访问 DataReader</param>
        /// <param name="columnName">列名</param>
        /// <returns>指定列的 8 位无符号的整数值。</returns>
        public static byte GetByte(this IDataRecord record, string columnName)
        {
            return record.GetByte(record.GetOrdinal(columnName));
        }

        /// <summary>
        /// 获取指定列的字符值。
        /// </summary>
        /// <param name="record">提供对每行中的列值的访问 DataReader</param>
        /// <param name="columnName">列名</param>
        /// <returns>指定列的字符值。</returns>
        public static char GetChar(this IDataRecord record, string columnName)
        {
            return record.GetChar(record.GetOrdinal(columnName));
        }

        /// <summary>
        /// 获取指定字段的数据类型信息。
        /// </summary>
        /// <param name="record">提供对每行中的列值的访问 DataReader</param>
        /// <param name="columnName">列名</param>
        /// <returns>数据类型指定字段的信息。</returns>
        public static string GetDataTypeName(this IDataRecord record, string columnName)
        {
            return record.GetDataTypeName(record.GetOrdinal(columnName));
        }

        /// <summary>
        /// 获取指定字段的日期和时间数据值。
        /// </summary>
        /// <param name="record">提供对每行中的列值的访问 DataReader</param>
        /// <param name="columnName">列名</param>
        /// <returns>指定字段的日期和时间数据值。</returns>
        public static DateTime GetDateTime(this IDataRecord record, string columnName)
        {
            return record.GetDateTime(record.GetOrdinal(columnName));
        }

        /// <summary>
        /// 获取指定字段的数值固定位置。
        /// </summary>
        /// <param name="record">提供对每行中的列值的访问 DataReader</param>
        /// <param name="columnName">列名</param>
        /// <returns>指定字段固定位置数字值。</returns>
        public static decimal GetDecimal(this IDataRecord record, string columnName)
        {
            return record.GetDecimal(record.GetOrdinal(columnName));
        }

        /// <summary>
        /// 获取指定字段的双精度浮点数。
        /// </summary>
        /// <param name="record">提供对每行中的列值的访问 DataReader</param>
        /// <param name="columnName">列名</param>
        /// <returns>双精度浮点数的指定字段。</returns>
        public static double GetDouble(this IDataRecord record, string columnName)
        {
            return record.GetDouble(record.GetOrdinal(columnName));
        }

        /// <summary>
        /// 获取 System.Type 信息对应的一种 System.Object
        /// </summary>
        /// <param name="record">提供对每行中的列值的访问 DataReader</param>
        /// <param name="columnName">列名</param>
        /// <returns>获取 System.Type 信息对应的一种 System.Object</returns>
        public static Type GetFieldType(this IDataRecord record, string columnName)
        {
            return record.GetFieldType(record.GetOrdinal(columnName));
        }

        /// <summary>
        /// 获取指定字段的单精度浮点数。
        /// </summary>
        /// <param name="record">提供对每行中的列值的访问 DataReader</param>
        /// <param name="columnName">列名</param>
        /// <returns>单精度浮点数的指定字段。</returns>
        public static float GetFloat(this IDataRecord record, string columnName)
        {
            return record.GetFloat(record.GetOrdinal(columnName));
        }


        /// <summary>
        /// 返回指定字段的 GUID 值。
        /// </summary>
        /// <param name="record">提供对每行中的列值的访问 DataReader</param>
        /// <param name="columnName">列名</param>
        /// <returns>指定字段的 GUID 值。</returns>
        public static Guid GetGuid(this IDataRecord record, string columnName)
        {
            return record.GetGuid(record.GetOrdinal(columnName));
        }

        /// <summary>
        /// 获取指定字段的 16 位有符号的整数值。
        /// </summary>
        /// <param name="record">提供对每行中的列值的访问 DataReader</param>
        /// <param name="columnName">列名</param>
        /// <returns>指定字段的 16 位带符号的整数值。</returns>
        public static short GetInt16(this IDataRecord record, string columnName)
        {
            return record.GetInt16(record.GetOrdinal(columnName));
        }
 
        /// <summary>
        /// 获取指定字段的 32 位有符号的整数值。
        /// </summary>
        /// <param name="record">提供对每行中的列值的访问 DataReader</param>
        /// <param name="columnName">列名</param>
        /// <returns>指定字段的 32 位有符号的整数值。</returns>
        public static int GetInt32(this IDataRecord record, string columnName)
        {
            return record.GetInt32(record.GetOrdinal(columnName));
        }

        /// <summary>
        /// 获取指定字段的 64 位有符号的整数值。
        /// </summary>
        /// <param name="record">提供对每行中的列值的访问 DataReader</param>
        /// <param name="columnName">列名</param>
        /// <returns>指定字段的 64 位带符号的整数值。</returns>
        public static long GetInt64(this IDataRecord record, string columnName)
        {
            return record.GetInt64(record.GetOrdinal(columnName));
        }

        /// <summary>
        /// 获取指定字段的字符串值。
        /// </summary>
        /// <param name="record">提供对每行中的列值的访问 DataReader</param>
        /// <param name="columnName">列名</param>
        /// <returns>指定字段的字符串值。</returns>
        public static string GetString(this IDataRecord record, string columnName)
        {
            return record.GetString(record.GetOrdinal(columnName));
        }

        /// <summary>
        /// 返回指定字段的值。
        /// </summary>
        /// <param name="record"></param>
        /// <param name="columnName"></param>
        /// <returns>System.Object 它将包含在返回的字段值。</returns>
        public static object GetValue(this IDataRecord record, string columnName)
        {
            return record.GetString(record.GetOrdinal(columnName));
        }

        /// <summary>
        /// 返回指示指定的字段是否设置为 null。
        /// </summary>
        /// <param name="record"></param>
        /// <param name="columnName"></param>
        /// <returns>true 如果指定的字段是否设置为 null;否则为 false。</returns>
        public static bool IsDBNull(this IDataRecord record, string columnName)
        {
            return record.IsDBNull(record.GetOrdinal(columnName));
        }
    }
}
