using Sylvan.Data.Csv;
using System;

namespace Elars.CsvToSql.Core.Extensions
{
    internal static class CsvDataReaderExtension
    {
        internal static Type GetFieldTypeExtended(this CsvDataReader reader, int index)
        {
            var value = reader.GetValue(index).ToString();

            if (DateTime.TryParse(value, out _))
                return typeof(DateTime);
            
            var cleaned = value.Replace(",", "$");
            if (int.TryParse(cleaned, out _))
                return typeof(int);

            if (decimal.TryParse(cleaned, out _))
                return typeof(decimal);

            return typeof(string);
        }
    }
}
