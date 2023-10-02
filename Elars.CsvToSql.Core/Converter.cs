using Elars.CsvToSql.Core.Extensions;
using Sylvan.Data.Csv;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elars.CsvToSql.Core
{
    /// <summary>
    /// Convert CSV and tab-delimited to SQL INSERT statements
    /// </summary>
    public class Converter
    {
        /// <summary>
        /// Flag indicating whether to generate the CREATE TABLE staement
        /// Default: true
        /// </summary>
        public bool CreateTable { get; set; } = true;

        /// <summary>
        /// Allow spaces in table and column names
        /// Default: false
        /// </summary>
        public bool AllowSpaces { get; set; } = false;

        /// <summary>
        /// Name of the database table to insert records into
        /// Default: #tmp
        /// </summary>
        public string TableName { get; set; } = "#tmp";

        /// <summary>
        /// Size of INSERT batches; set to 1 to not use batches
        /// Default: 50
        /// </summary>
        public int BatchSize { get; set; } = 50;

        /// <summary>
        /// Toggle IDENTITY_INSERT to allow ID fields to be inserted
        /// Default: false
        /// </summary>
        public bool IdentityInsert { get; set; } = false;

        /// <summary>
        /// Toggle NOCOUNT to prevent records from being counted
        /// Default: true
        /// </summary>
        public bool NoCount { get; set; } = true;

        /// <summary>
        /// true: create clustered index
        /// false: create nonclustered index
        /// </summary>
        /// <value>Default: false (non-clustered)</value>
        public bool ClusteredIndex { get; set; } = false;
        
        /// <summary>
        /// Index of which column to create an index of
        /// If NULL, don't create an index
        /// Default: null
        /// </summary>
        public int? IndexColumn { get; set; } = null;
        
        /// <summary>
        /// Reseed primary key, useful when IDENTITY_INSERT is on
        /// Assumes the first column is the index
        /// Default: false
        /// </summary>
        public bool Reseed { get; set; } = false;

        /// <summary>
        /// Fields that should always be treated as strings (e.g. Zip Code)
        /// Default: none
        /// </summary>
        public IList<string> StringFields { get; set; } = new List<string>();

        /// <summary>
        /// Size for VARCHAR fields
        /// Default: 100
        /// </summary>
        public int StringSize { get; set; } = 100;

        /// <summary>
        /// How many decimal places should DECIMAL fields have
        /// Default: 2
        /// </summary>
        public int DecimalPlaces { get; set; } = 2;

        /// <summary>
        /// true: Include times in Date/Time fields
        /// false: Only include the date in Date/Time fields
        /// Default: false
        /// </summary>
        public bool IncludeTime { get; set; } = false;
        
        /// <summary>
        /// Delete all records from a table before inserting
        /// Default: false
        /// </summary>
        public bool TruncateTable { get; set; } = false;

        /// <summary>
        /// Count of records processed
        /// </summary>
        public int NumberOfRecords { get; private set; }

        /// <summary>
        /// Convert a .csv or .txt file to SQL INSERT statements
        /// </summary>
        /// <param name="csvFilePath">File path of .csv or .txt</param>
        /// <returns>SQL script</returns>
        /// <exception cref="FileNotFoundException"></exception>
        public async Task<string> ProcessFile(string csvFilePath)
        {
            if (!File.Exists(csvFilePath))
            {
                throw new FileNotFoundException();
            }

            var text = await File.ReadAllTextAsync(csvFilePath);
            return await ProcessString(text);
        }

        /// <summary>
        /// Convert CSV or tab-delimited records to SQL INSERT statements
        /// </summary>
        /// <param name="csvText">Text of CSV or tab-delimited records</param>
        /// <returns>SQL script</returns>
        /// <exception cref="Exception"></exception>
        public async Task<string> ProcessString(string csvText)
        {
            if (string.IsNullOrWhiteSpace(TableName))
            {
                throw new Exception("Table name is required");
            }

            var csv = await CsvDataReader.CreateAsync(new StringReader(csvText));
            if (csv == null || !csv.HasRows)
            {
                throw new Exception("Data is empty");
            }

            return await GenerateSql(csv);
        }

        private string CreateTableSql(CsvDataReader csv, string tableName)
        {
            if (!CreateTable)
            {
                return string.Empty;
            }

            string objectId = tableName.Contains('#') ? $"tempdb..{tableName}" : tableName;

            var sql = $"IF OBJECT_ID('{objectId}', 'U') IS NOT NULL DROP TABLE {tableName};{Environment.NewLine}{Environment.NewLine}";

            sql += $"CREATE TABLE {tableName} ({Environment.NewLine}";

            var columns = Enumerable.Range(0, csv.FieldCount)
                .Select(i => $"  {EscapedName(csv.GetName(i), AllowSpaces)} {SqlColumnType(csv.GetFieldTypeExtended(i, StringFields))}");

            sql += string.Join(", " + Environment.NewLine, columns);

            sql += $");{Environment.NewLine}GO{Environment.NewLine}{Environment.NewLine}";

            return sql;
        }

        private string SqlColumnType(Type type)
        {
            if (type == typeof(int))
                return "INT";

            if (type == typeof(decimal))
                return $"DECIMAL({18 - DecimalPlaces},{DecimalPlaces})";

            if (type == typeof(DateTime) && IncludeTime)
                return "DATETIME";

            if (type == typeof(DateTime))
                return "DATE";

            return $"VARCHAR({StringSize})";
        }

        private static string EscapedName(string name, bool allowSpaces)
        {
            return $"[{(allowSpaces ? name : name.Replace(" ", ""))}]";
        }

        private async Task<string> GenerateSql(CsvDataReader csv)
        {
            var sql = new StringBuilder();

            var tableName = EscapedName(TableName, AllowSpaces);
            sql.Append(CreateTableSql(csv, tableName));

            if (TruncateTable)
            {
                sql.Append($"TRUNCATE TABLE {tableName};{Environment.NewLine}{Environment.NewLine}");
            }

            var columns = Enumerable.Range(0, csv.FieldCount)
                .Select(i => EscapedName(csv.GetName(i), AllowSpaces));

            var insertStatement = $"INSERT INTO {tableName} ({string.Join(", ", columns)}) VALUES ";
            var valueCollection = new List<string>();

            while (await csv.ReadAsync())
            {
                var values = Enumerable.Range(0, csv.FieldCount)
                    .Select(i => EscapeValue(csv.GetValue(i), csv.GetFieldTypeExtended(i, StringFields)));

                valueCollection.Add("(" + string.Join(", ", values) + ")");
            }

            NumberOfRecords = valueCollection.Count;

            if (BatchSize > 1)
            {
                insertStatement = $"{insertStatement}{Environment.NewLine}    ";

                for (var i = 0; i < valueCollection.Count; i += BatchSize)
                {
                    var batch = valueCollection.Skip(i).Take(BatchSize);
                    sql.AppendLine(insertStatement + string.Join($",{Environment.NewLine}    ", batch) + ";");
                    sql.AppendLine("GO" + Environment.NewLine);
                }
            }
            else
            {
                sql.AppendLine(string.Join($"{Environment.NewLine}", valueCollection.Select(s => insertStatement + s + ";")));
                sql.AppendLine();
            }

            if (IndexColumn.HasValue)
            {
                var header = csv.GetName(IndexColumn.Value + 1).Trim('[', ']');
                var indexType = ClusteredIndex ? "CLUSTERED INDEX cx" : "NONCLUSTERED INDEX ix";

                sql.AppendLine($"CREATE {indexType}_{TableName.Replace("#", "").Replace(" ", "")}_{header.Replace(" ", "")} ON {tableName} ([{header}]);");
                sql.AppendLine("GO");
                sql.AppendLine();
            }

            if (IdentityInsert)
            {
                sql.Insert(0, $"SET IDENTITY_INSERT {tableName} ON;{Environment.NewLine}{Environment.NewLine}");
                sql.AppendLine($"SET IDENTITY_INSERT {tableName} OFF;");

                if (Reseed)
                {
                    sql.AppendLine($"{Environment.NewLine}DECLARE @Id INT = (SELECT MAX([{csv.GetName(0)}]) FROM {tableName});");
                    sql.AppendLine($"DBCC CHECKIDENT ('{tableName}', RESEED, @Id);");
                }
            }

            if (NoCount)
            {
                sql.Insert(0, $"SET NOCOUNT ON;{Environment.NewLine}{Environment.NewLine}");
                sql.AppendLine($"SET NOCOUNT OFF;");
            }

            return sql.ToString();
        }
        
        private string EscapeValue(object value, Type type)
        {
            if (value == null)
                return "NULL";

            if (type == typeof(DateTime))
            {
                if (DateTime.TryParse(value.ToString(), out var date) && date != DateTime.MinValue)
                {
                    if (IncludeTime)
                    {
                        return "'" + date.ToString("yyyyMMdd hh:mm:ss tt") + "'";
                    }

                    return "'" + date.ToString("yyyyMMdd") + "'";
                }

                return "NULL";
            }

            if (type == typeof(int) || type == typeof(decimal))
            {
                return value.ToString();
            }

            if (value.ToString().Equals("null", StringComparison.OrdinalIgnoreCase))
                return "NULL";

            return "'" + value.ToString().Replace("'", "''") + "'";
        }
    }
}
