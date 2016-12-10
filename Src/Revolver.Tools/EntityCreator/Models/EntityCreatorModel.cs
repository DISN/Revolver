using Engine.System.Entities;
using Magnum.Core.Managers;
using Magnum.Core.Models;
using Magnum.Core.Services;
using Magnum.Core.ViewModels;
using Magnum.Core.Windows;
using Magnum.IconLibrary;
using Magnum.Tools.PropertyGrid.ViewModels;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Unity;
using Microsoft.Win32;
using Revolver.Tools.Viewer.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml.Serialization;
using Magnum.Core.Extensions;
using Engine.Wrappers.Services;
using Magnum.Core.Utils;
using Magnum.Tools.SelectionDrivenEditor.ViewModels;
using Magnum.Tools.SelectionDrivenEditor.Models;
using Magnum.Core.Args;

namespace Revolver.Tools.EntityCreator.Models
{
  class EntityCreatorModel : ToolModel
  {
    public EntityCreatorModel(string displayName, IUnityContainer container, string category, string description, bool isPinned = true, string shortcut = null)
      : base(displayName, container, category, description, isPinned, shortcut)
    {
      Container = container;

      InitializeCommands();

      BackCommand = new UndoableCommand<object>(GoBack, CanGoBack);
      NextCommand = new UndoableCommand<object>(GoNext, CanGoNext);

      EntityItem testEntity = new EntityItem("Test Entity");
      EntityStateItem testState = new EntityStateItem("Test State");
      EntityComponentItem testComponent1 = new EntityComponentItem("Test Component #1", ComponentType.Mesh);
      EntityComponentItem testComponent2 = new EntityComponentItem("Test Component #2", ComponentType.Visual);
      EntityActionItem testAction1 = new EntityActionItem("Button Press : Enter", ActionType.ButtonPress);
      testState.Components.Add(testComponent1);
      testState.Components.Add(testComponent2);
      testState.Actions.Add(testAction1);
      testEntity.States.Add(testState);

      LoadedEntity = testEntity;
    }

    #region Properties
    private EntityItem _LoadedEntity = null;
    public EntityItem LoadedEntity
    {
      get { return _LoadedEntity; }
      set
      {
        if (_LoadedEntity != value)
        {
          _LoadedEntity = value;
        }
      }
    }
    #endregion

    public override void GoBack(object obj)
    {
      /*_selectedItemsBeforeGoingBack.Push(CurrentlySelectedItem);
      CurrentlySelectedItem = _previouslySelectedItems.Pop();*/
    }

    protected override bool CanGoBack(object obj)
    {
      /*if (_previouslySelectedItems.Count > 0)
        return true;
      else
        return false;*/
      return false;
    }

    public override void GoNext(object obj)
    {
      //CurrentlySelectedItem = _selectedItemsBeforeGoingBack.Pop();
    }

    protected override bool CanGoNext(object obj)
    {
      /*if (_selectedItemsBeforeGoingBack.Count > 0 && _selectedItemsBeforeGoingBack.First() != CurrentlySelectedItem)
        return true;
      else
        return false;*/
      return false;
    }

    #region Properties

    #endregion

    #region Methods

    private void InitializeCommands()
    {
      var commandManager = Container.Resolve<ICommandManager>();
      var ribbonService = Container.Resolve<IRibbonService>();

      // Ribbon
      var editSettingsCommand = new UndoableCommand<object>(EditSettings);
      commandManager.RegisterCommand("EDITENTITYCREATORSETTINGS", editSettingsCommand);

      ribbonService.Add(new RibbonItemViewModel("ENTITY CREATOR", 1, "G", true));
      ribbonService.Get("ENTITY CREATOR").Add(new RibbonItemViewModel("Settings", 3));
      ribbonService.Get("ENTITY CREATOR").Get("Settings").Add(new MenuItemViewModel("Edit Settings", 1,
                                                            BitmapImages.LoadBitmapFromResourceKey("Settings_16x16"),
                                                            commandManager.GetCommand("EDITENTITYCREATORSETTINGS"), "EDITENTITYCREATORSETTINGS", null, false, null,
                                                            String.Empty, "Edit tool settings."));
    }

    public void EditSettings(object obj)
    {
      var selectionManager = Container.Resolve<ISelectionManagerService>();
      if (selectionManager != null)
        selectionManager.SelectObject(Container.Resolve<IEntityCreator>());
    }

    public WindowClosingEventArgs OnClosing(object info)
    {
      IWorkspace workspace = Container.Resolve<WorkspaceBase>();
      var statusBar = Container.Resolve<IStatusBarService>();
      var saveManager = Container.Resolve<ISaveManagerService>();
      WindowClosingEventArgs e = info as WindowClosingEventArgs;

      if (e == null) // This means we closed the Pattern Creator via the X button and not by closing the MainWindow
      {
        e = new WindowClosingEventArgs(true);

        ToolViewModel viewer = workspace.Tools.First(f => f.ContentId == "EntityCreator");
        if (viewer != null)
        {
          viewer.IsVisible = false;
          viewer.IsActive = false;
        }
      }
      return e;
    }
    #endregion
  }
}
