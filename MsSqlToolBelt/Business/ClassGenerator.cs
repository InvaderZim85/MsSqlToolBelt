using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MsSqlToolBelt.DataObjects;
using MsSqlToolBelt.DataObjects.ClassGenerator;

namespace MsSqlToolBelt.Business
{
    /// <summary>
    /// Provides the functions to create a class of a table
    /// </summary>
    internal static class ClassGenerator
    {
        /// <summary>
        /// Contains the tab indent
        /// </summary>
        private static readonly string Tab = new(' ', 4);

        /// <summary>
        /// Generates the code
        /// </summary>
        /// <param name="table">The selected table</param>
        /// <param name="className">The name of the desired class</param>
        /// <param name="backingField">true to create a backing field, otherwise false</param>
        /// <returns>The CSharp code and the sql statement</returns>
        public static (string ClassCode, string SqlStatement) Generate(Table table, string className, bool backingField)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var classCode = GenerateClass(table.Columns, string.IsNullOrEmpty(className) ? table.Name : className, backingField);

            var sql = GenerateSqlQuery(table);

            return (classCode, sql);
        }

        /// <summary>
        /// Generates the code of the CSharp class
        /// </summary>
        /// <param name="columns">The list with the columns</param>
        /// <param name="className">The name of the class</param>
        /// <param name="backingField">true to create a backing field, otherwise false</param>
        /// <returns>The CSharp code</returns>
        private static string GenerateClass(List<TableColumn> columns, string className, bool backingField)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"public class {className.FirstCharToUpper()}");
            sb.AppendLine("{");

            var count = 1;
            var columnCount = columns.Count(c => c.Use);
            foreach (var column in columns.Where(w => w.Use))
            {
                var fieldName = string.IsNullOrEmpty(column.Alias) ? column.Column.Replace(" ", "") : column.Alias;
                if (fieldName.IsNumeric())
                    fieldName = $"Column{fieldName}";

                var dataType = GetDataType(column.DataType);
                if (backingField)
                {
                    
                    var field = $"_{fieldName.FirstCharToLower()}";

                    sb.AppendLine($"{Tab}private {dataType} {field};");
                    sb.AppendLine($"{Tab}public {dataType} {fieldName}");
                    sb.AppendLine($"{Tab}{{");
                    sb.AppendLine($"{Tab}{Tab}get => {field};");
                    sb.AppendLine($"{Tab}{Tab}set => {field} = value;");
                    sb.AppendLine($"{Tab}}}");
                }
                else
                {
                    sb.AppendLine($"{Tab}public {dataType} {fieldName} {{ get; set; }}");
                }

                // Add only a space when there are columns left
                if (count != columnCount)
                    sb.AppendLine();

                count++;
            }

            sb.AppendLine("}");

            return sb.ToString();
        }

        /// <summary>
        /// Generates the sql query for the selected columns
        /// </summary>
        /// <param name="table">The table name</param>
        /// <returns></returns>
        private static string GenerateSqlQuery(Table table)
        {
            var sb = new StringBuilder();

            sb.AppendLine("SELECT");

            var count = 1;
            var columnCount = table.Columns.Count(w => w.Use);
            foreach (var column in table.Columns.Where(w => w.Use))
            {
                var comma = count++ == columnCount ? "" : ",";

                sb.AppendLine(string.IsNullOrEmpty(column.Alias)
                    ? $"{Tab}[{column.Column}]{comma}"
                    : $"{Tab}[{column.Column}] AS [{column.Alias}]{comma}");
            }

            sb.AppendLine("FROM");
            sb.AppendLine($"{Tab}[{table.Schema}].[{table.Name}]");

            return sb.ToString();
        }

        /// <summary>
        /// Gets the corresponding c# data type of the sql type
        /// </summary>
        /// <param name="sqlType">The sql data type</param>
        /// <returns>The c# data type</returns>
        private static string GetDataType(string sqlType)
        {
            return sqlType.ToLower() switch
            {
                "bigint" => "double",
                "smallmoney" => "decimal",
                "money" => "decimal",
                "decimal" => "decimal",
                "numeric" => "decimal",
                "int" => "int",
                "tinyint" => "int",
                "smallint" => "int",
                "bit" => "bool",
                "nvarchar" => "string",
                "char" => "string",
                "text" => "string",
                "varchar" => "string",
                "nchar" => "string",
                "ntext" => "string",
                "date" => "DateTime",
                "datetime" => "DateTime",
                "datetime2" => "DateTime",
                "uniqueidentifier" => "GUID",
                _ => sqlType
            };
        }
    }
}
