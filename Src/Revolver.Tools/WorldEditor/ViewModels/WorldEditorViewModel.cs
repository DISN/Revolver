using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Revolver.Tools.WorldEditor.Models;
using System.Windows.Controls;
using System.Windows.Data;
using System.Collections.ObjectModel;
using Magnum.Core.ViewModels;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Unity;
using Revolver.Tools.WorldEditor.Views;
using Magnum.Core.Windows;
using Magnum.Core.Services;
using Microsoft.Windows.Controls.Ribbon;
using Magnum.Core.Events;
using Revolver.Tools.WorldEditor.Events;
using System.Windows;
using System.Windows.Media;
using Microsoft.Practices.Prism.Commands;
using Engine.Wrappers.Services;

namespace Revolver.Tools.WorldEditor.ViewModels
{
  public class WorldEditorViewModel : ToolViewModel, IWorldEditor
  {
    #region Fields
    private readonly WorldEditorModel _model;
    private readonly WorldEditorView _view;
    #endregion

    #region Constructors
    public WorldEditorViewModel(IUnityContainer container, WorkspaceBase workspace)
      : base(container, workspace)
    {
      PreferredLocation = PaneLocation.Left;
      Name = "World Editor";
      Title = "World Editor";
      ContentId = "WorldEditor";
      RibbonHeader = "WORLD EDITOR";
      Description = "Create and manage game resources";
      IsVisible = false;
      IconSource = Magnum.IconLibrary.BitmapImages.LoadBitmapFromResourceKey("World_16x16");
      _model = new WorldEditorModel(Name, container, "Standard", Description);
      Model = _model;

      _view = new WorldEditorView();
      _view.DataContext = _model;
      View = _view;

      LoadSettings();

      CloseCommand = new UndoableCommand<object>(Close);
    }
    #endregion

    #region Methods
    public override void Close(object info)
    {
      base.Close(info);

      SaveToolSettings();
    }

    private void LoadSettings()
    {
      var statusBar = Container.Resolve<IStatusBarService>();
      //Set the preferred height, width and location of the tool based on previous session values
      WorldEditorToolSettings worldEditorToolSettings = Container.Resolve<WorldEditorToolSettings>();
      this.PreferredHeight = worldEditorToolSettings.PreferredHeight;
      this.PreferredWidth = worldEditorToolSettings.PreferredWidth;
      this.PreferredLocation = worldEditorToolSettings.PreferredLocation;
      this.FocusOnRibbonOnClick = worldEditorToolSettings.FocusOnRibbonOnClick;
      this.KeepRibbonTabActive = worldEditorToolSettings.KeepRibbonTabActive;
      if (!String.IsNullOrEmpty(worldEditorToolSettings.LastLoadedGame))
      {
        Magnum.Core.ApplicationModel.Application.IsGameLoaded = true;
        (this.Model as WorldEditorModel).LastLoadedGame = worldEditorToolSettings.LastLoadedGame;
        (this.Model as WorldEditorModel).LoadLastLoadedGame();
        this.FocusOnRibbonOnClick = false;
      }
    }
    #endregion

    #region Events
    void SaveToolSettings()
    {
      IEventAggregator eventAggregator = Container.Resolve<IEventAggregator>();
      eventAggregator.GetEvent<WorldEditorSettingsChangeEvent>().Publish(this);
    }
    #endregion
  }
}
