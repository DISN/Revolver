using Magnum.Controls.SearchBox;
using Magnum.Core.Windows;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Unity;
using System;
using System.Windows.Input;

namespace Revolver.Shell.Commands
{
  public class ShellCommands : IShellCommands
  {
    IUnityContainer _container;

    public ShellCommands(IUnityContainer container)
    {
      _container = container;

      OpenQuickLaunch = new DelegateCommand<SearchBox>(OpenQuickLaunchImpl);
    }

    #region OpenQuickLaunch

    public DelegateCommand<SearchBox> OpenQuickLaunch;

    private void OpenQuickLaunchImpl(SearchBox searchBox)
    {
      searchBox.Focus();
    }

    #endregion
  }
}
