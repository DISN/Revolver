using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Unity;
using Magnum.Core.Windows;
using Magnum.Core.Events;
using Magnum.Core.Models;
using Magnum.Core.ViewModels;
using Revolver.Tools.Viewer.ViewModels;
using Engine.Wrappers.Services;

namespace Revolver.Shell
{
  internal class RevolverWorkspace : WorkspaceBase
  {
    #region Fields
    /// <summary>
    /// The save manager service
    /// </summary>
    protected ISaveManagerService _saveManagerService;
    /// <summary>
    /// The selection manager service
    /// </summary>
    protected ISelectionManagerService _selectionManagerService;
    private string _document;
    private const string _title = "Revolver";
    #endregion

    #region Constructors
    public RevolverWorkspace(IUnityContainer container)
      : base(container)
    {
      IEventAggregator aggregator = container.Resolve<IEventAggregator>();
      aggregator.GetEvent<ActiveContentChangedEvent>().Subscribe(ContentChanged);
      _document = "";
      _saveManagerService = container.Resolve<ISaveManagerService>();
      _selectionManagerService = container.Resolve<ISelectionManagerService>();
    }
    #endregion

    #region Properties
    public override ImageSource Icon
    {
      get
      {
        ImageSource imageSource = new BitmapImage(new Uri("pack://application:,,,/Revolver.Shell;component/Logo.ico", UriKind.Absolute));
        return imageSource;
      }
    }

    public override string Title
    {
      get
      {
        string newTitle = _title;
        if (_document != String.Empty)
        {
          newTitle += " - " + _document;
        }
        return newTitle;
      }
    }
    
    /// <summary>
    /// Gets the save manager.
    /// </summary>
    /// <value>The save manager.</value>
    public ISaveManagerService SaveManager
    {
      get { return _saveManagerService; }
    }
    
    /// <summary>
    /// Gets the selection manager.
    /// </summary>
    /// <value>The selection manager.</value>
    public ISelectionManagerService SelectionManager
    {
      get { return _selectionManagerService; }
    }
    #endregion

    #region Methods
    private void ContentChanged(object model)
    {
      DocumentViewModel dvm = model as DocumentViewModel;
      ToolViewModel tvm = model as ToolViewModel;

      // We only want to show the map name when the Viewer has a loaded map
      if (tvm != null && tvm is ViewerViewModel)
      {
        _document = tvm.Title == "Viewer" ? String.Empty : tvm.Title;
        RaisePropertyChanged("Title");
      }
      else if (dvm != null)
      {
        _document = dvm.Title;
        RaisePropertyChanged("Title");
      }
    }
    #endregion
  }
}
