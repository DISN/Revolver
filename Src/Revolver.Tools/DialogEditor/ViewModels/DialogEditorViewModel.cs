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
using Revolver.Tools.DialogEditor.Events;
using Revolver.Tools.DialogEditor.Models;
using Revolver.Tools.DialogEditor.Views;

namespace Revolver.Tools.DialogEditor.ViewModels
{
  public class DialogEditorViewModel : ToolViewModel, IDialogEditor
  {
    #region Fields
    private readonly DialogEditorModel _model;
    private readonly DialogEditorView _view;
    #endregion

    #region Constructors
    public DialogEditorViewModel(IUnityContainer container, WorkspaceBase workspace)
      : base(container, workspace)
    {
      PreferredLocation = PaneLocation.Center;
      Name = "Dialog Editor";
      Title = "Dialog Editor";
      ContentId = "DialogEditor";
      RibbonHeader = "DIALOG EDITOR";
      Description = "Used to create dialogs that can be shown depending on certain conditions";
      IsVisible = false;
      IconSource = Magnum.IconLibrary.BitmapImages.LoadBitmapFromResourceKey("DialogEditor_128x128");
      _model = new DialogEditorModel(Name, container, "Gameplay", Description);
      Model = _model;

      _view = new DialogEditorView();
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
      DialogEditorToolSettings patternCreatorToolSettings = Container.Resolve<DialogEditorToolSettings>();
      this.PreferredHeight = patternCreatorToolSettings.PreferredHeight;
      this.PreferredWidth = patternCreatorToolSettings.PreferredWidth;
      this.PreferredLocation = patternCreatorToolSettings.PreferredLocation;
      this.FocusOnRibbonOnClick = patternCreatorToolSettings.FocusOnRibbonOnClick;
      this.KeepRibbonTabActive = patternCreatorToolSettings.KeepRibbonTabActive;
      this._model.DataPaneWidth = patternCreatorToolSettings.DataPaneWidth;
    }
    #endregion

    #region Events
    void SaveToolSettings()
    {
      IEventAggregator eventAggregator = Container.Resolve<IEventAggregator>();
      eventAggregator.GetEvent<DialogEditorSettingsChangeEvent>().Publish(this);
    }
    #endregion
  }
}
