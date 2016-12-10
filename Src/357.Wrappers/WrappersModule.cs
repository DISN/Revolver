using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Unity;
using System.ComponentModel;
using Magnum.Core.Events;
using Engine.Wrappers.Managers;
using Engine.Wrappers.Services;

namespace Engine.Wrappers
{
  [Module(ModuleName = "Engine.Wrappers")]
  public class WrappersModule : IModule
  {
    private readonly IUnityContainer _container;

    #region Constructors
    public WrappersModule(IUnityContainer container)
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
      EventAggregator.GetEvent<SplashMessageUpdateEvent>().Publish(new SplashMessageUpdateEvent { Message = "Loading Wrappers Module" });
      _container.RegisterType<ISaveManagerService, SaveManager>(new ContainerControlledLifetimeManager());
      _container.RegisterType<ISelectionManagerService, SelectionManager>(new ContainerControlledLifetimeManager());
    }

    #endregion
  }
}
