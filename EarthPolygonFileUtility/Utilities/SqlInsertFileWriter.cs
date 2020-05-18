using System.Collections.Generic;
using System.Linq;

namespace EarthPolygonFileUtility
{
    public class SqlInsertFileWriter
    {
        private List<string> fields;
        private List<List<string>> rows;

        public SqlInsertFileWriter(List<string> fields, List<List<string>> rows)
        {
            this.fields = fields;
            this.rows = rows;
        }

        public string CreateSqlInserts(string table)
        {
            string sql = $"INSERT INTO {table} ({string.Join(",", fields)}) VALUES ";

            List<string> rowStrs = new List<string>();
            rows.ForEach(it => rowStrs.Add($"({string.Join(",", it)})"));
            sql += string.Join(",\r\n", rowStrs);
            sql += ";";
            return sql;
        }
    }
}