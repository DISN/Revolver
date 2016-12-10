using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Unity;
using Magnum.Core.Windows;
using Magnum.Core.Events;
using Revolver.Tools.BehaviorTreeEditor.ViewModels;

namespace Revolver.Tools.BehaviorTreeEditor
{
  [Module(ModuleName = "Revolver.Tools.BehaviorTreeEditor")]
  public class BehaviorTreeEditorModule : IModule
  {
    private readonly IUnityContainer _container;

    #region Constructors
    public BehaviorTreeEditorModule(IUnityContainer container)
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
      EventAggregator.GetEvent<SplashMessageUpdateEvent>().Publish(new SplashMessageUpdateEvent { Message = "Loading Behavior Tree Editor Module" });
      _container.RegisterType<BehaviorTreeEditorToolSettings>(new ContainerControlledLifetimeManager());
      _container.RegisterType<BehaviorTreeEditorViewModel>();
      IWorkspace workspace = _container.Resolve<WorkspaceBase>();
      workspace.Tools.Add(_container.Resolve<BehaviorTreeEditorViewModel>());
    }

    #endregion
  }
}