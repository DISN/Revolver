using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Unity;
using Magnum.Core.Windows;
using Magnum.Core.Events;
using Revolver.Tools.FightEditor.ViewModels;

namespace Revolver.Tools.FightEditor
{
  [Module(ModuleName = "Revolver.Tools.FightEditor")]
  public class FightEditorModule : IModule
  {
    private readonly IUnityContainer _container;

    #region Constructors
    public FightEditorModule(IUnityContainer container)
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
      EventAggregator.GetEvent<SplashMessageUpdateEvent>().Publish(new SplashMessageUpdateEvent { Message = "Loading Fight Editor Module" });
      _container.RegisterType<FightEditorToolSettings>(new ContainerControlledLifetimeManager());
      _container.RegisterType<FightEditorViewModel>();
      IWorkspace workspace = _container.Resolve<WorkspaceBase>();
      workspace.Tools.Add(_container.Resolve<FightEditorViewModel>());
    }

    #endregion
  }
}