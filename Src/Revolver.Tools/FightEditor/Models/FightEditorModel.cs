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
using Magnum.Controls.Breadcrumb;

namespace Revolver.Tools.FightEditor.Models
{
  class FightEditorModel : SelectionDrivenEditorViewModel
  {
    double _dataPaneWidth;
    double _contextualPaneHeight;

    public FightEditorModel(string displayName, IUnityContainer container, string category, string description, bool isPinned = true, string shortcut = null)
      : base(displayName, container, category, description, isPinned, shortcut)
    {
      Container = container;
      _dataPaneWidth = 350.0;
      _contextualPaneHeight = 300.0;

      InitializeCommands();

      DataPane = new EditorPane(this);

      // TODO(ndistefano): Create tree node objects specific for this editor. Useful for ISearchable and INavigation implementation.
      ObjectTreeNode genericNode = new ObjectTreeNode(DataPane, "TEST NODE", 0, IItemType.Unknown, BitmapImages.LoadBitmapFromIconType(IconType.Entity), Container) { AssignedTool = "Fight Editor \u2192", OpenCommand = new UndoableCommand<object>(OpenSelectedItem) };
      genericNode.Children.Add(new ObjectTreeNode(DataPane, "LOL NODE", 0, IItemType.Unknown, BitmapImages.LoadBitmapFromIconType(IconType.Map), Container) { AssignedTool = "Fight Editor \u2192", OpenCommand = new UndoableCommand<object>(OpenSelectedItem) });
      DataPane.Items.Add(genericNode);
    }

    #region Properties

    public double DataPaneWidth
    {
      get { return _dataPaneWidth; }
      set
      {
        _dataPaneWidth = value;
        RaisePropertyChanged("DataPaneWidth");
      }
    }

    public double ContextualPaneHeight
    {
      get { return _contextualPaneHeight; }
      set
      {
        _contextualPaneHeight = value;
        RaisePropertyChanged("ContextualPaneHeight");
      }
    }

    #endregion

    #region Methods
    private void OpenSelectedItem(object obj)
    {
      var workspace = Container.Resolve<WorkspaceBase>();
      if (workspace != null)
      {
        ToolViewModel tvm = workspace.Tools.First(f => f.ContentId == "FightEditor");
        if (tvm != null)
        {
          if (!tvm.IsActive)
          {
            tvm.IsActive = true;
          }
        }
      }
    }

    public override void ApplyFilter()
    {
      foreach (var node in DataPane.Items)
        node.ApplyCriteria(FilterText, new Stack<ITreeNodeItem>());
    }

    private void InitializeCommands()
    {
      var commandManager = Container.Resolve<ICommandManager>();
      var ribbonService = Container.Resolve<IRibbonService>();

      // Ribbon
      var editSettingsCommand = new UndoableCommand<object>(EditSettings);
      commandManager.RegisterCommand("EDITFIGHTEDITORSETTINGS", editSettingsCommand);

      ribbonService.Add(new RibbonItemViewModel("FIGHT EDITOR", 1, "F", true));
      ribbonService.Get("FIGHT EDITOR").Add(new RibbonItemViewModel("Settings", 3));
      ribbonService.Get("FIGHT EDITOR").Get("Settings").Add(new MenuItemViewModel("Edit Settings", 1,
                                                            BitmapImages.LoadBitmapFromResourceKey("Settings_16x16"),
                                                            commandManager.GetCommand("EDITFIGHTEDITORSETTINGS"), "EDITFIGHTEDITORSETTINGS", null, false, null,
                                                            String.Empty, "Edit tool settings."));
    }

    public void EditSettings(object obj)
    {
      var selectionManager = Container.Resolve<ISelectionManagerService>();
      if (selectionManager != null)
        selectionManager.SelectObject(Container.Resolve<IFightEditor>());
    }

    public WindowClosingEventArgs OnClosing(object info)
    {
      IWorkspace workspace = Container.Resolve<WorkspaceBase>();
      var statusBar = Container.Resolve<IStatusBarService>();
      var saveManager = Container.Resolve<ISaveManagerService>();
      WindowClosingEventArgs e = info as WindowClosingEventArgs;

      if (e == null) // This means we closed the Fight Editor via the X button and not by closing the MainWindow
      {
        e = new WindowClosingEventArgs(true);

        ToolViewModel viewer = workspace.Tools.First(f => f.ContentId == "FightEditor");
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
