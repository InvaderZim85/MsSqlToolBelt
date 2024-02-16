using MsSqlToolBelt.Business;

namespace MsSqlToolBelt.Ui.View.Controls;

internal interface IUserControl
{
    /// <summary>
    /// Init the control
    /// </summary>
    /// <param name="manager">The instance of the settings manager</param>
    public void InitControl(SettingsManager manager);
}