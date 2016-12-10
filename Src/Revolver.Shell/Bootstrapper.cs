using Engine.Wrappers;
using Magnum.Core.Windows;
using Magnum.Tools.Magnum.Core;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.UnityExtensions;
using Microsoft.Practices.Unity;
using Revolver.Tools.Splash;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Revolver.Shell
{
  public class Bootstrapper : UnityBootstrapper
  {
    #region Constructors
    public Bootstrapper()
    {
    }
    #endregion

    #region Properties
    public bool HideSplashWindow { get; set; }
    #endregion

    #region Methods
    protected override void InitializeModules()
    {
      HideSplashWindow = false;
      if (!HideSplashWindow)
      {
        IModule splashModule = Container.Resolve<SplashModule>();
        splashModule.Initialize();
      }

      IModule toolsModule = Container.Resolve<ToolsModule>();
      toolsModule.Initialize();

      IModule wrappersModule = Container.Resolve<WrappersModule>();
      wrappersModule.Initialize();

      //Register a workspace here
      Container.RegisterType<WorkspaceBase, RevolverWorkspace>(new ContainerControlledLifetimeManager());

      IModule shellModule = Container.Resolve<ShellModule>();
      shellModule.Initialize();

      base.InitializeModules();

      Application.Current.MainWindow.DataContext = Container.Resolve<WorkspaceBase>();
      Magnum.Core.ApplicationModel.Application.IsInitialized = true;
      if (HideSplashWindow)
      {
        (Shell as Window).Show();
      }
    }

    protected override void ConfigureContainer()
    {
      //Create an instance of the workspace
      Container.RegisterType<IShell, MainWindow.MainWindow>(new ContainerControlledLifetimeManager());
      base.ConfigureContainer();
    }

    protected override DependencyObject CreateShell()
    {
      return (DependencyObject)Container.Resolve<IShell>();
    }

    protected override void InitializeShell()
    {
      base.InitializeShell();
      Application.Current.MainWindow = (Window)Shell;
    }

    public void CloseShell()
    {
      if (Application.Current != null && Application.Current.MainWindow != null)
        Application.Current.MainWindow.Close();
    }
    #endregion
  }
}
