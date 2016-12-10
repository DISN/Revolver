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

namespace Revolver.Tools.WorldEditor
{
  [Module(ModuleName = "Revolver.Tools.WorldEditor")]
  public class WorldEditorModule : IModule
  {
    private readonly IUnityContainer _container;

    #region Constructors
    public WorldEditorModule(IUnityContainer container)
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
      EventAggregator.GetEvent<SplashMessageUpdateEvent>().Publish(new SplashMessageUpdateEvent { Message = "Loading World Editor Module" });
      _container.RegisterType<WorldEditorToolSettings>(new ContainerControlledLifetimeManager());
      _container.RegisterType<WorldEditorViewModel>();
      IWorkspace workspace = _container.Resolve<WorkspaceBase>();
      workspace.Tools.Add(_container.Resolve<WorldEditorViewModel>());
    }

    #endregion
  }
}