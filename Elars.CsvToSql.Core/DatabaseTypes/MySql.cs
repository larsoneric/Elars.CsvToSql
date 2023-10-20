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

        public string CreateIndex(string tableName, string column, bool clustered)
        {
            if (clustered)
                return $"ALTER TABLE {tableName} ADD PRIMARY KEY ({column});";

            var header = column.Trim('[', ']');
            var indexName = $"ix_{tableName.Replace(" ", "")}_{header.Replace(" ", "")}";
            return $"ALTER TABLE {tableName} ADD INDEX {indexName} ({column});";
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
            return $"`{(allowSpaces ? name : name.Replace(" ", ""))}`";
        }

        public string IdentityInsert(string tableName, bool on)
        {
            return $"SET FOREIGN_KEY_CHECKS = {(on ? 0 : 1)};";
        }

        public string NoCount(bool on)
        {
            throw new NotImplementedException();
        }

        public string Reseed(string tableName, string columnName)
        {
            var sql = $"SET @m = (SELECT MAX({columnName}) + 1 FROM `{tableName}`);" + Environment.NewLine;
            sql += $"SET @s = CONCAT('ALTER TABLE `{tableName}` AUTO_INCREMENT = ', @m);" + Environment.NewLine;
            sql += "PREPARE stmt1 FROM @s;" + Environment.NewLine;
            sql += "EXECUTE stmt1;" + Environment.NewLine;
            sql += "DEALLOCATE PREPARE stmt1;";

            return sql;
        }

        public string TempTableName(string tableName, bool tempTable)
        {
            return tableName;
        }
    }
}
