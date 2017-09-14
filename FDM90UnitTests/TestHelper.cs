using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FDM90UnitTests
{
    public class TestHelper
    {
        public static bool CheckSqlStatementString(StatementType statementType, string sqlTable, string[] columns,
                                                        string[] parameters, string stringToTest, int skipLastAmount = 0)
        {
            Regex format = null;

            switch (statementType)
            {
                case StatementType.Select:
                    string select = @"SELECT \* FROM\s.*" + SqlTable(sqlTable);

                    if (parameters.Length > 0)
                    {
                        select += CreateWhereCondition(columns, parameters);
                    }

                    select += @".*;";

                    format = new Regex(select);
                    break;

                case StatementType.Insert:
                    format = new Regex(@"INSERT INTO\s.*" + SqlTable(sqlTable) + @"." + CreateColumnString(columns)
                        + @". VALUES ." + CreateParameterString(parameters) + @";");
                    break;

                case StatementType.Update:
                    format = new Regex(@"UPDATE\s.*" + SqlTable(sqlTable) +
                                    @"\s.*SET\s.*" + CreateColumnParameter(columns, parameters, skipLastAmount) 
                                    + CreateWhereCondition(columns.Skip(columns.Count() - skipLastAmount).ToArray(),
                                                    parameters.Skip(parameters.Count() - skipLastAmount).ToArray()) + @"\s;");
                    break;
                case StatementType.Delete:
                    format = new Regex(@"DELETE FROM\s.*" + SqlTable(sqlTable) + CreateWhereCondition(columns, parameters));
                    break;

                case StatementType.Batch:
                    throw new ArgumentOutOfRangeException(nameof(statementType), statementType, null);
                default:
                    throw new ArgumentOutOfRangeException(nameof(statementType), statementType, null);
            }

            return format.IsMatch(stringToTest);
        }

        private static string CreateColumnParameter(string[] columns, string[] parameters, int skipLastAmount = 0)
        {
            string columnParams = string.Empty;
            int count = skipLastAmount > 0 ? columns.Length - skipLastAmount : columns.Length;
            for (int i = 0; i < count; i++)
            {
                columnParams += @"\[" + columns[i] + @"\]\s=\s" + parameters[i] + @".";
            }

            return columnParams.Substring(0, columnParams.LastIndexOf(@"."));
        }

        public static string CreateColumnString(string[] columns)
        {
            string columnString = string.Empty;

            foreach (var col in columns)
            {
                columnString += @"\[" + col + @"\].\s";
            }
            return columnString.Substring(0, columnString.LastIndexOf('.'));
        }

        public static string CreateParameterString(string[] parameters)
        {
            string parameterString = string.Empty;

            foreach (var param in parameters)
            {
                parameterString += param + @".\s";
            }

            return parameterString;
        }

        public static string CreateWhereCondition(string[] columns, string[] parameters)
        {
            string where = @"\s.*WHERE\s.*";

            for (int i = 0; i < columns.Length; i++)
            {
                where += @"\[" + columns[i] + @"\]\s.*=\s.*" + parameters[i] + @"\s.*AND\s.*";
            }

            return where.Substring(0, where.LastIndexOf(@"\s.*AND\s.*"));
        }

        public static string SqlTable(string sqlTable)
        {
            return sqlTable.Replace("[", @"\[").Replace("]", @"\]").Replace(".", @"\.");
        }
    }
}
