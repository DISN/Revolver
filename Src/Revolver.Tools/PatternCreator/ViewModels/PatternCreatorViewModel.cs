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
using Revolver.Tools.PatternCreator.Views;
using Revolver.Tools.PatternCreator.Models;
using Revolver.Tools.PatternCreator.Events;

namespace Revolver.Tools.PatternCreator.ViewModels
{
  public class PatternCreatorViewModel : ToolViewModel, IPatternCreator
  {
    #region Fields
    private readonly PatternCreatorModel _model;
    private readonly PatternCreatorView _view;
    #endregion

    #region Constructors
    public PatternCreatorViewModel(IUnityContainer container, WorkspaceBase workspace)
      : base(container, workspace)
    {
      PreferredLocation = PaneLocation.Center;
      Name = "Pattern Creator";
      Title = "Pattern Creator";
      ContentId = "PatternCreator";
      RibbonHeader = "PATTERN CREATOR";
      Description = "Create and manage game resources";
      IsVisible = false;
      IconSource = Magnum.IconLibrary.BitmapImages.LoadBitmapFromResourceKey("PatternCreator_128x128");
      _model = new PatternCreatorModel(Name, container, "Gameplay", Description);
      Model = _model;

      _view = new PatternCreatorView();
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
      PatternCreatorToolSettings patternCreatorToolSettings = Container.Resolve<PatternCreatorToolSettings>();
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
      eventAggregator.GetEvent<PatternCreatorSettingsChangeEvent>().Publish(this);
    }
    #endregion
  }
}
