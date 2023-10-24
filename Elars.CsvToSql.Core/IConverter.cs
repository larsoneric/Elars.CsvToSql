using System.Collections.Generic;
using System.Threading.Tasks;

namespace Elars.CsvToSql.Core
{
    public interface IConverter
    {
        bool AllowSpaces { get; set; }
        int BatchSize { get; set; }
        bool ClusteredIndex { get; set; }
        bool CreateIndex { get; set; }
        bool AllowNulls { get; set; }
        bool CreateIdentity { get; set; }
        bool CreateTable { get; set; }
        DatabaseTypes.DatabaseTypes DatabaseType { get; set; }
        int DecimalPlaces { get; set; }
        bool IdentityInsert { get; set; }
        bool IncludeTime { get; set; }
        int IndexColumn { get; set; }
        int IdentityColumn { get; set; }
        bool IsTempTable { get; set; }
        bool NoCount { get; set; }
        int NumberOfRecords { get; }
        bool Reseed { get; set; }
        IList<string> StringFields { get; set; }
        int StringSize { get; set; }
        string TableName { get; set; }
        bool TruncateTable { get; set; }
        bool UseGoStatements { get; set; }

        Task<string> ProcessFile(string csvFilePath);
        Task<string> ProcessString(string csvText);
    }
}