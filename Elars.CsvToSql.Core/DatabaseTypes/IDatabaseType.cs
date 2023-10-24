namespace Elars.CsvToSql.Core.DatabaseTypes
{
    internal interface IDatabaseType
    {
        string DateFormat { get; }
        string DateTimeFormat { get; }
        bool SupportsNoCount { get; }
        bool SupportsGoStatements { get; }
        string NoCount(bool on);
        string DropTable(bool tempTable, string tableName);
        string CreateTable(bool tempTable, string tableName);
        string EscapeName(string name, bool allowSpaces);
        string CreateIndex(string tableName, string column, bool clustered, bool allowSpaces);
        string IdentityInsert(string tableName, bool on);
        string Reseed(string tableName, string columnName, bool allowSpaces);
        string TempTableName(string tableName, bool tempTable);
        string IdentityConstraint(string tableName, string columnName, bool allowSpaces);
        string IdentityColumnIncrement();
    }
}
