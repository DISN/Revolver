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
using Revolver.Tools.EntityCreator.ViewModels;

namespace Revolver.Tools.EntityCreator
{
  [Module(ModuleName = "Revolver.Tools.EntityCreator")]
  public class EntityCreatorModule : IModule
  {
    private readonly IUnityContainer _container;

    #region Constructors
    public EntityCreatorModule(IUnityContainer container)
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
      EventAggregator.GetEvent<SplashMessageUpdateEvent>().Publish(new SplashMessageUpdateEvent { Message = "Loading Entity Creator Module" });
      _container.RegisterType<EntityCreatorToolSettings>(new ContainerControlledLifetimeManager());
      _container.RegisterType<EntityCreatorViewModel>();
      IWorkspace workspace = _container.Resolve<WorkspaceBase>();
      workspace.Tools.Add(_container.Resolve<EntityCreatorViewModel>());
    }

    #endregion
  }
}