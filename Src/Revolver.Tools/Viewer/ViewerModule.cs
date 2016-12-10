using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Unity;
using Magnum.Core.Windows;
using Magnum.Core.Events;
using Revolver.Tools.Splash.ViewModels;
using Revolver.Tools.Splash.Views;
using Revolver.Tools.WorldEditor.ViewModels;
using Revolver.Tools.Viewer.ViewModels;
using Revolver.Tools.Viewer.Models;

namespace Revolver.Tools.Viewer
{
  [Module(ModuleName = "Revolver.Tools.Viewer")]
  public class ViewerModule : IModule
  {
    private readonly IUnityContainer _container;

    #region Constructors
    public ViewerModule(IUnityContainer container)
    {
      _container = container;
    }
    #endregion

    private IEventAggregator EventAggregator
    {
      get { return _container.Resolve<IEventAggregator>(); }
    }

    #region IModule Members

    public void Initialize()
    {
      EventAggregator.GetEvent<SplashMessageUpdateEvent>().Publish(new SplashMessageUpdateEvent { Message = "Loading Viewer Module" });
      _container.RegisterType<ViewerToolSettings>(new ContainerControlledLifetimeManager());
      _container.RegisterType<ViewerViewModel>();
      IWorkspace workspace = _container.Resolve<WorkspaceBase>();
      workspace.Tools.Add(_container.Resolve<ViewerViewModel>());

      //TEMP! Just to add something to save in the manager...
      (_container.Resolve<ViewerViewModel>().Model as ViewerModel).Viewer._map_OnIsDirtyChanged(this, null);
    }

    #endregion
  }
}