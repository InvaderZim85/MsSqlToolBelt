using MsSqlToolBelt.Business;
using MsSqlToolBelt.Ui.View.Common;

namespace MsSqlToolBelt.Ui.View.Controls;

internal interface IUserControl : IConnection
{
    /// <summary>
    /// Init the control
    /// </summary>
    /// <param name="manager">The instance of the settings manager</param>
    public void InitControl(SettingsManager manager);
}