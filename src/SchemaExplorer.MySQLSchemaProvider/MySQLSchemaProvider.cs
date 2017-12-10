using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeSmith.Core.Collections;
using System.Data.Common;
using MySql.Data.MySqlClient;
using System.Data;
using System.Text.RegularExpressions;

namespace SchemaExplorer
{
    public class MySQLSchemaProvider : IDbSchemaProvider, INamedObject
    {
        public string Name { get { return "MySQLSchemaProvider"; } }

        public string Description { get { return "MySql Schema Provider"; } }

        #region private
        /// <summary>
        /// 创建链接字符串
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        private static DbConnection CreateConnection(string connectionString)
        {
            DbConnection dbConnection = new MySqlConnection(connectionString);
            return dbConnection;
        }

        /// <summary>
        /// 将DataReader转换为DataSet
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private DataSet ConvertDataReaderToDataSet(IDataReader reader)
        {
            return new DataSet
            {
                Tables =
                {
                    ConvertDataReaderToDataTable(reader)
                }
            };
        }

        /// <summary>
        /// 将DataReader转换为DataTable
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private DataTable ConvertDataReaderToDataTable(IDataReader reader)
        {
            DataTable schemaTable = reader.GetSchemaTable();
            DataTable dataTable = new DataTable();
            foreach (DataRow schemaDataRow in schemaTable.Rows)
            {
                dataTable.Columns.Add(new DataColumn((string)schemaDataRow["ColumnName"], (Type)schemaDataRow["DataType"]));
            }

            while (reader.Read())
            {
                DataRow dataRow = dataTable.NewRow();
                for (int i = 0; i <= reader.FieldCount - 1; i++)
                {
                    dataRow[reader.GetName(i)] = reader.GetValue(i);
                }
                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }

        private IEnumerable<TableKeySchema> GetMyTableKeys(string connectionString, SchemaObjectBase table)
        {
            string commandText = string.Format("SELECT CONSTRAINT_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS t1 WHERE t1.TABLE_SCHEMA = '{0}' AND t1.TABLE_NAME = '{1}'  AND CONSTRAINT_TYPE = 'FOREIGN KEY'", table.Database.Name, table.Name);
            string commandText2 = string.Format("SELECT t1.CONSTRAINT_NAME, t1.COLUMN_NAME, t1.POSITION_IN_UNIQUE_CONSTRAINT,  t1.REFERENCED_TABLE_NAME, REFERENCED_COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE t1  INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS t2  ON t2.TABLE_SCHEMA = t1.TABLE_SCHEMA  AND t2.TABLE_NAME = t1.TABLE_NAME  AND t2.CONSTRAINT_NAME = t1.CONSTRAINT_NAME WHERE t1.TABLE_SCHEMA = '{0}' AND t1.TABLE_NAME = '{1}'  AND t2.CONSTRAINT_TYPE = 'FOREIGN KEY' ORDER BY t1.CONSTRAINT_NAME, t1.POSITION_IN_UNIQUE_CONSTRAINT", table.Database.Name, table.Name);
            DataSet dataSet;
            using (DbConnection dbConnection = CreateConnection(connectionString))
            {
                dbConnection.ConnectionString = connectionString;
                dbConnection.Open();
                using (DbCommand dbCommand = dbConnection.CreateCommand())
                {
                    dbCommand.CommandText = commandText;
                    dbCommand.Connection = dbConnection;
                    dataSet = this.ConvertDataReaderToDataSet(dbCommand.ExecuteReader());
                }
                if (dbConnection.State != ConnectionState.Closed)
                {
                    dbConnection.Close();
                }
            }
            using (DbConnection dbConnection2 = CreateConnection(connectionString))
            {
                dbConnection2.ConnectionString = connectionString;
                dbConnection2.Open();
                using (DbCommand dbCommand2 = dbConnection2.CreateCommand())
                {
                    dbCommand2.CommandText = commandText2;
                    dbCommand2.Connection = dbConnection2;
                    dataSet.Tables.Add(this.ConvertDataReaderToDataTable(dbCommand2.ExecuteReader()));
                }
                if (dbConnection2.State != ConnectionState.Closed)
                {
                    dbConnection2.Close();
                }
            }
            List<TableKeySchema> list = new List<TableKeySchema>();
            if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
            {
                dataSet.Relations.Add("Contraint_to_Keys", dataSet.Tables[0].Columns["CONSTRAINT_NAME"], dataSet.Tables[1].Columns["CONSTRAINT_NAME"]);
                foreach (DataRow dataRow in dataSet.Tables[0].Rows)
                {
                    string text = dataRow["CONSTRAINT_NAME"].ToString();
                    List<DataRow> list2 = new List<DataRow>(dataRow.GetChildRows("Contraint_to_Keys"));
                    List<string> list3 = new List<string>(list2.Count);
                    List<string> list4 = new List<string>(list2.Count);
                    string name = table.Name;
                    string text2 = list2[0]["REFERENCED_TABLE_NAME"].ToString();
                    foreach (DataRow current in list2)
                    {
                        list4.Add(current["COLUMN_NAME"].ToString());
                        list3.Add(current["REFERENCED_COLUMN_NAME"].ToString());
                    }
                    list.Add(new TableKeySchema(table.Database, text, list4.ToArray(), name, list3.ToArray(), text2));
                }
            }
            if (list.Count > 0)
            {
                return list;
            }
            return new List<TableKeySchema>();
        }

        private IEnumerable<TableKeySchema> GetOthersTableKeys(string connectionString, SchemaObjectBase table)
        {
            string commandText = string.Format("SELECT DISTINCT CONSTRAINT_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE t1 WHERE t1.TABLE_SCHEMA = '{0}' AND t1.REFERENCED_TABLE_NAME = '{1}'", table.Database.Name, table.Name);
            string commandText2 = string.Format("SELECT t1.CONSTRAINT_NAME, t1.TABLE_NAME, t1.COLUMN_NAME, t1.POSITION_IN_UNIQUE_CONSTRAINT,  t1.REFERENCED_TABLE_NAME, REFERENCED_COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE t1  INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS t2  ON t2.TABLE_SCHEMA = t1.TABLE_SCHEMA  AND t2.TABLE_NAME = t1.TABLE_NAME  AND t2.CONSTRAINT_NAME = t1.CONSTRAINT_NAME WHERE t1.TABLE_SCHEMA = '{0}' AND t1.REFERENCED_TABLE_NAME = '{1}'  AND t2.CONSTRAINT_TYPE = 'FOREIGN KEY' ORDER BY t1.CONSTRAINT_NAME, t1.POSITION_IN_UNIQUE_CONSTRAINT", table.Database.Name, table.Name);
            DataSet dataSet;
            using (DbConnection dbConnection = CreateConnection(connectionString))
            {
                dbConnection.ConnectionString = connectionString;
                dbConnection.Open();
                using (DbCommand dbCommand = dbConnection.CreateCommand())
                {
                    dbCommand.CommandText = commandText;
                    dbCommand.Connection = dbConnection;
                    dataSet = this.ConvertDataReaderToDataSet(dbCommand.ExecuteReader());
                }
                if (dbConnection.State != ConnectionState.Closed)
                {
                    dbConnection.Close();
                }
            }
            using (DbConnection dbConnection2 = CreateConnection(connectionString))
            {
                dbConnection2.ConnectionString = connectionString;
                dbConnection2.Open();
                using (DbCommand dbCommand2 = dbConnection2.CreateCommand())
                {
                    dbCommand2.CommandText = commandText2;
                    dbCommand2.Connection = dbConnection2;
                    dataSet.Tables.Add(this.ConvertDataReaderToDataTable(dbCommand2.ExecuteReader()));
                }
                if (dbConnection2.State != ConnectionState.Closed)
                {
                    dbConnection2.Close();
                }
            }
            List<TableKeySchema> list = new List<TableKeySchema>();
            if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
            {
                dataSet.Relations.Add("Contraint_to_Keys", dataSet.Tables[0].Columns["CONSTRAINT_NAME"], dataSet.Tables[1].Columns["CONSTRAINT_NAME"]);
                foreach (DataRow dataRow in dataSet.Tables[0].Rows)
                {
                    string text = dataRow["CONSTRAINT_NAME"].ToString();
                    List<DataRow> list2 = new List<DataRow>(dataRow.GetChildRows("Contraint_to_Keys"));
                    List<string> list3 = new List<string>(list2.Count);
                    List<string> list4 = new List<string>(list2.Count);
                    string text2 = list2[0]["TABLE_NAME"].ToString();
                    string text3 = list2[0]["REFERENCED_TABLE_NAME"].ToString();
                    foreach (DataRow current in list2)
                    {
                        list4.Add(current["COLUMN_NAME"].ToString());
                        list3.Add(current["REFERENCED_COLUMN_NAME"].ToString());
                    }
                    list.Add(new TableKeySchema(table.Database, text, list4.ToArray(), text2, list3.ToArray(), text3));
                }
            }
            if (list.Count > 0)
            {
                return list;
            }
            return new List<TableKeySchema>();
        }


        #endregion

        public ParameterSchema[] GetCommandParameters(string connectionString, CommandSchema command)
        {
            throw new NotSupportedException("GetCommandParameters() is not supported in this release.");
        }

        public CommandResultSchema[] GetCommandResultSchemas(string connectionString, CommandSchema command)
        {
            throw new NotSupportedException("GetCommandResultSchemas() is not supported in this release.");
        }

        public CommandSchema[] GetCommands(string connectionString, DatabaseSchema database)
        {
            string commandText = string.Format("SELECT ROUTINE_NAME, '' OWNER, CREATED FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = '{0}' AND ROUTINE_TYPE = 'PROCEDURE' ORDER BY 1", database.Name);
            List<CommandSchema> list = new List<CommandSchema>();
            using (DbConnection dbConnection = MySQLSchemaProvider.CreateConnection(connectionString))
            {
                dbConnection.Open();
                DbCommand dbCommand = dbConnection.CreateCommand();
                dbCommand.CommandText = commandText;
                dbCommand.Connection = dbConnection;
                using (IDataReader dataReader = dbCommand.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    while (dataReader.Read())
                    {
                        list.Add(new CommandSchema(database, dataReader.GetString(0), dataReader.GetString(1), dataReader.GetDateTime(2)));
                    }
                    if (!dataReader.IsClosed)
                    {
                        dataReader.Close();
                    }
                }
                if (dbConnection.State != ConnectionState.Closed)
                {
                    dbConnection.Close();
                }
            }
            return list.ToArray();
        }

        public string GetCommandText(string connectionString, CommandSchema command)
        {
            StringBuilder stringBuilder = new StringBuilder();
            string commandText = string.Format("SELECT ROUTINE_DEFINITION FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = '{0}' AND ROUTINE_NAME = '{1}'", command.Database.Name, command.Name);
            using (DbConnection dbConnection = CreateConnection(connectionString))
            {
                dbConnection.Open();
                DbCommand dbCommand = dbConnection.CreateCommand();
                dbCommand.CommandText = commandText;
                dbCommand.Connection = dbConnection;
                using (IDataReader dataReader = dbCommand.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    while (dataReader.Read())
                    {
                        stringBuilder.Append(dataReader.GetString(0));
                    }
                    if (!dataReader.IsClosed)
                    {
                        dataReader.Close();
                    }
                }
                if (dbConnection.State != ConnectionState.Closed)
                {
                    dbConnection.Close();
                }
            }
            return stringBuilder.ToString();
        }

        public string GetDatabaseName(string connectionString)
        {
            Regex regex = new Regex("Database\\W*=\\W*(?<database>[^;]*)", RegexOptions.IgnoreCase);
            Match match = regex.Match(connectionString);
            if (match.Success)
            {
                return match.Groups["database"].ToString();
            }
            return connectionString;
        }

        public ExtendedProperty[] GetExtendedProperties(string connectionString, SchemaObjectBase schemaObject)
        {
            List<ExtendedProperty> list = new List<ExtendedProperty>();
            if (schemaObject is ColumnSchema)
            {
                ColumnSchema columnSchema = schemaObject as ColumnSchema;
                string commandText = string.Format(
                    @"SELECT EXTRA, COLUMN_DEFAULT, COLUMN_TYPE,COLUMN_COMMENT
                     FROM INFORMATION_SCHEMA.COLUMNS
                     WHERE TABLE_SCHEMA = '{0}' AND TABLE_NAME = '{1}' AND COLUMN_NAME = '{2}'", columnSchema.Table.Database.Name, columnSchema.Table.Name, columnSchema.Name);
                using (DbConnection dbConnection = MySQLSchemaProvider.CreateConnection(connectionString))
                {
                    dbConnection.Open();
                    DbCommand dbCommand = dbConnection.CreateCommand();
                    dbCommand.CommandText = commandText;
                    dbCommand.Connection = dbConnection;
                    using (IDataReader dataReader = dbCommand.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (dataReader.Read())
                        {
                            string text = dataReader.GetString(0).ToLower();
                            bool flag = dataReader.IsDBNull(1);
                            string text2 = "";
                            if (!flag)
                            {
                                text2 = dataReader.GetString(1).ToUpper();
                            }
                            string text3 = dataReader.GetString(2).ToUpper();
                            string textCOMMENT = dataReader.GetString(3).ToUpper();
                            bool flag2 = text.IndexOf("auto_increment") > -1;
                            list.Add(new ExtendedProperty("CS_IsIdentity", flag2, columnSchema.DataType));
                            if (flag2)
                            {
                                list.Add(new ExtendedProperty("CS_IdentitySeed", 1, columnSchema.DataType));
                                list.Add(new ExtendedProperty("CS_IdentityIncrement", 1, columnSchema.DataType));
                            }
                            list.Add(new ExtendedProperty("CS_ColumnDefaultIsNull", flag, DbType.Boolean));
                            list.Add(new ExtendedProperty("CS_Default", text2, DbType.String));
                            list.Add(new ExtendedProperty("CS_ColumnDefault", text2, DbType.String));
                            list.Add(new ExtendedProperty("CS_SystemType", text3, DbType.String));
                            list.Add(new ExtendedProperty("CS_ColumnType", text3, DbType.String));
                            list.Add(new ExtendedProperty("CS_Description", textCOMMENT, DbType.String));
                            list.Add(new ExtendedProperty("CS_ColumnExtra", text.ToUpper(), DbType.String));
                        }
                        if (!dataReader.IsClosed)
                        {
                            dataReader.Close();
                        }
                    }
                    if (dbConnection.State != ConnectionState.Closed)
                    {
                        dbConnection.Close();
                    }
                }
            }
            if (schemaObject is TableSchema)
            {
                TableSchema tableSchema = schemaObject as TableSchema;
                string commandText2 = string.Format("SHOW CREATE TABLE `{0}`.`{1}`", tableSchema.Database.Name, tableSchema.Name);
                using (DbConnection dbConnection2 = MySQLSchemaProvider.CreateConnection(connectionString))
                {
                    dbConnection2.Open();
                    DbCommand dbCommand2 = dbConnection2.CreateCommand();
                    dbCommand2.CommandText = commandText2;
                    dbCommand2.Connection = dbConnection2;
                    using (IDataReader dataReader2 = dbCommand2.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (dataReader2.Read())
                        {
                            string @string = dataReader2.GetString(1);
                            list.Add(new ExtendedProperty("CS_CreateTableScript", @string, DbType.String));
                            list.Add(new ExtendedProperty("CS_CreateTableScript", @string, DbType.String));
                        }
                        if (!dataReader2.IsClosed)
                        {
                            dataReader2.Close();
                        }
                    }
                    if (dbConnection2.State != ConnectionState.Closed)
                    {
                        dbConnection2.Close();
                    }
                }
                string commandTextTABLES = string.Format(
                    @"SELECT TABLE_COMMENT
                     FROM INFORMATION_SCHEMA.TABLES
                     WHERE TABLE_SCHEMA = '{0}' AND TABLE_NAME = '{1}'", tableSchema.Database.Name, tableSchema.Name);
                using (DbConnection dbConnection3 = MySQLSchemaProvider.CreateConnection(connectionString))
                {
                    dbConnection3.Open();
                    DbCommand dbCommand2 = dbConnection3.CreateCommand();
                    dbCommand2.CommandText = commandTextTABLES;
                    dbCommand2.Connection = dbConnection3;
                    using (IDataReader dataReader2 = dbCommand2.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (dataReader2.Read())
                        {
                            string textCOMMENT = dataReader2.GetString(0);
                            list.Add(new ExtendedProperty("CS_Description", textCOMMENT, DbType.String));
                        }
                        if (!dataReader2.IsClosed)
                        {
                            dataReader2.Close();
                        }
                    }
                    if (dbConnection3.State != ConnectionState.Closed)
                    {
                        dbConnection3.Close();
                    }
                }
            }
            return list.ToArray();
        }

        public ColumnSchema[] GetTableColumns(string connectionString, TableSchema table)
        {
            string commandText = string.Format("SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_OCTET_LENGTH, NUMERIC_PRECISION, NUMERIC_SCALE, CASE IS_NULLABLE WHEN 'NO' THEN 0 ELSE 1 END IS_NULLABLE, COLUMN_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = '{0}' AND TABLE_NAME = '{1}' ORDER BY ORDINAL_POSITION", table.Database.Name, table.Name);
            List<ColumnSchema> list = new List<ColumnSchema>();
            using (DbConnection dbConnection = MySQLSchemaProvider.CreateConnection(connectionString))
            {
                dbConnection.Open();
                DbCommand dbCommand = dbConnection.CreateCommand();
                dbCommand.CommandText = commandText;
                dbCommand.Connection = dbConnection;
                using (IDataReader dataReader = dbCommand.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    while (dataReader.Read())
                    {
                        string @string = dataReader.GetString(0);
                        string string2 = dataReader.GetString(1);
                        long num = (!dataReader.IsDBNull(2)) ? dataReader.GetInt64(2) : 0L;
                        byte b = (byte)((!dataReader.IsDBNull(3)) ? dataReader.GetInt32(3) : 0);
                        int num2 = (!dataReader.IsDBNull(4)) ? dataReader.GetInt32(4) : 0;
                        bool flag = !dataReader.IsDBNull(5) && dataReader.GetBoolean(5);
                        string string3 = dataReader.GetString(6);
                        int num3 = (num < 2147483647L) ? ((int)num) : 2147483647;
                        bool isUnsigned = string3.IndexOf("unsigned") > -1;
                        DbType dbType = string2.ToDbType(isUnsigned);
                        list.Add(new ColumnSchema(table, @string, dbType, string2, num3, b, num2, flag));
                    }
                    if (!dataReader.IsClosed)
                    {
                        dataReader.Close();
                    }
                }
                if (dbConnection.State != ConnectionState.Closed)
                {
                    dbConnection.Close();
                }
            }
            return list.ToArray();
        }

        public DataTable GetTableData(string connectionString, TableSchema table)
        {
            string commandText = string.Format("SELECT * FROM {0}", table.Name);
            DataSet dataSet;
            using (DbConnection dbConnection = CreateConnection(connectionString))
            {
                dbConnection.ConnectionString = connectionString;
                dbConnection.Open();
                DbCommand dbCommand = dbConnection.CreateCommand();
                dbCommand.CommandText = commandText;
                dbCommand.Connection = dbConnection;
                dataSet = this.ConvertDataReaderToDataSet(dbCommand.ExecuteReader());
                if (dbConnection.State != ConnectionState.Closed)
                {
                    dbConnection.Close();
                }
            }
            if (dataSet.Tables.Count > 0)
            {
                return dataSet.Tables[0];
            }
            return new DataTable(table.Name);
        }

        public IndexSchema[] GetTableIndexes(string connectionString, TableSchema table)
        {
            string commandText = string.Format("SELECT INDEX_NAME, COUNT(*) AS COLUMN_COUNT, MAX(NON_UNIQUE) NON_UNIQUE, CASE INDEX_NAME WHEN 'PRIMARY' THEN 1 ELSE 0 END IS_PRIMARY FROM INFORMATION_SCHEMA.STATISTICS WHERE  TABLE_SCHEMA = '{0}' AND TABLE_NAME = '{1}' GROUP BY INDEX_NAME ORDER BY INDEX_NAME;", table.Database.Name, table.Name);
            string commandText2 = string.Format("SELECT INDEX_NAME, COLUMN_NAME FROM INFORMATION_SCHEMA.STATISTICS WHERE  TABLE_SCHEMA = '{0}' AND TABLE_NAME = '{1}' ORDER BY INDEX_NAME, SEQ_IN_INDEX;", table.Database.Name, table.Name);
            DataSet dataSet;
            using (DbConnection dbConnection = CreateConnection(connectionString))
            {
                dbConnection.ConnectionString = connectionString;
                dbConnection.Open();
                using (DbCommand dbCommand = dbConnection.CreateCommand())
                {
                    dbCommand.CommandText = commandText;
                    dbCommand.Connection = dbConnection;
                    dataSet = this.ConvertDataReaderToDataSet(dbCommand.ExecuteReader());
                }
                if (dbConnection.State != ConnectionState.Closed)
                {
                    dbConnection.Close();
                }
            }
            using (DbConnection dbConnection2 = CreateConnection(connectionString))
            {
                dbConnection2.ConnectionString = connectionString;
                dbConnection2.Open();
                using (DbCommand dbCommand2 = dbConnection2.CreateCommand())
                {
                    dbCommand2.CommandText = commandText2;
                    dbCommand2.Connection = dbConnection2;
                    dataSet.Tables.Add(this.ConvertDataReaderToDataTable(dbCommand2.ExecuteReader()));
                }
                if (dbConnection2.State != ConnectionState.Closed)
                {
                    dbConnection2.Close();
                }
            }
            List<IndexSchema> list = new List<IndexSchema>();
            if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
            {
                dataSet.Relations.Add("INDEX_to_COLUMNS", dataSet.Tables[0].Columns["INDEX_NAME"], dataSet.Tables[1].Columns["INDEX_NAME"]);
                foreach (DataRow dataRow in dataSet.Tables[0].Rows)
                {
                    string text = dataRow["INDEX_NAME"].ToString();
                    bool flag = (int)dataRow["IS_PRIMARY"] == 1;
                    bool flag2 = (long)dataRow["NON_UNIQUE"] != 1L;
                    bool flag3 = flag;
                    List<DataRow> list2 = new List<DataRow>(dataRow.GetChildRows("INDEX_to_COLUMNS"));
                    List<string> list3 = new List<string>(list2.Count);
                    foreach (DataRow current in list2)
                    {
                        list3.Add(current["COLUMN_NAME"].ToString());
                    }
                    list.Add(new IndexSchema(table, text, flag, flag2, flag3, list3.ToArray()));
                }
            }
            if (list.Count > 0)
            {
                return list.ToArray();
            }
            return new List<IndexSchema>().ToArray();
        }

        public TableKeySchema[] GetTableKeys(string connectionString, TableSchema table)
        {
            List<TableKeySchema> list = new List<TableKeySchema>();
            list.AddRange(this.GetMyTableKeys(connectionString, table));
            list.AddRange(this.GetOthersTableKeys(connectionString, table));
            return list.ToArray();
        }

        public PrimaryKeySchema GetTablePrimaryKey(string connectionString, TableSchema table)
        {
            string commandText = string.Format("SELECT t1.CONSTRAINT_NAME, t1.COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE t1  INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS t2  ON t2.TABLE_SCHEMA = t1.TABLE_SCHEMA  AND t2.TABLE_NAME = t1.TABLE_NAME  AND t2.CONSTRAINT_NAME = t1.CONSTRAINT_NAME WHERE t1.TABLE_SCHEMA = '{0}' AND t1.TABLE_NAME = '{1}' AND t2.CONSTRAINT_TYPE = 'PRIMARY KEY' ORDER BY t1.ORDINAL_POSITION", table.Database.Name, table.Name);
            DataSet dataSet;
            using (DbConnection dbConnection = CreateConnection(connectionString))
            {
                dbConnection.ConnectionString = connectionString;
                dbConnection.Open();
                DbCommand dbCommand = dbConnection.CreateCommand();
                dbCommand.CommandText = commandText;
                dbCommand.Connection = dbConnection;
                dataSet = this.ConvertDataReaderToDataSet(dbCommand.ExecuteReader());
                if (dbConnection.State != ConnectionState.Closed)
                {
                    dbConnection.Close();
                }
            }
            if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
            {
                string name = dataSet.Tables[0].Rows[0]["CONSTRAINT_NAME"].ToString();
                string[] array = new string[dataSet.Tables[0].Rows.Count];
                for (int i = 0; i < dataSet.Tables[0].Rows.Count; i++)
                {
                    array[i] = dataSet.Tables[0].Rows[i]["COLUMN_NAME"].ToString();
                }
                return new PrimaryKeySchema(table, name, array);
            }
            return null;
        }

        public TableSchema[] GetTables(string connectionString, DatabaseSchema database)
        {
            string commandText = string.Format("SELECT TABLE_NAME, '' OWNER, CREATE_TIME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{0}' AND TABLE_TYPE = 'BASE TABLE' ORDER BY 1", database.Name);
            List<TableSchema> list = new List<TableSchema>();
            using (DbConnection dbConnection =CreateConnection(connectionString))
            {
                dbConnection.Open();
                DbCommand dbCommand = dbConnection.CreateCommand();
                dbCommand.CommandText = commandText;
                dbCommand.Connection = dbConnection;
                using (IDataReader dataReader = dbCommand.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    while (dataReader.Read())
                    {
                        DateTime dateCreated = (!dataReader.IsDBNull(2)) ? dataReader.GetDateTime(2) : DateTime.MinValue;
                        list.Add(new TableSchema(database, dataReader.GetString(0), dataReader.GetString(1), dateCreated));
                    }
                    if (!dataReader.IsClosed)
                    {
                        dataReader.Close();
                    }
                }
                if (dbConnection.State != ConnectionState.Closed)
                {
                    dbConnection.Close();
                }
            }
            return list.ToArray();
        }

        public ViewColumnSchema[] GetViewColumns(string connectionString, ViewSchema view)
        {
            string commandText = string.Format("SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_OCTET_LENGTH, NUMERIC_PRECISION, NUMERIC_SCALE, CASE IS_NULLABLE WHEN 'NO' THEN 0 ELSE 1 END IS_NULLABLE, COLUMN_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = '{0}' AND TABLE_NAME = '{1}'ORDER BY ORDINAL_POSITION", view.Database.Name, view.Name);
            List<ViewColumnSchema> list = new List<ViewColumnSchema>();
            using (DbConnection dbConnection = CreateConnection(connectionString))
            {
                dbConnection.Open();
                DbCommand dbCommand = dbConnection.CreateCommand();
                dbCommand.CommandText = commandText;
                dbCommand.Connection = dbConnection;
                using (IDataReader dataReader = dbCommand.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    while (dataReader.Read())
                    {
                        string @string = dataReader.GetString(0);
                        string string2 = dataReader.GetString(1);
                        long num = (!dataReader.IsDBNull(2)) ? dataReader.GetInt64(2) : 0L;
                        byte precision = (byte)((!dataReader.IsDBNull(3)) ? dataReader.GetInt32(3) : 0);
                        int scale = (!dataReader.IsDBNull(4)) ? dataReader.GetInt32(4) : 0;
                        bool allowDBNull = !dataReader.IsDBNull(5) && dataReader.GetBoolean(5);
                        string string3 = dataReader.GetString(6);
                        int size = (num < 2147483647L) ? ((int)num) : int.MaxValue;
                        bool isUnsigned = string3.IndexOf("unsigned") > -1;
                        DbType dbType = string2.ToDbType(isUnsigned);
                        list.Add(new ViewColumnSchema(view, @string, dbType, string2, size, precision, scale, allowDBNull));
                    }
                    if (!dataReader.IsClosed)
                    {
                        dataReader.Close();
                    }
                }
                if (dbConnection.State != ConnectionState.Closed)
                {
                    dbConnection.Close();
                }
            }
            return list.ToArray();
        }

        public DataTable GetViewData(string connectionString, ViewSchema view)
        {
            string commandText = string.Format("SELECT * FROM {0}", view.Name);
            DataSet dataSet;
            using (DbConnection dbConnection = CreateConnection(connectionString))
            {
                dbConnection.ConnectionString = connectionString;
                dbConnection.Open();
                DbCommand dbCommand = dbConnection.CreateCommand();
                dbCommand.CommandText = commandText;
                dbCommand.Connection = dbConnection;
                dataSet = this.ConvertDataReaderToDataSet(dbCommand.ExecuteReader());
                if (dbConnection.State != ConnectionState.Closed)
                {
                    dbConnection.Close();
                }
            }
            if (dataSet.Tables.Count > 0)
            {
                return dataSet.Tables[0];
            }
            return new DataTable(view.Name);
        }

        public ViewSchema[] GetViews(string connectionString, DatabaseSchema database)
        {
            throw new NotImplementedException();
        }

        public string GetViewText(string connectionString, ViewSchema view)
        {
            StringBuilder stringBuilder = new StringBuilder();
            string commandText = string.Format("SELECT VIEW_DEFINITION FROM INFORMATION_SCHEMA.VIEWS WHERE TABLE_SCHEMA = '{0}' AND TABLE_NAME = '{1}'", view.Database.Name, view.Name);
            using (DbConnection dbConnection = CreateConnection(connectionString))
            {
                dbConnection.Open();
                DbCommand dbCommand = dbConnection.CreateCommand();
                dbCommand.CommandText = commandText;
                dbCommand.Connection = dbConnection;
                using (IDataReader dataReader = dbCommand.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    while (dataReader.Read())
                    {
                        stringBuilder.Append(dataReader.GetString(0));
                    }
                    if (!dataReader.IsClosed)
                    {
                        dataReader.Close();
                    }
                }
                if (dbConnection.State != ConnectionState.Closed)
                {
                    dbConnection.Close();
                }
            }
            return stringBuilder.ToString();
        }

        public void SetExtendedProperties(string connectionString, SchemaObjectBase schemaObject)
        {
            throw new NotImplementedException();
        }
    }
}
