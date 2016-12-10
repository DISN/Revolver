using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Unity;
using Magnum.Core.Windows;
using Magnum.Core.Events;
using Revolver.Tools.FeedbackEditor.ViewModels;

namespace Revolver.Tools.FeedbackEditor
{
  [Module(ModuleName = "Revolver.Tools.FeedbackEditor")]
  public class FeedbackEditorModule : IModule
  {
    private readonly IUnityContainer _container;

    #region Constructors
    public FeedbackEditorModule(IUnityContainer container)
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
      EventAggregator.GetEvent<SplashMessageUpdateEvent>().Publish(new SplashMessageUpdateEvent { Message = "Loading Feedback Editor Module" });
      _container.RegisterType<FeedbackEditorToolSettings>(new ContainerControlledLifetimeManager());
      _container.RegisterType<FeedbackEditorViewModel>();
      IWorkspace workspace = _container.Resolve<WorkspaceBase>();
      workspace.Tools.Add(_container.Resolve<FeedbackEditorViewModel>());
    }

    #endregion
  }
}