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

    /// <summary>
    /// Loads the data of the control
    /// </summary>
    /// <param name="showProgress"><see langword="true"/> to show the progress dialog of the control, otherwise <see langword="false"/></param>
    void LoadData(bool showProgress);
}