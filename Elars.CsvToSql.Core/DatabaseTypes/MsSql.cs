using System;

namespace Elars.CsvToSql.Core.DatabaseTypes
{
    internal class MsSql : IDatabaseType
    {
        public bool SupportsGoStatements => true;
        public bool SupportsNoCount => true; 
        public string DateFormat => "yyyyMMdd";
        public string DateTimeFormat => "yyyyMMdd hh:mm:ss tt";

        public string NoCount(bool on)
        {
            return "SET NOCOUNT " + (on ? "ON" : "OFF") + ";";
        }

        public string DropTable(bool tempTable, string tableName)
        {
            string objectId = tempTable ? $"tempdb..{tableName}" : tableName;
            return $"IF OBJECT_ID('{objectId}', 'U') IS NOT NULL DROP TABLE {tableName};";
        }

        public string CreateTable(bool tempTable, string tableName)
        {
            return $"CREATE TABLE {tableName} (";
        }

        public string EscapeName(string name, bool allowSpaces)
        {
            return $"[{(allowSpaces ? name : name.Replace(" ", ""))}]";
        }

        public string CreateIndex(string tableName, string column, bool clustered)
        {
            var header = column.Trim('[', ']');
            var indexType = clustered ? "CLUSTERED INDEX cx" : "NONCLUSTERED INDEX ix";

            return $"CREATE {indexType}_{tableName.Replace(" ", "")}_{header.Replace(" ", "")} ON {tableName} ([{header}]);";
        }

        public string IdentityInsert(string tableName, bool on)
        {
            return $"SET IDENTITY_INSERT {EscapeName(tableName, false)} {( on ? "ON" : "OFF" )};";
        }

        public string Reseed(string tableName, string columnName)
        {
            return $"{Environment.NewLine}DECLARE @Id INT = (SELECT MAX([{columnName}]) FROM {tableName});{Environment.NewLine}" +
                   $"DBCC CHECKIDENT ('{tableName}', RESEED, @Id);{Environment.NewLine}";
        }

        public string TempTableName(string tableName, bool tempTable)
        {
            return tempTable ? $"#{tableName}" : tableName;
        }
    }
}
