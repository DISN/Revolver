using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Unity;
using Magnum.Core.Windows;
using Magnum.Core.Events;
using Revolver.Tools.DialogEditor.ViewModels;

namespace Revolver.Tools.DialogEditor
{
  [Module(ModuleName = "Revolver.Tools.DialogEditor")]
  public class DialogEditorModule : IModule
  {
    private readonly IUnityContainer _container;

    #region Constructors
    public DialogEditorModule(IUnityContainer container)
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
      EventAggregator.GetEvent<SplashMessageUpdateEvent>().Publish(new SplashMessageUpdateEvent { Message = "Loading Dialog Editor Module" });
      _container.RegisterType<DialogEditorToolSettings>(new ContainerControlledLifetimeManager());
      _container.RegisterType<DialogEditorViewModel>();
      IWorkspace workspace = _container.Resolve<WorkspaceBase>();
      workspace.Tools.Add(_container.Resolve<DialogEditorViewModel>());
    }

    #endregion
  }
}