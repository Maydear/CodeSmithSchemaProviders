using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CodeSmith.Core.Collections;
using Microsoft.Data.ConnectionUI;

namespace SchemaExplorer
{
    public class SqlAzureSchemaProvider : IDbSchemaProvider, INamedObject, IDbConnectionStringEditor
    {
        public string Name { get { return "SqlAzureSchemaProvider"; } }

        public string Description { get { return "Azure SQL Server Schema Provider"; } }

        public string GetDatabaseName(string connectionString)
        {
            if (this._databaseName == string.Empty)
            {
                SqlHelper sqlHelper = new SqlHelper(connectionString)
                {
                    IsSingleRow = true
                };
                using (IDataReader sqlDataReader = sqlHelper.ExecuteSqlReader(Constants.SQL_GetDatabaseName))
                {
                    while (sqlDataReader.Read())
                    {
                        this._databaseName = sqlDataReader.GetString(0);
                    }
                }
            }
            return this._databaseName;
        }


        public TableSchema[] GetTables(string connectionString, DatabaseSchema database)
        {
            SqlBuilder sqlBuilder = new SqlBuilder();
            List<TableSchema> list = new List<TableSchema>();
            List<ExtendedProperty> list2 = new List<ExtendedProperty>();
            if (SqlProductInfo == null)
            {
                SqlProductInfo = GetSqlServerVersion(connectionString);
            }

            int sqlServerMajorVersion = this.GetSqlServerMajorVersion(connectionString);

            sqlBuilder.AppendStatement(SqlProductInfo.GetTables());

            if (database.DeepLoad)
            {
                sqlBuilder.AppendStatement(SqlProductInfo.GetAllTableColumns());
                sqlBuilder.AppendStatement(SqlProductInfo.GetIndexes());
                sqlBuilder.AppendStatement(SqlProductInfo.GetKeys());
                sqlBuilder.AppendStatement(SqlProductInfo.GetColumnConstraints());
                sqlBuilder.AppendStatement(SqlProductInfo.GetExtendedData());
            }
            SqlHelper sqlHelper = new SqlHelper(connectionString);
            using (IDataReader dataReader = sqlHelper.ExecuteSqlReader(sqlBuilder))
            {
                while (dataReader.Read())
                {
                    list2.Clear();
                    list2.Add(new ExtendedProperty("CS_FileGroup", dataReader.GetString(4), DbType.AnsiString, PropertyStateEnum.ReadOnly));
                    list2.Add(new ExtendedProperty("CS_ObjectID", dataReader.GetInt32(5), DbType.Int32, PropertyStateEnum.ReadOnly));
                    TableSchema item = new TableSchema(database, dataReader.GetString(0), dataReader.GetString(1), dataReader.GetDateTime(3), list2.ToArray());
                    list.Add(item);
                }
                if (database.DeepLoad)
                {

                    if (dataReader.NextResult())
                    {

                        this.PopulateTableColumns(dataReader, list);

                    }
                    if (dataReader.NextResult())
                    {

                        this.PopulateTableIndexes(dataReader, list);
                    }
                    if (dataReader.NextResult())
                    {

                        this.PopulateTableKeys(dataReader, list);
                    }
                    if (dataReader.NextResult())
                    {

                        this.PopulateTableColumnConstraints(dataReader, list);
                    }
                    if (dataReader.NextResult())
                    {

                        this.PopulateTableExtendedData(dataReader, list);
                    }

                    this.PopulateTableDescriptions(list);
                }
            }
            return list.ToArray();
        }

        public ColumnSchema[] GetTableColumns(string connectionString, TableSchema table)
        {
            //System.IO.File.WriteAllText("c://ccc.txt", $"TableSchema.Owner:{table.Owner},TableSchema.Name:{table.Name}");

            int sqlServerMajorVersion = this.GetSqlServerMajorVersion(connectionString);
            string tableColumns = SqlProductInfo.GetTableColumns();
            SqlHelper sqlHelper = new SqlHelper(connectionString);
            sqlHelper.AddParameter("@SchemaName", SqlDbType.NVarChar, table.Owner, 255);
            sqlHelper.AddParameter("@TableName", SqlDbType.NVarChar, table.Name, 255);
            List<ColumnSchema> columnsFromReader;
            using (IDataReader dataReader = sqlHelper.ExecuteSqlReader(tableColumns))
            {
                columnsFromReader = this.GetColumnsFromReader(table, dataReader);
            }
            return columnsFromReader.ToArray();
        }

        private List<ColumnSchema> GetColumnsFromReader(TableSchema table, IDataReader reader)
        {
            return GetColumnsFromReader(table.ToDictionary(), reader, false);
        }

        private List<ColumnSchema> GetColumnsFromReader(IDictionary<string, TableSchema> tables, IDataReader reader, bool populateTable)
        {

            List<ColumnSchema> list = new List<ColumnSchema>();
            List<ExtendedProperty> list2 = new List<ExtendedProperty>();
            while (reader.Read())
            {
                string name = reader.IsDBNull("Name") ? null : reader.GetString("Name");
                string dataType = reader.IsDBNull("DataType") ? null : reader.GetString("DataType");
                string systemType = reader.IsDBNull("SystemType") ? null : reader.GetString("SystemType");
                DbType dbType = systemType.ToDbType();
                int length = reader.IsDBNull("Length") ? 0 : reader.GetInt32("Length");
                byte numericPrecision = reader.IsDBNull("NumericPrecision") ? (byte)0 : reader.GetByte("NumericPrecision");
                int numericScale = reader.IsDBNull("NumericScale") ? 0 : reader.GetInt32("NumericScale");
                bool isNullable = reader.IsDBNull("IsNullable") || reader.GetBoolean("IsNullable");
                string defaultValue = reader.IsDBNull("DefaultValue") ? null : reader.GetString("DefaultValue");
                bool identity = reader.IsDBNull("Identity") || reader.GetInt32("Identity") == 1;
                bool isRowGuid = reader.IsDBNull("IsRowGuid") || reader.GetInt32("IsRowGuid") == 1;
                bool isComputed = reader.IsDBNull("IsComputed") || reader.GetInt32("IsComputed") == 1;
                bool isDeterministic = reader.IsDBNull("IsDeterministic") || reader.GetInt32("IsDeterministic") == 1;
                string identitySeed = reader.IsDBNull("IdentitySeed") ? null : reader.GetString("IdentitySeed");
                string identityIncrement = reader.IsDBNull("IdentityIncrement") ? null : reader.GetString("IdentityIncrement");
                string computedDefinition = reader.IsDBNull("ComputedDefinition") ? null : reader.GetString("ComputedDefinition");
                string collation = reader.IsDBNull("Collation") ? null : reader.GetString("Collation");
                int objectId = reader.IsDBNull("ObjectId") ? 0 : reader.GetInt32("ObjectId");
                string schemaName = reader.IsDBNull("SchemaName") ? null : reader.GetString("SchemaName");
                string tableName = reader.IsDBNull("TableName") ? null : reader.GetString("TableName");
                list2.Clear();
                list2.Add(new ExtendedProperty("CS_IsRowGuidCol", isRowGuid, DbType.Boolean, PropertyStateEnum.ReadOnly));
                list2.Add(new ExtendedProperty("CS_IsIdentity", identity, DbType.Boolean, PropertyStateEnum.ReadOnly));
                list2.Add(new ExtendedProperty("CS_IsComputed", isComputed, DbType.Boolean, PropertyStateEnum.ReadOnly));
                list2.Add(new ExtendedProperty("CS_IsDeterministic", isDeterministic, DbType.Boolean, PropertyStateEnum.ReadOnly));
                list2.Add(new ExtendedProperty("CS_IdentitySeed", identitySeed, DbType.String, PropertyStateEnum.ReadOnly));
                list2.Add(new ExtendedProperty("CS_IdentityIncrement", identityIncrement, DbType.String, PropertyStateEnum.ReadOnly));
                list2.Add(new ExtendedProperty("CS_Default", defaultValue, DbType.String, PropertyStateEnum.ReadOnly));
                list2.Add(new ExtendedProperty("CS_ComputedDefinition", computedDefinition, DbType.String, PropertyStateEnum.ReadOnly));
                list2.Add(new ExtendedProperty("CS_Collation", collation, DbType.String, PropertyStateEnum.ReadOnly));
                list2.Add(new ExtendedProperty("CS_ObjectID", objectId, DbType.Int32, PropertyStateEnum.ReadOnly));
                list2.Add(new ExtendedProperty("CS_SystemType", systemType, DbType.String, PropertyStateEnum.ReadOnly));
                list2.Add(new ExtendedProperty("CS_UserDefinedType", dataType, DbType.String, PropertyStateEnum.ReadOnly));
                list2.Add(new ExtendedProperty("CS_UserType", dataType, DbType.String, PropertyStateEnum.ReadOnly));
                ExtendedProperty[] extendedProperties = list2.ToArray();
                TableSchema tableSchema;

                if (tables.TryGetValue(SchemaObjectBase.FormatFullName(schemaName, tableName), out tableSchema))
                {
                    ColumnSchema columnSchema = new ColumnSchema(tableSchema, name, dbType, string.IsNullOrEmpty(systemType) ? dataType : systemType, length, numericPrecision, numericScale, isNullable, extendedProperties);
                    if (populateTable)
                    {
                        tableSchema.Columns.Add(columnSchema);
                    }
                    list.Add(columnSchema);
                }
            }
            return list;
        }

        private void PopulateTableColumns(IDataReader reader, IList<TableSchema> tables)
        {
            if (tables == null || tables.Count < 1)
            {
                return;
            }
            Dictionary<string, TableSchema> dictionary = new Dictionary<string, TableSchema>();
            foreach (TableSchema tableSchema in tables)
            {

                dictionary.Add(tableSchema.FullName, tableSchema);
                tableSchema.Columns = new ColumnSchemaCollection();
            }

            this.GetColumnsFromReader(dictionary, reader, true);
        }

        private void PopulateTableIndexes(IDataReader reader, IList<TableSchema> tables)
        {
            if (tables == null || tables.Count < 1)
            {
                return;
            }
            Dictionary<string, TableSchema> dictionary = new Dictionary<string, TableSchema>();
            foreach (TableSchema tableSchema in tables)
            {
                dictionary.Add(tableSchema.FullName, tableSchema);
                tableSchema.Indexes = new IndexSchemaCollection();
            }
            this.GetIndexesFromReader(dictionary, reader, true);
        }

        private void PopulateTableKeys(IDataReader reader, IList<TableSchema> tables)
        {
            if (tables == null || tables.Count < 1)
            {
                return;
            }
            Dictionary<string, TableSchema> dictionary = new Dictionary<string, TableSchema>();
            foreach (TableSchema tableSchema in tables)
            {
                dictionary.Add(tableSchema.FullName, tableSchema);
                tableSchema.Keys = new TableKeySchemaCollection();
            }
            this.GetKeysFromReader(dictionary, reader, true);
        }

        private void PopulateTableColumnConstraints(IDataReader reader, IList<TableSchema> tables)
        {
            if (tables == null || tables.Count < 1)
            {
                return;
            }
            Dictionary<string, TableSchema> dictionary = tables.ToDictionary();
            while (reader.Read())
            {
                string tableName = reader.IsDBNull("TableName") ? null : reader.GetString("TableName");
                string schemaName = reader.IsDBNull("SchemaName") ? null : reader.GetString("SchemaName");
                string columnName = reader.IsDBNull("ColumnName") ? null : reader.GetString("ColumnName");
                string constraintName = reader.IsDBNull("ConstraintName") ? null : reader.GetString("ConstraintName");
                string constraintType = reader.IsDBNull("ConstraintType") ? null : reader.GetString("ConstraintType");
                string constraintDef = reader.IsDBNull("ConstraintDef") ? null : reader.GetString("ConstraintDef");
                TableSchema tableSchema;
                if (!StringArrayHelper.IsAnyNullOrEmpty(tableName, schemaName, columnName) && dictionary.TryGetValue(SchemaObjectBase.FormatFullName(schemaName, tableName), out tableSchema))
                {
                    ColumnSchema columnSchema = tableSchema.Columns[columnName];
                    if (columnSchema != null)
                    {
                        columnSchema.MarkLoaded();
                        columnSchema.ExtendedProperties.Add(new ExtendedProperty(string.Format("CS_Constraint_{0}_Name", constraintName), constraintName, DbType.String));
                        columnSchema.ExtendedProperties.Add(new ExtendedProperty(string.Format("CS_Constraint_{0}_Type", constraintName), constraintType, DbType.String));
                        columnSchema.ExtendedProperties.Add(new ExtendedProperty(string.Format("CS_Constraint_{0}_Definition", constraintName), constraintDef, DbType.String));
                    }
                }
            }
        }

        private void PopulateTableExtendedData(IDataReader reader, IList<TableSchema> tables)
        {
            if (tables == null || tables.Count < 1)
            {
                return;
            }
            Dictionary<string, TableSchema> dictionary = tables.ToDictionary();
            while (reader.Read())
            {
                string propertyName = reader.IsDBNull("PropertyName") ? null : reader.GetString("PropertyName");
                object propertyValue = reader["PropertyValue"];
                string propertyBaseType = reader.IsDBNull("PropertyBaseType") ? null : reader.GetString("PropertyBaseType");
                DbType dataType = string.IsNullOrEmpty(propertyBaseType) ? DbType.Object : propertyBaseType.ToDbType();
                string objectName = reader.IsDBNull("PropertyName") ? null : reader.GetString("ObjectName");
                string objectOwner = reader.IsDBNull("ObjectOwner") ? null : reader.GetString("ObjectOwner");
                string objectType = reader.IsDBNull("ObjectType") ? null : reader.GetString("ObjectType").Trim();
                string parentName = reader.IsDBNull("ParentName") ? null : reader.GetString("ParentName");
                string parentOwner = reader.IsDBNull("ParentOwner") ? null : reader.GetString("ParentOwner");
                string fieldName = reader.IsDBNull("FieldName") ? null : reader.GetString("FieldName");
                string indexName = reader.IsDBNull("IndexName") ? null : reader.GetString("IndexName");
                int type = reader.IsDBNull("Type") ? 0 : Convert.ToInt32(reader.GetByte("Type"));
                int minor = reader.IsDBNull("Minor") ? 0 : reader.GetInt32("Minor");
                if (propertyName == null || !propertyName.StartsWith("microsoft_database_tools", StringComparison.Ordinal))
                {
                    ExtendedProperty extendedProperty = new ExtendedProperty(propertyName, propertyValue, dataType);
                    if (objectType == "U")
                    {
                        TableSchema tableSchema;
                        dictionary.TryGetValue(SchemaObjectBase.FormatFullName(objectOwner, objectName), out tableSchema);
                        if (tableSchema != null)
                        {
                            if (type == 1)
                            {
                                if (minor > 0 && !string.IsNullOrEmpty(fieldName))
                                {
                                    ColumnSchema columnSchema = tableSchema.Columns[fieldName];
                                    columnSchema.MarkLoaded();
                                    columnSchema.ExtendedProperties.Add(extendedProperty);
                                }
                                else
                                {
                                    tableSchema.MarkLoaded();
                                    tableSchema.ExtendedProperties.Add(extendedProperty);
                                }
                            }
                            else if (type == 7 && !string.IsNullOrEmpty(indexName))
                            {
                                IndexSchema indexSchema = tableSchema.Indexes[indexName];
                                indexSchema.MarkLoaded();
                                indexSchema.ExtendedProperties.Add(extendedProperty);
                            }
                        }
                    }
                    else if (objectType == "PK" || objectType == "K")
                    {
                        TableSchema tableSchema;
                        dictionary.TryGetValue(SchemaObjectBase.FormatFullName(parentOwner, parentName), out tableSchema);
                        if (tableSchema != null && tableSchema.HasPrimaryKey)
                        {
                            tableSchema.PrimaryKey.MarkLoaded();
                            tableSchema.PrimaryKey.ExtendedProperties.Add(extendedProperty);
                        }
                    }
                    else if (objectType == "F")
                    {
                        TableSchema tableSchema;
                        dictionary.TryGetValue(SchemaObjectBase.FormatFullName(parentOwner, parentName), out tableSchema);
                        if (tableSchema != null)
                        {
                            TableKeySchema tableKeySchema = tableSchema.Keys[objectName];
                            tableKeySchema.MarkLoaded();
                            tableKeySchema.ExtendedProperties.Add(extendedProperty);
                        }
                    }
                }
            }
        }

        private void PopulateTableDescriptions(IEnumerable<TableSchema> tables)
        {
            foreach (TableSchema tableSchema in tables)
            {
                SyncDescription(tableSchema);
                if (tableSchema.HasPrimaryKey)
                {
                    this.SyncDescription(tableSchema.PrimaryKey);
                }
                foreach (ColumnSchema schema in tableSchema.Columns)
                {
                    this.SyncDescription(schema);
                }
                foreach (IndexSchema indexSchema in tableSchema.Indexes)
                {
                    this.SyncDescription(indexSchema);
                    foreach (MemberColumnSchema schema2 in indexSchema.MemberColumns)
                    {
                        this.SyncDescription(schema2);
                    }
                }
                foreach (TableKeySchema tableKeySchema in tableSchema.Keys)
                {
                    this.SyncDescription(tableKeySchema);
                    foreach (MemberColumnSchema schema3 in tableKeySchema.ForeignKeyMemberColumns)
                    {
                        this.SyncDescription(schema3);
                    }
                    foreach (MemberColumnSchema schema4 in tableKeySchema.PrimaryKeyMemberColumns)
                    {
                        this.SyncDescription(schema4);
                    }
                }
            }
        }

        private void SyncDescription(SchemaObjectBase schema)
        {
            schema.MarkLoaded();
            foreach (ExtendedProperty item in schema.ExtendedProperties)
            {
                Console.WriteLine($"ExtendedProperty.name:{item.Name},ExtendedProperty.value:{item.Value}");
            }
            
            if (schema is MemberColumnSchema)
            {
                ((MemberColumnSchema)schema).Column.MarkLoaded();
            }
            string text = string.Empty;
            if (schema.ExtendedProperties.Contains("MS_Description"))
            {
                text = (schema.ExtendedProperties["MS_Description"].Value as string);
            }
            schema.ExtendedProperties["CS_Description"] = new ExtendedProperty("CS_Description", text ?? string.Empty, DbType.String, PropertyStateEnum.ReadOnly);
        }

        public ViewSchema[] GetViews(string connectionString, DatabaseSchema database)
        {
            SqlBuilder sqlBuilder = new SqlBuilder();
            List<ViewSchema> list = new List<ViewSchema>();
            List<ExtendedProperty> list2 = new List<ExtendedProperty>();
            int sqlServerMajorVersion = this.GetSqlServerMajorVersion(connectionString);
            sqlBuilder.AppendStatement(SqlProductInfo.GetViews());
            if (database.DeepLoad)
            {
                sqlBuilder.AppendStatement(SqlProductInfo.GetAllViewColumns());
                sqlBuilder.AppendStatement(SqlProductInfo.GetExtendedData());
            }
            SqlHelper SqlHelper = new SqlHelper(connectionString);
            using (IDataReader dataReader = SqlHelper.ExecuteSqlReader(sqlBuilder))
            {
                while (dataReader.Read())
                {
                    list2.Clear();
                    list2.Add(new ExtendedProperty("CS_ObjectID", dataReader.GetInt32(4), DbType.Int32, PropertyStateEnum.ReadOnly));
                    ViewSchema item = new ViewSchema(database, dataReader.GetString(0), dataReader.GetString(1), dataReader.GetDateTime(3), list2.ToArray());
                    list.Add(item);
                }
                if (database.DeepLoad)
                {
                    if (dataReader.NextResult())
                    {
                        this.PopulateViewColumns(dataReader, list);
                    }
                    if (dataReader.NextResult())
                    {
                        this.PopulateViewExtendedData(dataReader, list);
                    }
                    this.PopulateViewDescriptions(list);
                }
            }
            return list.ToArray();
        }
        private void PopulateViewColumns(IDataReader reader, IList<ViewSchema> views)
        {
            if (views == null || views.Count < 1)
            {
                return;
            }
            Dictionary<string, ViewSchema> dictionary = new Dictionary<string, ViewSchema>();
            foreach (ViewSchema viewSchema in views)
            {
                dictionary.Add(viewSchema.FullName, viewSchema);
                viewSchema.Columns = new ViewColumnSchemaCollection();
            }
            this.GetViewColumnsFromReader(dictionary, reader, true);
        }

        private void PopulateViewExtendedData(IDataReader reader, IList<ViewSchema> views)
        {
            if (views == null || views.Count < 1)
            {
                return;
            }
            Dictionary<string, ViewSchema> dictionary = views.ToDictionary();
            while (reader.Read())
            {
                string @string = reader.GetString("PropertyName");
                object value = reader["PropertyValue"];
                string string2 = reader.GetString("PropertyBaseType");
                DbType dataType = string.IsNullOrEmpty(string2) ? DbType.Object : string2.ToDbType();
                string string3 = reader.GetString("ObjectName");
                string string4 = reader.GetString("ObjectOwner");
                string a = reader.GetString("ObjectType").Trim();
                string string5 = reader.GetString("FieldName");
                int @int = reader.GetInt32("Minor");
                if (@string == null || !@string.StartsWith("microsoft_database_tools", StringComparison.Ordinal))
                {
                    ExtendedProperty extendedProperty = new ExtendedProperty(@string, value, dataType);
                    if (a == "V")
                    {
                        ViewSchema viewSchema;
                        dictionary.TryGetValue(SchemaObjectBase.FormatFullName(string4, string3), out viewSchema);
                        if (@int > 0 && !string.IsNullOrEmpty(string5))
                        {
                            ViewColumnSchema viewColumnSchema = viewSchema.Columns[string5];
                            viewColumnSchema.MarkLoaded();
                            viewColumnSchema.ExtendedProperties.Add(extendedProperty);
                        }
                        else
                        {
                            viewSchema.MarkLoaded();
                            viewSchema.ExtendedProperties.Add(extendedProperty);
                        }
                    }
                }
            }
        }

        private void PopulateViewDescriptions(IEnumerable<ViewSchema> views)
        {
            foreach (ViewSchema viewSchema in views)
            {
                this.SyncDescription(viewSchema);
                foreach (ViewColumnSchema schema in viewSchema.Columns)
                {
                    this.SyncDescription(schema);
                }
            }
        }

        public ViewColumnSchema[] GetViewColumns(string connectionString, ViewSchema view)
        {
            int sqlServerMajorVersion = this.GetSqlServerMajorVersion(connectionString);
            string viewColumns = SqlProductInfo.GetViewColumns();
            SqlHelper sqlHelper = new SqlHelper(connectionString);
            sqlHelper.AddParameter("@SchemaName", SqlDbType.NVarChar, view.Owner);
            sqlHelper.AddParameter("@ViewName", SqlDbType.NVarChar, view.Name);
            List<ViewColumnSchema> viewColumnsFromReader;
            using (IDataReader dataReader = sqlHelper.ExecuteSqlReader(viewColumns))
            {
                viewColumnsFromReader = this.GetViewColumnsFromReader(view, dataReader);
            }
            return viewColumnsFromReader.ToArray();
        }

        private List<ViewColumnSchema> GetViewColumnsFromReader(ViewSchema view, IDataReader reader)
        {
            return this.GetViewColumnsFromReader(view.ToDictionary(), reader, false);
        }

        private List<ViewColumnSchema> GetViewColumnsFromReader(IDictionary<string, ViewSchema> views, IDataReader reader, bool populateView)
        {
            List<ViewColumnSchema> list = new List<ViewColumnSchema>();
            List<ExtendedProperty> list2 = new List<ExtendedProperty>();
            while (reader.Read())
            {
                string @string = reader.GetString("Name");
                string string2 = reader.GetString("DataType");
                string string3 = reader.GetString("SystemType");
                DbType dbType = string3.ToDbType();
                int @int = reader.GetInt32("Length");
                byte @byte = reader.GetByte("NumericPrecision");
                int int2 = reader.GetInt32("NumericScale");
                bool boolean = reader.GetBoolean("IsNullable");
                string string4 = reader.GetString("DefaultValue");
                bool flag = reader.GetInt32("IsComputed") == 1;
                bool flag2 = reader.IsDBNull("IsDeterministic") || reader.GetInt32("IsDeterministic") == 1;
                string string5 = reader.GetString("ComputedDefinition");
                string string6 = reader.GetString("Collation");
                int int3 = reader.GetInt32("ObjectId");
                string string7 = reader.GetString("SchemaName");
                string string8 = reader.GetString("ViewName");
                list2.Clear();
                list2.Add(new ExtendedProperty("CS_IsComputed", flag, DbType.Boolean, PropertyStateEnum.ReadOnly));
                list2.Add(new ExtendedProperty("CS_IsDeterministic", flag2, DbType.Boolean, PropertyStateEnum.ReadOnly));
                list2.Add(new ExtendedProperty("CS_Default", string4, DbType.String, PropertyStateEnum.ReadOnly));
                list2.Add(new ExtendedProperty("CS_ComputedDefinition", string5, DbType.String, PropertyStateEnum.ReadOnly));
                list2.Add(new ExtendedProperty("CS_Collation", string6, DbType.String, PropertyStateEnum.ReadOnly));
                list2.Add(new ExtendedProperty("CS_ObjectID", int3, DbType.Int32, PropertyStateEnum.ReadOnly));
                list2.Add(new ExtendedProperty("CS_SystemType", string3, DbType.String, PropertyStateEnum.ReadOnly));
                list2.Add(new ExtendedProperty("CS_UserDefinedType", string2, DbType.String, PropertyStateEnum.ReadOnly));
                list2.Add(new ExtendedProperty("CS_UserType", string2, DbType.String, PropertyStateEnum.ReadOnly));
                ViewSchema viewSchema;
                if (views.TryGetValue(SchemaObjectBase.FormatFullName(string7, string8), out viewSchema))
                {
                    ViewColumnSchema viewColumnSchema = new ViewColumnSchema(viewSchema, @string, dbType, string.IsNullOrEmpty(string3) ? string2 : string3, @int, @byte, int2, boolean, list2.ToArray());
                    if (populateView)
                    {
                        viewSchema.Columns.Add(viewColumnSchema);
                    }
                    list.Add(viewColumnSchema);
                }
            }
            return list;
        }

        public PrimaryKeySchema GetTablePrimaryKey(string connectionString, TableSchema table)
        {
            PrimaryKeySchema primaryKeySchema = null;
            foreach (IndexSchema indexSchema in table.Indexes)
            {
                if (indexSchema.IsPrimaryKey)
                {
                    primaryKeySchema = new PrimaryKeySchema(table, indexSchema.Name);
                    foreach (MemberColumnSchema memberColumnSchema in indexSchema.MemberColumns)
                    {
                        primaryKeySchema.MemberColumns.Add(memberColumnSchema);
                    }
                    if (indexSchema.ExtendedProperties.Contains("CS_FileGroup"))
                    {
                        primaryKeySchema.ExtendedProperties.Add(indexSchema.ExtendedProperties["CS_FileGroup"]);
                    }
                    if (indexSchema.ExtendedProperties.Contains("CS_OrigFillFactor"))
                    {
                        primaryKeySchema.ExtendedProperties.Add(indexSchema.ExtendedProperties["CS_OrigFillFactor"]);
                    }
                    primaryKeySchema.ExtendedProperties.Add(new ExtendedProperty("CS_IsClustered", indexSchema.IsClustered, DbType.Boolean, PropertyStateEnum.ReadOnly));
                    break;
                }
            }
            return primaryKeySchema;
        }

        public IndexSchema[] GetTableIndexes(string connectionString, TableSchema table)
        {
            SqlHelper sqlHelper = new SqlHelper(connectionString);
            sqlHelper.AddParameter("@TableName", SqlDbType.NVarChar, table.Name, 200);
            sqlHelper.AddParameter("@SchemaName", SqlDbType.NVarChar, table.Owner, 200);
            string tableIndexes = SqlProductInfo.GetTableIndexes();
            List<IndexSchema> indexesFromReader;
            using (IDataReader dataReader = sqlHelper.ExecuteSqlReader(tableIndexes))
            {
                indexesFromReader = this.GetIndexesFromReader(table, dataReader);
            }
            return indexesFromReader.ToArray();
        }

        private List<IndexSchema> GetIndexesFromReader(TableSchema table, IDataReader reader)
        {
            return this.GetIndexesFromReader(table.ToDictionary(), reader, false);
        }

        private List<IndexSchema> GetIndexesFromReader(IDictionary<string, TableSchema> tables, IDataReader reader, bool populateTable)
        {
            Dictionary<string, IndexSchema> dictionary = new Dictionary<string, IndexSchema>();
            List<ExtendedProperty> list = new List<ExtendedProperty>();
            while (reader.Read())
            {
                bool flag = false;
                string @string = reader.GetString("IndexName");
                bool boolean = reader.GetBoolean("IsPrimary");
                bool boolean2 = reader.GetBoolean("IsUnique");
                bool boolean3 = reader.GetBoolean("IsClustered");
                bool boolean4 = reader.GetBoolean("IgnoreDupKey");
                bool boolean5 = reader.GetBoolean("IsHypothetical");
                bool boolean6 = reader.GetBoolean("IsPadIndex");
                bool boolean7 = reader.GetBoolean("IsUniqueConstraint");
                bool boolean8 = reader.GetBoolean("IsIndex");
                bool flag2 = false;
                bool boolean9 = reader.GetBoolean("NoRecompute");
                bool boolean10 = reader.GetBoolean("IsFullTextKey");
                bool boolean11 = reader.GetBoolean("IsTable");
                bool boolean12 = reader.GetBoolean("IsStatistics");
                bool boolean13 = reader.GetBoolean("IsAutoStatistics");
                bool boolean14 = reader.GetBoolean("IsUniqueConstraint");
                string string2 = reader.GetString("SchemaName");
                string string3 = reader.GetString("ParentName");
                string string4 = reader.GetString("ColumnName");
                string key = IndexSchema.FormatFullName(string2, string3, @string);
                TableSchema tableSchema;
                if (!StringArrayHelper.IsAnyNullOrEmpty(@string, string3, string4) && tables.TryGetValue(SchemaObjectBase.FormatFullName(string2, string3), out tableSchema))
                {
                    IndexSchema indexSchema = null;
                    if (dictionary.ContainsKey(key))
                    {
                        indexSchema = dictionary[key];
                    }
                    if (indexSchema == null)
                    {
                        list.Clear();
                        list.Add(new ExtendedProperty("CS_FileGroup", reader.GetString("FileGroup"), DbType.AnsiString, PropertyStateEnum.ReadOnly));
                        list.Add(new ExtendedProperty("CS_IsFullTextKey", boolean10, DbType.Boolean, PropertyStateEnum.ReadOnly));
                        list.Add(new ExtendedProperty("CS_IsTable", boolean11, DbType.Boolean, PropertyStateEnum.ReadOnly));
                        list.Add(new ExtendedProperty("CS_IsStatistics", boolean12, DbType.Boolean, PropertyStateEnum.ReadOnly));
                        list.Add(new ExtendedProperty("CS_IsAutoStatistics", boolean13, DbType.Boolean, PropertyStateEnum.ReadOnly));
                        list.Add(new ExtendedProperty("CS_IsHypothetical", boolean5, DbType.Boolean, PropertyStateEnum.ReadOnly));
                        list.Add(new ExtendedProperty("CS_IgnoreDupKey", boolean4, DbType.Boolean, PropertyStateEnum.ReadOnly));
                        list.Add(new ExtendedProperty("CS_PadIndex", boolean6, DbType.Boolean, PropertyStateEnum.ReadOnly));
                        list.Add(new ExtendedProperty("CS_DRIPrimaryKey", boolean, DbType.Boolean, PropertyStateEnum.ReadOnly));
                        list.Add(new ExtendedProperty("CS_DRIUniqueKey", boolean7, DbType.Boolean, PropertyStateEnum.ReadOnly));
                        list.Add(new ExtendedProperty("CS_DRIIndex", boolean8, DbType.Boolean, PropertyStateEnum.ReadOnly));
                        list.Add(new ExtendedProperty("CS_DropExist", flag2, DbType.Boolean, PropertyStateEnum.ReadOnly));
                        list.Add(new ExtendedProperty("CS_NoRecompute", boolean9, DbType.Boolean, PropertyStateEnum.ReadOnly));
                        list.Add(new ExtendedProperty("CS_IsConstraint", boolean14, DbType.Boolean, PropertyStateEnum.ReadOnly));
                        list.Add(new ExtendedProperty("CS_OrigFillFactor", reader.GetByte("FillFactor"), DbType.Byte, PropertyStateEnum.ReadOnly));
                        indexSchema = new IndexSchema(tableSchema, @string, boolean, boolean2, boolean3, list.ToArray());
                        dictionary.Add(key, indexSchema);
                        flag = true;
                    }
                    list.Clear();
                    list.Add(new ExtendedProperty("CS_IsDescending", reader.GetBoolean("IsDescending"), DbType.Boolean, PropertyStateEnum.ReadOnly));
                    list.Add(new ExtendedProperty("CS_IsComputed", reader.GetBoolean("IsComputed"), DbType.Boolean, PropertyStateEnum.ReadOnly));
                    MemberColumnSchema memberColumnSchema = new MemberColumnSchema(tableSchema.Columns[string4], list.ToArray());
                    indexSchema.MemberColumns.Add(memberColumnSchema);
                    if (populateTable)
                    {
                        if (!tableSchema.Indexes.Contains(@string))
                        {
                            tableSchema.Indexes.Add(indexSchema);
                        }
                        if (boolean)
                        {
                            if (flag)
                            {
                                list.Clear();
                                list.Add(new ExtendedProperty("CS_FileGroup", reader.GetString("FileGroup"), DbType.AnsiString, PropertyStateEnum.ReadOnly));
                                list.Add(new ExtendedProperty("CS_IsClustered", boolean3, DbType.Boolean, PropertyStateEnum.ReadOnly));
                                list.Add(new ExtendedProperty("CS_OrigFillFactor", reader.GetByte("FillFactor"), DbType.Byte, PropertyStateEnum.ReadOnly));
                                PrimaryKeySchema primaryKey = new PrimaryKeySchema(tableSchema, @string, list.ToArray());
                                tableSchema.PrimaryKey = primaryKey;
                            }
                            tableSchema.PrimaryKey.MemberColumns.Add(memberColumnSchema);
                        }
                    }
                }
            }
            return new List<IndexSchema>(dictionary.Values);
        }

        public TableKeySchema[] GetTableKeys(string connectionString, TableSchema table)
        {
            SqlHelper sqlHelper = new SqlHelper(connectionString);
            sqlHelper.AddParameter("@TableName", SqlDbType.NVarChar, table.Name, 200);
            sqlHelper.AddParameter("@SchemaName", SqlDbType.NVarChar, table.Owner, 200);
            string sql = SqlProductInfo.GetTableKeys();
            List<TableKeySchema> keysFromReader;
            using (IDataReader dataReader = sqlHelper.ExecuteSqlReader(sql))
            {
                keysFromReader = this.GetKeysFromReader(table, dataReader);
            }
            return keysFromReader.ToArray();
        }

        private List<TableKeySchema> GetKeysFromReader(TableSchema table, IDataReader reader)
        {
            return this.GetKeysFromReader(table.ToDictionary(), reader, false);
        }

        private List<TableKeySchema> GetKeysFromReader(IDictionary<string, TableSchema> tables, IDataReader reader, bool populateTable)
        {
            Dictionary<string, TableKeySchema> dictionary = new Dictionary<string, TableKeySchema>();
            List<ExtendedProperty> list = new List<ExtendedProperty>();
            DatabaseSchema databaseSchema = (from t in tables.Values
                                             select t.Database).FirstOrDefault<DatabaseSchema>();
            if (databaseSchema == null)
            {
                return new List<TableKeySchema>();
            }
            while (reader.Read())
            {
                string @string = reader.GetString("ConstraintName");
                string string2 = reader.GetString("PrimaryTableOwner");
                string string3 = reader.GetString("PrimaryTableName");
                string string4 = reader.GetString("PrimaryColumnName");
                string string5 = reader.GetString("ForeignTableOwner");
                string string6 = reader.GetString("ForeignTableName");
                string string7 = reader.GetString("ForeignColumnName");
                bool boolean = reader.GetBoolean("IsNotForReplication");
                bool flag = reader.GetByte("DeleteReferentialAction") == 1;
                bool flag2 = reader.GetByte("UpdateReferentialAction") == 1;
                bool boolean2 = reader.GetBoolean("WithNoCheck");
                if (!StringArrayHelper.IsAnyNullOrEmpty(@string, string3, string4, string6, string7))
                {
                    string key = string.Format("{0}.{1}", TableKeySchema.FormatFullName(string2, string3, @string), SchemaObjectBase.FormatFullName(string5, string6));
                    TableSchema tableSchema;
                    if (!tables.TryGetValue(SchemaObjectBase.FormatFullName(string2, string3), out tableSchema))
                    {
                        tableSchema = databaseSchema.Tables[string2, string3];
                    }
                    TableSchema tableSchema2;
                    if (!tables.TryGetValue(SchemaObjectBase.FormatFullName(string5, string6), out tableSchema2))
                    {
                        tableSchema2 = databaseSchema.Tables[string5, string6];
                    }
                    if (tableSchema != null && tableSchema2 != null)
                    {
                        TableKeySchema tableKeySchema;
                        if (!dictionary.TryGetValue(key, out tableKeySchema))
                        {
                            list.Clear();
                            list.Add(new ExtendedProperty("CS_CascadeDelete", flag, DbType.Boolean, PropertyStateEnum.ReadOnly));
                            list.Add(new ExtendedProperty("CS_CascadeUpdate", flag2, DbType.Boolean, PropertyStateEnum.ReadOnly));
                            list.Add(new ExtendedProperty("CS_IsNotForReplication", boolean, DbType.Boolean, PropertyStateEnum.ReadOnly));
                            list.Add(new ExtendedProperty("CS_WithNoCheck", boolean2, DbType.Boolean, PropertyStateEnum.ReadOnly));
                            tableKeySchema = new TableKeySchema(@string, tableSchema2, tableSchema, list.ToArray());
                            dictionary.Add(key, tableKeySchema);
                            if (populateTable)
                            {
                                if (!this.ContainsKey(tableSchema, tableKeySchema))
                                {
                                    tableSchema.Keys.Add(tableKeySchema);
                                }
                                if (!this.ContainsKey(tableSchema2, tableKeySchema))
                                {
                                    tableSchema2.Keys.Add(tableKeySchema);
                                }
                            }
                        }
                        MemberColumnSchema memberColumnSchema = new MemberColumnSchema(tableSchema.Columns[string4]);
                        tableKeySchema.PrimaryKeyMemberColumns.Add(memberColumnSchema);
                        MemberColumnSchema memberColumnSchema2 = new MemberColumnSchema(tableSchema2.Columns[string7]);
                        tableKeySchema.ForeignKeyMemberColumns.Add(memberColumnSchema2);
                    }
                }
            }
            return new List<TableKeySchema>(dictionary.Values);
        }

        private bool ContainsKey(TableSchema tablesSchema, TableKeySchema keySchema)
        {
            return tablesSchema.Keys.Any((TableKeySchema t) => t.Name == keySchema.Name && t.PrimaryKeyTable.FullName == keySchema.PrimaryKeyTable.FullName && t.ForeignKeyTable.FullName == keySchema.ForeignKeyTable.FullName);
        }

        public DataTable GetTableData(string connectionString, TableSchema table)
        {
            string sql = string.Format("SELECT * FROM [{0}].[{1}]", table.Owner, table.Name);
            SqlHelper sqlHelper = new SqlHelper(connectionString);
            return sqlHelper.ExecuteSqlDataSet(sql).Tables[0];
        }

        public DataTable GetViewData(string connectionString, ViewSchema view)
        {
            string sql = string.Format("SELECT * FROM [{0}].[{1}]", view.Owner, view.Name);
            SqlHelper sqlHelper = new SqlHelper(connectionString);
            return sqlHelper.ExecuteSqlDataSet(sql).Tables[0];
        }

        public string GetViewText(string connectionString, ViewSchema view)
        {
            StringBuilder stringBuilder = new StringBuilder();
            SqlHelper sqlHelper = new SqlHelper(connectionString);
            sqlHelper.AddParameter("@objectname", SqlDbType.NVarChar, string.Format("[{0}].[{1}]", view.Owner, view.Name.Replace("'", "''")), 200);
            using (IDataReader sqlDataReader = sqlHelper.ExecuteSqlReader("EXEC sp_helptext @objectname"))
            {
                while (sqlDataReader.Read())
                {
                    stringBuilder.Append(sqlDataReader.GetString(0));
                }
            }
            return stringBuilder.ToString();
        }

        public CommandSchema[] GetCommands(string connectionString, DatabaseSchema database)
        {
            List<CommandSchema> list = new List<CommandSchema>();
            List<ExtendedProperty> list2 = new List<ExtendedProperty>();
            SqlBuilder sqlBuilder = new SqlBuilder();
            int sqlServerMajorVersion = this.GetSqlServerMajorVersion(connectionString);
            sqlBuilder.AppendStatement(SqlProductInfo.GetCommands());
            if (database.DeepLoad)
            {
                sqlBuilder.AppendStatement(SqlProductInfo.GetAllCommandParameters());
                sqlBuilder.AppendStatement(SqlProductInfo.GetExtendedData());
            }
            SqlHelper sqlHelper = new SqlHelper(connectionString);
            using (IDataReader dataReader = sqlHelper.ExecuteSqlReader(sqlBuilder))
            {
                while (dataReader.Read())
                {
                    list2.Clear();
                    list2.Add(new ExtendedProperty("CS_ObjectID", dataReader.GetInt32(3), DbType.Int32, PropertyStateEnum.ReadOnly));
                    string text = dataReader.GetString(4).Trim().ToUpper();
                    bool flag2 = text == "FN" || text == "FS" || text == "IF" || text == "TF";
                    if (!flag2 || database.IncludeFunctions)
                    {
                        bool value = text == "FS" || text == "PC";
                        bool value2 = text == "FN" || text == "FS";
                        bool value3 = text == "TF" || text == "IF";
                        list2.Add(ExtendedProperty.Readonly("CS_ObjectType", text));
                        list2.Add(ExtendedProperty.Readonly("CS_IsCLR", value));
                        list2.Add(ExtendedProperty.Readonly("CS_IsScalarFunction", value2));
                        list2.Add(ExtendedProperty.Readonly("CS_IsTableValuedFunction", value3));
                        list2.Add(ExtendedProperty.Readonly("CS_IsInlineTableValuedFunction", text == "IF"));
                        list2.Add(ExtendedProperty.Readonly("CS_IsMultiStatementTableValuedFunction", text == "TF"));
                        CommandSchema item = new CommandSchema(database, dataReader.GetString(0), dataReader.GetString(1), dataReader.GetDateTime(2), list2.ToArray());
                        list.Add(item);
                    }
                }
                if (database.DeepLoad)
                {
                    if (dataReader.NextResult())
                    {
                        this.PopulateCommandParameters(dataReader, list);
                    }
                    if (dataReader.NextResult())
                    {
                        this.PopulateCommandExtendedData(dataReader, list);
                    }
                    this.PopulateCommandDescriptions(list);
                }
            }
            return list.ToArray();
        }

        private void PopulateCommandParameters(IDataReader reader, IList<CommandSchema> commands)
        {
            if (commands == null || commands.Count < 1)
            {
                return;
            }
            Dictionary<string, CommandSchema> dictionary = new Dictionary<string, CommandSchema>();
            foreach (CommandSchema commandSchema in commands)
            {
                dictionary.Add(commandSchema.FullName, commandSchema);
                commandSchema.Parameters = new ParameterSchemaCollection();
                if (!commandSchema.ParseBooleanExtendedProperty("CS_IsScalarFunction"))
                {
                    commandSchema.Parameters.Add(this.GetReturnParameter(commandSchema));
                }
            }
            this.GetParametersFromReader(dictionary, reader, true);
        }

        private void PopulateCommandExtendedData(IDataReader reader, IList<CommandSchema> commands)
        {
            if (commands == null || commands.Count < 1)
            {
                return;
            }
            Dictionary<string, CommandSchema> dictionary = commands.ToDictionary();
            while (reader.Read())
            {
                string propertyName = reader.IsDBNull("PropertyName") ? null : reader.GetString("PropertyName");
                object value = reader["PropertyValue"];
                string propertyBaseType = reader.IsDBNull("PropertyBaseType") ? null : reader.GetString("PropertyBaseType");
                DbType dataType = string.IsNullOrEmpty(propertyBaseType) ? DbType.Object : propertyBaseType.ToDbType();
                string objectName = reader.IsDBNull("ObjectName") ? null : reader.GetString("ObjectName");
                string objectOwner = reader.IsDBNull("ObjectOwner") ? null : reader.GetString("ObjectOwner");
                string objectType = reader.IsDBNull("ObjectType") ? null : reader.GetString("ObjectType").Trim();
                string fieldName = reader.IsDBNull("FieldName") ? null : reader.GetString("FieldName");
                int minor = reader.IsDBNull("Minor") ? 0 : reader.GetInt32("Minor");
                if (propertyName == null || !propertyName.StartsWith("microsoft_database_tools", StringComparison.Ordinal))
                {
                    ExtendedProperty extendedProperty = new ExtendedProperty(propertyName, value, dataType);
                    if (objectType == "P")
                    {
                        CommandSchema commandSchema;
                        dictionary.TryGetValue(SchemaObjectBase.FormatFullName(objectOwner, objectName), out commandSchema);
                        if (minor == 0 && string.IsNullOrEmpty(fieldName))
                        {
                            commandSchema.MarkLoaded();
                            commandSchema.ExtendedProperties.Add(extendedProperty);
                        }
                    }
                }
            }
        }

        private void PopulateCommandDescriptions(IEnumerable<CommandSchema> commands)
        {
            foreach (CommandSchema schema in commands)
            {
                this.SyncDescription(schema);
            }
        }

        public ParameterSchema[] GetCommandParameters(string connectionString, CommandSchema command)
        {
            List<ParameterSchema> list = new List<ParameterSchema>();
            int sqlServerMajorVersion = this.GetSqlServerMajorVersion(connectionString);
            string commandParameters = SqlProductInfo.GetCommandParameters();
            SqlHelper sqlHelper = new SqlHelper(connectionString);
            sqlHelper.AddParameter("@CommandName", SqlDbType.NVarChar, command.Name);
            sqlHelper.AddParameter("@SchemaName", SqlDbType.NVarChar, command.Owner);
            if (!command.ParseBooleanExtendedProperty("CS_IsScalarFunction"))
            {
                list.Add(this.GetReturnParameter(command));
            }
            using (IDataReader dataReader = sqlHelper.ExecuteSqlReader(commandParameters))
            {
                list.AddRange(this.GetParametersFromReader(command, dataReader));
            }
            return list.ToArray();
        }

        private IEnumerable<ParameterSchema> GetParametersFromReader(CommandSchema command, IDataReader reader)
        {
            return this.GetParametersFromReader(command.ToDictionary(), reader, false);
        }

        private List<ParameterSchema> GetParametersFromReader(IDictionary<string, CommandSchema> commands, IDataReader reader, bool populateCommand)
        {
            List<ExtendedProperty> list = new List<ExtendedProperty>();
            List<ParameterSchema> list2 = new List<ParameterSchema>();
            while (reader.Read())
            {
                string parameterName = reader.IsDBNull("ParameterName") ? null : reader.GetString("ParameterName");
                bool isOutput = reader.IsDBNull("IsOutput") || reader.GetBoolean("IsOutput");
                ParameterDirection direction = isOutput ? ParameterDirection.InputOutput : ParameterDirection.Input;
                string baseTypeName = reader.IsDBNull("BaseTypeName") ? null : reader.GetString("BaseTypeName");
                DbType dataType = string.IsNullOrEmpty(baseTypeName) ? DbType.Object : baseTypeName.ToDbType();
                string typeName = reader.IsDBNull("TypeName") ? null : reader.GetString("TypeName");
                DbType dataType2 = string.IsNullOrEmpty(typeName) ? DbType.Object : typeName.ToDbType();
                int length = reader.IsDBNull("Length") ? 0 : reader.GetInt32("Length");
                byte precision = reader.IsDBNull("Precision") ? (byte)0 : reader.GetByte("Precision");
                int scale = reader.IsDBNull("Scale") ? 0 : Convert.ToInt32(reader.GetByte("Scale"));
                string defaultValue = reader.IsDBNull("DefaultValue") ? null : reader.GetString("DefaultValue");
                int parameterID = reader.IsDBNull("ParameterID") ? 0 : reader.GetInt32("ParameterID");
                string commandName = reader.IsDBNull("CommandName") ? null : reader.GetString("CommandName");
                string schemaName = reader.IsDBNull("SchemaName") ? null : reader.GetString("SchemaName");
                if (StringArrayHelper.IsAnyNullOrEmpty(parameterName, commandName))
                {
                    CommandSchema commandSchema;
                    if (isOutput && commands.TryGetValue(SchemaObjectBase.FormatFullName(schemaName, commandName), out commandSchema) && commandSchema.ParseBooleanExtendedProperty("CS_IsScalarFunction"))
                    {
                        list.Clear();
                        list.Add(new ExtendedProperty("CS_Default", "0", DbType.String, PropertyStateEnum.ReadOnly));
                        list.Add(new ExtendedProperty("CS_ParameterID", 0, DbType.Int32, PropertyStateEnum.ReadOnly));
                        ParameterSchema parameterSchema = new ParameterSchema(commandSchema, "@RETURN_VALUE", ParameterDirection.ReturnValue, dataType, baseTypeName, length, Convert.ToByte(precision), scale, true, list.ToArray());
                        if (populateCommand)
                        {
                            commandSchema.Parameters.Add(parameterSchema);
                        }
                        list2.Add(parameterSchema);
                    }
                }
                else
                {
                    list.Clear();
                    list.Add(new ExtendedProperty("CS_Default", defaultValue, DbType.String, PropertyStateEnum.ReadOnly));
                    list.Add(new ExtendedProperty("CS_ParameterID", parameterID, DbType.Int32, PropertyStateEnum.ReadOnly));
                    if (string.IsNullOrEmpty(baseTypeName) || (!string.IsNullOrEmpty(baseTypeName) && !baseTypeName.Equals(typeName, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        list.Add(new ExtendedProperty("CS_IsUserDefinedTableType", string.IsNullOrEmpty(baseTypeName), DbType.Boolean, PropertyStateEnum.ReadOnly));
                        list.Add(new ExtendedProperty("CS_UserDefinedType", typeName, dataType2, PropertyStateEnum.ReadOnly));
                    }
                    CommandSchema commandSchema2;
                    if (commands.TryGetValue(SchemaObjectBase.FormatFullName(schemaName, commandName), out commandSchema2))
                    {
                        ParameterSchema parameterSchema2 = new ParameterSchema(commandSchema2, parameterName, direction, dataType, baseTypeName, length, precision, scale, true, list.ToArray());
                        if (populateCommand)
                        {
                            commandSchema2.Parameters.Add(parameterSchema2);
                        }
                        list2.Add(parameterSchema2);
                    }
                }
            }
            return list2;
        }

        private ParameterSchema GetReturnParameter(CommandSchema command)
        {
            List<ExtendedProperty> list = new List<ExtendedProperty>();
            list.Add(new ExtendedProperty("CS_Default", "0", DbType.String, PropertyStateEnum.ReadOnly));
            list.Add(new ExtendedProperty("CS_ParameterID", 0, DbType.Int32, PropertyStateEnum.ReadOnly));
            return new ParameterSchema(command, "@RETURN_VALUE", ParameterDirection.ReturnValue, DbType.Int32, "int", 4, Convert.ToByte(10), 0, false, list.ToArray());
        }

        public string GetCommandText(string connectionString, CommandSchema command)
        {
            if (!command.ParseBooleanExtendedProperty("CS_IsCLR"))
            {
                StringBuilder stringBuilder = new StringBuilder();
                SqlHelper sqlHelper = new SqlHelper(connectionString);
                sqlHelper.AddParameter("@objectname", SqlDbType.NVarChar, string.Format("[{0}].[{1}]", command.Owner, command.Name.Replace("'", "''")), 200);
                using (IDataReader sqlDataReader = sqlHelper.ExecuteSqlReader("EXEC sp_helptext @objectname"))
                {
                    while (sqlDataReader.Read())
                    {
                        stringBuilder.Append(sqlDataReader.GetString(0));
                    }
                }
                return stringBuilder.ToString();
            }
            return string.Empty;
        }

        public CommandResultSchema[] GetCommandResultSchemas(string connectionString, CommandSchema command)
        {
            SqlHelper sqlHelper = new SqlHelper(connectionString);
            IDataReader sqlDataReader = null;
            bool flag = false;
            bool flag2 = true;
            try
            {
                sqlDataReader = sqlHelper.ExecuteSqlReader(command.CommandResultSchemaFMTQuery());
            }
            catch (SqlException)
            {
                flag = true;
            }
            if (flag)
            {
                try
                {
                    flag2 = false;
                    sqlDataReader = sqlHelper.ExecuteSqlReader(command.CommandResultSchemaTransactionalQuery());
                }
                catch (SqlException ex)
                {
                    Trace.WriteLine(string.Format("An error occurred while getting CommandResultSchemas for the command: '{0}'\r\nException:{1}", command.Name, ex.Message));
                    return new CommandResultSchema[0];
                }
            }
            List<CommandResultSchema> list = new List<CommandResultSchema>();
            try
            {
                list = SqlAzureSchemaProvider.CommandResultSchemaHelper(command, sqlDataReader);
            }
            catch (SqlException)
            {
                if (!flag)
                {
                    flag = true;
                }
            }
            finally
            {
                if (sqlDataReader != null && !sqlDataReader.IsClosed)
                {
                    sqlDataReader.Close();
                }
            }
            if (flag && flag2)
            {
                try
                {
                    sqlDataReader = sqlHelper.ExecuteSqlReader(command.CommandResultSchemaTransactionalQuery());
                    list = SqlAzureSchemaProvider.CommandResultSchemaHelper(command, sqlDataReader);
                }
                catch (SqlException ex2)
                {
                    Trace.WriteLine(string.Format("An error occurred while getting CommandResultSchemas for the command: '{0}'\r\nException:{1}", command.Name, ex2.Message));
                }
                finally
                {
                    if (sqlDataReader != null && !sqlDataReader.IsClosed)
                    {
                        sqlDataReader.Close();
                    }
                }
            }
            return list.ToArray();
        }

        private static List<CommandResultSchema> CommandResultSchemaHelper(CommandSchema command, IDataReader reader)
        {
            List<CommandResultSchema> list = new List<CommandResultSchema>();
            if (reader.IsClosed)
            {
                return list;
            }
            int num = 0;
            do
            {
                DataTable schemaTable = reader.GetSchemaTable();
                if (schemaTable != null)
                {
                    num++;
                    List<CommandResultColumnSchema> list2 = new List<CommandResultColumnSchema>();
                    for (int i = 0; i < schemaTable.Rows.Count; i++)
                    {
                        string text = schemaTable.Rows[i].IsNull("ColumnName") ? ("Column" + (i + 1)) : ((string)schemaTable.Rows[i]["ColumnName"]);
                        if (string.IsNullOrEmpty(text))
                        {
                            text = "Column" + (i + 1);
                        }
                        SqlDbType sqlDbType = schemaTable.Rows[i].IsNull("ProviderType") ? SqlDbType.Variant : ((SqlDbType)schemaTable.Rows[i]["ProviderType"]);
                        string nativeType = sqlDbType.ToNativeType();
                        DbType dbType = nativeType.ToDbType();
                        int size = schemaTable.Rows[i].IsNull("ColumnSize") ? 0 : ((int)schemaTable.Rows[i]["ColumnSize"]);
                        byte precision = schemaTable.Rows[i].IsNull("NumericPrecision") ? (byte)0 : Convert.ToByte((short)schemaTable.Rows[i]["NumericPrecision"]);
                        int scale = schemaTable.Rows[i].IsNull("NumericScale") ? 0 : Convert.ToInt32((short)schemaTable.Rows[i]["NumericScale"]);
                        bool allowDBNull = !schemaTable.Rows[i].IsNull("AllowDBNull") && (bool)schemaTable.Rows[i]["AllowDBNull"];
                        CommandResultColumnSchema item = new CommandResultColumnSchema(command, text, dbType, nativeType, size, precision, scale, allowDBNull);
                        list2.Add(item);
                    }
                    CommandResultSchema item2 = new CommandResultSchema(command, num.ToString(), list2.ToArray());
                    list.Add(item2);
                    schemaTable.Dispose();
                }
            }
            while (reader.NextResult());
            reader.Close();
            return list;
        }

        public ExtendedProperty[] GetExtendedProperties(string connectionString, SchemaObjectBase schemaObject)
        {
            if (schemaObject is DatabaseSchema)
            {
                ExtendedPropertyCollection databaseExtendedProperties = this.GetDatabaseExtendedProperties(connectionString);
                return databaseExtendedProperties.ToArray();
            }
            if (schemaObject is TableSchema)
            {
                TableSchema tableSchema = (TableSchema)schemaObject;
                return this.GetExtendedProperties(connectionString, tableSchema.Owner, "Table", tableSchema.Name);
            }
            if (schemaObject is ColumnSchema)
            {
                ColumnSchema column = (ColumnSchema)schemaObject;
                ExtendedPropertyCollection columnExtendedProperties = this.GetColumnExtendedProperties(connectionString, column);
                return columnExtendedProperties.ToArray();
            }
            if (schemaObject is ViewSchema)
            {
                ViewSchema viewSchema = (ViewSchema)schemaObject;
                return this.GetExtendedProperties(connectionString, viewSchema.Owner, "View", viewSchema.Name);
            }
            if (schemaObject is ViewColumnSchema)
            {
                ViewColumnSchema viewColumnSchema = (ViewColumnSchema)schemaObject;
                return this.GetExtendedProperties(connectionString, viewColumnSchema.View.Owner, "View", viewColumnSchema.View.Name, "Column", viewColumnSchema.Name);
            }
            if (schemaObject is IndexSchema)
            {
                IndexSchema indexSchema = (IndexSchema)schemaObject;
                return this.GetExtendedProperties(connectionString, indexSchema.Table.Owner, "Table", indexSchema.Table.Name, "Index", indexSchema.Name);
            }
            if (schemaObject is CommandSchema)
            {
                CommandSchema commandSchema = (CommandSchema)schemaObject;
                return this.GetExtendedProperties(connectionString, commandSchema.Owner, "Procedure", commandSchema.Name);
            }
            if (schemaObject is ParameterSchema)
            {
                ParameterSchema parameter = (ParameterSchema)schemaObject;
                ExtendedPropertyCollection parameterExtendedData = this.GetParameterExtendedData(connectionString, parameter);
                return parameterExtendedData.ToArray();
            }
            if (schemaObject is PrimaryKeySchema)
            {
                PrimaryKeySchema primaryKeySchema = (PrimaryKeySchema)schemaObject;
                return this.GetExtendedProperties(connectionString, primaryKeySchema.Table.Owner, "Table", primaryKeySchema.Table.Name, "Constraint", primaryKeySchema.Name);
            }
            if (schemaObject is TableKeySchema)
            {
                TableKeySchema tableKeySchema = (TableKeySchema)schemaObject;
                return this.GetExtendedProperties(connectionString, tableKeySchema.ForeignKeyTable.Owner, "Table", tableKeySchema.ForeignKeyTable.Name, "Constraint", tableKeySchema.Name);
            }
            return new ExtendedProperty[0];
        }

        private ExtendedPropertyCollection GetParameterExtendedData(string connectionString, ParameterSchema parameter)
        {
            ExtendedPropertyCollection extendedPropertyCollection = new ExtendedPropertyCollection();
            extendedPropertyCollection.AddRange(this.GetExtendedProperties(connectionString, parameter.Command.Owner, "Procedure", parameter.Command.Name, "Parameter", parameter.Name));
            this.GetParameterExtendedData(parameter, extendedPropertyCollection);
            return extendedPropertyCollection;
        }

        private void GetParameterExtendedData(ParameterSchema parameter, ExtendedPropertyCollection properties)
        {
            if (parameter.Direction == ParameterDirection.ReturnValue)
            {
                return;
            }
            int hashCode = parameter.Command.CommandText.GetHashCode();
            if (SqlAzureSchemaProvider._parameterSchemaExtendedDataCache.ContainsKey(hashCode) && SqlAzureSchemaProvider._parameterSchemaExtendedDataCache[hashCode] == null)
            {
                return;
            }
            if (!SqlAzureSchemaProvider._parameterSchemaExtendedDataCache.ContainsKey(hashCode))
            {
                Match match = SqlAzureSchemaProvider._sprocHeaderRegex.Match(parameter.Command.CommandText);
                if (!match.Success)
                {
                    SqlAzureSchemaProvider._parameterSchemaExtendedDataCache[hashCode] = null;
                    return;
                }
                List<ParameterSchemaExtendedData> list = new List<ParameterSchemaExtendedData>();
                foreach (object obj in SqlAzureSchemaProvider._sprocParamRegex.Matches(parameter.Command.CommandText, match.Index + match.Length))
                {
                    Match match2 = (Match)obj;
                    string defaultValue = match2.Groups["pdefault"].Value.Replace("''", "'");
                    string comment = match2.Groups["pcomment"].Value.TrimEnd(new char[]
                    {
                        ' ',
                        '\r',
                        '\n'
                    });
                    list.Add(new ParameterSchemaExtendedData(match2.Groups["pname"].Value, defaultValue, comment));
                }
                SqlAzureSchemaProvider._parameterSchemaExtendedDataCache[hashCode] = list;
            }
            ParameterSchemaExtendedData parameterSchemaExtendedData = SqlAzureSchemaProvider._parameterSchemaExtendedDataCache[hashCode].FirstOrDefault((ParameterSchemaExtendedData r) => parameter.Name.Equals(r.Name, StringComparison.OrdinalIgnoreCase));
            if (parameterSchemaExtendedData == null)
            {
                return;
            }
            properties.Add(ExtendedProperty.Readonly("CS_Default", parameterSchemaExtendedData.DefaultValue));
            properties.Add(ExtendedProperty.Readonly("CS_Comment", parameterSchemaExtendedData.Comment));
        }

        private ExtendedPropertyCollection GetColumnExtendedProperties(string connectionString, ColumnSchema column)
        {
            ExtendedPropertyCollection extendedPropertyCollection = new ExtendedPropertyCollection();
            extendedPropertyCollection.AddRange(this.GetExtendedProperties(connectionString, column.Table.Owner, "Table", column.Table.Name, "Column", column.Name));
            if (!SqlProductInfo.IsSql2000)
            {
                return extendedPropertyCollection;
            }
            int sqlServerMajorVersion = this.GetSqlServerMajorVersion(connectionString);
            string text = SqlProductInfo.GetColumnConstraints();
            text += SqlProductInfo.GetColumnConstraintsWhere();
            SqlHelper sqlHelper = new SqlHelper(connectionString);
            sqlHelper.AddParameter("@SchemaName", SqlDbType.NVarChar, column.Table.Owner);
            sqlHelper.AddParameter("@TableName", SqlDbType.NVarChar, column.Table.Name);
            sqlHelper.AddParameter("@ColumnName", SqlDbType.NVarChar, column.Name);
            using (IDataReader dataReader = sqlHelper.ExecuteSqlReader(text))
            {
                while (dataReader.Read())
                {
                    string @string = dataReader.GetString("ConstraintName");
                    string string2 = dataReader.GetString("ConstraintType");
                    string string3 = dataReader.GetString("ConstraintDef");
                    extendedPropertyCollection.Add(new ExtendedProperty(string.Format("CS_Constraint_{0}_Name", @string), @string, DbType.String));
                    extendedPropertyCollection.Add(new ExtendedProperty(string.Format("CS_Constraint_{0}_Type", @string), string2, DbType.String));
                    extendedPropertyCollection.Add(new ExtendedProperty(string.Format("CS_Constraint_{0}_Definition", @string), string3, DbType.String));
                }
            }
            return extendedPropertyCollection;
        }

        private ExtendedPropertyCollection GetDatabaseExtendedProperties(string connectionString)
        {
            ExtendedPropertyCollection extendedPropertyCollection = new ExtendedPropertyCollection();
            extendedPropertyCollection.AddRange(this.GetExtendedProperties(connectionString, "", "", "", "", "", ""));
            extendedPropertyCollection.Add(new ExtendedProperty("CS_DatabaseVersion", this.GetSqlServerVersion(connectionString), DbType.String));
            extendedPropertyCollection.Add(new ExtendedProperty("CS_DatabaseMajorVersion", this.GetSqlServerMajorVersion(connectionString), DbType.Int32));
            return extendedPropertyCollection;
        }

        private ExtendedProperty[] GetExtendedProperties(string connectionString, string owner, string level1type, string level1name)
        {
            return this.GetExtendedProperties(connectionString, this.GetLevelZero(connectionString), owner, level1type, level1name, "", "");
        }

        private ExtendedProperty[] GetExtendedProperties(string connectionString, string owner, string level1type, string level1name, string level2type, string level2name)
        {
            return this.GetExtendedProperties(connectionString, this.GetLevelZero(connectionString), owner, level1type, level1name, level2type, level2name);
        }
        private ExtendedProperty[] GetExtendedProperties(string connectionString, string level0type, string level0name, string level1type, string level1name, string level2type, string level2name)
        {
            Dictionary<string, ExtendedProperty> dictionary = new Dictionary<string, ExtendedProperty>(StringComparer.OrdinalIgnoreCase);
            string value = string.Empty;
            SqlHelper sqlHelper = new SqlHelper(connectionString);
            sqlHelper.AddParameter("@level0type", SqlDbType.VarChar, level0type, true);
            sqlHelper.AddParameter("@level0name", SqlDbType.VarChar, level0name, true);
            sqlHelper.AddParameter("@level1type", SqlDbType.VarChar, level1type, true);
            sqlHelper.AddParameter("@level1name", SqlDbType.VarChar, level1name, true);
            sqlHelper.AddParameter("@level2type", SqlDbType.VarChar, level2type, true);
            sqlHelper.AddParameter("@level2name", SqlDbType.VarChar, level2name, true);
            using (IDataReader dataReader = sqlHelper.ExecuteSqlReader(SqlProductInfo.GetExtendedProperties()))
            {
                while (dataReader.Read())
                {
                    ExtendedProperty extendedProperty = new ExtendedProperty(dataReader.GetString(0), dataReader.GetValue(1), dataReader.IsDBNull(2) ? DbType.Object : dataReader.GetString(2).ToDbType());
                    dictionary[extendedProperty.Name] = extendedProperty;
                    if (extendedProperty.Name == "MS_Description" && extendedProperty.Value != null)
                    {
                        value = extendedProperty.Value.ToString();
                    }
                }
            }
            if (string.IsNullOrEmpty(value) && dictionary.ContainsKey("CS_Description"))
            {
                value = ((dictionary["CS_Description"].Value as string) ?? string.Empty);
            }
            dictionary["CS_Description"] = ExtendedProperty.Readonly("CS_Description", value);
            return dictionary.Values.ToArray<ExtendedProperty>();
        }

        public void SetExtendedProperties(string connectionString, SchemaObjectBase schemaObject)
        {
            if (schemaObject is DatabaseSchema)
            {
                this.SetExtendedProperties(schemaObject, connectionString, "", "", "", "", "", "");
                return;
            }
            if (schemaObject is TableSchema)
            {
                this.SetExtendedProperties(schemaObject, connectionString, this.GetLevelZero(connectionString), ((TableSchema)schemaObject).Owner, "Table", schemaObject.Name, "", "");
                return;
            }
            if (schemaObject is ColumnSchema)
            {
                this.SetExtendedProperties(schemaObject, connectionString, this.GetLevelZero(connectionString), ((ColumnSchema)schemaObject).Table.Owner, "Table", ((ColumnSchema)schemaObject).Table.Name, "Column", schemaObject.Name);
                return;
            }
            if (schemaObject is ViewSchema)
            {
                this.SetExtendedProperties(schemaObject, connectionString, this.GetLevelZero(connectionString), ((ViewSchema)schemaObject).Owner, "View", schemaObject.Name, "", "");
                return;
            }
            if (schemaObject is ViewColumnSchema)
            {
                this.SetExtendedProperties(schemaObject, connectionString, this.GetLevelZero(connectionString), ((ViewColumnSchema)schemaObject).View.Owner, "View", ((ViewColumnSchema)schemaObject).View.Name, "Column", schemaObject.Name);
                return;
            }
            if (schemaObject is IndexSchema)
            {
                this.SetExtendedProperties(schemaObject, connectionString, this.GetLevelZero(connectionString), ((IndexSchema)schemaObject).Table.Owner, "Table", ((IndexSchema)schemaObject).Table.Name, "Index", schemaObject.Name);
                return;
            }
            if (schemaObject is CommandSchema)
            {
                this.SetExtendedProperties(schemaObject, connectionString, this.GetLevelZero(connectionString), ((CommandSchema)schemaObject).Owner, "Procedure", schemaObject.Name, "", "");
                return;
            }
            if (schemaObject is ParameterSchema)
            {
                this.SetExtendedProperties(schemaObject, connectionString, this.GetLevelZero(connectionString), ((ParameterSchema)schemaObject).Command.Owner, "Procedure", ((ParameterSchema)schemaObject).Command.Name, "Parameter", schemaObject.Name);
                return;
            }
            if (schemaObject is PrimaryKeySchema)
            {
                this.SetExtendedProperties(schemaObject, connectionString, this.GetLevelZero(connectionString), ((PrimaryKeySchema)schemaObject).Table.Owner, "Table", ((PrimaryKeySchema)schemaObject).Table.Name, "Constraint", schemaObject.Name);
                return;
            }
            if (schemaObject is TableKeySchema)
            {
                this.SetExtendedProperties(schemaObject, connectionString, this.GetLevelZero(connectionString), ((TableKeySchema)schemaObject).ForeignKeyTable.Owner, "Table", ((TableKeySchema)schemaObject).ForeignKeyTable.Name, "Constraint", schemaObject.Name);
            }
        }

        private void SetExtendedProperties(SchemaObjectBase schemaObject, string connectionString, string level0type, string level0name, string level1type, string level1name, string level2type, string level2name)
        {
            SqlHelper sqlHelper = null;
            try
            {
                bool flag = false;
                sqlHelper = new SqlHelper(connectionString);
                foreach (ExtendedProperty extendedProperty in schemaObject.ExtendedProperties)
                {
                    string text = extendedProperty.Name;
                    if (text == "CS_Description")
                    {
                        text = "MS_Description";
                        if (!schemaObject.ExtendedProperties.Contains(text))
                        {
                            extendedProperty.PropertyState = PropertyStateEnum.New;
                        }
                    }
                    if (extendedProperty.PropertyState == PropertyStateEnum.New)
                    {
                        sqlHelper.Reset();
                        sqlHelper.AddParameter("@name", SqlDbType.VarChar, text, true);
                        sqlHelper.AddParameter("@value", extendedProperty.DataType, extendedProperty.Value, true);
                        sqlHelper.AddParameter("@level0type", SqlDbType.VarChar, level0type, true);
                        sqlHelper.AddParameter("@level0name", SqlDbType.VarChar, level0name, true);
                        sqlHelper.AddParameter("@level1type", SqlDbType.VarChar, level1type, true);
                        sqlHelper.AddParameter("@level1name", SqlDbType.VarChar, level1name, true);
                        sqlHelper.AddParameter("@level2type", SqlDbType.VarChar, level2type, true);
                        sqlHelper.AddParameter("@level2name", SqlDbType.VarChar, level2name, true);
                        sqlHelper.ExecuteSP("sp_addextendedproperty");
                        flag = true;
                    }
                    else if (extendedProperty.PropertyState == PropertyStateEnum.Dirty)
                    {
                        sqlHelper.Reset();
                        sqlHelper.AddParameter("@name", SqlDbType.VarChar, text, true);
                        sqlHelper.AddParameter("@value", extendedProperty.DataType, extendedProperty.Value, true);
                        sqlHelper.AddParameter("@level0type", SqlDbType.VarChar, level0type, true);
                        sqlHelper.AddParameter("@level0name", SqlDbType.VarChar, level0name, true);
                        sqlHelper.AddParameter("@level1type", SqlDbType.VarChar, level1type, true);
                        sqlHelper.AddParameter("@level1name", SqlDbType.VarChar, level1name, true);
                        sqlHelper.AddParameter("@level2type", SqlDbType.VarChar, level2type, true);
                        sqlHelper.AddParameter("@level2name", SqlDbType.VarChar, level2name, true);
                        sqlHelper.ExecuteSP("sp_updateextendedproperty");
                        flag = true;
                    }
                    else if (extendedProperty.PropertyState == PropertyStateEnum.Deleted)
                    {
                        sqlHelper.Reset();
                        sqlHelper.AddParameter("@name", SqlDbType.VarChar, text, true);
                        sqlHelper.AddParameter("@level0type", SqlDbType.VarChar, level0type, true);
                        sqlHelper.AddParameter("@level0name", SqlDbType.VarChar, level0name, true);
                        sqlHelper.AddParameter("@level1type", SqlDbType.VarChar, level1type, true);
                        sqlHelper.AddParameter("@level1name", SqlDbType.VarChar, level1name, true);
                        sqlHelper.AddParameter("@level2type", SqlDbType.VarChar, level2type, true);
                        sqlHelper.AddParameter("@level2name", SqlDbType.VarChar, level2name, true);
                        sqlHelper.ExecuteSP("sp_dropextendedproperty");
                        flag = true;
                    }
                }
                if (flag)
                {
                    schemaObject.Refresh();
                }
            }
            finally
            {
                if (sqlHelper != null)
                {
                    sqlHelper.Dispose();
                }
            }
        }

        private string GetLevelZero(string connectionString)
        {
            if (SqlProductInfo == null)
            {
                SqlProductInfo = GetSqlServerVersion(connectionString);
            }
            if (!SqlProductInfo.IsSql2005OrNewer)
            {
                return "USER";
            }
            return "SCHEMA";
        }

        private SqlProductInfo GetSqlServerVersion(string connectionString)
        {
            if (SqlProductInfo == null)
            {
                object @lock = SqlAzureSchemaProvider._lock;
                lock (@lock)
                {
                    if (SqlProductInfo != null)
                    {
                        return SqlProductInfo;
                    }
                    try
                    {
                        SqlHelper sqlHelper = new SqlHelper(connectionString);
                        DataSet ds = sqlHelper.ExecuteSqlDataSet(Constants.SQL_GetSqlServerVersion);
                        DataTable dataTable = ds.Tables[0];
                        SqlProductInfo = new SqlProductInfo((string)dataTable.Rows[0]["ProductVersion"], (string)dataTable.Rows[0]["Edition"]);
                    }
                    catch
                    {
                        throw new ArgumentOutOfRangeException();
                    }
                }
            }
            return SqlProductInfo;
        }

        private int GetSqlServerMajorVersion(string connectionString)
        {
            if (SqlProductInfo == null)
            {
                SqlProductInfo = GetSqlServerVersion(connectionString);
            }
            return SqlProductInfo.MajorVersion;
        }

        public bool ShowEditor(string currentConnectionString)
        {
            bool flag = false;
            if (!string.IsNullOrEmpty(currentConnectionString))
            {
                DbConnectionStringBuilder dbConnectionStringBuilder = new DbConnectionStringBuilder();
                try
                {
                    dbConnectionStringBuilder.ConnectionString = currentConnectionString;
                }
                catch (ArgumentException)
                {
                }
                if (dbConnectionStringBuilder.ContainsKey("User Instance"))
                {
                    bool.TryParse(dbConnectionStringBuilder["User Instance"] as string, out flag);
                }
            }
            DataConnectionDialog dataConnectionDialog = new DataConnectionDialog();
            dataConnectionDialog.DataSources.Add(Microsoft.Data.ConnectionUI.DataSource.SqlDataSource);
            dataConnectionDialog.DataSources.Add(Microsoft.Data.ConnectionUI.DataSource.SqlFileDataSource);
            dataConnectionDialog.SelectedDataSource = (flag ? Microsoft.Data.ConnectionUI.DataSource.SqlFileDataSource : Microsoft.Data.ConnectionUI.DataSource.SqlDataSource);
            if (!flag)
            {
                dataConnectionDialog.SelectedDataProvider = DataProvider.SqlDataProvider;
            }
            try
            {
                dataConnectionDialog.ConnectionString = currentConnectionString;
            }
            catch
            {
            }
            DialogResult dialogResult = DataConnectionDialog.Show(dataConnectionDialog);
            if (dialogResult == DialogResult.OK)
            {
                this.ConnectionString = dataConnectionDialog.ConnectionString;
            }
            return dialogResult == DialogResult.OK;
        }

        public string ConnectionString { get; private set; }

        public bool EditorAvailable { get { return true; } }

        private string _databaseName = string.Empty;

        internal SqlProductInfo SqlProductInfo { get; set; }

        private static readonly object _lock = new object();

        private const string _whitespaceCommentRegex = "\r\n            (?:\r\n                \\s\r\n              |\r\n                --[^\\r\\n]*[\\r\\n]+\r\n              |\r\n                /\\*(?:[^\\*]|\\*[^/])*\\*/\r\n            )\r\n            ";


        private static readonly Regex _sprocHeaderRegex = new Regex(string.Format("\r\n                ^[\\ \\t]*CREATE{0}+PROC(?:EDURE)?{0}+(?:\\[?(?<owner>[a-zA-Z]\\w*)\\]?\\s*\\.\\s*)?(?:\\[?(?<name>[a-zA-Z]\\w*)\\]?)(?:{0}*\\({0}*|{0}+)\r\n            ", "\r\n            (?:\r\n                \\s\r\n              |\r\n                --[^\\r\\n]*[\\r\\n]+\r\n              |\r\n                /\\*(?:[^\\*]|\\*[^/])*\\*/\r\n            )\r\n            "), RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant);


        private static readonly Regex _sprocParamRegex = new Regex(string.Format("\r\n                \\G(?:{0}*,)?\r\n                {0}*(?<pname>@[a-zA-Z@][\\w@]*)\r\n                (?:{0}+AS)?\r\n                {0}+(?:(?<ptypeowner>[a-zA-Z]\\w*)\\s+\\.\\s+)?\r\n                (?:(?<ptype>[a-zA-Z]\\w*(?:\\s*\\(\\s*\\w+(?:\\s*,\\s*\\d+)?\\s*\\))?))\r\n                (?:{0}*={0}*(?:(?<pdefault>[A-Z0-9]+)|'(?<pdefault>(?:[^']|'')*)'))?\r\n                (?:{0}+(?<poutput>OUT(?:PUT)?))?\r\n                (?:{0}+(?<preadonly>READONLY))?\r\n                (?:{0}*,)?\r\n                (?:\\s*--(?<pcomment>[^\\r\\n]*))?\r\n            ", "\r\n            (?:\r\n                \\s\r\n              |\r\n                --[^\\r\\n]*[\\r\\n]+\r\n              |\r\n                /\\*(?:[^\\*]|\\*[^/])*\\*/\r\n            )\r\n            "), RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant);


        private static readonly Dictionary<int, List<ParameterSchemaExtendedData>> _parameterSchemaExtendedDataCache = new Dictionary<int, List<ParameterSchemaExtendedData>>();

    }
}
