using Microsoft.Data.SqlClient;

namespace UPLOADER
{
    /// <summary>
    /// Microsoft.Data.SqlClient's SqlDataReader only exposes GetString/GetInt32/etc.
    /// by ordinal (int), unlike MySqlDataReader which also accepted a column name.
    /// These extensions restore that convenience so the rest of the codebase
    /// (which was written against MySqlDataReader's by-name API) keeps working
    /// unchanged.
    /// </summary>
    internal static class SqlDataReaderExtensions
    {
        public static string GetString(this SqlDataReader r, string columnName)
            => r.GetString(r.GetOrdinal(columnName));

        public static int GetInt32(this SqlDataReader r, string columnName)
            => r.GetInt32(r.GetOrdinal(columnName));

        public static long GetInt64(this SqlDataReader r, string columnName)
            => r.GetInt64(r.GetOrdinal(columnName));

        public static bool GetBoolean(this SqlDataReader r, string columnName)
            => r.GetBoolean(r.GetOrdinal(columnName));

        public static DateTime GetDateTime(this SqlDataReader r, string columnName)
            => r.GetDateTime(r.GetOrdinal(columnName));

        public static decimal GetDecimal(this SqlDataReader r, string columnName)
            => r.GetDecimal(r.GetOrdinal(columnName));

        public static double GetDouble(this SqlDataReader r, string columnName)
            => r.GetDouble(r.GetOrdinal(columnName));

        public static float GetFloat(this SqlDataReader r, string columnName)
            => r.GetFloat(r.GetOrdinal(columnName));

        public static byte GetByte(this SqlDataReader r, string columnName)
            => r.GetByte(r.GetOrdinal(columnName));

        public static bool IsDBNull(this SqlDataReader r, string columnName)
            => r.IsDBNull(r.GetOrdinal(columnName));
    }
}
