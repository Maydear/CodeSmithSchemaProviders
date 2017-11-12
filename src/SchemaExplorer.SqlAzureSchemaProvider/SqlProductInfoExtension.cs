using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchemaExplorer
{
    internal static class SqlProductInfoExtension
    {
        public static string GetTables(this SqlProductInfo productInfo)
        {
            if (productInfo.IsSqlAzure)
            {
                return Constants.SQL_GetTablesAzure;
            }
            if (productInfo.IsSql2005OrNewer)
            {
                return Constants.SQL_GetTables2005;
            }
            return Constants.SQL_GetTables;
        }

        public static string GetAllTableColumns(this SqlProductInfo productInfo)
        {
            if (productInfo.IsSql2005OrNewer)
            {
                return Constants.SQL_GetAllTableColumns2005;
            }
            return Constants.SQL_GetAllTableColumns;
        }


        public static string GetTableIndexes(this SqlProductInfo productInfo)
        {
            if (productInfo.IsSqlAzure)
            {
                return Constants.SQL_GetTableIndexesAzure;
            }
            if (productInfo.IsSql2005OrNewer)
            {
                return Constants.SQL_GetTableIndexes2005;
            }
            return Constants.SQL_GetTableIndexes;
        }

        public static string GetTableColumns(this SqlProductInfo productInfo)
        {
            if (productInfo.IsSql2005OrNewer)
            {
                return Constants.SQL_GetTableColumns2005;
            }
            return Constants.SQL_GetTableColumns;
        }

        public static string GetColumnConstraints(this SqlProductInfo productInfo)
        {
            if (productInfo.IsSql2005OrNewer)
            {
                return Constants.SQL_GetColumnConstraints2005;
            }
            return Constants.SQL_GetColumnConstraints;
        }

        public static string GetColumnConstraintsWhere(this SqlProductInfo productInfo)
        {
            if (productInfo.IsSql2005OrNewer)
            {
                return " WHERE SCHEMA_NAME([t].[schema_id]) = @SchemaName AND [t].[name] = @TableName AND [c].[name] = @ColumnName";
            }
            return " AND [stbl].[name] = @SchemaName AND [tbl].[name] = @TableName AND [clmns].[name] = @ColumnName";
        }

        public static string GetIndexes(this SqlProductInfo productInfo)
        {
            if (productInfo.IsSqlAzure)
            {
                return Constants.SQL_GetIndexesAzure;
            }
            if (productInfo.IsSql2005OrNewer)
            {
                return Constants.SQL_GetIndexes2005;
            }
            return Constants.SQL_GetIndexes;
        }

        public static string GetKeys(this SqlProductInfo productInfo)
        {
            if (productInfo.IsSql2005OrNewer)
            {
                return Constants.SQL_GetKeys2005;
            }
            return Constants.SQL_GetKeys;
        }

        public static string GetExtendedData(this SqlProductInfo productInfo)
        {
            if (productInfo.IsSql2005OrNewer)
            {
                return Constants.SQL_GetExtendedData2005;
            }
            return Constants.SQL_GetExtenedData;
        }

        public static string GetExtendedProperties(this SqlProductInfo productInfo)
        {
            return Constants.SQL_GetExtendedProperties;
        }

        public static string GetViews(this SqlProductInfo productInfo)
        {
            if (productInfo.IsSqlAzure)
            {
                return Constants.SQL_GetViewsAzure;
            }
            if (productInfo.IsSql2005OrNewer)
            {
                return Constants.SQL_GetViews2005;
            }
            return Constants.SQL_GetViews;
        }

        public static string GetViewColumns(this SqlProductInfo productInfo)
        {
            if (productInfo.IsSql2005OrNewer)
            {
                return Constants.SQL_GetViewColumns2005;
            }
            return Constants.SQL_GetViewColumns;
        }

        public static string GetAllViewColumns(this SqlProductInfo productInfo)
        {
            if (productInfo.IsSql2005OrNewer)
            {
                return Constants.SQL_GetAllViewColumns2005;
            }
            return Constants.SQL_GetAllViewColumns;
        }

        public static string GetCommands(this SqlProductInfo productInfo)
        {
            if (productInfo.IsSqlAzure)
            {
                return Constants.SQL_GetCommandsAzure;
            }
            if (productInfo.IsSql2005OrNewer)
            {
                return Constants.SQL_GetCommands2005;
            }
            return Constants.SQL_GetCommands;
        }

        public static string GetCommandParameters(this SqlProductInfo productInfo)
        {
            if (productInfo.IsSql2005OrNewer)
            {
                return Constants.SQL_GetCommandParameters2005;
            }
            return Constants.SQL_GetCommandParameters;
        }

        public static string GetAllCommandParameters(this SqlProductInfo productInfo)
        {
            if (productInfo.IsSql2005OrNewer)
            {
                return Constants.SQL_GetAllCommandParameters2005;
            }
            return Constants.SQL_GetAllCommandParameters;
        }

        public static string GetTableKeys(this SqlProductInfo productInfo)
        {
            if (productInfo.IsSql2005OrNewer)
            {
                return Constants.SQL_GetTableKeys2005;
            }
            return Constants.SQL_GetTableKeys;
        }
    }


}
