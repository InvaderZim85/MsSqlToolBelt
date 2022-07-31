namespace MsSqlToolBelt.Ui.View.Common;

internal interface IConnection
{
    /// <summary>
    /// Sets the connection
    /// </summary>
    /// <param name="dataSource">The name / path of the MSSQL server</param>
    /// <param name="database">The name of the database</param>
    void SetConnection(string dataSource, string database);

    /// <summary>
    /// Closes the connection
    /// </summary>
    void CloseConnection();
}