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
using Revolver.Tools.FightEditor.Events;
using Revolver.Tools.FightEditor.Models;
using Revolver.Tools.FightEditor.Views;

namespace Revolver.Tools.FightEditor.ViewModels
{
  public class FightEditorViewModel : ToolViewModel, IFightEditor
  {
    #region Fields
    private readonly FightEditorModel _model;
    private readonly FightEditorView _view;
    #endregion

    #region Constructors
    public FightEditorViewModel(IUnityContainer container, WorkspaceBase workspace)
      : base(container, workspace)
    {
      PreferredLocation = PaneLocation.Center;
      Name = "Fight Editor";
      Title = "Fight Editor";
      ContentId = "FightEditor";
      RibbonHeader = "FIGHT EDITOR";
      Description = "Used to create sequences of animations after certain conditions are met";
      IsVisible = false;
      IconSource = Magnum.IconLibrary.BitmapImages.LoadBitmapFromResourceKey("FightEditor_128x128");
      _model = new FightEditorModel(Name, container, "Gameplay", Description);
      Model = _model;

      _view = new FightEditorView();
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

      _model.OnClosing(info);
      SaveToolSettings();
    }

    private void LoadSettings()
    {
      //Set the preferred height, width and location of the tool based on previous session values
      FightEditorToolSettings fightEditorToolSettings = Container.Resolve<FightEditorToolSettings>();
      this.PreferredHeight = fightEditorToolSettings.PreferredHeight;
      this.PreferredWidth = fightEditorToolSettings.PreferredWidth;
      this.PreferredLocation = fightEditorToolSettings.PreferredLocation;
      this.FocusOnRibbonOnClick = fightEditorToolSettings.FocusOnRibbonOnClick;
      this.KeepRibbonTabActive = fightEditorToolSettings.KeepRibbonTabActive;
      this._model.DataPaneWidth = fightEditorToolSettings.DataPaneWidth;
      this._model.ContextualPaneHeight = fightEditorToolSettings.ContextualPaneHeight;
    }
    #endregion

    #region Events
    void SaveToolSettings()
    {
      IEventAggregator eventAggregator = Container.Resolve<IEventAggregator>();
      eventAggregator.GetEvent<FightEditorSettingsChangeEvent>().Publish(this);
    }
    #endregion
  }
}
