using Microsoft.Data.Sqlite;

namespace AnkiNet.Helpers;

internal static class SqLiteHelper
{
    internal static void ExecuteSqLiteCommand(SqliteConnection conn, string toExecute)
    {
        try
        {
            using SqliteCommand command = new SqliteCommand(toExecute, conn);
            command.ExecuteNonQuery();
        }
        catch (Exception)
        {
            throw new Exception("Can't execute query : " + toExecute);
        }
    }
}
