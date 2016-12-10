using Magnum.Core.Args;
using Magnum.Core.Events;
using Magnum.Core.Services;
using Magnum.Core.ViewModels;
using Magnum.Core.Windows;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Unity;
using Microsoft.Windows.Controls.Ribbon;
using Revolver.Tools.Viewer.Events;
using Revolver.Tools.Viewer.Models;
using Revolver.Tools.Viewer.Views;
using System;

namespace Revolver.Tools.Viewer.ViewModels
{
  public class ViewerViewModel : ToolViewModel, IViewer
  {
    #region Fields
    private readonly ViewerModel _model;
    private readonly ViewerView _view;
    #endregion

    #region Constructors
    public ViewerViewModel(IUnityContainer container, WorkspaceBase workspace)
      : base (container, workspace)
    {
      PreferredLocation = PaneLocation.Center;
      Name = "Viewer";
      Title = "Viewer";
      ContentId = "Viewer";
      RibbonHeader = "VIEWER";
      Description = "Modify or play the currently opened game";
      IsVisible = false;
      IconSource = Magnum.IconLibrary.BitmapImages.LoadBitmapFromResourceKey("Viewer_128x128");
      _model = new ViewerModel(Name, container, "Standard", Description);
      Model = _model;

      _view = new ViewerView();
      _view.DataContext = _model;
      View = _view;

      LoadSettings();

      OnActiveChanged += ViewerViewModel_OnActiveChanged;
      _model.OnIsDirtyChanged += _model_OnIsDirtyChanged;
      _model.OnLoadedMapNameChanged += _model_OnLoadedMapNameChanged;

      var commandManager = Container.Resolve<ICommandManager>();
      if (commandManager != null)
        SaveCommand = commandManager.GetCommand("SAVEMAP");
      CloseCommand = new UndoableCommand<object>(Close);

      _model_OnLoadedMapNameChanged(null, null);
    }

    private void LoadSettings()
    {
      //Set the preferred height, width and location of the tool based on previous session values
      ViewerToolSettings viewerToolSettings = Container.Resolve<ViewerToolSettings>();
      this.PreferredHeight = viewerToolSettings.PreferredHeight;
      this.PreferredWidth = viewerToolSettings.PreferredWidth;
      this.PreferredLocation = viewerToolSettings.PreferredLocation;
      this.FocusOnRibbonOnClick = viewerToolSettings.FocusOnRibbonOnClick;
      this.KeepRibbonTabActive = viewerToolSettings.KeepRibbonTabActive;
      (this.Model as ViewerModel).Viewer.IsGridActivated = viewerToolSettings.IsGridActivated;
      (this.Model as ViewerModel).LastRatio = viewerToolSettings.LastRatio;
      if (!String.IsNullOrEmpty(viewerToolSettings.LastLoadedMap))
      {
        (this.Model as ViewerModel).LastLoadedMap = viewerToolSettings.LastLoadedMap;
        (this.Model as ViewerModel).LoadLastLoadedMap();
        this.FocusOnRibbonOnClick = false;
      }
    }

    public override void Close(object info)
    {
      base.Close(info);

      WindowClosingEventArgs e = _model.OnClosing(info);
      SaveToolSettings();

      // Return to default name
      if (e != null && e.WindowClosing && !e.Cancel)
      {
        Title = "Viewer";
        var eventAggregator = Container.Resolve<IEventAggregator>();
        eventAggregator.GetEvent<ActiveContentChangedEvent>().Publish(this);
      }
    }

    void ViewerViewModel_OnActiveChanged(object sender, System.EventArgs e)
    {
      if (IsActive)
        _model.Viewer.IsViewerFocused = true;
      else
        _model.Viewer.IsViewerFocused = false;
    }

    void _model_OnIsDirtyChanged(object sender, System.EventArgs e)
    {
      RaisePropertyChanged("Title"); // change the document header

      // Change the application title
      var eventAggregator = Container.Resolve<IEventAggregator>();
      eventAggregator.GetEvent<ActiveContentChangedEvent>().Publish(this);
    }

    void _model_OnLoadedMapNameChanged(object sender, System.EventArgs e)
    {
      if (_model.LoadedMapName != String.Empty)
      {
        Title = _model.LoadedMapName;

        // Change the application title
        var eventAggregator = Container.Resolve<IEventAggregator>();
        eventAggregator.GetEvent<ActiveContentChangedEvent>().Publish(this);
      }
    }

    void SaveToolSettings()
    {
      IEventAggregator eventAggregator = Container.Resolve<IEventAggregator>();
      eventAggregator.GetEvent<ViewerSettingsChangeEvent>().Publish(this);
    }
    #endregion
  }
}
