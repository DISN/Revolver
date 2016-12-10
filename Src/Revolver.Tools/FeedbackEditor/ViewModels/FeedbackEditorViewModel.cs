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
using Revolver.Tools.FeedbackEditor.Events;
using Revolver.Tools.FeedbackEditor.Models;
using Revolver.Tools.FeedbackEditor.Views;

namespace Revolver.Tools.FeedbackEditor.ViewModels
{
  public class FeedbackEditorViewModel : ToolViewModel, IFeedback
  {
    #region Fields
    private readonly FeedbackEditorModel _model;
    private readonly FeedbackEditorView _view;
    #endregion

    #region Constructors
    public FeedbackEditorViewModel(IUnityContainer container, WorkspaceBase workspace)
      : base(container, workspace)
    {
      PreferredLocation = PaneLocation.Center;
      Name = "Feedback Editor";
      Title = "Feedback Editor";
      ContentId = "FeedbackEditor";
      RibbonHeader = "FEEDBACK EDITOR";
      Description = "Used to spawn sound or FX events on actors depending on certain conditions";
      IsVisible = false;
      IconSource = Magnum.IconLibrary.BitmapImages.LoadBitmapFromResourceKey("FeedbackEditor_128x128");
      _model = new FeedbackEditorModel(Name, container, "Gameplay", Description);
      Model = _model;

      _view = new FeedbackEditorView();
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
      FeedbackEditorToolSettings patternCreatorToolSettings = Container.Resolve<FeedbackEditorToolSettings>();
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
      eventAggregator.GetEvent<FeedbackEditorSettingsChangeEvent>().Publish(this);
    }
    #endregion
  }
}
