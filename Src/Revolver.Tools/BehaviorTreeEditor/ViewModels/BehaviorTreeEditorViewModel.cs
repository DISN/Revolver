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
using Revolver.Tools.BehaviorTreeEditor.Events;
using Revolver.Tools.BehaviorTreeEditor.Models;
using Revolver.Tools.BehaviorTreeEditor.Views;

namespace Revolver.Tools.BehaviorTreeEditor.ViewModels
{
  public class BehaviorTreeEditorViewModel : ToolViewModel, IBehaviorTree
  {
    #region Fields
    private readonly BehaviorTreeEditorModel _model;
    private readonly BehaviorTreeEditorView _view;
    #endregion

    #region Constructors
    public BehaviorTreeEditorViewModel(IUnityContainer container, WorkspaceBase workspace)
      : base(container, workspace)
    {
      PreferredLocation = PaneLocation.Center;
      Name = "Behavior Tree Editor";
      Title = "Behavior Tree Editor";
      ContentId = "BehaviorTreeEditor";
      RibbonHeader = "BEHAVIOR TREE EDITOR";
      Description = "Used to create behaviors of actors depending on certain conditions";
      IsVisible = false;
      IconSource = Magnum.IconLibrary.BitmapImages.LoadBitmapFromResourceKey("BehaviorTreeEditor_128x128");
      _model = new BehaviorTreeEditorModel(Name, container, "Gameplay", Description);
      Model = _model;

      _view = new BehaviorTreeEditorView();
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
      BehaviorTreeEditorToolSettings patternCreatorToolSettings = Container.Resolve<BehaviorTreeEditorToolSettings>();
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
      eventAggregator.GetEvent<BehaviorTreeEditorSettingsChangeEvent>().Publish(this);
    }
    #endregion
  }
}
