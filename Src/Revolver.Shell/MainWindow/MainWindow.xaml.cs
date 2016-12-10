using Magnum.Core.Events;
using Magnum.Core.Models;
using Magnum.Core.Natives;
using Magnum.Core.Services;
using Magnum.Core.ViewModels;
using Magnum.Core.Windows;
using MahApps.Metro.Native;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Unity;
using Revolver.Shell.Commands;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout;
using Xceed.Wpf.AvalonDock.Layout.Serialization;

namespace Revolver.Shell.MainWindow
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  internal partial class MainWindow : IShell
  {
    #region Fields
    private readonly IUnityContainer _container;
    private IEventAggregator _eventAggregator;
    private ILoggerService _logger;
    private IWorkspace _workspace;
    private readonly int doubleclick = UnsafeNativeMethods.GetDoubleClickTime();
    private DateTime lastMouseClick;
    private Border PART_Border;
    object lastActiveContent;
    object lastActiveContentBeforeOpeningErrorList;
    #endregion

    #region Constructors
    public MainWindow(IUnityContainer container, IEventAggregator eventAggregator)
    {
      InitializeComponent();

      _container = container;
      _eventAggregator = eventAggregator;
      Loaded += MainWindow_Loaded;
      Unloaded += MainWindow_Unloaded;
      Activated += MainWindow_Activated;
    }
    #endregion

    #region Properties

    private ToolViewModel LastActiveTool
    {
      get
      {
        return _container.Resolve<WorkspaceBase>().LastActiveTool;
      }
      set
      {
        _container.Resolve<WorkspaceBase>().LastActiveTool = value;
      }
    }

    private ILoggerService Logger
    {
      get
      {
        if (_logger == null)
          _logger = _container.Resolve<ILoggerService>();

        return _logger;
      }
    }
    #endregion

    #region Methods
    private static Point GetCorrectPosition(Visual relativeTo)
    {
      UnsafeNativeMethods.Win32Point w32Mouse;
      UnsafeNativeMethods.GetCursorPos(out w32Mouse);
      return relativeTo.PointFromScreen(new Point(w32Mouse.X, w32Mouse.Y));
    }

    private static void ShowSystemMenuPhysicalCoordinates(Window window, Point physicalScreenLocation)
    {
      if (window == null) return;

      var hwnd = new WindowInteropHelper(window).Handle;
      if (hwnd == IntPtr.Zero || !UnsafeNativeMethods.IsWindow(hwnd))
        return;

      var hmenu = UnsafeNativeMethods.GetSystemMenu(hwnd, false);

      var cmd = UnsafeNativeMethods.TrackPopupMenuEx(hmenu, Constants.TPM_LEFTBUTTON | Constants.TPM_RETURNCMD, (int)physicalScreenLocation.X, (int)physicalScreenLocation.Y, hwnd, IntPtr.Zero);
      if (0 != cmd)
        UnsafeNativeMethods.PostMessage(hwnd, Constants.SYSCOMMAND, new IntPtr(cmd), IntPtr.Zero);
    }
    #endregion

    #region Events
    private void MainWindow_Unloaded(object sender, RoutedEventArgs e)
    {
      SaveLayout();
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
      Magnum.Core.ApplicationModel.Application.DockingManager = dockManager;
      Magnum.Core.ApplicationModel.Application.StatusBar = _container.Resolve<IStatusBarService>();
      Magnum.Core.ApplicationModel.Application.Workspace = _container.Resolve<WorkspaceBase>();
      Magnum.Core.ApplicationModel.Application.MainWindow = _container.Resolve<IShell>();

      // Load shortcuts
      var shellCommands = _container.Resolve<ShellCommands>();
      InputBindings.Add(new KeyBinding(shellCommands.OpenQuickLaunch, new KeyGesture(Key.Q, ModifierKeys.Control)) { CommandParameter = _SearchBox_QuickLaunch });

      // Remove the stupid grey border when the window is inactive on fullscreen... the other way is to edit the ContentTemplate
      // for the WindowTemplateKey in MahApps.Metro
      Visual grid = (Visual)VisualTreeHelper.GetChild(this, 0);

      for (int i = 0; i < VisualTreeHelper.GetChildrenCount(grid); i++)
      {
        // Retrieve child visual at specified index value.
        Visual childVisual = (Visual)VisualTreeHelper.GetChild(grid, i);

        if (childVisual.GetType() == typeof(Border))
        {
          PART_Border = (childVisual as Border);
          break;
        }
      }

      LoadLayout();

      WindowPositionSettings position = _container.Resolve<WindowPositionSettings>();
      this.WindowState = position.State;
      this.Topmost = false;
      if (this.IsFocused)
        this.Activate();
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
      var workspace = DataContext as IWorkspace;
      if (workspace == null)
        return;

      if (!workspace.Closing(e))
      {
        e.Cancel = true;
        return;
      }

      // Save window settings
      _eventAggregator.GetEvent<WindowClosingEvent>().Publish(this);

      // Save status bar settings
      var statusBar = _container.Resolve<IStatusBarService>();
      _eventAggregator.GetEvent<StatusBarClosingEvent>().Publish(statusBar);

      //Save Ribbon settings
      var ribbonService = _container.Resolve<IRibbonService>();
      _eventAggregator.GetEvent<RibbonClosingEvent>().Publish(ribbonService);
    }

    private void dockManager_ActiveContentChanged(object sender, EventArgs e)
    {
      // Force a IsActive changed here in case the ribbon header doesn't collapse for some reason
      if (lastActiveContent != null)
      {
        (lastActiveContent as ToolViewModel).IsActive = false;

        // Set the lastActiveTool so in case we close a tool, we focus on the last one.
        // Also, we check if the Viewer is really closed so we don't set it as LastActiveTool twice.
        if (lastActiveContent != LastActiveTool)
          LastActiveTool = lastActiveContent as ToolViewModel;
      }

      DockingManager manager = sender as DockingManager;
      lastActiveContent = manager.ActiveContent;

      // Just making sure that when we close the Error List from the button on top, it focuses on the lastActiveContent that is not the Error List
      if (manager.ActiveContent != null && (manager.ActiveContent is ToolViewModel) && (manager.ActiveContent as ToolViewModel).ContentId != "Error List")
        lastActiveContentBeforeOpeningErrorList = manager.ActiveContent;

      object cvm = manager.ActiveContent;
      if (manager.ActiveContent != null)
        (manager.ActiveContent as ToolViewModel).IsActive = true;
      _eventAggregator.GetEvent<ActiveContentChangedEvent>().Publish(cvm);
      if (cvm != null) Logger.Log("Active document changed to " + (cvm is DocumentViewModel ? (cvm as DocumentViewModel).Title : (cvm as ToolViewModel).Title), LogCategory.Info, LogPriority.None);
    }

    private void ErrorList_Click(object sender, RoutedEventArgs e)
    {
      IWorkspace workspace = _container.Resolve<WorkspaceBase>();
      var menuService = _container.Resolve<IMenuService>();
      ToolViewModel errorList = workspace.Tools.First(f => f.ContentId == "Error List");
      if (errorList != null && !errorList.IsVisible)
      {
        lastActiveContentBeforeOpeningErrorList = dockManager.ActiveContent;
        errorList.IsVisible = true;
        errorList.IsActive = true;
        errorList.IsSelected = true;
        dockManager.ActiveContent = errorList;
      }
      else if (errorList != null && errorList.IsVisible)
      {
        errorList.IsVisible = false;
        errorList.IsActive = false;
        errorList.IsSelected = false;
        if (lastActiveContentBeforeOpeningErrorList != null)
        {
          dockManager.ActiveContent = lastActiveContentBeforeOpeningErrorList;
          dockManager_ActiveContentChanged(dockManager, EventArgs.Empty);
        }
      }
    }

    private void SaveManager_Click(object sender, RoutedEventArgs e)
    {
      _PopupSaveManager.IsOpen = true;
    }

    /// <summary>
    /// Little fix here that makes sure the mouse events are handled to the child controls. Need to find another way here...
    /// (ex. FireMouseDown, FireMouseMove && FireMouseUp events in IToolWindow interface, which ToolViewModels would have to override?)
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MainWindow_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
      /*IWorkspace workspace = _container.Resolve<WorkspaceBase>();
      ToolViewModel currentlySelectedTool = workspace.Tools.Where(x => x.IsActive).FirstOrDefault();
      DockingManager dockingManager = dockManager;
      if ((dockingManager.ActiveContent as ToolViewModel) != currentlySelectedTool)
        dockingManager.ActiveContent = currentlySelectedTool as ToolViewModel;*/

      // Permet de faire qu'on ne bouge pas la MainWindow si on clique quelque chose d'autre que la barre principal du haut.
      if (dockManager.IsMouseOver || StatusBar.IsMouseOver || _Ribbon.IsMouseOver)
        e.Handled = true;
      else
        e.Handled = false;
    }

    private void _mainTitle_MouseUp(object sender, MouseButtonEventArgs e)
    {
      var mousePosition = GetCorrectPosition(this);

      if (mousePosition.X <= 20 && mousePosition.Y <= 20)
      {
        if ((DateTime.Now - lastMouseClick).TotalMilliseconds <= doubleclick)
        {
          Close();
          return;
        }
        lastMouseClick = DateTime.Now;

        ShowSystemMenuPhysicalCoordinates(this, PointToScreen(new Point(0, 20)));
      }
      else if (e.ChangedButton == MouseButton.Right)
      {
        ShowSystemMenuPhysicalCoordinates(this, PointToScreen(GetCorrectPosition(this)));
      }
    }

    private void MainWindow_Activated(object sender, EventArgs e)
    {
      if (PART_Border != null && this.WindowState == WindowState.Maximized)
        PART_Border.Visibility = System.Windows.Visibility.Visible;
    }

    private void MainWindow_Deactivated(object sender, EventArgs e)
    {
      if (PART_Border != null && this.WindowState == WindowState.Maximized)
        PART_Border.Visibility = System.Windows.Visibility.Collapsed;
    }

    private void _PopupToolsLibrary_MouseDown(object sender, MouseButtonEventArgs e)
    {
      if (Mouse.DirectlyOver.GetType() == typeof(System.Windows.Controls.ScrollViewer))
        e.Handled = true;
    }

    private void _PopupToolsLibrary_Opened(object sender, EventArgs e)
    {
      _ToolsLibraryView.FocusSearchBox();
    }

    private void _PopupQuickLaunch_MouseDown(object sender, MouseButtonEventArgs e)
    {
      if (Mouse.DirectlyOver.GetType() == typeof(System.Windows.Controls.ScrollViewer))
        e.Handled = true;
    }

    private void _SearchBox_QuickLaunch_Search(object sender, RoutedEventArgs e)
    {
      var workspace = _container.Resolve<WorkspaceBase>();
      if (workspace != null)
        workspace.QuickLaunch.FilterText = _SearchBox_QuickLaunch.Text;
    }

    private void _SearchBox_QuickLaunch_PreviewKeyDown(object sender, KeyEventArgs e)
    {
      var assets = (System.Windows.Data.ListCollectionView)_QuickLaunchView.ListBox.ItemsSource;

      switch (e.Key)
      {
        case Key.Up:
          assets.MoveCurrentToPrevious();
          if (assets.IsCurrentBeforeFirst)
            assets.MoveCurrentToFirst();
          e.Handled = true;
          break;
        case Key.Down:
          assets.MoveCurrentToNext();
          if (assets.IsCurrentAfterLast)
            assets.MoveCurrentToLast();
          _QuickLaunchView.ListBox.ScrollIntoView(assets.CurrentItem);
          e.Handled = true;
          break;
        case Key.Tab:
          e.Handled = true;
          break;
        case Key.Enter:
          var currentItem = assets.CurrentItem as ISearchable;

          if (currentItem != null)
          {
            currentItem.Open();
            e.Handled = true;
          }
          break;
      }
    }
    #endregion

    #region IShell Members
    public void LoadLayout()
    {
      var layoutSerializer = new XmlLayoutSerializer(dockManager);
      layoutSerializer.LayoutSerializationCallback += (s, e) =>
      {
        var anchorable = e.Model as LayoutAnchorable;
        var document = e.Model as LayoutDocument;
        _workspace = _container.Resolve<WorkspaceBase>();

        if (anchorable != null)
        {
          ToolViewModel model =
              _workspace.Tools.FirstOrDefault(
                  f => f.ContentId == e.Model.ContentId);
          if (model != null)
          {
            e.Content = model;
            model.IsVisible = anchorable.IsVisible;
            model.IsActive = anchorable.IsActive;
            model.IsSelected = anchorable.IsSelected;
            if (anchorable.IsLastFocusedDocument)
              dockManager.ActiveContent = model;
          }
        }
        if (document != null)
        {
          var fileService = _container.Resolve<IOpenFileService>();
          DocumentViewModel model = fileService.OpenFromID(e.Model.ContentId) as DocumentViewModel;
          if (model != null)
          {
            e.Content = model;
            model.IsActive = document.IsActive;
            model.IsSelected = document.IsSelected;
          }
        }
      };
      try
      {
        layoutSerializer.Deserialize(@".\AvalonDock.Layout.config");
      }
      catch (Exception)
      {
      }
    }

    public void SaveLayout()
    {
      var layoutSerializer = new XmlLayoutSerializer(dockManager);
      layoutSerializer.Serialize(@".\AvalonDock.Layout.config");
    }
    #endregion
  }
}
