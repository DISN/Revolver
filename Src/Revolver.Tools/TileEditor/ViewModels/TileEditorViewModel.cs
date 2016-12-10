using Magnum.Core.Services;
using Magnum.Core.ViewModels;
using Magnum.Core.Windows;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Unity;
using Revolver.Tools.TileEditor.Events;
using Revolver.Tools.TileEditor.Models;
using Revolver.Tools.TileEditor.Views;
using System;

namespace Revolver.Tools.TileEditor.ViewModels
{
  public class TileEditorViewModel : ToolViewModel, ITileEditor
  {
    #region Fields
    private readonly TileEditorModel _model;
    private readonly TileEditorView _view;
    #endregion

    #region Constructors
    public TileEditorViewModel(IUnityContainer container, WorkspaceBase workspace)
      : base(container, workspace)
    {
      Name = "Tile Editor";
      Title = "Tile Editor";
      ContentId = "TileEditor";
      RibbonHeader = "TILE EDITOR";
      Description = "Create and manage game resources";
      IsVisible = false;
      IconSource = Magnum.IconLibrary.BitmapImages.LoadBitmapFromResourceKey("Map_16x16");
      _model = new TileEditorModel(Name, container, "Standard", Description);
      Model = _model;

      _view = new TileEditorView();
      _view.DataContext = _model;
      View = _view;

      LoadSettings();

      CloseCommand = new UndoableCommand<object>(CloseTileEditor);
    }

    private void LoadSettings()
    {
      //Set the preferred height, width and location of the tool based on previous session values
      TileEditorToolSettings tileEditorToolSettings = Container.Resolve<TileEditorToolSettings>();
      this.PreferredHeight = tileEditorToolSettings.PreferredHeight;
      this.PreferredWidth = tileEditorToolSettings.PreferredWidth;
      this.PreferredLocation = tileEditorToolSettings.PreferredLocation;
      this.FocusOnRibbonOnClick = tileEditorToolSettings.FocusOnRibbonOnClick;
      this.KeepRibbonTabActive = tileEditorToolSettings.KeepRibbonTabActive;
      if (!String.IsNullOrEmpty(tileEditorToolSettings.LastLoadedResource))
      {
        (this.Model as TileEditorModel).LastLoadedResource = tileEditorToolSettings.LastLoadedResource;
        (this.Model as TileEditorModel).LoadLastLoadedResource();
      }
      else
        this.FocusOnRibbonOnClick = false;
    }

    public void CloseTileEditor(object info)
    {
      _model.OnClosing(info);
      SaveToolSettings();
    }

    void SaveToolSettings()
    {
      IEventAggregator eventAggregator = Container.Resolve<IEventAggregator>();
      eventAggregator.GetEvent<TileEditorSettingsChangeEvent>().Publish(this);
    }
    #endregion
  }
}
