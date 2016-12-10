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
using Revolver.Tools.PatternCreator.ViewModels;

namespace Revolver.Tools.PatternCreator
{
  [Module(ModuleName = "Revolver.Tools.PatternCreator")]
  public class PatternCreatorModule : IModule
  {
    private readonly IUnityContainer _container;

    #region Constructors
    public PatternCreatorModule(IUnityContainer container)
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
      EventAggregator.GetEvent<SplashMessageUpdateEvent>().Publish(new SplashMessageUpdateEvent { Message = "Loading Pattern Creator Module" });
      _container.RegisterType<PatternCreatorToolSettings>(new ContainerControlledLifetimeManager());
      _container.RegisterType<PatternCreatorViewModel>();
      IWorkspace workspace = _container.Resolve<WorkspaceBase>();
      workspace.Tools.Add(_container.Resolve<PatternCreatorViewModel>());
    }

    #endregion
  }
}