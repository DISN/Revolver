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

namespace Revolver.Tools.DialogEditor.Models
{
  class DialogEditorModel : SelectionDrivenEditorViewModel
  {
    double _dataPaneWidth;

    public DialogEditorModel(string displayName, IUnityContainer container, string category, string description, bool isPinned = true, string shortcut = null)
      : base(displayName, container, category, description, isPinned, shortcut)
    {
      Container = container;
      _dataPaneWidth = 350.0;

      InitializeCommands();

      DataPane = new EditorPane(this);
      ObjectTreeNode genericNode = new ObjectTreeNode(DataPane, "TEST NODE", 0, IItemType.Unknown, BitmapImages.LoadBitmapFromIconType(IconType.Entity), Container);
      genericNode.Children.Add(new ObjectTreeNode(DataPane, "LOL NODE", 0, IItemType.Unknown, BitmapImages.LoadBitmapFromIconType(IconType.Map), Container));
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

    #endregion

    #region Methods
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
      commandManager.RegisterCommand("EDITDIALOGEDITORSETTINGS", editSettingsCommand);

      ribbonService.Add(new RibbonItemViewModel("DIALOG EDITOR", 1, "D", true));
      ribbonService.Get("DIALOG EDITOR").Add(new RibbonItemViewModel("Settings", 3));
      ribbonService.Get("DIALOG EDITOR").Get("Settings").Add(new MenuItemViewModel("Edit Settings", 1,
                                                            BitmapImages.LoadBitmapFromResourceKey("Settings_16x16"),
                                                            commandManager.GetCommand("EDITDIALOGEDITORSETTINGS"), "EDITDIALOGEDITORSETTINGS", null, false, null,
                                                            String.Empty, "Edit tool settings."));
    }

    public void EditSettings(object obj)
    {
      var selectionManager = Container.Resolve<ISelectionManagerService>();
      if (selectionManager != null)
        selectionManager.SelectObject(Container.Resolve<IDialogEditor>());
    }

    public WindowClosingEventArgs OnClosing(object info)
    {
      IWorkspace workspace = Container.Resolve<WorkspaceBase>();
      var statusBar = Container.Resolve<IStatusBarService>();
      var saveManager = Container.Resolve<ISaveManagerService>();
      WindowClosingEventArgs e = info as WindowClosingEventArgs;

      if (e == null) // This means we closed the Dialog Editor via the X button and not by closing the MainWindow
      {
        e = new WindowClosingEventArgs(true);

        ToolViewModel viewer = workspace.Tools.First(f => f.ContentId == "DialogEditor");
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
