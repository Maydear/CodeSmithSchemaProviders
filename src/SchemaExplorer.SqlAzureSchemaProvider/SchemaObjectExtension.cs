using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeSmith.Core.Collections;
using System.Data;

namespace SchemaExplorer
{
    internal static class SchemaObjectExtension
    {
        internal static string CommandResultSchemaFMTQuery(this CommandSchema command)
        {
            bool flag = command.ParseBooleanExtendedProperty( "CS_IsTableValuedFunction");
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("SET FMTONLY ON\r\n");
            if (!flag)
            {
                stringBuilder.AppendFormat("EXEC [{0}].[{1}]\r\n", command.Owner, command.Name);
                for (int i = 0; i < command.Parameters.Count; i++)
                {
                    if (command.Parameters[i].Direction != ParameterDirection.ReturnValue)
                    {
                        if (!command.Parameters[i].ParseBooleanExtendedProperty("CS_IsUserDefinedTableType"))
                        {
                            stringBuilder.Append("\tNULL");
                        }
                        else
                        {
                            stringBuilder.Append("\tDEFAULT");
                        }
                        if (i < command.Parameters.Count - 1)
                        {
                            stringBuilder.Append(",");
                        }
                        stringBuilder.Append("\r\n");
                    }
                }
            }
            else
            {
                stringBuilder.AppendFormat("SELECT * FROM [{0}].[{1}] (", command.Owner, command.Name);
                for (int j = 0; j < command.Parameters.Count; j++)
                {
                    if (command.Parameters[j].Direction != ParameterDirection.ReturnValue)
                    {
                        if (!command.Parameters[j].ParseBooleanExtendedProperty( "CS_IsUserDefinedTableType"))
                        {
                            stringBuilder.Append("\tNULL");
                        }
                        else
                        {
                            stringBuilder.Append("\tDEFAULT");
                        }
                        if (j < command.Parameters.Count - 1)
                        {
                            stringBuilder.Append(", ");
                        }
                    }
                }
                stringBuilder.Append(")\r\n");
            }
            stringBuilder.Append("SET FMTONLY OFF\r\n");
            return stringBuilder.ToString();
        }

        internal static string CommandResultSchemaTransactionalQuery(this CommandSchema command)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("BEGIN TRANSACTION\r\n");
            stringBuilder.AppendFormat("EXEC [{0}].[{1}]\r\n", command.Owner, command.Name);
            for (int i = 0; i < command.Parameters.Count; i++)
            {
                if (command.Parameters[i].Direction != ParameterDirection.ReturnValue)
                {
                    if (!command.Parameters[i].ParseBooleanExtendedProperty("CS_IsUserDefinedTableType"))
                    {
                        stringBuilder.Append("\tNULL");
                    }
                    else
                    {
                        stringBuilder.Append("\tDEFAULT");
                    }
                    if (i < command.Parameters.Count - 1)
                    {
                        stringBuilder.Append(",");
                    }
                    stringBuilder.Append("\r\n");
                }
            }
            stringBuilder.Append("ROLLBACK TRANSACTION\r\n");
            return stringBuilder.ToString();
        }

        internal static bool ParseBooleanExtendedProperty(this SchemaObjectBase schemaObjectBase, string extendedProperty)
        {
            if (schemaObjectBase == null)
            {
                return false;
            }
            ExtendedPropertyCollection loadedExtendedProperties = schemaObjectBase.GetLoadedExtendedProperties();
            if (!loadedExtendedProperties.Contains(extendedProperty))
            {
                return false;
            }
            bool result;
            bool.TryParse(loadedExtendedProperties[extendedProperty].Value.ToString(), out result);
            return result;
        }

        internal static Dictionary<string, TableSchema> ToDictionary(this TableSchema schema)
            => new Dictionary<string, TableSchema>()
            {
                {
                    schema.FullName,
                    schema
                }
            };

        internal static Dictionary<string, TableSchema> ToDictionary(this IEnumerable<TableSchema> schemas)
        {
            Dictionary<string, TableSchema> dictionary = new Dictionary<string, TableSchema>();
            foreach (TableSchema schema in schemas)
            {
                dictionary.Add(schema.FullName, schema);
            }
            return dictionary;
        }

        internal static Dictionary<string, DatabaseSchema> ToDictionary(this DatabaseSchema schema)
           => new Dictionary<string, DatabaseSchema>()
           {
                {
                    schema.FullName,
                    schema
                }
           };

        internal static Dictionary<string, DatabaseSchema> ToDictionary(this IEnumerable<DatabaseSchema> schemas)
        {
            Dictionary<string, DatabaseSchema> dictionary = new Dictionary<string, DatabaseSchema>();
            foreach (DatabaseSchema schema in schemas)
            {
                dictionary.Add(schema.FullName, schema);
            }
            return dictionary;
        }

        internal static Dictionary<string, ViewSchema> ToDictionary(this ViewSchema schema) => new Dictionary<string, ViewSchema>()
        {
                {
                    schema.FullName,
                    schema
                }
        };

        internal static Dictionary<string, ViewSchema> ToDictionary(this IEnumerable<ViewSchema> schemas)
        {
            Dictionary<string, ViewSchema> dictionary = new Dictionary<string, ViewSchema>();
            foreach (ViewSchema schema in schemas)
            {
                dictionary.Add(schema.FullName, schema);
            }
            return dictionary;
        }

        internal static Dictionary<string, CommandSchema> ToDictionary(this CommandSchema schema) => new Dictionary<string, CommandSchema>()
        {
                {
                    schema.FullName,
                    schema
                }
        };

        internal static Dictionary<string, CommandSchema> ToDictionary(this IEnumerable<CommandSchema> schemas)
        {
            Dictionary<string, CommandSchema> dictionary = new Dictionary<string, CommandSchema>();
            foreach (CommandSchema schema in schemas)
            {
                dictionary.Add(schema.FullName, schema);
            }
            return dictionary;
        }

    }
}
