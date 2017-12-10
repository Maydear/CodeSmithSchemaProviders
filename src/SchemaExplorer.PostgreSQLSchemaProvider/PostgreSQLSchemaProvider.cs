
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;

namespace SchemaExplorer
{
    public class PostgreSQLSchemaProvider : IDbSchemaProvider, IDbConnectionStringEditor
    {
        public string Name
        {
            get
            {
                return "PostgreSQLSchemaProvider";
            }
        }

        public string Description
        {
            get
            {
                return "PostgreSQL Schema Provider";
            }
        }

        public string ConnectionString
        {
            get
            {
                return string.Empty;
            }
        }

        public bool EditorAvailable
        {
            get
            {
                return false;
            }
        }

        public bool ShowEditor(string currentConnectionString)
        {
            return false;
        }

        public TableSchema[] GetTables(string connectionString, DatabaseSchema database)
        {
            List<TableSchema> list = new List<TableSchema>();
            using (NpgsqlConnection npgsqlConnection = new NpgsqlConnection(connectionString))
            {
                npgsqlConnection.Open();
                using (NpgsqlCommand npgsqlCommand = new NpgsqlCommand("select tablename, tableowner from pg_catalog.pg_tables where schemaname = 'public' order by tablename", npgsqlConnection))
                {
                    using (NpgsqlDataReader npgsqlDataReader = npgsqlCommand.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (npgsqlDataReader.Read())
                        {
                            if (npgsqlDataReader.GetString(0).ToUpper() != "CODESMITH_EXTENDED_PROPERTIES")
                            {
                                list.Add(new TableSchema(database, npgsqlDataReader.GetString(0), npgsqlDataReader.GetString(1), DateTime.MinValue));
                            }
                        }
                        if (!npgsqlDataReader.IsClosed)
                        {
                            npgsqlDataReader.Close();
                        }
                    }
                }
                if (npgsqlConnection.State != ConnectionState.Closed)
                {
                    npgsqlConnection.Close();
                }
            }
            return list.ToArray();
        }

        public IndexSchema[] GetTableIndexes(string connectionString, TableSchema table)
        {
            List<IndexSchema> list = new List<IndexSchema>();
            using (NpgsqlConnection npgsqlConnection = new NpgsqlConnection(connectionString))
            {
                npgsqlConnection.Open();
                string text = string.Format("select * from pg_catalog.pg_indexes where schemaname='public' and tablename = '{0}'", table.Name);
                using (NpgsqlCommand npgsqlCommand = new NpgsqlCommand(text, npgsqlConnection))
                {
                    using (NpgsqlDataReader npgsqlDataReader = npgsqlCommand.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (npgsqlDataReader.Read())
                        {
                            string @string = npgsqlDataReader.GetString(2);
                            string text2 = string.Format("SELECT n.nspname AS schemaname, c.relname AS tablename, i.relname AS indexname, t.spcname AS \"tablespace\", a.attname as \"colname\", x.indisunique as \"unique\", x.indisprimary as \"primary\", x.indisclustered as \"clustered\" FROM pg_catalog.pg_index x JOIN pg_catalog.pg_class c ON c.oid = x.indrelid JOIN pg_catalog.pg_class i ON i.oid = x.indexrelid JOIN pg_catalog.pg_attribute a ON a.attrelid = i.relfilenode LEFT JOIN pg_catalog.pg_namespace n ON n.oid = c.relnamespace LEFT JOIN pg_catalog.pg_tablespace t ON t.oid = i.reltablespace WHERE c.relkind = 'r'::\"char\" AND i.relkind = 'i'::\"char\" AND n.nspname='public' AND c.relname='{0}' AND i.relname= '{1}'", table.Name, @string);
                            using (NpgsqlCommand npgsqlCommand2 = new NpgsqlCommand(text2, npgsqlConnection))
                            {
                                using (NpgsqlDataReader npgsqlDataReader2 = npgsqlCommand2.ExecuteReader())
                                {
                                    List<string> list2 = new List<string>();
                                    bool isPrimaryKey = false;
                                    bool isUnique = false;
                                    bool isClustered = false;
                                    while (npgsqlDataReader2.Read())
                                    {
                                        isPrimaryKey = (!npgsqlDataReader2.IsDBNull(6) && npgsqlDataReader2.GetBoolean(6));
                                        isUnique = (!npgsqlDataReader2.IsDBNull(5) && npgsqlDataReader2.GetBoolean(5));
                                        isClustered = (!npgsqlDataReader2.IsDBNull(7) && npgsqlDataReader2.GetBoolean(7));
                                        list2.Add(npgsqlDataReader2.IsDBNull(4) ? string.Empty : npgsqlDataReader2.GetString(4));
                                    }
                                    list.Add(new IndexSchema(table, @string, isPrimaryKey, isUnique, isClustered, list2.ToArray()));
                                    if (!npgsqlDataReader2.IsClosed)
                                    {
                                        npgsqlDataReader2.Close();
                                    }
                                }
                            }
                        }
                        if (!npgsqlDataReader.IsClosed)
                        {
                            npgsqlDataReader.Close();
                        }
                    }
                }
                if (npgsqlConnection.State != ConnectionState.Closed)
                {
                    npgsqlConnection.Close();
                }
            }
            return list.ToArray();
        }

        public ColumnSchema[] GetTableColumns(string connectionString, TableSchema table)
        {
            List<ColumnSchema> list = new List<ColumnSchema>();
            using (NpgsqlConnection npgsqlConnection = new NpgsqlConnection(connectionString))
            {
                npgsqlConnection.Open();
                string text = string.Format("select column_name, is_nullable, character_maximum_length, numeric_precision, numeric_scale, data_type, udt_name from information_schema.columns where table_schema = 'public' and table_name='{0}'", table.Name);
                using (NpgsqlCommand npgsqlCommand = new NpgsqlCommand(text, npgsqlConnection))
                {
                    DataTable dt = new DataTable();
                    using (NpgsqlDataAdapter nda = new NpgsqlDataAdapter(npgsqlCommand))
                    {
                        nda.Fill(dt);
                        nda.Dispose();
                    }

                    foreach (DataRow item in dt.Rows)
                    {
                        bool allowDBNull = item["is_nullable"] == null ? false : item["is_nullable"].ToString().Equals("YES");
                        int? numeric_precision = item.Field<int?>("numeric_precision");
                        byte precision = (byte)(numeric_precision ?? 0);
                        int? size = item.Field<int?>("character_maximum_length");
                        int? scale = item.Field<int?>("numeric_scale");
                        string name = item["column_name"] == null ? string.Empty : item["column_name"].ToString();
                        string text2 = item["data_type"] == null ? string.Empty : item["data_type"].ToString();
                        string type = item["udt_name"] == null ? string.Empty : item["udt_name"].ToString();
                        list.Add(new ColumnSchema(table, name, PostgreSQLSchemaProvider.GetDbType(type), text2, size ?? 0, precision, scale ?? 0, allowDBNull, new ExtendedProperty[]
                            {
                                new ExtendedProperty("NpgsqlDbType", PostgreSQLSchemaProvider.GetNativeDbType(text2), DbType.String)
                            }));
                    }

                    //using (NpgsqlDataReader npgsqlDataReader = npgsqlCommand.ExecuteReader(CommandBehavior.CloseConnection))
                    //{
                    //    while (npgsqlDataReader.Read())
                    //    {
                    //        bool allowDBNull = npgsqlDataReader.IsDBNull(1) || npgsqlDataReader.GetString(1) == "YES";
                    //        byte precision = (byte)(npgsqlDataReader.IsDBNull(3) ? 0 : npgsqlDataReader.GetInt32(3));
                    //        int size = npgsqlDataReader.IsDBNull(2) ? 0 : npgsqlDataReader.GetInt32(2);
                    //        int scale = npgsqlDataReader.IsDBNull(4) ? 0 : npgsqlDataReader.GetInt32(4);
                    //        string name = npgsqlDataReader.IsDBNull(0) ? string.Empty : npgsqlDataReader.GetString(0);
                    //        string text2 = npgsqlDataReader.IsDBNull(5) ? string.Empty : npgsqlDataReader.GetString(5);
                    //        string type = npgsqlDataReader.IsDBNull(6) ? string.Empty : npgsqlDataReader.GetString(6);
                    //        list.Add(new ColumnSchema(table, name, PostgreSQLSchemaProvider.GetDbType(type), text2, size, precision, scale, allowDBNull, new ExtendedProperty[]
                    //        {
                    //            new ExtendedProperty("NpgsqlDbType", PostgreSQLSchemaProvider.GetNativeDbType(text2), DbType.String)
                    //        }));
                    //    }
                    //    if (!npgsqlDataReader.IsClosed)
                    //    {
                    //        npgsqlDataReader.Close();
                    //    }
                    //}
                }
                if (npgsqlConnection.State != ConnectionState.Closed)
                {
                    npgsqlConnection.Close();
                }
            }
            return list.ToArray();
        }

        public TableKeySchema[] GetTableKeys(string connectionString, TableSchema table)
        {
            List<TableKeySchema> list = new List<TableKeySchema>();
            using (NpgsqlConnection npgsqlConnection = new NpgsqlConnection(connectionString))
            {
                npgsqlConnection.Open();
                string text = string.Format("SELECT constraint_name as constrname FROM information_schema.table_constraints WHERE table_name = '{0}' AND constraint_type = 'FOREIGN KEY' AND constraint_schema='public'", table.Name);
                using (NpgsqlCommand npgsqlCommand = new NpgsqlCommand(text, npgsqlConnection))
                {
                    string text2 = string.Format("SELECT px.conname as constrname, att.attname as colname, fore.relname as reftabname, fatt.attname as refcolname, CASE px.confupdtype WHEN 'a' THEN 'NO ACTION' WHEN 'r' THEN 'RESTRICT' WHEN 'c' THEN 'CASCADE' WHEN 'n' THEN 'SET NULL' WHEN 'd' THEN 'SET DEFAULT' END AS on_update, CASE px.confdeltype WHEN 'a' THEN 'NO ACTION' WHEN 'r' THEN 'RESTRICT' WHEN 'c' THEN 'CASCADE' WHEN 'n' THEN 'SET NULL' WHEN 'd' THEN 'SET DEFAULT' END AS on_delete, CASE px.contype WHEN 'p' THEN true WHEN 'f' THEN false END as IsPrimaryKey from pg_constraint px left join pg_class home on (home.oid = px.conrelid) left join pg_class fore on (fore.oid = px.confrelid) left join pg_attribute att on (att.attrelid = px.conrelid AND att.attnum = ANY(px.conkey)) left join pg_attribute fatt on (fatt.attrelid = px.confrelid AND fatt.attnum = ANY(px.confkey)) where (home.relname = '{0}') and px.contype = 'f' order by constrname", table.Name);
                    using (NpgsqlCommand npgsqlCommand2 = new NpgsqlCommand(text2, npgsqlConnection))
                    {
                        NpgsqlDataAdapter npgsqlDataAdapter = new NpgsqlDataAdapter();
                        DataSet dataSet = new DataSet();
                        npgsqlDataAdapter.SelectCommand = npgsqlCommand;
                        npgsqlDataAdapter.Fill(dataSet, "constraint");
                        npgsqlDataAdapter.SelectCommand = npgsqlCommand2;
                        npgsqlDataAdapter.Fill(dataSet, "keys");
                        if (dataSet.Tables[0].Rows.Count > 0)
                        {
                            dataSet.Relations.Add("Contraint_to_Keys", dataSet.Tables[0].Columns["constrname"], dataSet.Tables[1].Columns["constrname"]);
                            foreach (DataRow dataRow in dataSet.Tables[0].Rows)
                            {
                                string name = dataRow["constrname"].ToString();
                                DataRow[] childRows = dataRow.GetChildRows("Contraint_to_Keys");
                                string[] array = new string[childRows.Length];
                                string[] array2 = new string[childRows.Length];
                                string name2 = table.Name;
                                string primaryKeyTable = childRows[0]["reftabname"].ToString();
                                for (int i = 0; i < childRows.Length; i++)
                                {
                                    array2[i] = childRows[i]["colname"].ToString();
                                    array[i] = childRows[i]["refcolname"].ToString();
                                }
                                list.Add(new TableKeySchema(table.Database, name, array2, name2, array, primaryKeyTable));
                            }
                        }
                    }
                }
                string text3 = string.Format("SELECT px.conname as constrname FROM pg_constraint px left join pg_class fore on fore.oid = px.confrelid where fore.relname = '{0}'", table.Name);
                using (NpgsqlCommand npgsqlCommand3 = new NpgsqlCommand(text3, npgsqlConnection))
                {
                    string text4 = string.Format("SELECT px.conname as constrname, fatt.attname as colname, home.relname as reftabname, att.attname as refcolname, CASE px.confupdtype WHEN 'a' THEN 'NO ACTION' WHEN 'r' THEN 'RESTRICT' WHEN 'c' THEN 'CASCADE' WHEN 'n' THEN 'SET NULL' WHEN 'd' THEN 'SET DEFAULT' END AS on_update, CASE px.confdeltype WHEN 'a' THEN 'NO ACTION' WHEN 'r' THEN 'RESTRICT' WHEN 'c' THEN 'CASCADE' WHEN 'n' THEN 'SET NULL' WHEN 'd' THEN 'SET DEFAULT' END AS on_delete, CASE px.contype WHEN 'p' THEN true WHEN 'f' THEN false END as IsPrimaryKey from pg_constraint px left join pg_class home on (home.oid = px.conrelid) left join pg_class fore on (fore.oid = px.confrelid) left join pg_attribute att on (att.attrelid = px.conrelid AND att.attnum = ANY(px.conkey)) left join pg_attribute fatt on (fatt.attrelid = px.confrelid AND fatt.attnum = ANY(px.confkey)) where (fore.relname = '{0}') order by constrname", table.Name);
                    using (NpgsqlCommand npgsqlCommand4 = new NpgsqlCommand(text4, npgsqlConnection))
                    {
                        NpgsqlDataAdapter npgsqlDataAdapter2 = new NpgsqlDataAdapter();
                        DataSet dataSet2 = new DataSet();
                        npgsqlDataAdapter2.SelectCommand = npgsqlCommand3;
                        npgsqlDataAdapter2.Fill(dataSet2, "constraint");
                        npgsqlDataAdapter2.SelectCommand = npgsqlCommand4;
                        npgsqlDataAdapter2.Fill(dataSet2, "keys");
                        if (dataSet2.Tables[0].Rows.Count > 0)
                        {
                            dataSet2.Relations.Add("Contraint_to_Keys", dataSet2.Tables[0].Columns["constrname"], dataSet2.Tables[1].Columns["constrname"]);
                            foreach (DataRow dataRow2 in dataSet2.Tables[0].Rows)
                            {
                                string name3 = dataRow2["constrname"].ToString();
                                DataRow[] childRows2 = dataRow2.GetChildRows("Contraint_to_Keys");
                                string[] array3 = new string[childRows2.Length];
                                string[] array4 = new string[childRows2.Length];
                                string foreignKeyTable = childRows2[0]["reftabname"].ToString();
                                string name4 = table.Name;
                                for (int j = 0; j < childRows2.Length; j++)
                                {
                                    array4[j] = childRows2[j]["refcolname"].ToString();
                                    array3[j] = childRows2[j]["colname"].ToString();
                                }
                                list.Add(new TableKeySchema(table.Database, name3, array4, foreignKeyTable, array3, name4));
                            }
                        }
                    }
                }
            }
            return list.ToArray();
        }

        public PrimaryKeySchema GetTablePrimaryKey(string connectionString, TableSchema table)
        {
            PrimaryKeySchema result = null;
            using (NpgsqlConnection npgsqlConnection = new NpgsqlConnection(connectionString))
            {
                npgsqlConnection.Open();
                string text = string.Format("select constraint_name from information_schema.table_constraints where constraint_schema='public' and table_name='{0}' and constraint_type='PRIMARY KEY'", table.Name);

                using (NpgsqlCommand npgsqlCommand = new NpgsqlCommand(text, npgsqlConnection))
                {
                    using (NpgsqlDataReader npgsqlDataReader = npgsqlCommand.ExecuteReader())
                    {
                        while (npgsqlDataReader.Read())
                        {
                            string text2 = string.Format("select px.conname as ConstraintName, att.attname as ColumnName from pg_constraint px inner join pg_class home on (home.oid = px.conrelid) left join pg_attribute att on (att.attrelid = px.conrelid AND att.attnum = ANY(px.conkey)) where (home.relname = '{0}') and px.contype = 'p'", table.Name);
                            using (NpgsqlCommand npgsqlCommand2 = new NpgsqlCommand(text2, npgsqlConnection))
                            {
                                using (NpgsqlDataReader npgsqlDataReader2 = npgsqlCommand2.ExecuteReader())
                                {
                                    List<string> list = new List<string>();
                                    while (npgsqlDataReader2.Read())
                                    {
                                        list.Add(npgsqlDataReader2.IsDBNull(1) ? string.Empty : npgsqlDataReader2.GetString(1));
                                    }
                                    result = new PrimaryKeySchema(table, npgsqlDataReader.GetString(0), list.ToArray());
                                }
                            }
                        }
                        if (!npgsqlDataReader.IsClosed)
                        {
                            npgsqlDataReader.Close();
                        }
                    }
                }
                if (npgsqlConnection.State != ConnectionState.Closed)
                {
                    npgsqlConnection.Close();
                }
            }
            return result;
        }

        public DataTable GetTableData(string connectionString, TableSchema table)
        {
            DataTable dataTable;
            using (NpgsqlConnection npgsqlConnection = new NpgsqlConnection(connectionString))
            {
                dataTable = new DataTable(table.Name);
                string text = string.Format("SELECT * FROM {0}", table.Name);
                using (NpgsqlDataAdapter npgsqlDataAdapter = new NpgsqlDataAdapter(text, npgsqlConnection))
                {
                    npgsqlDataAdapter.Fill(dataTable);
                }
                if (npgsqlConnection.State != ConnectionState.Closed)
                {
                    npgsqlConnection.Close();
                }
            }
            return dataTable;
        }

        public ExtendedProperty[] GetExtendedProperties(string connectionString, SchemaObjectBase schemaObject)
        {
            List<ExtendedProperty> list = new List<ExtendedProperty>();
            if (schemaObject is ColumnSchema)
            {
                ColumnSchema columnSchema = schemaObject as ColumnSchema;
                string text = string.Format("select pg_get_serial_sequence(c.table_name, c.column_name) as EXTRA, COLUMN_DEFAULT, data_type \r\n                          from pg_tables t\r\n                          INNER JOIN information_schema.columns c on t.tablename = c.table_name\r\n                          WHERE schemaname = '{0}' \r\n                          AND table_name = '{1}'\r\n                          AND COLUMN_NAME = '{2}'\r\n                          order by ordinal_position", columnSchema.Table.Database.Name, columnSchema.Table.Name, columnSchema.Name);
                using (NpgsqlConnection npgsqlConnection = new NpgsqlConnection(connectionString))
                {
                    npgsqlConnection.Open();
                    using (NpgsqlCommand npgsqlCommand = new NpgsqlCommand(text, npgsqlConnection))
                    {
                        using (IDataReader dataReader = npgsqlCommand.ExecuteReader(CommandBehavior.CloseConnection))
                        {
                            while (dataReader.Read())
                            {
                                string text2 = dataReader.IsDBNull(0) ? string.Empty : dataReader.GetString(0).ToLower();
                                string value = dataReader.IsDBNull(1) ? null : dataReader.GetString(1).ToUpper();
                                string value2 = dataReader.GetString(2).ToUpper();
                                bool flag = !string.IsNullOrEmpty(text2);
                                list.Add(new ExtendedProperty("CS_IsIdentity", flag, columnSchema.DataType));
                                if (flag)
                                {
                                    list.Add(new ExtendedProperty("CS_IdentitySeed", 1, columnSchema.DataType));
                                    list.Add(new ExtendedProperty("CS_IdentityIncrement", 1, columnSchema.DataType));
                                }
                                list.Add(new ExtendedProperty("CS_Default", value, DbType.String));
                                list.Add(new ExtendedProperty("CS_SystemType", value2, DbType.String));
                                list.Add(new ExtendedProperty("CS_Sequence", text2.ToUpper(), DbType.String));
                            }
                            if (!dataReader.IsClosed)
                            {
                                dataReader.Close();
                            }
                        }
                    }
                    if (npgsqlConnection.State != ConnectionState.Closed)
                    {
                        npgsqlConnection.Close();
                    }
                }
            }
            return list.ToArray();
        }

        public void SetExtendedProperties(string connectionString, SchemaObjectBase schemaObject)
        {
            throw new NotImplementedException();
        }

        public ViewColumnSchema[] GetViewColumns(string connectionString, ViewSchema view)
        {
            List<ViewColumnSchema> list = new List<ViewColumnSchema>();
            using (NpgsqlConnection npgsqlConnection = new NpgsqlConnection(connectionString))
            {
                npgsqlConnection.Open();
                string text = string.Format("SELECT column_name, is_nullable, character_maximum_length, numeric_precision, numeric_scale, data_type, udt_name FROM information_schema.columns WHERE table_schema='public' AND table_name='{0}' ORDER BY ordinal_position", view.Name);
                using (NpgsqlCommand npgsqlCommand = new NpgsqlCommand(text, npgsqlConnection))
                {
                    using (NpgsqlDataReader npgsqlDataReader = npgsqlCommand.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (npgsqlDataReader.Read())
                        {
                            bool allowDBNull = npgsqlDataReader.IsDBNull(1) || npgsqlDataReader.GetString(1) == "YES";
                            int size = npgsqlDataReader.IsDBNull(2) ? 0 : npgsqlDataReader.GetInt32(2);
                            byte precision = (byte)(npgsqlDataReader.IsDBNull(3) ? 0 : npgsqlDataReader.GetInt32(3));
                            int scale = npgsqlDataReader.IsDBNull(4) ? 0 : npgsqlDataReader.GetInt32(4);
                            string text2 = npgsqlDataReader.IsDBNull(5) ? string.Empty : npgsqlDataReader.GetString(5);
                            string type = npgsqlDataReader.IsDBNull(6) ? string.Empty : npgsqlDataReader.GetString(6);
                            list.Add(new ViewColumnSchema(view, npgsqlDataReader.GetString(0), PostgreSQLSchemaProvider.GetDbType(type), text2, size, precision, scale, allowDBNull, new ExtendedProperty[]
							{
								new ExtendedProperty("NpgsqlDbType", PostgreSQLSchemaProvider.GetNativeDbType(text2), DbType.String)
							}));
                        }
                        if (!npgsqlDataReader.IsClosed)
                        {
                            npgsqlDataReader.Close();
                        }
                    }
                }
                if (npgsqlConnection.State != ConnectionState.Closed)
                {
                    npgsqlConnection.Close();
                }
            }
            return list.ToArray();
        }

        public DataTable GetViewData(string connectionString, ViewSchema view)
        {
            DataTable dataTable;
            using (NpgsqlConnection npgsqlConnection = new NpgsqlConnection(connectionString))
            {
                dataTable = new DataTable(view.Name);
                string text = string.Format("SELECT * FROM {0}", view.Name);
                using (NpgsqlDataAdapter npgsqlDataAdapter = new NpgsqlDataAdapter(text, npgsqlConnection))
                {
                    npgsqlDataAdapter.Fill(dataTable);
                }
                if (npgsqlConnection.State != ConnectionState.Closed)
                {
                    npgsqlConnection.Close();
                }
            }
            return dataTable;
        }

        public ViewSchema[] GetViews(string connectionString, DatabaseSchema database)
        {
            List<ViewSchema> list = new List<ViewSchema>();
            using (NpgsqlConnection npgsqlConnection = new NpgsqlConnection(connectionString))
            {
                npgsqlConnection.Open();
                using (NpgsqlCommand npgsqlCommand = new NpgsqlCommand("select viewname, viewowner from pg_catalog.pg_views where schemaname = 'public' order by viewname;", npgsqlConnection))
                {
                    using (NpgsqlDataReader npgsqlDataReader = npgsqlCommand.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (npgsqlDataReader.Read())
                        {
                            if (npgsqlDataReader.GetString(0).ToUpper() != "CODESMITH_EXTENDED_PROPERTIES")
                            {
                                list.Add(new ViewSchema(database, npgsqlDataReader.GetString(0), npgsqlDataReader.GetString(1), DateTime.MinValue));
                            }
                        }
                        if (!npgsqlDataReader.IsClosed)
                        {
                            npgsqlDataReader.Close();
                        }
                    }
                }
                if (npgsqlConnection.State != ConnectionState.Closed)
                {
                    npgsqlConnection.Close();
                }
            }
            return list.ToArray();
        }

        public string GetViewText(string connectionString, ViewSchema view)
        {
            string result;
            using (NpgsqlConnection npgsqlConnection = new NpgsqlConnection(connectionString))
            {
                npgsqlConnection.Open();
                string text = string.Format("select view_definition from information_schema.views where table_schema='public' and table_name = '{0}'", view.Name);
                using (NpgsqlCommand npgsqlCommand = new NpgsqlCommand(text, npgsqlConnection))
                {
                    result = (string)npgsqlCommand.ExecuteScalar();
                }
                if (npgsqlConnection.State != ConnectionState.Closed)
                {
                    npgsqlConnection.Close();
                }
            }
            return result;
        }

        public CommandSchema[] GetCommands(string connectionString, DatabaseSchema database)
        {
            List<CommandSchema> list = new List<CommandSchema>();
            using (NpgsqlConnection npgsqlConnection = new NpgsqlConnection(connectionString))
            {
                npgsqlConnection.Open();
                using (NpgsqlCommand npgsqlCommand = new NpgsqlCommand("SELECT routine_name, rolname, specific_name, data_type from information_schema.routines LEFT JOIN pg_catalog.pg_proc p ON p.proname = routine_name INNER JOIN pg_catalog.pg_namespace n ON n.oid = p.pronamespace INNER JOIN pg_catalog.pg_authid a on a.oid = proowner WHERE routine_schema='public' ORDER BY routine_name ", npgsqlConnection))
                {
                    using (NpgsqlDataReader npgsqlDataReader = npgsqlCommand.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (npgsqlDataReader.Read())
                        {
                            bool flag = !npgsqlDataReader.IsDBNull(3) && npgsqlDataReader.GetString(3).Trim().Equals("VOID", StringComparison.InvariantCultureIgnoreCase);
                            if (!flag || database.IncludeFunctions)
                            {
                                List<ExtendedProperty> list2 = new List<ExtendedProperty>
								{
									new ExtendedProperty("CS_Name", npgsqlDataReader.GetString(2), DbType.String, PropertyStateEnum.ReadOnly),
									new ExtendedProperty("CS_IsScalarFunction", flag, DbType.Boolean, PropertyStateEnum.ReadOnly),
									new ExtendedProperty("CS_IsProcedure", flag, DbType.Boolean, PropertyStateEnum.ReadOnly),
									new ExtendedProperty("CS_IsTrigger", npgsqlDataReader.GetString(3).Equals("TRIGGER", StringComparison.InvariantCultureIgnoreCase), DbType.Boolean, PropertyStateEnum.ReadOnly)
								};
                                list.Add(new CommandSchema(database, npgsqlDataReader.GetString(0), npgsqlDataReader.GetString(1), DateTime.MinValue, list2.ToArray()));
                            }
                        }
                        if (!npgsqlDataReader.IsClosed)
                        {
                            npgsqlDataReader.Close();
                        }
                    }
                }
                if (npgsqlConnection.State != ConnectionState.Closed)
                {
                    npgsqlConnection.Close();
                }
            }
            return list.ToArray();
        }

        public ParameterSchema[] GetCommandParameters(string connectionString, CommandSchema commandSchema)
        {
            string arg = commandSchema.ExtendedProperties["CS_Name"].Value as string;
            List<ParameterSchema> list = new List<ParameterSchema>();
            using (NpgsqlConnection npgsqlConnection = new NpgsqlConnection(connectionString))
            {
                npgsqlConnection.Open();
                string text = string.Format("select * from information_schema.parameters where specific_schema='public' and specific_name = '{0}' order by ordinal_position", arg);
                using (NpgsqlCommand npgsqlCommand = new NpgsqlCommand(text, npgsqlConnection))
                {
                    using (NpgsqlDataReader npgsqlDataReader = npgsqlCommand.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (npgsqlDataReader.Read())
                        {
                            string name = npgsqlDataReader.IsDBNull(7) ? string.Empty : npgsqlDataReader.GetString(7);
                            int size = npgsqlDataReader.IsDBNull(9) ? 0 : npgsqlDataReader.GetInt32(9);
                            int scale = npgsqlDataReader.IsDBNull(19) ? 0 : npgsqlDataReader.GetInt32(19);
                            byte precision = npgsqlDataReader.IsDBNull(17) ? (byte)0 : npgsqlDataReader.GetByte(17);
                            string @string = npgsqlDataReader.GetString(8);
                            list.Add(new ParameterSchema(commandSchema, name, PostgreSQLSchemaProvider.GetParameterDirection(npgsqlDataReader.GetString(4)), PostgreSQLSchemaProvider.GetDbType(npgsqlDataReader.GetString(8)), @string, size, precision, scale, false, new ExtendedProperty[]
							{
								new ExtendedProperty("NpgsqlDbType", PostgreSQLSchemaProvider.GetNativeDbType(@string), DbType.String)
							}));
                        }
                        if (!npgsqlDataReader.IsClosed)
                        {
                            npgsqlDataReader.Close();
                        }
                    }
                }
                if (npgsqlConnection.State != ConnectionState.Closed)
                {
                    npgsqlConnection.Close();
                }
            }
            return list.ToArray();
        }

        public CommandResultSchema[] GetCommandResultSchemas(string connectionString, CommandSchema command)
        {
            CommandResultSchema[] array = null;
            string arg = command.ExtendedProperties["CS_Name"].Value as string;
            using (NpgsqlConnection npgsqlConnection = new NpgsqlConnection(connectionString))
            {
                npgsqlConnection.Open();
                string text = string.Format("select data_type from information_schema.routines where specific_schema='public' and specific_name = '{0}'", arg);
                using (NpgsqlCommand npgsqlCommand = new NpgsqlCommand(text, npgsqlConnection))
                {
                    using (NpgsqlDataReader npgsqlDataReader = npgsqlCommand.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (npgsqlDataReader.Read())
                        {
                            string @string = npgsqlDataReader.GetString(0);
                            if (@string == "void")
                            {
                                array = new CommandResultSchema[0];
                            }
                            else if (@string == "USER-DEFINED")
                            {
                                NpgsqlCommand npgsqlCommand2 = new NpgsqlCommand(string.Format("SELECT t.typname, attname, a.typname from pg_type t JOIN pg_class on (reltype = t.oid) JOIN pg_attribute on (attrelid = pg_class.oid) JOIN pg_type a on (atttypid = a.oid) WHERE t.typname = (SELECT t.typname FROM pg_catalog.pg_proc p LEFT JOIN pg_catalog.pg_namespace n ON n.oid = p.pronamespace INNER JOIN pg_type t ON p.prorettype = t.oid WHERE n.nspname = 'public' and proname = '{0}' ORDER BY proname);", command.Name), npgsqlConnection);
                                using (NpgsqlDataReader npgsqlDataReader2 = npgsqlCommand2.ExecuteReader(CommandBehavior.CloseConnection))
                                {
                                    string text2 = null;
                                    List<CommandResultColumnSchema> list = new List<CommandResultColumnSchema>();
                                    while (npgsqlDataReader2.Read())
                                    {
                                        if (string.IsNullOrEmpty(text2))
                                        {
                                            text2 = npgsqlDataReader2.GetString(0);
                                        }
                                        string string2 = npgsqlDataReader2.GetString(2);
                                        list.Add(new CommandResultColumnSchema(command, npgsqlDataReader2.GetString(1), PostgreSQLSchemaProvider.GetDbType(string2), string2, 0, 0, 0, true, new ExtendedProperty[]
										{
											new ExtendedProperty("NpgsqlDbType", PostgreSQLSchemaProvider.GetNativeDbType(string2), DbType.String)
										}));
                                    }
                                    array = new CommandResultSchema[]
									{
										new CommandResultSchema(command, text2, list.ToArray())
									};
                                }
                            }
                        }
                        if (!npgsqlDataReader.IsClosed)
                        {
                            npgsqlDataReader.Close();
                        }
                    }
                }
                if (npgsqlConnection.State != ConnectionState.Closed)
                {
                    npgsqlConnection.Close();
                }
            }
            return array ?? new CommandResultSchema[0];
        }

        public string GetCommandText(string connectionString, CommandSchema commandSchema)
        {
            string result = string.Empty;
            string arg = commandSchema.ExtendedProperties["CS_Name"].Value as string;
            using (NpgsqlConnection npgsqlConnection = new NpgsqlConnection(connectionString))
            {
                npgsqlConnection.Open();
                string text = string.Format("select routine_definition from information_schema.routines where specific_schema='public' and specific_name = '{0}'", arg);
                using (NpgsqlCommand npgsqlCommand = new NpgsqlCommand(text, npgsqlConnection))
                {
                    using (NpgsqlDataReader npgsqlDataReader = npgsqlCommand.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        if (npgsqlDataReader.Read())
                        {
                            result = npgsqlDataReader.GetString(0);
                        }
                        if (!npgsqlDataReader.IsClosed)
                        {
                            npgsqlDataReader.Close();
                        }
                    }
                }
                if (npgsqlConnection.State != ConnectionState.Closed)
                {
                    npgsqlConnection.Close();
                }
            }
            return result;
        }

        public string GetDatabaseName(string connectionString)
        {
            Regex regex = new Regex("Database\\W*=\\W*(?<database>[^;]*)", RegexOptions.IgnoreCase);
            if (regex.IsMatch(connectionString))
            {
                return regex.Match(connectionString).Groups["database"].ToString();
            }
            return connectionString;
        }

        private static DbType GetDbType(string type)
        {
            switch (type)
            {
                case "bit":
                case "bool":
                case "boolean":
                    return DbType.Boolean;
                case "bytea":
                    return DbType.Binary;
                case "bpchar":
                case "char":
                case "character":
                case "text":
                case "varchar":
                case "character varying":
                    return DbType.String;
                case "date":
                    return DbType.Date;
                case "float4":
                case "single precision":
                case "real":
                    return DbType.Single;
                case "float8":
                case "double precision":
                    return DbType.Double;
                case "int2":
                case "smallint":
                    return DbType.Int16;
                case "int4":
                case "integer":
                    return DbType.Int32;
                case "int8":
                case "bigint":
                    return DbType.Int64;
                case "money":
                case "numeric":
                    return DbType.Decimal;
                case "time":
                case "timetz":
                case "time without time zone":
                case "time without timezone":
                case "time with time zone":
                case "time with timezone":
                    return DbType.Time;
                case "interval":
                case "timestamp":
                case "timestamptz":
                case "timestamp without time zone":
                case "timestamp without timezone":
                case "timestamp with time zone":
                case "timestamp with timezone":
                    return DbType.DateTime;
                case "uuid":
                    return DbType.Guid;
                case "box":
                case "circle":
                case "inet":
                case "line":
                case "lseg":
                case "path":
                case "point":
                case "polygon":
                case "refcursor":
                    return DbType.Object;
            }
            return DbType.Object;
        }

        private static NpgsqlDbType GetNativeDbType(string type)
        {
            string key;
            switch (key = type.ToLower())
            {
                case "array":
                    return NpgsqlDbType.Array;
                case "bit":
                    return NpgsqlDbType.Bit;
                case "box":
                    return NpgsqlDbType.Box;
                case "bool":
                case "boolean":
                    return NpgsqlDbType.Boolean;
                case "bytea":
                    return NpgsqlDbType.Bytea;
                case "char":
                    return NpgsqlDbType.Char;
                case "bpchar":
                case "character":
                case "varchar":
                case "character varying":
                    return NpgsqlDbType.Varchar;
                case "date":
                    return NpgsqlDbType.Date;
                case "float4":
                case "single precision":
                case "real":
                    return NpgsqlDbType.Real;
                case "float8":
                case "double precision":
                case "double":
                    return NpgsqlDbType.Double;
                case "int2":
                case "smallint":
                    return NpgsqlDbType.Smallint;
                case "int4":
                case "integer":
                    return NpgsqlDbType.Integer;
                case "int8":
                case "bigint":
                    return NpgsqlDbType.Bigint;
                case "money":
                    return NpgsqlDbType.Money;
                case "name":
                    return NpgsqlDbType.Name;
                case "numeric":
                    return NpgsqlDbType.Numeric;
                case "text":
                case "user-defined":
                    return NpgsqlDbType.Text;
                case "oidvector":
                    return NpgsqlDbType.Oidvector;
                case "abstime":
                    return NpgsqlDbType.Abstime;
                case "time":
                case "time without time zone":
                case "time without timezone":
                    return NpgsqlDbType.Time;
                case "timetz":
                case "time with time zone":
                case "time with timezone":
                    return NpgsqlDbType.TimeTZ;
                case "interval":
                    return NpgsqlDbType.Interval;
                case "timestamp":
                case "timestamptz":
                case "timestamp without time zone":
                case "timestamp without timezone":
                    return NpgsqlDbType.Timestamp;
                case "timestamp with time zone":
                case "timestamp with timezone":
                    return NpgsqlDbType.TimestampTZ;
                case "uuid":
                    return NpgsqlDbType.Uuid;
                case "circle":
                    return NpgsqlDbType.Circle;
                case "inet":
                    return NpgsqlDbType.Inet;
                case "line":
                    return NpgsqlDbType.Line;
                case "lseg":
                    return NpgsqlDbType.LSeg;
                case "path":
                    return NpgsqlDbType.Path;
                case "point":
                    return NpgsqlDbType.Point;
                case "polygon":
                    return NpgsqlDbType.Polygon;
                case "refcursor":
                    return NpgsqlDbType.Refcursor;
                case "xml":
                    return NpgsqlDbType.Xml;
            }
            throw new ArgumentOutOfRangeException();
        }

        private static ParameterDirection GetParameterDirection(string direction)
        {
            if (direction != null)
            {
                if (direction == "IN")
                {
                    return ParameterDirection.Input;
                }
                if (direction == "OUT")
                {
                    return ParameterDirection.Output;
                }
            }
            return ParameterDirection.InputOutput;
        }
    }
}
