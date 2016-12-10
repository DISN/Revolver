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
using Revolver.Tools.EntityCreator.Views;
using Revolver.Tools.EntityCreator.Models;
using Revolver.Tools.EntityCreator.Events;

namespace Revolver.Tools.EntityCreator.ViewModels
{
  public class EntityCreatorViewModel : ToolViewModel, IEntityCreator
  {
    #region Fields
    private readonly EntityCreatorModel _model;
    private readonly EntityCreatorView _view;
    #endregion

    #region Constructors
    public EntityCreatorViewModel(IUnityContainer container, WorkspaceBase workspace)
      : base(container, workspace)
    {
      PreferredLocation = PaneLocation.Center;
      Name = "Entity Creator";
      Title = "Entity Creator";
      ContentId = "EntityCreator";
      RibbonHeader = "ENTITY CREATOR";
      Description = "Used to create entities that will be used in the game. Can be NPCs or interactable/unanimated objects.";
      IsVisible = false;
      IconSource = Magnum.IconLibrary.BitmapImages.LoadBitmapFromResourceKey("EntityCreator_128x128");
      _model = new EntityCreatorModel(Name, container, "Gameplay", Description);
      Model = _model;

      _view = new EntityCreatorView();
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
      EntityCreatorToolSettings patternCreatorToolSettings = Container.Resolve<EntityCreatorToolSettings>();
      this.PreferredHeight = patternCreatorToolSettings.PreferredHeight;
      this.PreferredWidth = patternCreatorToolSettings.PreferredWidth;
      this.PreferredLocation = patternCreatorToolSettings.PreferredLocation;
      this.FocusOnRibbonOnClick = patternCreatorToolSettings.FocusOnRibbonOnClick;
      this.KeepRibbonTabActive = patternCreatorToolSettings.KeepRibbonTabActive;
    }
    #endregion

    #region Events
    void SaveToolSettings()
    {
      IEventAggregator eventAggregator = Container.Resolve<IEventAggregator>();
      eventAggregator.GetEvent<EntityCreatorSettingsChangeEvent>().Publish(this);
    }
    #endregion
  }
}
