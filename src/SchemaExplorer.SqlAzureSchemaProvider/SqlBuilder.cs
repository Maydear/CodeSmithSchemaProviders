using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchemaExplorer
{
    internal class SqlBuilder
    {
        private readonly StringBuilder _sqlStatements = new StringBuilder();

        public void AppendStatement(string sql)
        {
            string text = sql.Trim();
            _sqlStatements.Append(text);
            if (!text.EndsWith(";", StringComparison.OrdinalIgnoreCase))
            {
                _sqlStatements.Append(';');
            }
            _sqlStatements.AppendLine();
        }

        public override string ToString()
        {
            return _sqlStatements.ToString();
        }

        public static implicit operator string(SqlBuilder builder)
        {
            return builder.ToString();
        }


    }
}
