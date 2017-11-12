using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchemaExplorer
{
    internal static class Constants
    {
        internal const string SQL_GetAllCommandParameters= @"SELECT
	[t].[name] AS [CommandName],
	[stbl].[name] AS [SchemaName],
	[clmns].[name] AS [ParameterName],
	CAST([clmns].[colid] AS int) AS [ParameterID],
	CAST([clmns].[xprec] AS tinyint) AS [Precision],
	[usrt].[name] AS [TypeName],
	ISNULL([baset].[name], N'') AS [BaseTypeName],
	CAST(CASE WHEN [baset].[name] IN (N'char', N'varchar', N'binary', N'varbinary', N'nchar', N'nvarchar') THEN [clmns].[prec] ELSE [clmns].[length] END AS int) AS [Length],
	CAST([clmns].[xscale] AS tinyint) AS [Scale],
	CAST(CASE [clmns].[isoutparam] WHEN 1 THEN [clmns].[isoutparam] WHEN 0 THEN CASE COALESCE([clmns].[name], '') WHEN '' THEN 1 ELSE 0 END END AS bit) AS [IsOutput],
	[defaults].[text] AS [DefaultValue]	
FROM 
	[dbo].[sysobjects] AS [t] WITH (NOLOCK)
	INNER JOIN [dbo].[sysusers] AS [stbl] WITH (NOLOCK) ON [stbl].[uid] = [t].[uid]
	INNER JOIN [dbo].[syscolumns] AS [clmns] WITH (NOLOCK) ON [clmns].[id] = [t].[id]
	LEFT JOIN [dbo].[systypes] AS [usrt] WITH (NOLOCK) ON [usrt].[xusertype] = [clmns].[xusertype]
	LEFT JOIN [dbo].[sysusers] AS [sclmns] WITH (NOLOCK) ON [sclmns].[uid] = [usrt].[uid]
	LEFT JOIN [dbo].[systypes] AS [baset] WITH (NOLOCK) ON [baset].[xusertype] = [clmns].[xtype] and [baset].[xusertype] = [baset].[xtype]
	LEFT JOIN [dbo].[syscomments] AS [defaults] WITH (NOLOCK) ON [defaults].[id] = [clmns].[cdefault]
WHERE [t].[type] IN ('P', 'RF', 'PC', 'FN', 'FS', 'IF', 'TF') 
ORDER BY [t].[name], [clmns].[colorder]";

        internal const string SQL_GetAllCommandParameters2005= @"SELECT [t].[name] AS [CommandName], 
	[sc].[name] AS [SchemaName], 
	[c].[name] AS [ParameterName], 
	[c].[parameter_id] AS [ParameterID], 
	[c].[precision] AS [Precision],
	[types].[name] AS [TypeName],
	[basetypes].[name] AS [BaseTypeName],
	CASE WHEN [c].[max_length] >= 0
		AND [types].[name] IN (N'nchar', N'nvarchar') THEN [c].[max_length]/2 
		ELSE [c].[max_length] 
	END AS [Length],
	[c].[scale] AS [Scale],
	[is_output] as [IsOutput],
	[default_value] as [DefaultValue]
FROM [sys].[parameters] [c] WITH (NOLOCK)
	INNER JOIN [sys].[objects] [t] WITH (NOLOCK) ON [c].[object_id] = [t].[object_id]
	LEFT JOIN [sys].[schemas] [sc] WITH (NOLOCK) ON [t].[schema_id] = [sc].[schema_id]
	LEFT JOIN [sys].[types] [basetypes] WITH (NOLOCK) ON [c].[system_type_id] = [basetypes].[system_type_id] AND [basetypes].[system_type_id] = [basetypes].[user_type_id]
	LEFT JOIN [sys].[types] [types] WITH (NOLOCK) ON [c].[user_type_id] = [types].[user_type_id]
	LEFT JOIN [sys].[schemas] [st] WITH (NOLOCK) ON [st].[schema_id] = [types].[schema_id]
WHERE [t].[type] in ('P', 'RF', 'PC', 'FN', 'FS', 'IF', 'TF')
ORDER BY [t].[name], [c].[parameter_id]";

        internal const string SQL_GetAllTableColumns= @"SELECT
	clmns.[name] AS [Name],
	usrt.[name] AS [DataType],
	ISNULL(baset.[name], N'') AS [SystemType],
	CAST(CASE WHEN baset.[name] IN (N'char', N'varchar', N'binary', N'varbinary', N'nchar', N'nvarchar') THEN clmns.prec ELSE clmns.length END AS int) AS [Length],
	CAST(clmns.xprec AS tinyint) AS [NumericPrecision],
	CAST(clmns.xscale AS int) AS [NumericScale],
	CAST(clmns.isnullable AS bit) AS [IsNullable],
	defaults.text AS [DefaultValue],
	CAST(COLUMNPROPERTY(clmns.id, clmns.[name], N'IsIdentity') AS int) AS [Identity],
	CAST(COLUMNPROPERTY(clmns.id, clmns.[name], N'IsRowGuidCol') AS int) AS IsRowGuid,
	CAST(COLUMNPROPERTY(clmns.id, clmns.[name], N'IsComputed') AS int) AS IsComputed,
	CAST(COLUMNPROPERTY(clmns.id, clmns.[name], N'IsDeterministic') AS int) AS IsDeterministic,
	CAST(CASE COLUMNPROPERTY(clmns.id, clmns.[name], N'IsIdentity') WHEN 1 THEN IDENT_SEED(QUOTENAME(stbl.[name]) + '.' + QUOTENAME(tbl.[name])) ELSE 0 END AS NVARCHAR(40)) AS [IdentitySeed],
	CAST(CASE COLUMNPROPERTY(clmns.id, clmns.[name], N'IsIdentity') WHEN 1 THEN IDENT_INCR(QUOTENAME(stbl.[name]) + '.' + QUOTENAME(tbl.[name])) ELSE 0 END AS NVARCHAR(40)) AS [IdentityIncrement],
	cdef.[text] AS ComputedDefinition,
	clmns.[collation] AS Collation,
	CAST(clmns.colid AS int) AS ObjectId,
	stbl.[name] AS [SchemaName],
	tbl.[name] AS [TableName]
FROM dbo.sysobjects AS tbl WITH (NOLOCK)
	INNER JOIN dbo.sysusers AS stbl WITH (NOLOCK) ON stbl.[uid] = tbl.[uid]
	INNER JOIN dbo.syscolumns AS clmns WITH (NOLOCK) ON clmns.id=tbl.id
	LEFT JOIN dbo.systypes AS usrt WITH (NOLOCK) ON usrt.xusertype = clmns.xusertype
	LEFT JOIN dbo.sysusers AS sclmns WITH (NOLOCK) ON sclmns.uid = usrt.uid
	LEFT JOIN dbo.systypes AS baset WITH (NOLOCK) ON baset.xusertype = clmns.xtype and baset.xusertype = baset.xtype
	LEFT JOIN dbo.syscomments AS defaults WITH (NOLOCK) ON defaults.id = clmns.cdefault
	LEFT JOIN dbo.syscomments AS cdef WITH (NOLOCK) ON cdef.id = clmns.id AND cdef.number = clmns.colid
WHERE (tbl.[type] = 'U')
ORDER BY tbl.[name], clmns.colorder";

        internal const string SQL_GetAllTableColumns2005= @"SELECT
	clmns.[name] AS [Name],
	usrt.[name] AS [DataType],
	ISNULL(baset.[name], N'') AS [SystemType],
	CAST(CASE WHEN baset.[name] IN (N'char', N'varchar', N'binary', N'varbinary', N'nchar', N'nvarchar') THEN clmns.prec ELSE clmns.length END AS int) AS [Length],
	CAST(clmns.xprec AS tinyint) AS [NumericPrecision],
	CAST(clmns.xscale AS int) AS [NumericScale],
	CAST(clmns.isnullable AS bit) AS [IsNullable],
	object_definition(defaults.default_object_id) AS [DefaultValue],
	CAST(COLUMNPROPERTY(clmns.id, clmns.[name], N'IsIdentity') AS int) AS [Identity],
	CAST(COLUMNPROPERTY(clmns.id, clmns.[name], N'IsRowGuidCol') AS int) AS IsRowGuid,
	CAST(COLUMNPROPERTY(clmns.id, clmns.[name], N'IsComputed') AS int) AS IsComputed,
	CAST(COLUMNPROPERTY(clmns.id, clmns.[name], N'IsDeterministic') AS int) AS IsDeterministic,
	CAST(CASE COLUMNPROPERTY(clmns.id, clmns.[name], N'IsIdentity') WHEN 1 THEN IDENT_SEED(QUOTENAME(SCHEMA_NAME(tbl.uid)) + '.' + QUOTENAME(tbl.[name])) ELSE 0 END AS nvarchar(40)) AS [IdentitySeed],
	CAST(CASE COLUMNPROPERTY(clmns.id, clmns.[name], N'IsIdentity') WHEN 1 THEN IDENT_INCR(QUOTENAME(SCHEMA_NAME(tbl.uid)) + '.' + QUOTENAME(tbl.[name])) ELSE 0 END AS nvarchar(40)) AS [IdentityIncrement],
	cdef.definition AS ComputedDefinition,
	clmns.[collation] AS Collation,
	CAST(clmns.colid AS int) AS ObjectId,
	SCHEMA_NAME(tbl.uid) AS [SchemaName],
	tbl.[name] AS [TableName]
FROM dbo.sysobjects AS tbl WITH (NOLOCK)
	INNER JOIN dbo.syscolumns AS clmns WITH (NOLOCK) ON clmns.id=tbl.id
	LEFT JOIN dbo.systypes AS usrt WITH (NOLOCK) ON usrt.xusertype = clmns.xusertype
	LEFT JOIN dbo.sysusers AS sclmns WITH (NOLOCK) ON sclmns.uid = usrt.uid
	LEFT JOIN dbo.systypes AS baset WITH (NOLOCK) ON baset.xusertype = clmns.xtype and baset.xusertype = baset.xtype
	LEFT JOIN sys.columns AS defaults WITH (NOLOCK) ON defaults.name = clmns.name and defaults.object_id = clmns.id
	LEFT JOIN sys.computed_columns AS cdef WITH (NOLOCK) ON cdef.object_id = clmns.id AND cdef.column_id = clmns.colid
WHERE (tbl.[type] = 'U')
ORDER BY tbl.[name], clmns.colorder";

        internal const string SQL_GetAllViewColumns= @"SELECT
	clmns.[name] AS [Name],
	usrt.[name] AS [DataType],
	ISNULL(baset.[name], N'') AS [SystemType],
	CAST(CASE WHEN baset.[name] IN (N'char', N'varchar', N'binary', N'varbinary', N'nchar', N'nvarchar') THEN clmns.prec ELSE clmns.length END AS int) AS [Length],
	CAST(clmns.xprec AS tinyint) AS [NumericPrecision],
	CAST(clmns.xscale AS int) AS [NumericScale],
	CAST(clmns.isnullable AS bit) AS [IsNullable],
	defaults.text AS [DefaultValue],
	CAST(COLUMNPROPERTY(clmns.id, clmns.[name], N'IsComputed') AS int) AS IsComputed,
	CAST(COLUMNPROPERTY(clmns.id, clmns.[name], N'IsDeterministic') AS int) AS IsDeterministic,
	cdef.[text] AS ComputedDefinition,
	clmns.[collation] AS Collation,
	CAST(clmns.colid AS int) AS ObjectId,
	stbl.[name] AS [SchemaName],
	tbl.[name] AS [ViewName]
FROM dbo.sysobjects AS tbl WITH (NOLOCK)
	INNER JOIN dbo.sysusers AS stbl WITH (NOLOCK) ON stbl.[uid] = tbl.[uid]
	INNER JOIN dbo.syscolumns AS clmns WITH (NOLOCK) ON clmns.id=tbl.id
	LEFT JOIN dbo.systypes AS usrt WITH (NOLOCK) ON usrt.xusertype = clmns.xusertype
	LEFT JOIN dbo.sysusers AS sclmns WITH (NOLOCK) ON sclmns.uid = usrt.uid
	LEFT JOIN dbo.systypes AS baset WITH (NOLOCK) ON baset.xusertype = clmns.xtype and baset.xusertype = baset.xtype
	LEFT JOIN dbo.syscomments AS defaults WITH (NOLOCK) ON defaults.id = clmns.cdefault
	LEFT JOIN dbo.syscomments AS cdef WITH (NOLOCK) ON cdef.id = clmns.id AND cdef.number = clmns.colid
WHERE (tbl.[type] = 'V')
ORDER BY tbl.[name], clmns.colorder";

        internal const string SQL_GetAllViewColumns2005= @"SELECT
	clmns.[name] AS [Name],
	usrt.[name] AS [DataType],
	ISNULL(baset.[name], N'') AS [SystemType],
	CAST(CASE WHEN baset.[name] IN (N'char', N'varchar', N'binary', N'varbinary', N'nchar', N'nvarchar') THEN clmns.prec ELSE clmns.length END AS int) AS [Length],
	CAST(clmns.xprec AS tinyint) AS [NumericPrecision],
	CAST(clmns.xscale AS int) AS [NumericScale],
	CAST(clmns.isnullable AS bit) AS [IsNullable],
	object_definition(defaults.default_object_id) AS [DefaultValue],
	CAST(COLUMNPROPERTY(clmns.id, clmns.[name], N'IsComputed') AS int) AS IsComputed,
	CAST(COLUMNPROPERTY(clmns.id, clmns.[name], N'IsDeterministic') AS int) AS IsDeterministic,
	cdef.definition AS ComputedDefinition,
	clmns.[collation] AS Collation,
	CAST(clmns.colid AS int) AS ObjectId,
	SCHEMA_NAME(tbl.uid) AS [SchemaName],
	tbl.[name] AS [ViewName]
FROM dbo.sysobjects AS tbl WITH (NOLOCK)
	INNER JOIN dbo.syscolumns AS clmns WITH (NOLOCK) ON clmns.id=tbl.id
	LEFT JOIN dbo.systypes AS usrt WITH (NOLOCK) ON usrt.xusertype = clmns.xusertype
	LEFT JOIN dbo.sysusers AS sclmns WITH (NOLOCK) ON sclmns.uid = usrt.uid
	LEFT JOIN dbo.systypes AS baset WITH (NOLOCK) ON baset.xusertype = clmns.xtype and baset.xusertype = baset.xtype
	LEFT JOIN sys.columns AS defaults WITH (NOLOCK) ON defaults.name = clmns.name and defaults.object_id = clmns.id
	LEFT JOIN sys.computed_columns AS cdef WITH (NOLOCK) ON cdef.object_id = clmns.id AND cdef.column_id = clmns.colid
WHERE (tbl.[type] = 'V')
ORDER BY tbl.[name], clmns.colorder";

        internal const string SQL_GetColumnConstraints= @"SELECT
  [tbl].[name] AS [TableName],
  [stbl].[name] AS [SchemaName], 
  [clmns].[name] AS [ColumnName],
  OBJECT_NAME([const].[constid]) AS ConstraintName,
  CASE
    WHEN [const].[status] & 5 = 5 THEN 'DEFAULT'
    WHEN [const].[status] & 4 = 4 THEN 'CHECK'
    ELSE ''
  END AS ConstraintType,
  [constdef].[text] AS ConstraintDef
FROM
  dbo.sysobjects AS tbl WITH (NOLOCK)
  INNER JOIN dbo.sysusers AS stbl WITH (NOLOCK) ON [stbl].[uid] = [tbl].[uid]
  INNER JOIN dbo.syscolumns AS clmns WITH (NOLOCK) ON [clmns].[id] = [tbl].[id]
  INNER JOIN dbo.sysconstraints const WITH (NOLOCK) ON [clmns].[id] = [const].[id] and [clmns].[colid] = [const].[colid]
  LEFT OUTER JOIN dbo.syscomments constdef WITH (NOLOCK) ON [const].[constid] = [constdef].[id]
WHERE ([const].[status] & 4 = 4 OR [const].[status] & 5 = 5)";

        internal const string SQL_GetColumnConstraints2005 = @"WITH constraints AS (
    SELECT parent_object_id, parent_column_id, Name, definition, 'DEFAULT' AS [ConstraintType] FROM sys.default_constraints
    UNION ALL
    SELECT parent_object_id, parent_column_id, Name, definition, 'CHECK' AS [ConstraintType]  FROM sys.check_constraints)

SELECT 
    t.Name AS [TableName],
    SCHEMA_NAME(t.schema_id) AS [SchemaName], 
    c.Name AS [ColumnName],
    dc.Name AS ConstraintName,
    dc.ConstraintType,
    dc.definition AS ConstraintDef
FROM sys.tables t
INNER JOIN constraints dc ON t.object_id = dc.parent_object_id
INNER JOIN sys.columns c ON dc.parent_object_id = c.object_id AND c.column_id = dc.parent_column_id";

        internal const string SQL_GetCommandParameters= @"SELECT
	[t].[name] AS [CommandName],
	[stbl].[name] AS [SchemaName],
	[clmns].[name] AS [ParameterName],
	CAST([clmns].[colid] AS int) AS [ParameterID],
	CAST([clmns].[xprec] AS tinyint) AS [Precision],
	[usrt].[name] AS [TypeName],
	ISNULL([baset].[name], N'') AS [BaseTypeName],
	CAST(CASE WHEN [baset].[name] IN (N'char', N'varchar', N'binary', N'varbinary', N'nchar', N'nvarchar') THEN [clmns].[prec] ELSE [clmns].[length] END AS int) AS [Length],
	CAST([clmns].[xscale] AS tinyint) AS [Scale],
	CAST(CASE [clmns].[isoutparam] WHEN 1 THEN [clmns].[isoutparam] WHEN 0 THEN CASE COALESCE([clmns].[name], '') WHEN '' THEN 1 ELSE 0 END END AS bit) AS [IsOutput],
	[defaults].[text] AS [DefaultValue]	
FROM [dbo].[sysobjects] AS [t] WITH (NOLOCK)
	INNER JOIN [dbo].[sysusers] AS [stbl] WITH (NOLOCK) ON [stbl].[uid] = [t].[uid]
	INNER JOIN [dbo].[syscolumns] AS [clmns] WITH (NOLOCK) ON [clmns].[id] = [t].[id]
	LEFT JOIN [dbo].[systypes] AS [usrt] WITH (NOLOCK) ON [usrt].[xusertype] = [clmns].[xusertype]
	LEFT JOIN [dbo].[sysusers] AS [sclmns] WITH (NOLOCK) ON [sclmns].[uid] = [usrt].[uid]
	LEFT JOIN [dbo].[systypes] AS [baset] WITH (NOLOCK) ON [baset].[xusertype] = [clmns].[xtype] and [baset].[xusertype] = [baset].[xtype]
	LEFT JOIN [dbo].[syscomments] AS [defaults] WITH (NOLOCK) ON [defaults].[id] = [clmns].[cdefault]
WHERE [t].[type] IN ('P', 'RF', 'PC', 'FN', 'FS', 'IF', 'TF') 
	AND [t].[name] = @CommandName
	AND [stbl].[name]= @SchemaName
ORDER BY [t].[name], [clmns].[colorder]";

        internal const string SQL_GetCommandParameters2005= @"SELECT [t].[name] AS [CommandName], 
	[sc].[name] AS [SchemaName], 
	[c].[name] AS [ParameterName], 
	[c].[parameter_id] AS [ParameterID], 
	[c].[precision] AS [Precision],
	[types].[name] AS [TypeName],
	[basetypes].[name] AS [BaseTypeName],
	CASE WHEN [c].[max_length] >= 0
		AND [types].[name] IN (N'nchar', N'nvarchar') THEN [c].[max_length]/2 
		ELSE [c].[max_length] 
	END AS [Length],
	[c].[scale] AS [Scale],
	[is_output] as [IsOutput],
	[default_value] as [DefaultValue]
FROM [sys].[parameters] [c] WITH (NOLOCK)
	INNER JOIN [sys].[objects] [t] WITH (NOLOCK) ON [c].[object_id] = [t].[object_id]
	LEFT JOIN [sys].[schemas] [sc] WITH (NOLOCK) ON [t].[schema_id] = [sc].[schema_id]
	LEFT JOIN [sys].[types] [basetypes] WITH (NOLOCK) ON [c].[system_type_id] = [basetypes].[system_type_id] AND [basetypes].[system_type_id] = [basetypes].[user_type_id]
	LEFT JOIN [sys].[types] [types] WITH (NOLOCK) ON [c].[user_type_id] = [types].[user_type_id]
	LEFT JOIN [sys].[schemas] [st] WITH (NOLOCK) ON [st].[schema_id] = [types].[schema_id]
WHERE [t].[type] in ('P', 'RF', 'PC', 'FN', 'FS', 'IF', 'TF')
	AND [t].[name] = @CommandName
	AND [sc].[name]= @SchemaName
ORDER BY [t].[name], [c].[parameter_id]";

        internal const string SQL_GetCommands= @"SELECT
  object_name(id) AS OBJECT_NAME,
  user_name(uid) AS USER_NAME,
  crdate AS DATE_CREATED,
  id as OBJECT_ID,
  type as COMMAND_TYPE
FROM
  sysobjects
WHERE
  type IN (
		N'P', -- SQL Stored Procedure
		N'PC', --Assembly (CLR) stored-procedure
		N'FN', --SQL scalar function
		N'FS', --Assembly (CLR) scalar-function
		N'IF', --SQL inline table-valued function
		N'TF' --SQL table-valued-function
	  )
  --AND permissions(id) & 32 <> 0 
  AND ObjectProperty(id, N'IsMSShipped') = 0
ORDER BY object_name(id)";

        internal const string SQL_GetCommands2005= @"SELECT
  object_name(id) AS OBJECT_NAME,
  schema_name(uid) AS USER_NAME,
  crdate AS DATE_CREATED,
  id as OBJECT_ID,
  type as COMMAND_TYPE
FROM
  sysobjects
WHERE
	type IN (
		N'P', -- SQL Stored Procedure
		N'PC', --Assembly (CLR) stored-procedure
		N'FN', --SQL scalar function
		N'FS', --Assembly (CLR) scalar-function
		N'IF', --SQL inline table-valued function
		N'TF' --SQL table-valued-function
	  )
	  --AND permissions(id) & 32 <> 0 
	  AND ObjectProperty(id, N'IsMSShipped') = 0
	  AND NOT EXISTS (SELECT * FROM sys.extended_properties WHERE major_id = id AND name = 'microsoft_database_tools_support' AND value = 1)
ORDER BY object_name(id)";

        internal const string SQL_GetCommandsAzure= @"SELECT
  object_name(id) AS OBJECT_NAME,
  schema_name(uid) AS USER_NAME,
  crdate AS DATE_CREATED,
  id as OBJECT_ID,
  type as COMMAND_TYPE
FROM
  sysobjects
WHERE
	type IN (
		N'P', -- SQL Stored Procedure
		N'PC', --Assembly (CLR) stored-procedure
		N'FN', --SQL scalar function
		N'FS', --Assembly (CLR) scalar-function
		N'IF', --SQL inline table-valued function
		N'TF' --SQL table-valued-function
	  )
	  --AND permissions(id) & 32 <> 0 
	  AND ObjectProperty(id, N'IsMSShipped') = 0
ORDER BY object_name(id)";

        internal const string SQL_GetExtendedData2005= @"SELECT  [sp].[major_id] AS [ID], 
        [so].[name] AS [ObjectName], 
        SCHEMA_NAME([so].[schema_id]) AS [ObjectOwner],  
        [so].[type] AS [ObjectType], 
        [sp].[minor_id] AS [Minor],  
        [sp].[name] AS [PropertyName], 
        [sp].[value] AS [PropertyValue],
        SQL_VARIANT_PROPERTY([sp].[value],'BaseType') AS [PropertyBaseType],			    
		CASE [sp].[class] WHEN 4 THEN USER_NAME([sp].[major_id]) END AS [UserName],
        CASE [sp].[class]
	        WHEN 2 THEN [spar].[name]
	        ELSE [sc].[name]
        END AS [FieldName],
        [si].[name] AS [IndexName],
        [sop].[name] AS [ParentName],
        SCHEMA_NAME([sop].[schema_id]) AS [ParentOwner],
        [sop].[type] AS [ParentType],
        [sp].[class] AS [Type]        
FROM [sys].[extended_properties] AS [sp] WITH (NOLOCK) 
	LEFT JOIN [sys].[objects] AS [so] WITH (NOLOCK) ON [so].[object_id] = [sp].[major_id]
	LEFT JOIN [sys].[columns] AS [sc] WITH (NOLOCK) ON [sc].[object_id] = [sp].[major_id] AND [sc].[column_id] = [sp].[minor_id]
	LEFT JOIN [sys].[parameters] AS [spar] WITH (NOLOCK) ON [spar].[object_id] = [sp].[major_id] AND [spar].[parameter_id] = [sp].[minor_id]
	LEFT JOIN [sysindexes] [si] WITH (NOLOCK) ON [si].[id] = [sp].[major_id] AND [si].[indid] = [sp].[minor_id]
	LEFT JOIN [sys].[objects] [sop] WITH (NOLOCK) ON [so].[parent_object_id] = [sop].[object_id]
";

        internal const string SQL_GetExtenedData= @"SELECT [sp].[id] AS [id], 
	[so].[name] AS [ObjectName], 
	[su].[name] AS [ObjectOwner],  
	[so].[type] AS [ObjectType], 
    CAST([sp].[smallid] AS INT) AS [Minor],
	[sp].[type] AS [type], 
	[sp].[name] AS [PropertyName], 
	[sp].[value] AS [PropertyValue],
    SQL_VARIANT_PROPERTY([sp].[value],'BaseType') AS [PropertyBaseType],
    CASE [sp].[type] WHEN 2 THEN USER_NAME([sp].[smallid]) END AS [UserName],
    CASE [sp].[type] WHEN 1 THEN (SELECT TOP 1 [name] FROM [dbo].[systypes] WHERE [xusertype] = [sp].[smallid]) END AS [UDTName],
    CASE [sp].[type] WHEN 1 THEN (SELECT TOP 1 [sysusers].[name] FROM [dbo].[sysusers] INNER JOIN [systypes] ON [systypes].[uid] = [sysusers].[uid] WHERE [xusertype] = [sp].[smallid]) END AS [UDTOwner],
    [sc].[name] AS [FieldName],
    [si].[name] AS [IndexName],
    [sop].[name] AS [ParentName],
    [sup].[name] AS [ParentOwner],
    [sop].[type] AS [ParentType]
FROM  [dbo].[sysproperties] [sp] WITH (NOLOCK)
    LEFT JOIN [dbo].[sysobjects] [so] WITH (NOLOCK) ON [so].[id] = [sp].[id]
    LEFT JOIN [dbo].[sysusers] [su] WITH (NOLOCK) ON [su].[uid] = [so].[uid]
    LEFT JOIN [dbo].[syscolumns] [sc] WITH (NOLOCK) ON [sc].[id] = [sp].[id] AND [sc].[colid] = [sp].[smallid]
    LEFT JOIN [dbo].[sysindexes] [si] WITH (NOLOCK) ON [si].[id] = [sp].[id] AND [si].[indid] = [sp].[smallid]
    LEFT JOIN [dbo].[sysobjects] [sop] WITH (NOLOCK) ON [so].[parent_obj] = [sop].[id]
    LEFT JOIN [dbo].[sysusers] [sup] WITH (NOLOCK) ON [sop].[uid] = [sup].[uid]
        -- eliminate the combination: (column and type 5 (Parameter)
WHERE    NOT    (NOT    (    (    [sc].[number] = 1
                    OR (    [sc].[number] = 0
                        AND OBJECTPROPERTY([sc].[id], N'IsScalarFunction') = 1
                        and ISNULL([sc].[name], '') != ''
                        )
                    )
                AND (    [sc].[id] =[so].[id])
                )
            AND [sp].[type] = 5
            )
      -- eliminate the combination: (param and type 4 (column)
      AND	NOT	(	(	(	[sc].[number] = 1 
					OR	(	[sc].[number] = 0 
						and OBJECTPROPERTY([sc].[id], N'IsScalarFunction')= 1 
						and ISNULL([sc].[name], '') != ''
						)
					) 
				AND	(	[sc].[id]=[so].[id])
				) 
			and		[sp].[type] = 4
		)
ORDER   BY [sp].[id], [sp].[smallid], [sp].[type], [sp].[name]";

        internal const string SQL_GetExtendedProperties= @"SELECT
    [ep].[name] AS [PropertyName],
    [ep].[value] AS [PropertyValue],
    SQL_VARIANT_PROPERTY([ep].[value],'BaseType') AS [PropertyBaseType],
    SQL_VARIANT_PROPERTY([ep].[value],'MaxLength') AS [PropertyMaxLength],
    SQL_VARIANT_PROPERTY([ep].[value],'Precision') AS [PropertyPrecision],
    SQL_VARIANT_PROPERTY([ep].[value],'Scale') AS [PropertyScale]
FROM
    sys.fn_listextendedproperty(NULL, @level0type, @level0name, @level1type, @level1name, @level2type, @level2name) as [ep]";

        internal const string SQL_GetIndexes= @"SELECT  [sysindexes].[name] AS [IndexName],         
		CONVERT(bit, INDEXPROPERTY([sysindexes].[id], [sysindexes].[name], N'IsClustered')) AS [IsClustered],
        CONVERT(bit, INDEXPROPERTY([sysindexes].[id], [sysindexes].[name], N'IsUnique')) AS [IsUnique],
        CONVERT(bit, CASE WHEN ([sysindexes].[status] & 4096) = 0 THEN 0 ELSE 1 END) AS [IsUniqueConstraint],
        CONVERT(bit, CASE WHEN ([sysindexes].[status] & 2048) = 0 THEN 0 ELSE 1 END) AS [IsPrimary],
        CONVERT(bit, CASE WHEN ([sysindexes].[status] & 0x1000000) = 0 THEN 0 ELSE 1 END) AS [NoRecompute],
        CONVERT(bit, CASE WHEN ([sysindexes].[status] & 0x1) = 0 THEN 0 ELSE 1 END) AS [IgnoreDupKey],
        CONVERT(bit, CASE WHEN ([sysindexes].[status] & 6144) = 0 THEN 0 ELSE 1 END) AS [IsIndex],
        CONVERT(bit, INDEXPROPERTY([sysindexes].[id], [sysindexes].[name], N'IsPadIndex')) AS [IsPadIndex],
        CONVERT(bit, OBJECTPROPERTY([sysindexes].[id], N'IsTable')) AS [IsTable],
        CONVERT(bit, OBJECTPROPERTY([sysindexes].[id], N'IsView')) AS [IsView],
        CONVERT(bit, INDEXPROPERTY([sysindexes].[id], [sysindexes].[name], N'IsFulltextKey')) AS [IsFullTextKey],
        CONVERT(bit, INDEXPROPERTY([sysindexes].[id], [sysindexes].[name], N'IsStatistics')) AS [IsStatistics],
        CONVERT(bit, INDEXPROPERTY([sysindexes].[id], [sysindexes].[name], N'IsAutoStatistics')) AS [IsAutoStatistics],
        CONVERT(bit, INDEXPROPERTY([sysindexes].[id], [sysindexes].[name], N'IsHypothetical')) AS [IsHypothetical],
        [sysfilegroups].[groupname] AS [FileGroup],
        [sysobjects].[name] AS [ParentName], 
        [sysusers].[name] AS [SchemaName], 
        [sysindexes].[OrigFillFactor] AS [FillFactor], 
        [sysindexes].[status] as [Status], 
        [syscolumns].[name] AS [ColumnName],
        CONVERT(bit, ISNULL(INDEXKEY_PROPERTY([syscolumns].[id], [sysindexkeys].[indid], [keyno], N'IsDescending'), 0)) AS [IsDescending],
        CONVERT(bit, ISNULL(INDEXKEY_PROPERTY([syscolumns].[id], [sysindexkeys].[indid], [keyno], N'IsComputed'), 0)) AS [IsComputed]
FROM [dbo].[sysindexes] WITH (NOLOCK) 
	INNER JOIN [dbo].[sysindexkeys] WITH (NOLOCK) ON [sysindexes].[indid] = [sysindexkeys].[indid] AND [sysindexkeys].[id] = [sysindexes].[id]
	INNER JOIN [dbo].[syscolumns] WITH (NOLOCK) ON [syscolumns].[colid] = [sysindexkeys].[colid] AND [syscolumns].[id] = [sysindexes].[id]
	INNER JOIN [dbo].[sysobjects] WITH (NOLOCK) ON [sysobjects].[id] = [sysindexes].[id] 
	LEFT JOIN [dbo].[sysusers] WITH (NOLOCK) ON [sysusers].[uid] = [sysobjects].[uid]
	LEFT JOIN [dbo].[sysfilegroups] WITH (NOLOCK) ON [sysfilegroups].[groupid] = [sysindexes].[groupid] 
WHERE   (OBJECTPROPERTY([sysindexes].[id], N'IsTable') = 1 OR OBJECTPROPERTY([sysindexes].[id], N'IsView') = 1)
	AND OBJECTPROPERTY([sysindexes].[id], N'IsSystemTable') = 0 
	AND INDEXPROPERTY([sysindexes].[id], [sysindexes].[name], N'IsAutoStatistics') = 0
	AND INDEXPROPERTY([sysindexes].[id], [sysindexes].[name], N'IsHypothetical') = 0
	AND [sysindexes].[name] IS NOT NULL
ORDER   BY [sysindexes].[id], [sysindexes].[name], [sysindexkeys].[keyno]";

        internal const string SQL_GetIndexes2005= @"SELECT  [i].[name] AS [IndexName],
        CONVERT(bit, CASE [i].[type] WHEN 1 THEN 1 ELSE 0 END) AS [IsClustered],
        [i].[is_unique] AS [IsUnique],
        [i].[is_unique_constraint] AS [IsUniqueConstraint],
        [i].[is_primary_key] AS [IsPrimary],
        [s].[no_recompute] AS [NoRecompute], 
        [i].[ignore_dup_key] AS [IgnoreDupKey],
        CONVERT(bit, 0) AS [IsIndex], -- TODO, find value
        [i].[is_padded] AS [IsPadIndex],
        CONVERT(bit, CASE WHEN [o].[type] = 'U' THEN 1 ELSE 0 END) AS [IsTable],
        CONVERT(bit, CASE WHEN [o].[type] = 'V' THEN 1 ELSE 0 END) AS [IsView],
        CONVERT(bit, INDEXPROPERTY([i].[object_id], [i].[name], N'IsFulltextKey')) AS [IsFullTextKey],
        CONVERT(bit, 0) AS [IsStatistics], 
        CONVERT(bit, 0) AS [IsAutoStatistics], -- TODO, find value
        [i].[is_hypothetical] AS [IsHypothetical], 
        [fg].[name] AS [FileGroup],
        [o].[name] AS [ParentName],
        [os].[name] AS [SchemaName],
        [i].[fill_factor] AS [FillFactor],
        0 as [Status], -- TODO, find value
        [c].[name] AS [ColumnName],
        [ic].[is_descending_key] AS [IsDescending],
        CONVERT(bit, 0) AS [IsComputed] -- TODO, find value
FROM [sys].[indexes] i WITH (NOLOCK)
	LEFT JOIN [sys].[data_spaces] [fg] WITH (NOLOCK) ON [fg].[data_space_id] = [i].[data_space_id]
	LEFT JOIN [sys].[objects] [o] WITH (NOLOCK) ON [o].[object_id] = [i].[object_id]
	LEFT JOIN [sys].[schemas] [os] WITH (NOLOCK) ON [os].[schema_id] = [o].[schema_id]
	LEFT JOIN [sys].[index_columns] [ic] WITH (NOLOCK) ON [ic].[object_id] = [i].[object_id] AND [ic].[index_id] = [i].[index_id] AND [ic].[is_included_column] = 0
	LEFT JOIN [sys].[columns] [c] WITH (NOLOCK) ON [c].[object_id] = [ic].[object_id] AND [c].[column_id] = [ic].[column_id]
	LEFT JOIN [sys].[stats] [s] WITH (NOLOCK) ON [s].[object_id] = [i].[object_id] AND [s].[name] = [i].[name]
WHERE [i].[type] IN (0, 1, 2, 3)
	AND [o].[type] IN ('U', 'V', 'TF')
ORDER BY [i].[object_id], [i].[name], [ic].[key_ordinal], [ic].[index_column_id]";

        internal const string SQL_GetIndexesAzure= @"SELECT  [i].[name] AS [IndexName],
        CONVERT(bit, CASE [i].[type] WHEN 1 THEN 1 ELSE 0 END) AS [IsClustered],
        [i].[is_unique] AS [IsUnique],
        [i].[is_unique_constraint] AS [IsUniqueConstraint],
        [i].[is_primary_key] AS [IsPrimary],
        [s].[no_recompute] AS [NoRecompute], 
        [i].[ignore_dup_key] AS [IgnoreDupKey],
        CONVERT(bit, 0) AS [IsIndex], -- TODO, find value
        [i].[is_padded] AS [IsPadIndex],
        CONVERT(bit, CASE WHEN [o].[type] = 'U' THEN 1 ELSE 0 END) AS [IsTable],
        CONVERT(bit, CASE WHEN [o].[type] = 'V' THEN 1 ELSE 0 END) AS [IsView],
        CONVERT(bit, INDEXPROPERTY([i].[object_id], [i].[name], N'IsFulltextKey')) AS [IsFullTextKey],
        CONVERT(bit, 0) AS [IsStatistics], 
        CONVERT(bit, 0) AS [IsAutoStatistics], -- TODO, find value
        [i].[is_hypothetical] AS [IsHypothetical], 
        'PRIMARY' AS [FileGroup],
        [o].[name] AS [ParentName],
        [os].[name] AS [SchemaName],
        [i].[fill_factor] AS [FillFactor],
        0 as [Status], -- TODO, find value
        [c].[name] AS [ColumnName],
        [ic].[is_descending_key] AS [IsDescending],
        CONVERT(bit, 0) AS [IsComputed] -- TODO, find value
FROM [sys].[indexes] i WITH (NOLOCK)
	LEFT JOIN [sys].[objects] [o] WITH (NOLOCK) ON [o].[object_id] = [i].[object_id]
	LEFT JOIN [sys].[schemas] [os] WITH (NOLOCK) ON [os].[schema_id] = [o].[schema_id]
	LEFT JOIN [sys].[index_columns] [ic] WITH (NOLOCK) ON [ic].[object_id] = [i].[object_id] AND [ic].[index_id] = [i].[index_id] AND [ic].[is_included_column] = 0
	LEFT JOIN [sys].[columns] [c] WITH (NOLOCK) ON [c].[object_id] = [ic].[object_id] AND [c].[column_id] = [ic].[column_id]
	LEFT JOIN [sys].[stats] [s] WITH (NOLOCK) ON [s].[object_id] = [i].[object_id] AND [s].[name] = [i].[name]
WHERE [i].[type] IN (0, 1, 2, 3)
	AND [o].[type] IN ('U', 'V', 'TF')
ORDER BY [i].[object_id], [i].[name], [ic].[key_ordinal], [ic].[index_column_id]";

        internal const string SQL_GetKeys= @"SELECT  [fs].[name] AS [ForeignTableName], 
        [fsysusers].[name] AS [ForeignTableOwner], 
        [rs].[name] AS [PrimaryTableName], 
        [rsysusers].[name] AS [PrimaryTableOwner], 
        [cs].[name] AS [ConstraintName], 
        [fc].[name] AS [ForeignColumnName],
        [rc].[name] AS [PrimaryColumnName],
        CONVERT(bit, OBJECTPROPERTY([constid], N'CnstIsDisabled')) AS [Disabled],
        CONVERT(bit, OBJECTPROPERTY([constid], N'CnstIsNotRepl')) AS [IsNotForReplication],
        CONVERT(tinyint, ISNULL(OBJECTPROPERTY([constid], N'CnstIsUpdateCascade'), 0)) AS [UpdateReferentialAction],
        CONVERT(tinyint, ISNULL(OBJECTPROPERTY([constid], N'CnstIsDeleteCascade'), 0)) AS [DeleteReferentialAction],
        CONVERT(bit, OBJECTPROPERTY([constid], N'CnstIsNotTrusted')) AS [WithNoCheck]
FROM [dbo].[sysforeignkeys] WITH (NOLOCK) 
	INNER JOIN [dbo].[sysobjects] [fs] WITH (NOLOCK) ON [sysforeignkeys].[fkeyid] = [fs].[id]
	INNER JOIN [dbo].[sysobjects] [rs] WITH (NOLOCK) ON [sysforeignkeys].[rkeyid] = [rs].[id]
	INNER JOIN [dbo].[sysobjects] [cs] WITH (NOLOCK) ON [sysforeignkeys].[constid] = [cs].[id]
	LEFT JOIN [dbo].[sysusers] [fsysusers] WITH (NOLOCK) ON [fsysusers].[uid] = [fs].[uid] 
	LEFT JOIN [dbo].[sysusers] [rsysusers] WITH (NOLOCK) ON [rsysusers].[uid] = [rs].[uid]
	INNER JOIN [dbo].[syscolumns] [fc] WITH (NOLOCK) ON [sysforeignkeys].[fkey] = [fc].[colid] AND [sysforeignkeys].[fkeyid] = [fc].[id]
	INNER JOIN [dbo].[syscolumns] [rc] WITH (NOLOCK) ON [sysforeignkeys].[rkey] = [rc].[colid] AND [sysforeignkeys].[rkeyid] = [rc].[id]
WHERE OBJECTPROPERTY(object_id([fs].[Name]), 'IsMSShipped') = 0 --Added to check for replication.
ORDER BY [cs].[name], [sysforeignkeys].[keyno]";

        internal const string SQL_GetKeys2005= @"SELECT  [fs].[name] AS [ForeignTableName],
        [fschemas].[name] AS [ForeignTableOwner],
        [rs].[name] AS [PrimaryTableName], 
        [rschemas].[name] AS [PrimaryTableOwner],
        [sfk].[name] AS [ConstraintName],
        [fc].[name] AS [ForeignColumnName],
        [rc].[name] AS [PrimaryColumnName],
        [sfk].[is_disabled] AS [Disabled],
        [sfk].[is_not_for_replication] AS [IsNotForReplication],
        [sfk].[update_referential_action] AS [UpdateReferentialAction],
        [sfk].[delete_referential_action] AS [DeleteReferentialAction],
        [sfk].[is_not_trusted] AS [WithNoCheck]
FROM    [sys].[foreign_keys] AS [sfk] WITH (NOLOCK)
	INNER JOIN [sys].[foreign_key_columns] AS [sfkc] WITH (NOLOCK) ON [sfk].[object_id] = [sfkc].[constraint_object_id]
	INNER JOIN [sys].[objects] [fs] WITH (NOLOCK) ON [sfk].[parent_object_id] = [fs].[object_id]
	INNER JOIN [sys].[objects] [rs] WITH (NOLOCK) ON [sfk].[referenced_object_id] = [rs].[object_id] 
	LEFT JOIN [sys].[schemas] [fschemas] WITH (NOLOCK) ON [fschemas].[schema_id] = [fs].[schema_id]
	LEFT JOIN [sys].[schemas] [rschemas] WITH (NOLOCK) ON [rschemas].[schema_id] = [rs].[schema_id]
	INNER JOIN [sys].[columns] [fc] WITH (NOLOCK) ON [sfkc].[parent_column_id] = [fc].[column_id] AND [fc].[object_id] = [sfk].[parent_object_id]
	INNER JOIN [sys].[columns] [rc] WITH (NOLOCK) ON [sfkc].[referenced_column_id] = [rc].[column_id] AND [rc].[object_id] = [sfk].[referenced_object_id]
WHERE [sfk].[is_ms_shipped] = 0 --Added to check for replication.
ORDER BY [sfk].[name],[sfkc].[constraint_column_id]";

        internal const string SQL_GetTableColumns= @"SELECT
	clmns.[name] AS [Name],
	usrt.[name] AS [DataType],
	ISNULL(baset.[name], N'') AS [SystemType],
	CAST(CASE WHEN baset.[name] IN (N'char', N'varchar', N'binary', N'varbinary', N'nchar', N'nvarchar') THEN clmns.prec ELSE clmns.length END AS int) AS [Length],
	CAST(clmns.xprec AS tinyint) AS [NumericPrecision],
	CAST(clmns.xscale AS int) AS [NumericScale],
	CAST(clmns.isnullable AS bit) AS [IsNullable],
	defaults.text AS [DefaultValue],
	CAST(COLUMNPROPERTY(clmns.id, clmns.[name], N'IsIdentity') AS int) AS [Identity],
	CAST(COLUMNPROPERTY(clmns.id, clmns.[name], N'IsRowGuidCol') AS int) AS IsRowGuid,
	CAST(COLUMNPROPERTY(clmns.id, clmns.[name], N'IsComputed') AS int) AS IsComputed,
	CAST(COLUMNPROPERTY(clmns.id, clmns.[name], N'IsDeterministic') AS int) AS IsDeterministic,
	CAST(CASE COLUMNPROPERTY(clmns.id, clmns.[name], N'IsIdentity') WHEN 1 THEN IDENT_SEED(QUOTENAME(stbl.[name]) + '.' + QUOTENAME(tbl.[name])) ELSE 0 END AS NVARCHAR(40)) AS [IdentitySeed],
	CAST(CASE COLUMNPROPERTY(clmns.id, clmns.[name], N'IsIdentity') WHEN 1 THEN IDENT_INCR(QUOTENAME(stbl.[name]) + '.' + QUOTENAME(tbl.[name])) ELSE 0 END AS NVARCHAR(40)) AS [IdentityIncrement],
	cdef.[text] AS ComputedDefinition,
	clmns.[collation] AS Collation,
	CAST(clmns.colid AS int) AS ObjectId,
	stbl.[name] AS [SchemaName],
	tbl.[name] AS [TableName]
FROM dbo.sysobjects AS tbl WITH (NOLOCK)
	INNER JOIN dbo.sysusers AS stbl WITH (NOLOCK) ON stbl.[uid] = tbl.[uid]
	INNER JOIN dbo.syscolumns AS clmns WITH (NOLOCK) ON clmns.id=tbl.id
	LEFT JOIN dbo.systypes AS usrt WITH (NOLOCK) ON usrt.xusertype = clmns.xusertype
	LEFT JOIN dbo.sysusers AS sclmns WITH (NOLOCK) ON sclmns.uid = usrt.uid
	LEFT JOIN dbo.systypes AS baset WITH (NOLOCK) ON baset.xusertype = clmns.xtype and baset.xusertype = baset.xtype
	LEFT JOIN dbo.syscomments AS defaults WITH (NOLOCK) ON defaults.id = clmns.cdefault
	LEFT JOIN dbo.syscomments AS cdef WITH (NOLOCK) ON cdef.id = clmns.id AND cdef.number = clmns.colid
WHERE (tbl.[type] = 'U') 
	AND stbl.[name] = @SchemaName
	AND tbl.[name] = @TableName
ORDER BY tbl.[name], clmns.colorder";

        internal const string SQL_GetTableColumns2005= @"SELECT
	clmns.[name] AS [Name],
	usrt.[name] AS [DataType],
	ISNULL(baset.[name], N'') AS [SystemType],
	CAST(CASE WHEN baset.[name] IN (N'char', N'varchar', N'binary', N'varbinary', N'nchar', N'nvarchar') THEN clmns.prec ELSE clmns.length END AS int) AS [Length],
	CAST(clmns.xprec AS tinyint) AS [NumericPrecision],
	CAST(clmns.xscale AS int) AS [NumericScale],
	CAST(clmns.isnullable AS bit) AS [IsNullable],
	object_definition(defaults.default_object_id) AS [DefaultValue],
	CAST(COLUMNPROPERTY(clmns.id, clmns.[name], N'IsIdentity') AS int) AS [Identity],
	CAST(COLUMNPROPERTY(clmns.id, clmns.[name], N'IsRowGuidCol') AS int) AS IsRowGuid,
	CAST(COLUMNPROPERTY(clmns.id, clmns.[name], N'IsComputed') AS int) AS IsComputed,
	CAST(COLUMNPROPERTY(clmns.id, clmns.[name], N'IsDeterministic') AS int) AS IsDeterministic,
	CAST(CASE COLUMNPROPERTY(clmns.id, clmns.[name], N'IsIdentity') WHEN 1 THEN IDENT_SEED(QUOTENAME(SCHEMA_NAME(tbl.uid)) + '.' + QUOTENAME(tbl.[name])) ELSE 0 END AS nvarchar(40)) AS [IdentitySeed],
	CAST(CASE COLUMNPROPERTY(clmns.id, clmns.[name], N'IsIdentity') WHEN 1 THEN IDENT_INCR(QUOTENAME(SCHEMA_NAME(tbl.uid)) + '.' + QUOTENAME(tbl.[name])) ELSE 0 END AS nvarchar(40)) AS [IdentityIncrement],
	cdef.definition AS ComputedDefinition,
	clmns.[collation] AS Collation,
	CAST(clmns.colid AS int) AS ObjectId,
	SCHEMA_NAME(tbl.uid) AS [SchemaName],
	tbl.[name] AS [TableName]
FROM dbo.sysobjects AS tbl WITH (NOLOCK)
	INNER JOIN dbo.syscolumns AS clmns WITH (NOLOCK) ON clmns.id=tbl.id
	LEFT JOIN dbo.systypes AS usrt WITH (NOLOCK) ON usrt.xusertype = clmns.xusertype
	LEFT JOIN dbo.sysusers AS sclmns WITH (NOLOCK) ON sclmns.uid = usrt.uid
	LEFT JOIN dbo.systypes AS baset WITH (NOLOCK) ON baset.xusertype = clmns.xtype and baset.xusertype = baset.xtype
	LEFT JOIN sys.columns AS defaults WITH (NOLOCK) ON defaults.name = clmns.name and defaults.object_id = clmns.id
	LEFT JOIN sys.computed_columns AS cdef WITH (NOLOCK) ON cdef.object_id = clmns.id AND cdef.column_id = clmns.colid
WHERE (tbl.[type] = 'U')
	AND SCHEMA_NAME(tbl.uid) = @SchemaName
	AND tbl.[name] = @TableName
ORDER BY tbl.[name], clmns.colorder";

        internal const string SQL_GetTableIndexes= @"SELECT  [sysindexes].[name] AS [IndexName],         
		CONVERT(bit, INDEXPROPERTY([sysindexes].[id], [sysindexes].[name], N'IsClustered')) AS [IsClustered],
        CONVERT(bit, INDEXPROPERTY([sysindexes].[id], [sysindexes].[name], N'IsUnique')) AS [IsUnique],
        CONVERT(bit, CASE WHEN ([sysindexes].[status] & 4096) = 0 THEN 0 ELSE 1 END) AS [IsUniqueConstraint],
        CONVERT(bit, CASE WHEN ([sysindexes].[status] & 2048) = 0 THEN 0 ELSE 1 END) AS [IsPrimary],
        CONVERT(bit, CASE WHEN ([sysindexes].[status] & 0x1000000) = 0 THEN 0 ELSE 1 END) AS [NoRecompute],
        CONVERT(bit, CASE WHEN ([sysindexes].[status] & 0x1) = 0 THEN 0 ELSE 1 END) AS [IgnoreDupKey],
        CONVERT(bit, CASE WHEN ([sysindexes].[status] & 6144) = 0 THEN 0 ELSE 1 END) AS [IsIndex],
        CONVERT(bit, INDEXPROPERTY([sysindexes].[id], [sysindexes].[name], N'IsPadIndex')) AS [IsPadIndex],
        CONVERT(bit, OBJECTPROPERTY([sysindexes].[id], N'IsTable')) AS [IsTable],
        CONVERT(bit, OBJECTPROPERTY([sysindexes].[id], N'IsView')) AS [IsView],
        CONVERT(bit, INDEXPROPERTY([sysindexes].[id], [sysindexes].[name], N'IsFulltextKey')) AS [IsFullTextKey],
        CONVERT(bit, INDEXPROPERTY([sysindexes].[id], [sysindexes].[name], N'IsStatistics')) AS [IsStatistics],
        CONVERT(bit, INDEXPROPERTY([sysindexes].[id], [sysindexes].[name], N'IsAutoStatistics')) AS [IsAutoStatistics],
        CONVERT(bit, INDEXPROPERTY([sysindexes].[id], [sysindexes].[name], N'IsHypothetical')) AS [IsHypothetical],
        [sysfilegroups].[groupname] AS [FileGroup],
        [sysobjects].[name] AS [ParentName], 
        [sysusers].[name] AS [SchemaName], 
        [sysindexes].[OrigFillFactor] AS [FillFactor], 
        [sysindexes].[status] as [Status], 
        [syscolumns].[name] AS [ColumnName],
        CONVERT(bit, ISNULL(INDEXKEY_PROPERTY([syscolumns].[id], [sysindexkeys].[indid], [keyno], N'IsDescending'), 0)) AS [IsDescending],
        CONVERT(bit, ISNULL(INDEXKEY_PROPERTY([syscolumns].[id], [sysindexkeys].[indid], [keyno], N'IsComputed'), 0)) AS [IsComputed]
FROM [dbo].[sysindexes] WITH (NOLOCK) 
	INNER JOIN [dbo].[sysindexkeys] WITH (NOLOCK) ON [sysindexes].[indid] = [sysindexkeys].[indid] AND [sysindexkeys].[id] = [sysindexes].[id]
	INNER JOIN [dbo].[syscolumns] WITH (NOLOCK) ON [syscolumns].[colid] = [sysindexkeys].[colid] AND [syscolumns].[id] = [sysindexes].[id]
	INNER JOIN [dbo].[sysobjects] WITH (NOLOCK) ON [sysobjects].[id] = [sysindexes].[id] 
	LEFT JOIN [dbo].[sysusers] WITH (NOLOCK) ON [sysusers].[uid] = [sysobjects].[uid]
	LEFT JOIN [dbo].[sysfilegroups] WITH (NOLOCK) ON [sysfilegroups].[groupid] = [sysindexes].[groupid] 
WHERE   (OBJECTPROPERTY([sysindexes].[id], N'IsTable') = 1 OR OBJECTPROPERTY([sysindexes].[id], N'IsView') = 1)
	AND OBJECTPROPERTY([sysindexes].[id], N'IsSystemTable') = 0 
	AND INDEXPROPERTY([sysindexes].[id], [sysindexes].[name], N'IsAutoStatistics') = 0
	AND INDEXPROPERTY([sysindexes].[id], [sysindexes].[name], N'IsHypothetical') = 0
	AND [sysindexes].[name] IS NOT NULL
	AND [sysobjects].[name] = @TableName
	AND [sysusers].[name] = @SchemaName
ORDER   BY [sysindexes].[id], [sysindexes].[name], [sysindexkeys].[keyno]";

        internal const string SQL_GetTableIndexes2005= @"SELECT  [i].[name] AS [IndexName],
        CONVERT(bit, CASE [i].[type] WHEN 1 THEN 1 ELSE 0 END) AS [IsClustered],
        [i].[is_unique] AS [IsUnique],
        [i].[is_unique_constraint] AS [IsUniqueConstraint],
        [i].[is_primary_key] AS [IsPrimary],
        [s].[no_recompute] AS [NoRecompute], 
        [i].[ignore_dup_key] AS [IgnoreDupKey],
        CONVERT(bit, 0) AS [IsIndex], -- TODO, find value
        [i].[is_padded] AS [IsPadIndex],
        CONVERT(bit, CASE WHEN [o].[type] = 'U' THEN 1 ELSE 0 END) AS [IsTable],
        CONVERT(bit, CASE WHEN [o].[type] = 'V' THEN 1 ELSE 0 END) AS [IsView],
        CONVERT(bit, INDEXPROPERTY([i].[object_id], [i].[name], N'IsFulltextKey')) AS [IsFullTextKey],
        CONVERT(bit, 0) AS [IsStatistics], 
        CONVERT(bit, 0) AS [IsAutoStatistics], -- TODO, find value
        [i].[is_hypothetical] AS [IsHypothetical], 
        [fg].[name] AS [FileGroup],
        [o].[name] AS [ParentName],
        [os].[name] AS [SchemaName],
        [i].[fill_factor] AS [FillFactor],
        0 as [Status], -- TODO, find value
        [c].[name] AS [ColumnName],
        [ic].[is_descending_key] AS [IsDescending],
        CONVERT(bit, 0) AS [IsComputed] -- TODO, find value
FROM [sys].[indexes] i WITH (NOLOCK)
	LEFT JOIN [sys].[data_spaces] [fg] WITH (NOLOCK) ON [fg].[data_space_id] = [i].[data_space_id]
	LEFT JOIN [sys].[objects] [o] WITH (NOLOCK) ON [o].[object_id] = [i].[object_id]
	LEFT JOIN [sys].[schemas] [os] WITH (NOLOCK) ON [os].[schema_id] = [o].[schema_id]
	LEFT JOIN [sys].[index_columns] [ic] WITH (NOLOCK) ON [ic].[object_id] = [i].[object_id] AND [ic].[index_id] = [i].[index_id] AND [ic].[is_included_column] = 0
	LEFT JOIN [sys].[columns] [c] WITH (NOLOCK) ON [c].[object_id] = [ic].[object_id] AND [c].[column_id] = [ic].[column_id]
	LEFT JOIN [sys].[stats] [s] WITH (NOLOCK) ON [s].[object_id] = [i].[object_id] AND [s].[name] = [i].[name]
WHERE [i].[type] IN (0, 1, 2, 3)
	AND [o].[type] IN ('U', 'V', 'TF')
	AND [o].[name] = @TableName
	AND [os].[name] = @SchemaName
ORDER BY [i].[object_id], [i].[name], [ic].[key_ordinal], [ic].[index_column_id]";

        internal const string SQL_GetTableIndexesAzure= @"SELECT  [i].[name] AS [IndexName],
        CONVERT(bit, CASE [i].[type] WHEN 1 THEN 1 ELSE 0 END) AS [IsClustered],
        [i].[is_unique] AS [IsUnique],
        [i].[is_unique_constraint] AS [IsUniqueConstraint],
        [i].[is_primary_key] AS [IsPrimary],
        [s].[no_recompute] AS [NoRecompute], 
        [i].[ignore_dup_key] AS [IgnoreDupKey],
        CONVERT(bit, 0) AS [IsIndex], -- TODO, find value
        [i].[is_padded] AS [IsPadIndex],
        CONVERT(bit, CASE WHEN [o].[type] = 'U' THEN 1 ELSE 0 END) AS [IsTable],
        CONVERT(bit, CASE WHEN [o].[type] = 'V' THEN 1 ELSE 0 END) AS [IsView],
        CONVERT(bit, INDEXPROPERTY([i].[object_id], [i].[name], N'IsFulltextKey')) AS [IsFullTextKey],
        CONVERT(bit, 0) AS [IsStatistics], 
        CONVERT(bit, 0) AS [IsAutoStatistics], -- TODO, find value
        [i].[is_hypothetical] AS [IsHypothetical], 
        'PRIMARY' AS [FileGroup],
        [o].[name] AS [ParentName],
        [os].[name] AS [SchemaName],
        [i].[fill_factor] AS [FillFactor],
        0 as [Status], -- TODO, find value
        [c].[name] AS [ColumnName],
        [ic].[is_descending_key] AS [IsDescending],
        CONVERT(bit, 0) AS [IsComputed] -- TODO, find value
FROM [sys].[indexes] i WITH (NOLOCK)
	LEFT JOIN [sys].[objects] [o] WITH (NOLOCK) ON [o].[object_id] = [i].[object_id]
	LEFT JOIN [sys].[schemas] [os] WITH (NOLOCK) ON [os].[schema_id] = [o].[schema_id]
	LEFT JOIN [sys].[index_columns] [ic] WITH (NOLOCK) ON [ic].[object_id] = [i].[object_id] AND [ic].[index_id] = [i].[index_id] AND [ic].[is_included_column] = 0
	LEFT JOIN [sys].[columns] [c] WITH (NOLOCK) ON [c].[object_id] = [ic].[object_id] AND [c].[column_id] = [ic].[column_id]
	LEFT JOIN [sys].[stats] [s] WITH (NOLOCK) ON [s].[object_id] = [i].[object_id] AND [s].[name] = [i].[name]
WHERE [i].[type] IN (0, 1, 2, 3)
	AND [o].[type] IN ('U', 'V', 'TF')
	AND [o].[name] = @TableName
	AND [os].[name] = @SchemaName
ORDER BY [i].[object_id], [i].[name], [ic].[key_ordinal], [ic].[index_column_id]";

        internal const string SQL_GetTableKeys= @"SELECT  [fs].[name] AS [ForeignTableName], 
        [fsysusers].[name] AS [ForeignTableOwner], 
        [rs].[name] AS [PrimaryTableName], 
        [rsysusers].[name] AS [PrimaryTableOwner], 
        [cs].[name] AS [ConstraintName], 
        [fc].[name] AS [ForeignColumnName],
        [rc].[name] AS [PrimaryColumnName],
        CONVERT(bit, OBJECTPROPERTY([constid], N'CnstIsDisabled')) AS [Disabled],
        CONVERT(bit, OBJECTPROPERTY([constid], N'CnstIsNotRepl')) AS [IsNotForReplication],
        CONVERT(tinyint, ISNULL(OBJECTPROPERTY([constid], N'CnstIsUpdateCascade'), 0)) AS [UpdateReferentialAction],
        CONVERT(tinyint, ISNULL(OBJECTPROPERTY([constid], N'CnstIsDeleteCascade'), 0)) AS [DeleteReferentialAction],
        CONVERT(bit, OBJECTPROPERTY([constid], N'CnstIsNotTrusted')) AS [WithNoCheck]
FROM [dbo].[sysforeignkeys] WITH (NOLOCK) 
	INNER JOIN [dbo].[sysobjects] [fs] WITH (NOLOCK) ON [sysforeignkeys].[fkeyid] = [fs].[id]
	INNER JOIN [dbo].[sysobjects] [rs] WITH (NOLOCK) ON [sysforeignkeys].[rkeyid] = [rs].[id]
	INNER JOIN [dbo].[sysobjects] [cs] WITH (NOLOCK) ON [sysforeignkeys].[constid] = [cs].[id]
	LEFT JOIN [dbo].[sysusers] [fsysusers] WITH (NOLOCK) ON [fsysusers].[uid] = [fs].[uid] 
	LEFT JOIN [dbo].[sysusers] [rsysusers] WITH (NOLOCK) ON [rsysusers].[uid] = [rs].[uid]
	INNER JOIN [dbo].[syscolumns] [fc] WITH (NOLOCK) ON [sysforeignkeys].[fkey] = [fc].[colid] AND [sysforeignkeys].[fkeyid] = [fc].[id]
	INNER JOIN [dbo].[syscolumns] [rc] WITH (NOLOCK) ON [sysforeignkeys].[rkey] = [rc].[colid] AND [sysforeignkeys].[rkeyid] = [rc].[id]
WHERE ([fs].[name] = @TableName AND [fsysusers].[name] = @SchemaName)
	OR ([rs].[name] = @TableName AND [rsysusers].[name] = @SchemaName)
ORDER BY [cs].[name], [sysforeignkeys].[keyno]";

        internal const string SQL_GetTableKeys2005= @"SELECT  [fs].[name] AS [ForeignTableName],
        [fschemas].[name] AS [ForeignTableOwner],
        [rs].[name] AS [PrimaryTableName], 
        [rschemas].[name] AS [PrimaryTableOwner],
        [sfk].[name] AS [ConstraintName],
        [fc].[name] AS [ForeignColumnName],
        [rc].[name] AS [PrimaryColumnName],
        [sfk].[is_disabled] AS [Disabled],
        [sfk].[is_not_for_replication] AS [IsNotForReplication],
        [sfk].[update_referential_action] AS [UpdateReferentialAction],
        [sfk].[delete_referential_action] AS [DeleteReferentialAction],
        [sfk].[is_not_trusted] AS [WithNoCheck]
FROM    [sys].[foreign_keys] AS [sfk] WITH (NOLOCK)
	INNER JOIN [sys].[foreign_key_columns] AS [sfkc] WITH (NOLOCK) ON [sfk].[object_id] = [sfkc].[constraint_object_id]
	INNER JOIN [sys].[objects] [fs] WITH (NOLOCK) ON [sfk].[parent_object_id] = [fs].[object_id]
	INNER JOIN [sys].[objects] [rs] WITH (NOLOCK) ON [sfk].[referenced_object_id] = [rs].[object_id] 
	LEFT JOIN [sys].[schemas] [fschemas] WITH (NOLOCK) ON [fschemas].[schema_id] = [fs].[schema_id]
	LEFT JOIN [sys].[schemas] [rschemas] WITH (NOLOCK) ON [rschemas].[schema_id] = [rs].[schema_id]
	INNER JOIN [sys].[columns] [fc] WITH (NOLOCK) ON [sfkc].[parent_column_id] = [fc].[column_id] AND [fc].[object_id] = [sfk].[parent_object_id]
	INNER JOIN [sys].[columns] [rc] WITH (NOLOCK) ON [sfkc].[referenced_column_id] = [rc].[column_id] AND [rc].[object_id] = [sfk].[referenced_object_id]
WHERE ([fs].[name] = @TableName AND [fschemas].[name] = @SchemaName)
	OR ([rs].[name] = @TableName AND [rschemas].[name] = @SchemaName)	
ORDER BY [sfk].[name],[sfkc].[constraint_column_id]
";

        internal const string SQL_GetTables= @"SELECT
  object_name(id)	AS [OBJECT_NAME],
  user_name(uid)	AS [USER_NAME],
  type				AS TYPE,
  crdate			AS DATE_CREATED,
  ''				AS FILE_GROUP,
  id				as [OBJECT_ID]
FROM
  sysobjects
WHERE
  type = N'U'
  AND permissions(id) & 4096 <> 0
  AND ObjectProperty(id, N'IsMSShipped') = 0
ORDER BY user_name(uid), object_name(id)";

        internal const string SQL_GetTables2005= @"SELECT
  TB.[OBJECT_NAME],
  TB.[USER_NAME],
  TB.[TYPE],
  TB.[DATE_CREATED],
  TB.[FILE_GROUP],
  TB.[OBJECT_ID]
FROM
  (
    SELECT
      T.name AS [OBJECT_NAME],
      SCHEMA_NAME(T.schema_id) AS [USER_NAME],
      T.schema_id AS [SCHEMA_ID],
      T.type AS [TYPE],
      T.create_date AS [DATE_CREATED],
      FG.file_group AS [FILE_GROUP],
      T.object_id AS [OBJECT_ID],
      HAS_PERMS_BY_NAME (QUOTENAME(SCHEMA_NAME(T.schema_id)) + '.' + QUOTENAME(T.name), 'OBJECT', 'SELECT') AS [HAVE_SELECT]
    FROM
      sys.tables T LEFT JOIN (
        SELECT
          S.name AS file_group,
          I.object_id AS id
        FROM sys.filegroups S INNER JOIN sys.indexes I ON I.data_space_id = S.data_space_id
        WHERE I.type < 2
      ) AS FG ON T.object_id = FG.id
    WHERE
      T.type = 'U'
  ) TB
WHERE
  TB.HAVE_SELECT = 1
  AND ObjectProperty(TB.[OBJECT_ID], N'IsMSShipped') = 0
  AND NOT EXISTS (SELECT * FROM sys.extended_properties WHERE major_id = TB.[OBJECT_ID] AND name = 'microsoft_database_tools_support' AND value = 1)
ORDER BY
  TB.USER_NAME,
  TB.OBJECT_NAME";

        internal const string SQL_GetTablesAzure= @"SELECT
  TB.[OBJECT_NAME],
  TB.[USER_NAME],
  TB.[TYPE],
  TB.[DATE_CREATED],
  'PRIMARY' AS [FILE_GROUP],
  TB.[OBJECT_ID]
FROM
  (
    SELECT
      T.name AS [OBJECT_NAME],
      SCHEMA_NAME(T.schema_id) AS [USER_NAME],
      T.schema_id AS [SCHEMA_ID],
      T.type AS [TYPE],
      T.create_date AS [DATE_CREATED],
      T.object_id AS [OBJECT_ID],
      HAS_PERMS_BY_NAME (QUOTENAME(SCHEMA_NAME(T.schema_id)) + '.' + QUOTENAME(T.name), 'OBJECT', 'SELECT') AS [HAVE_SELECT]
    FROM
      sys.tables T
    WHERE
      T.type = 'U'
  ) TB
WHERE
  TB.HAVE_SELECT = 1
  AND ObjectProperty(TB.[OBJECT_ID], N'IsMSShipped') = 0
ORDER BY
  TB.USER_NAME,
  TB.OBJECT_NAME";

        internal const string SQL_GetViewColumns= @"SELECT
	clmns.[name] AS [Name],
	usrt.[name] AS [DataType],
	ISNULL(baset.[name], N'') AS [SystemType],
	CAST(CASE WHEN baset.[name] IN (N'char', N'varchar', N'binary', N'varbinary', N'nchar', N'nvarchar') THEN clmns.prec ELSE clmns.length END AS int) AS [Length],
	CAST(clmns.xprec AS tinyint) AS [NumericPrecision],
	CAST(clmns.xscale AS int) AS [NumericScale],
	CAST(clmns.isnullable AS bit) AS [IsNullable],
	defaults.text AS [DefaultValue],
	CAST(COLUMNPROPERTY(clmns.id, clmns.[name], N'IsComputed') AS int) AS IsComputed,
	CAST(COLUMNPROPERTY(clmns.id, clmns.[name], N'IsDeterministic') AS int) AS IsDeterministic,
	cdef.[text] AS ComputedDefinition,
	clmns.[collation] AS Collation,
	CAST(clmns.colid AS int) AS ObjectId,
	stbl.[name] AS [SchemaName],
	tbl.[name] AS [ViewName]
FROM dbo.sysobjects AS tbl WITH (NOLOCK)
	INNER JOIN dbo.sysusers AS stbl WITH (NOLOCK) ON stbl.[uid] = tbl.[uid]
	INNER JOIN dbo.syscolumns AS clmns WITH (NOLOCK) ON clmns.id=tbl.id
	LEFT JOIN dbo.systypes AS usrt WITH (NOLOCK) ON usrt.xusertype = clmns.xusertype
	LEFT JOIN dbo.sysusers AS sclmns WITH (NOLOCK) ON sclmns.uid = usrt.uid
	LEFT JOIN dbo.systypes AS baset WITH (NOLOCK) ON baset.xusertype = clmns.xtype and baset.xusertype = baset.xtype
	LEFT JOIN dbo.syscomments AS defaults WITH (NOLOCK) ON defaults.id = clmns.cdefault
	LEFT JOIN dbo.syscomments AS cdef WITH (NOLOCK) ON cdef.id = clmns.id AND cdef.number = clmns.colid
WHERE (tbl.[type] = 'V')
	AND stbl.[name] = @SchemaName
	AND tbl.[name] = @ViewName
ORDER BY tbl.[name], clmns.colorder";

        internal const string SQL_GetViewColumns2005 = @"SELECT
	clmns.[name] AS [Name],
	usrt.[name] AS [DataType],
	ISNULL(baset.[name], N'') AS [SystemType],
	CAST(CASE WHEN baset.[name] IN (N'char', N'varchar', N'binary', N'varbinary', N'nchar', N'nvarchar') THEN clmns.prec ELSE clmns.length END AS int) AS [Length],
	CAST(clmns.xprec AS tinyint) AS [NumericPrecision],
	CAST(clmns.xscale AS int) AS [NumericScale],
	CAST(clmns.isnullable AS bit) AS [IsNullable],
	object_definition(defaults.default_object_id) AS [DefaultValue],
	CAST(COLUMNPROPERTY(clmns.id, clmns.[name], N'IsComputed') AS int) AS IsComputed,
	CAST(COLUMNPROPERTY(clmns.id, clmns.[name], N'IsDeterministic') AS int) AS IsDeterministic,
	cdef.definition AS ComputedDefinition,
	clmns.[collation] AS Collation,
	CAST(clmns.colid AS int) AS ObjectId,
	SCHEMA_NAME(tbl.uid) AS [SchemaName],
	tbl.[name] AS [ViewName]
FROM dbo.sysobjects AS tbl WITH (NOLOCK)
	INNER JOIN dbo.syscolumns AS clmns WITH (NOLOCK) ON clmns.id=tbl.id
	LEFT JOIN dbo.systypes AS usrt WITH (NOLOCK) ON usrt.xusertype = clmns.xusertype
	LEFT JOIN dbo.sysusers AS sclmns WITH (NOLOCK) ON sclmns.uid = usrt.uid
	LEFT JOIN dbo.systypes AS baset WITH (NOLOCK) ON baset.xusertype = clmns.xtype and baset.xusertype = baset.xtype
	LEFT JOIN sys.columns AS defaults WITH (NOLOCK) ON defaults.name = clmns.name and defaults.object_id = clmns.id
	LEFT JOIN sys.computed_columns AS cdef WITH (NOLOCK) ON cdef.object_id = clmns.id AND cdef.column_id = clmns.colid
WHERE (tbl.[type] = 'V')
	AND SCHEMA_NAME(tbl.uid) = @SchemaName
	AND tbl.[name] = @ViewName
ORDER BY tbl.[name], clmns.colorder";

        internal const string SQL_GetViews = @"SELECT
  object_name(id) AS OBJECT_NAME,
  user_name(uid) AS USER_NAME,
  type AS TYPE,
  crdate AS DATE_CREATED,
  id as OBJECT_ID
FROM sysobjects
WHERE type = N'V'
  AND permissions(id) & 4096 <> 0
  AND ObjectProperty(id, N'IsMSShipped') = 0
ORDER BY object_name(id)";

        internal const string SQL_GetViews2005 = @" SELECT
  object_name(id) AS OBJECT_NAME,
  schema_name(uid) AS USER_NAME,
  type AS TYPE,
  crdate AS DATE_CREATED,
  id as OBJECT_ID
FROM sysobjects
WHERE
  type = N'V'
  AND HAS_PERMS_BY_NAME (QUOTENAME(SCHEMA_NAME(uid)) + '.' + QUOTENAME(object_name(id)), 'OBJECT', 'SELECT') <> 0
  AND ObjectProperty(id, N'IsMSShipped') = 0
  AND NOT EXISTS (SELECT * FROM sys.extended_properties WHERE major_id = id AND name = 'microsoft_database_tools_support' AND value = 1)
ORDER BY object_name(id)";

        internal const string SQL_GetViewsAzure = @" SELECT
  object_name(id) AS OBJECT_NAME,
  schema_name(uid) AS USER_NAME,
  type AS TYPE,
  crdate AS DATE_CREATED,
  id as OBJECT_ID
FROM sysobjects
WHERE
  type = N'V'
  AND HAS_PERMS_BY_NAME (QUOTENAME(SCHEMA_NAME(uid)) + '.' + QUOTENAME(object_name(id)), 'OBJECT', 'SELECT') <> 0
  AND ObjectProperty(id, N'IsMSShipped') = 0
ORDER BY object_name(id)";

        internal const string SQL_GetSqlServerVersion = "SELECT SERVERPROPERTY('ProductVersion') AS [ProductVersion], SERVERPROPERTY('Edition') AS [Edition]";

        internal const string SQL_GetDatabaseName = "SELECT db_name() AS [DatabaseName]";

        internal const string SQL_GetObjectData = "SELECT * FROM [{0}].[{1}]";

        internal const string SQL_GetObjectSource = "EXEC sp_helptext @objectname";


    }
}
