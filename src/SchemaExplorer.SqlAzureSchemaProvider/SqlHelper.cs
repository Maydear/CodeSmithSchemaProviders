using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchemaExplorer
{
    internal class SqlHelper : IDisposable
    {
        #region ctor
        public SqlHelper()
            : this(ConfigurationManager.AppSettings["ConnectionString"]) { }

        public SqlHelper(string connectionString)
            : this(new SqlConnection(connectionString)) { }

        /// <summary>
        /// 通过<see cref="IDbConnection"/>构造对象
        /// </summary>
        /// <param name="connection">链接对象</param>
        public SqlHelper(IDbConnection connection)
        {
            Connection = connection;
            ConnectionString = connection.ConnectionString;
            AutoCloseConnection = false;
        }

        public SqlHelper(string server, string database, string user, string password)
        {
            ConnectionString = string.Concat("Server=", server, ";Database=", database, ";User ID=", user, ";Password=", password, ";");
        }

        public SqlHelper(string server, string database)
        {
            this.ConnectionString = string.Concat("Server=", server, ";Database=", database, ";Integrated Security=true;");
        }
        #endregion

        #region Attributess
        /// <summary>
        /// 获取数据库链接字符串
        /// </summary>
        public string ConnectionString { get; private set; }

        /// <summary>
        /// 获取数据库链接对象
        /// </summary>
        public IDbConnection Connection { get; private set; }

        public IDbTransaction Transaction { get; set; }


        /// <summary>
        /// 获取或设置将空值设置为DbNull类型
        /// </summary>
        public bool ConvertEmptyValuesToDbNull { get; set; } = true;

        /// <summary>
        /// 获取或设置将最小值值设置为DbNull类型
        /// </summary>
        public bool ConvertMinValuesToDbNull { get; set; } = true;

        /// <summary>
        /// 获取或设置将最大值值设置为DbNull类型
        /// </summary>
        public bool ConvertMaxValuesToDbNull { get; set; }

        /// <summary>
        /// 获取或设置自动关闭链接
        /// </summary>
        public bool AutoCloseConnection { get; set; } = true;

        /// <summary>
        /// 获取或设置自动关闭链接
        /// </summary>
        public bool IsSingleRow { get; set; } = false;

        /// <summary>
        /// 获取或设置执行SQL命令超时时间
        /// </summary>
        public int CommandTimeout { get; set; } = 30;

        #endregion


        #region Common Help
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sqlText"></param>
        /// <param name="commandType"></param>
        /// <param name="parms"></param>
        /// <returns></returns>
        private IDbCommand BuildCommand(string sqlText, CommandType commandType, params IDbDataParameter[] parms)
        {
            if (Connection == null)
                throw new Exception("IDbConnection is NULL");

            IDbCommand command = Connection.CreateCommand();
            command.CommandText = sqlText;
            command.CommandType = commandType;
            command.CommandTimeout = CommandTimeout;
            if (parms != null && parms.Length > 0)
            {
                foreach (var item in parms)
                {
                    command.Parameters.Add(item);
                }
            }

            return command;
        }

        /// <summary>
        /// 执行脚本
        /// </summary>
        /// <param name="sqlText">Sql文本</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="parms">参数</param>
        /// <returns>返回影响行数</returns>

        public int Execute(string sqlText, CommandType commandType = CommandType.Text, params IDbDataParameter[] parms)
        {
            Connect();
            if (Connection == null)
                throw new Exception("IDbConnection is NULL");

            ConnectionState originalState = Connection.State;

            if (originalState != ConnectionState.Open)
                Connection.Open();
            try
            {
                return BuildCommand(sqlText, commandType, parms).ExecuteNonQuery();
            }
            finally
            {
                if (originalState == ConnectionState.Closed)
                    Connection.Close();
            }
        }

        /// <summary>
        /// 执行脚本
        /// </summary>
        /// <param name="sqlText">Sql文本</param>
        /// <param name="func">返回IDbDataParameter[]的委托</param>
        /// <param name="commandType">命令类型</param>
        /// <returns></returns>
        public int Execute(string sqlText, Func<IDbDataParameter[]> func, CommandType commandType = CommandType.Text)
        {
            return Execute(sqlText, commandType, func());
        }

        /// <summary>
        /// 查询首行首列
        /// </summary>
        /// <typeparam name="T">仅支持基础类型</typeparam>
        /// <param name="sqlText">Sql文本</param>
        /// <param name="func">返回IDbDataParameter[]的委托</param>
        /// <param name="commandType">命令类型</param>
        /// <returns>首行首列的值</returns>
        public object ExecuteScalar(string sqlText, CommandType commandType = CommandType.Text, params IDbDataParameter[] parms)
        {
            if (Connection == null)
                throw new Exception("IDbConnection is NULL");
            Connect();
            ConnectionState originalState = Connection.State;

            if (originalState != ConnectionState.Open)
                Connection.Open();
            try
            {
                return BuildCommand(sqlText, commandType, parms).ExecuteScalar();
            }
            finally
            {
                if (originalState == ConnectionState.Closed)
                    Connection.Close();
            }
        }

        /// <summary>
        /// 查询首行首列
        /// </summary>
        /// <typeparam name="T">仅支持基础类型</typeparam>
        /// <param name="sqlText">Sql文本</param>
        /// <param name="func">返回IDbDataParameter[]的委托</param>
        /// <param name="commandType">命令类型</param>
        /// <returns>首行首列的值</returns>
        public string ExecuteScalarString(string sqlText, CommandType commandType = CommandType.Text, params IDbDataParameter[] parms)
        {
            return ExecuteScalar(sqlText, commandType, parms).ToString();
        }

        /// <summary>
        /// 查询首行首列
        /// </summary>
        /// <typeparam name="T">仅支持基础类型</typeparam>
        /// <param name="sqlText">Sql文本</param>
        /// <param name="func">返回IDbDataParameter[]的委托</param>
        /// <param name="commandType">命令类型</param>
        /// <returns>首行首列的值</returns>
        public T ExecuteScalar<T>(string sqlText, CommandType commandType = CommandType.Text, params IDbDataParameter[] parms) where T : struct
        {
            return (T)ExecuteScalar(sqlText, commandType, parms);
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <typeparam name="T">实体类</typeparam>
        /// <param name="sqlText">Sql文本</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="parms">参数</param>
        /// <returns>返回泛型的实体对象</returns>
        public IDataReader ExecuteReader(string sqlText, CommandType commandType = CommandType.Text, params IDbDataParameter[] parms)
        {
            if (Connection == null)
                throw new Exception("IDbConnection is NULL");
            Connect();
            ConnectionState originalState = Connection.State;
            CommandBehavior commandBehavior = CommandBehavior.Default;
            if (AutoCloseConnection)
            {
                commandBehavior |= CommandBehavior.CloseConnection;
            }
            if (IsSingleRow)
            {
                commandBehavior |= CommandBehavior.SingleRow;
            }
            if (originalState != ConnectionState.Open)
                Connection.Open();
            try
            {
                return BuildCommand(sqlText, commandType, parms).ExecuteReader(commandBehavior);
            }
            finally
            {
                if (originalState == ConnectionState.Closed)
                    Connection.Close();
            }
        }

        #endregion

        #region SchemaExplorer

        public SqlParameter AddParameter(string name, SqlDbType type, object value)
        {
            SqlParameter sqlParameter = new SqlParameter()
            {
                Direction = ParameterDirection.Input,
                ParameterName = name,
                SqlDbType = type,
                Value = PrepareSqlValue(value)
            };
            _parameters.Add(sqlParameter);
            return sqlParameter;
        }

        public SqlParameter AddStreamParameter(string name, Stream valueStream, SqlDbType type)
        {
            valueStream.Position = 0L;
            byte[] array = new byte[valueStream.Length];
            valueStream.Read(array, 0, (int)valueStream.Length);
            SqlParameter sqlParameter = new SqlParameter()
            {
                Direction = ParameterDirection.Input,
                ParameterName = name,
                SqlDbType = type,
                Value = array
            };
            _parameters.Add(sqlParameter);
            return sqlParameter;
        }

        public SqlParameter AddParameter(string name, SqlDbType type, object value, int size)
        {
            SqlParameter sqlParameter = new SqlParameter()
            {
                Direction = ParameterDirection.Input,
                ParameterName = name,
                SqlDbType = type,
                Size = size,
                Value = PrepareSqlValue(value)
            };
            _parameters.Add(sqlParameter);
            return sqlParameter;
        }

        public SqlParameter AddParameter(string name, DbType type, object value, bool convertZeroToDBNull)
        {
            SqlParameter sqlParameter = new SqlParameter()
            {
                Direction = ParameterDirection.Input,
                ParameterName = name,
                DbType = type,
                Value = PrepareSqlValue(value, convertZeroToDBNull)
            };
            _parameters.Add(sqlParameter);
            return sqlParameter;
        }

        public SqlParameter AddParameter(string name, SqlDbType type, object value, bool convertZeroToDBNull)
        {
            SqlParameter sqlParameter = new SqlParameter()
            {
                Direction = ParameterDirection.Input,
                ParameterName = name,
                SqlDbType = type,
                Value = PrepareSqlValue(value, convertZeroToDBNull)
            };
            _parameters.Add(sqlParameter);
            return sqlParameter;
        }

        public SqlDataReader ExecuteSqlReader(string sql)
        {
            SqlCommand sqlCommand = new SqlCommand();
            this.Connect();
            sqlCommand.CommandTimeout = this.CommandTimeout;
            sqlCommand.CommandText = sql;
            sqlCommand.Connection = Connection as SqlConnection;
            if (Transaction != null)
            {
                sqlCommand.Transaction = Transaction as SqlTransaction;
            }
            sqlCommand.CommandType = CommandType.Text;
            this.CopyParameters(sqlCommand);
            CommandBehavior commandBehavior = CommandBehavior.Default;
            if (this.AutoCloseConnection)
            {
                commandBehavior |= CommandBehavior.CloseConnection;
            }
            if (IsSingleRow)
            {
                commandBehavior |= CommandBehavior.SingleRow;
            }
            SqlDataReader result = sqlCommand.ExecuteReader(commandBehavior);
            sqlCommand.Dispose();
            return result;
        }

        public void ExecuteSP(string procedureName)
        {
            Connect();
            SqlCommand sqlCommand = new SqlCommand()
            {
                CommandTimeout = CommandTimeout,
                CommandText = procedureName,
                Connection = this.Connection as SqlConnection
            };
            if (Transaction != null)
            {
                sqlCommand.Transaction = Transaction as SqlTransaction;
            }
            sqlCommand.CommandType = CommandType.StoredProcedure;
            this.CopyParameters(sqlCommand);
            sqlCommand.ExecuteNonQuery();
            sqlCommand.Dispose();
            if (AutoCloseConnection)
            {
                Dispose();
            }
        }

        public DataSet ExecuteSqlDataSet(string sql)
        {
            this.Connect();
            var dataAdapter = new SqlDataAdapter(sql, Connection as SqlConnection);
            DataSet dataSet = new DataSet();
            dataAdapter.Fill(dataSet);
            dataAdapter.Dispose();
            if (AutoCloseConnection)
            {
                Dispose();
            }
            return dataSet;
        }

        #endregion

        public void Dispose()
        {
            if (Connection != null)
            {
                if (Connection.State != ConnectionState.Closed)
                    Connection.Close();
                Connection.Dispose();
            }

            if (Transaction != null)
            {
                Transaction.Dispose();
            }
            Connection = null;
            Transaction = null;

        }

        public void Connect()
        {
            if (Connection != null)
            {
                if (Connection.State != ConnectionState.Open)
                {
                    Connection.Open();
                    return;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(ConnectionString))
                {
                    throw new InvalidOperationException("You must set a connection object or specify a connection string before calling Connect.");
                }
                StringCollection stringCollection = new StringCollection();
                stringCollection.AddRange(new string[]
                {
                    "ARITHABORT",
                    "ANSI_NULLS",
                    "ANSI_WARNINGS",
                    "ARITHIGNORE",
                    "ANSI_DEFAULTS",
                    "ANSI_NULL_DFLT_OFF",
                    "ANSI_NULL_DFLT_ON",
                    "ANSI_PADDING",
                    "ANSI_WARNINGS"
                });
                StringBuilder sqlSetStringBuilder = new StringBuilder();
                StringBuilder connectionStringBuilder = new StringBuilder();
                Hashtable hashtable = ParseConfigString(ConnectionString);
                foreach (object obj in hashtable.Keys)
                {
                    string text = (string)obj;
                    if (stringCollection.Contains(text.Trim().ToUpper()))
                    {
                        sqlSetStringBuilder.AppendFormat("SET {0} {1};", text, hashtable[text]);
                    }
                    else if (text.Trim().Length > 0)
                    {
                        connectionStringBuilder.AppendFormat("{0}={1};", text, hashtable[text]);
                    }
                }
                Connection = new SqlConnection(connectionStringBuilder.ToString());
                Connection.Open();
                if (sqlSetStringBuilder.Length > 0)
                {
                    IDbCommand command = Connection.CreateCommand();
                    command.CommandTimeout = CommandTimeout;
                    command.CommandText = sqlSetStringBuilder.ToString();
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                    command.Dispose();
                    return;
                }
            }
        }

        #region common private 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        private Hashtable ParseConfigString(string config)
        {
            Hashtable hashtable = new Hashtable(10, StringComparer.InvariantCultureIgnoreCase);
            string[] array = config.Split(';');
            for (int i = 0; i < array.Length; i++)
            {
                string[] array2 = array[i].Split('=');
                if (array2.Length == 2)
                {
                    hashtable.Add(array2[0].Trim(), array2[1].Trim());
                }
                else
                {
                    hashtable.Add(array[i].Trim(), null);
                }
            }
            return hashtable;
        }

        private object PrepareSqlValue(object value, bool convertZeroToDBNull)
        {
            if (value is string)
            {
                if (ConvertEmptyValuesToDbNull && (string)value == string.Empty)
                {
                    return DBNull.Value;
                }
                return value;
            }
            else if (value is Guid)
            {
                if (ConvertEmptyValuesToDbNull && (Guid)value == Guid.Empty)
                {
                    return DBNull.Value;
                }
                return value;
            }
            else if (value is DateTime)
            {
                if ((ConvertMinValuesToDbNull && (DateTime)value == DateTime.MinValue) || (ConvertMaxValuesToDbNull && (DateTime)value == DateTime.MaxValue))
                {
                    return DBNull.Value;
                }
                return value;
            }
            else if (value is short)
            {
                if ((ConvertMinValuesToDbNull && (short)value == short.MinValue) || (ConvertMaxValuesToDbNull && (short)value == short.MaxValue) || (convertZeroToDBNull && (short)value == 0))
                {
                    return DBNull.Value;
                }
                return value;
            }
            else if (value is int)
            {
                if ((ConvertMinValuesToDbNull && (int)value == int.MinValue) || (ConvertMaxValuesToDbNull && (int)value == int.MaxValue) || (convertZeroToDBNull && (int)value == 0))
                {
                    return DBNull.Value;
                }
                return value;
            }
            else if (value is long)
            {
                if ((ConvertMinValuesToDbNull && (long)value == long.MinValue) || (ConvertMaxValuesToDbNull && (long)value == long.MaxValue) || (convertZeroToDBNull && (long)value == 0L))
                {
                    return DBNull.Value;
                }
                return value;
            }
            else if (value is float)
            {
                if ((ConvertMinValuesToDbNull && (float)value == float.MinValue) || (ConvertMaxValuesToDbNull && (float)value == float.MaxValue) || (convertZeroToDBNull && (float)value == 0f))
                {
                    return DBNull.Value;
                }
                return value;
            }
            else if (value is double)
            {
                if ((ConvertMinValuesToDbNull && (double)value == double.MinValue) || (ConvertMaxValuesToDbNull && (double)value == double.MaxValue) || (convertZeroToDBNull && (double)value == 0D))
                {
                    return DBNull.Value;
                }
                return value;
            }
            else
            {
                if (!(value is decimal))
                {
                    return value;
                }
                if ((ConvertMinValuesToDbNull && (decimal)value == decimal.MinValue) || (ConvertMaxValuesToDbNull && (decimal)value == decimal.MaxValue) || (convertZeroToDBNull && (decimal)value == decimal.Zero))
                {
                    return DBNull.Value;
                }
                return value;
            }
        }

        private object PrepareSqlValue(object value)
        {
            return PrepareSqlValue(value, false);
        }

        private void CopyParameters(SqlCommand command)
        {
            for (int i = 0; i < _parameters.Count; i++)
            {
                command.Parameters.Add(_parameters[i]);
            }
        }

        public void Reset()
        {
            if (this._parameters != null)
            {
                this._parameters.Clear();
            }
            if (this._parameterCollection != null)
            {
                this._parameterCollection = null;
            }
        }


        protected ArrayList _parameters = new ArrayList();
        protected SqlParameterCollection _parameterCollection;
        #endregion
    }
}
