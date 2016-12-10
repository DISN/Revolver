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
using Revolver.Tools.TileEditor.Models;
using Microsoft.Practices.Prism.Commands;
using Magnum.Core.ViewModels;
using Magnum.Core.Services;
using Revolver.Tools.TileEditor.ViewModels;

namespace Revolver.Tools.TileEditor
{
  [Module(ModuleName = "Revolver.Tools.TileEditor")]
  public class TileEditorModule : IModule
  {
    private readonly IUnityContainer _container;

    #region Constructors
    public TileEditorModule(IUnityContainer container)
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
      EventAggregator.GetEvent<SplashMessageUpdateEvent>().Publish(new SplashMessageUpdateEvent { Message = "Loading Tile Editor Module" });
      _container.RegisterType<TileEditorToolSettings>(new ContainerControlledLifetimeManager());
      _container.RegisterType<TileEditorViewModel>();
      IWorkspace workspace = _container.Resolve<WorkspaceBase>();
      workspace.Tools.Add(_container.Resolve<TileEditorViewModel>());
    }
    #endregion
  }
}