using System;
using System.Collections.Generic;

namespace Elars.CsvToSql.Core.DatabaseTypes
{
    internal class MySql : IDatabaseType
    {
        public bool SupportsNoCount => false;
        public bool SupportsGoStatements => false;
        public string DateFormat => "yyyy-MM-dd";

        public string DateTimeFormat => "yyyy-MM-dd hh:mm:ss tt";

        public string CreateIndex(string tableName, string column, bool clustered, bool allowSpaces)
        {
            if (clustered)
                return $"ALTER TABLE {EscapeName(tableName, allowSpaces)} ADD PRIMARY KEY ({EscapeName(column, allowSpaces)});";

            var indexName = $"ix_{SchemaName(tableName, false)}_{SchemaName(column, false)}";
            return $"ALTER TABLE {EscapeName(tableName, allowSpaces)} ADD INDEX {indexName} ({EscapeName(column, allowSpaces)});";
        }

        public string CreateTable(bool tempTable, string tableName)
        {
            if (tempTable)
                return "CREATE TEMPORARY TABLE " + tableName + " (";

            return "CREATE TABLE " + tableName + " (";
        }

        public string DropTable(bool tempTable, string tableName)
        {
            if (tempTable)
                return "DROP TEMPORARY TABLE IF EXISTS " + tableName + ";";

            return "DROP TABLE IF EXISTS " + tableName + ";";
        }

        public string EscapeName(string name, bool allowSpaces)
        {
            return $"`{SchemaName(name, allowSpaces)}`";
        }

        private string SchemaName(string name, bool allowSpaces)
        {
            return allowSpaces ? name : name.Replace(" ", "");
        }

        public string IdentityConstraint(string tableName, string column, bool allowSpaces)
        {
            return $"CONSTRAINT `PK_{tableName}` PRIMARY KEY (`{column}`)";
        }

        public string IdentityInsert(string tableName, bool on)
        {
            return $"SET FOREIGN_KEY_CHECKS = {(on ? 0 : 1)};";
        }

        public string NoCount(bool on)
        {
            throw new NotImplementedException();
        }

        public string Reseed(string tableName, string columnName, bool allowSpaces)
        {
            var sql = $"SET @m = (SELECT MAX({EscapeName(columnName, allowSpaces)}) + 1 FROM {tableName});" + Environment.NewLine;
            sql += $"SET @s = CONCAT('ALTER TABLE {tableName} AUTO_INCREMENT = ', @m);" + Environment.NewLine;
            sql += "PREPARE stmt1 FROM @s;" + Environment.NewLine;
            sql += "EXECUTE stmt1;" + Environment.NewLine;
            sql += "DEALLOCATE PREPARE stmt1;";

            return sql;
        }

        public string TempTableName(string tableName, bool tempTable)
        {
            return tableName;
        }

        public string IdentityColumnIncrement()
        {
            return "AUTO_INCREMENT";
        }
    }
}
