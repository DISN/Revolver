using Magnum.Controls.Themes;
using Magnum.Core.Events;
using Magnum.Core.Models;
using Magnum.Core.Services;
using Magnum.Core.ViewModels;
using Magnum.Core.Windows;
using Magnum.IconLibrary;
using Magnum.Tools.ErrorList;
using Magnum.Tools.ErrorList.Models;
using Magnum.Tools.Logger;
using Magnum.Tools.PropertyGrid;
using Magnum.Tools.PropertyGrid.Models;
using Magnum.Tools.ResourceBrowser;
using Magnum.Tools.ResourceBrowser.Models;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Unity;
using Microsoft.Windows.Controls.Ribbon;
using Revolver.Shell.Commands;
using Revolver.Shell.Services;
using Revolver.Tools.BehaviorTreeEditor;
using Revolver.Tools.BehaviorTreeEditor.Models;
using Revolver.Tools.DialogEditor;
using Revolver.Tools.DialogEditor.Models;
using Revolver.Tools.EntityCreator;
using Revolver.Tools.EntityCreator.Models;
using Revolver.Tools.FeedbackEditor;
using Revolver.Tools.FeedbackEditor.Models;
using Revolver.Tools.FightEditor;
using Revolver.Tools.FightEditor.Models;
using Revolver.Tools.Logger;
using Revolver.Tools.PatternCreator;
using Revolver.Tools.PatternCreator.Models;
using Revolver.Tools.TileEditor;
using Revolver.Tools.TileEditor.Models;
using Revolver.Tools.Viewer;
using Revolver.Tools.Viewer.Models;
using Revolver.Tools.WorldEditor;
using Revolver.Tools.WorldEditor.Models;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Shell;
using System.Xml;
using Xceed.Wpf.AvalonDock.Layout.Serialization;

namespace Revolver.Shell
{
  [Module(ModuleName = "Revolver.Shell")]
  [ModuleDependency("Magnum.Tools.Logger")]
  public class ShellModule : IModule
  {
    private IUnityContainer _container;
    private IEventAggregator _eventAggregator;

    public ShellModule(IUnityContainer container, IEventAggregator eventAggregator)
    {
      _container = container;
      _eventAggregator = eventAggregator;
    }

    public void Initialize()
    {
      _eventAggregator.GetEvent<SplashMessageUpdateEvent>().Publish(new SplashMessageUpdateEvent { Message = "Loading Shell Module" });
      _container.RegisterType<IStatusBarService, Revolver.Tools.StatusBar.StatusBar>(new ContainerControlledLifetimeManager());
      _container.RegisterType<IProgressBarService, Revolver.Tools.ProgressBar.ProgressBar>(new ContainerControlledLifetimeManager());
      _container.RegisterType<IThemeManager, ThemeManager>(new ContainerControlledLifetimeManager());

      // Try resolving a logger service - if not found, then register the NLog service
      try
      {
        _container.Resolve<ILoggerService>();
      }
      catch
      {
        _container.RegisterType<ILoggerService, NLogService>(new ContainerControlledLifetimeManager());
      }
      IModule loggerModule = _container.Resolve<ObjectEditorModule>();
      loggerModule.Initialize();

      // Register a default error List
      _container.RegisterType<IErrorList, Magnum.Tools.ErrorList.ViewModels.ErrorListViewModel>(new ContainerControlledLifetimeManager());
      IModule errorListModule = _container.Resolve<ErrorListModule>();
      errorListModule.Initialize();

      // Register a default file opener
      var registry = _container.Resolve<IContentHandlerRegistry>();
      registry.Register(_container.Resolve<AllFileHandler>());

      // Load other shell related stuff
      LoadTheme();
      LoadCommands(); // TODO: Have to put first or else Menu commands won't update on new command registered. Should automatically handle this...
      LoadRibbon();
      LoadMenus();
      LoadSettings();

      // Load Tools
      LoadLogger();
      LoadProperties();
      LoadResourceBrowser();
      LoadWorldEditor();
      LoadViewer();
      //LoadTileEditor();
      LoadPatternCreator();
      LoadEntityCreator();
      LoadFightEditor();
      LoadDialogEditor();
      LoadBehaviorTreeEditor();
      LoadFeedbackEditor();

      // Load other last second shell extras
      LoadToolsLibrary();
      LoadQuickLaunch();

      PostInitialize();
    }

    /// <summary>
    /// Load other items like TaskbarInfoItem, which will bind Engine commands on the application icon.
    /// </summary>
    public void PostInitialize()
    {
      var commandManager = _container.Resolve<ICommandManager>();
      var ribbonService = _container.Resolve<IRibbonService>();
      IWorkspace workspace = _container.Resolve<WorkspaceBase>();
      var menuService = _container.Resolve<IMenuService>();
      MainWindow.MainWindow mainWindow = _container.Resolve<IShell>() as MainWindow.MainWindow;

      // Set the play, stop and pause buttons in the MainWindow's TaskbarItemInfo
      ThumbButtonInfo playThumbButtonInfo = new ThumbButtonInfo();
      playThumbButtonInfo.Description = "Play the engine";
      playThumbButtonInfo.Command = commandManager.GetCommand("PLAYGAME");
      playThumbButtonInfo.ImageSource = BitmapImages.LoadBitmapFromIconType(IconType.Play);

      ThumbButtonInfo stopThumbButtonInfo = new ThumbButtonInfo();
      stopThumbButtonInfo.Description = "Stop the engine";
      stopThumbButtonInfo.Command = commandManager.GetCommand("STOPGAME");
      stopThumbButtonInfo.ImageSource = BitmapImages.LoadBitmapFromIconType(IconType.Stop);

      ThumbButtonInfo pauseThumbButtonInfo = new ThumbButtonInfo();
      pauseThumbButtonInfo.Description = "Pause the engine";
      pauseThumbButtonInfo.Command = commandManager.GetCommand("PAUSEGAME");
      pauseThumbButtonInfo.ImageSource = BitmapImages.LoadBitmapFromIconType(IconType.Pause);

      mainWindow.TaskbarItemInfo.ThumbButtonInfos.Add(playThumbButtonInfo);
      mainWindow.TaskbarItemInfo.ThumbButtonInfos.Add(stopThumbButtonInfo);
      mainWindow.TaskbarItemInfo.ThumbButtonInfos.Add(pauseThumbButtonInfo);

      // Load Ribbon settings after all the modules have added their tabs to the Ribbon.
      RibbonSettings ribbonSettings = _container.Resolve<RibbonSettings>();
      if (ribbonService.Ribbon != null)
      {
        ribbonService.Ribbon.IsMinimized = ribbonSettings.IsMinimized;

        if (!string.IsNullOrEmpty(ribbonSettings.RibbonQuickAccessToolBar))
        {
          QuickAccessToolbarButtonCollection buttons = null;
          using (StringReader stringReader = new StringReader(ribbonSettings.RibbonQuickAccessToolBar))
          {
            XmlReader xmlReader = XmlReader.Create(stringReader);
            buttons = (QuickAccessToolbarButtonCollection)XamlReader.Load(xmlReader);
            xmlReader.Close();
          }

          if (buttons != null)
          {
            ribbonService.Ribbon.QuickAccessToolBar = new RibbonQuickAccessToolBar();
            ribbonService.Ribbon.QuickAccessToolBar.Style = Application.Current.MainWindow.FindResource("RibbonQuickAccessToolBar") as Style;

            foreach (QuickAccessToolbarButton qaButton in buttons)
            {
              RibbonButton rButton = new RibbonButton()
              {
                Label = qaButton.Label,
                KeyTip = qaButton.KeyTip,
                LargeImageSource = qaButton.LargeImageSource,
                SmallImageSource = qaButton.SmallImageSource,
                ToolTip = qaButton.ToolTip,
                ToolTipDescription = qaButton.ToolTipDescription,
                ContentStringFormat = qaButton.CommandKey
              };
              if (qaButton.CommandKey != null)
                rButton.Command = commandManager.GetCommand(qaButton.CommandKey);
              rButton.Style = Application.Current.MainWindow.FindResource("RibbonButton") as Style;

              ribbonService.Ribbon.QuickAccessToolBar.Items.Add(rButton);
            }
          }
        }
      }
    }

    /// <summary>
    /// TODO(ndistefano) Add these loadings to the module initializations
    /// </summary>
    private void LoadLogger()
    {
      var commandManager = _container.Resolve<ICommandManager>();
      IWorkspace workspace = _container.Resolve<WorkspaceBase>();

      ToolViewModel logger = workspace.Tools.First(f => f.ContentId == "Logger");
      logger.OpenCommand = commandManager.GetCommand("LOGSHOW");
      /*var menuService = _container.Resolve<IMenuService>();
      if (propertyGrid != null)
        menuService.Get("_TOOLS").Add(new MenuItemViewModel("_Properties", 2,
                                                  BitmapImages.LoadBitmapFromResourceKey("Error_16x16"),
                                                  commandManager.GetCommand("PROPERTYGRIDSHOW"), "PROPERTYGRIDSHOW",
                                                  new KeyGesture(Key.P, ModifierKeys.Control, "Ctrl + P")) { IsCheckable = true, IsChecked = propertyGrid.IsVisible });*/
    }

    private void LoadProperties()
    {
      _eventAggregator.GetEvent<SplashMessageUpdateEvent>().Publish(new SplashMessageUpdateEvent { Message = "Loading Properties" });
      var commandManager = _container.Resolve<ICommandManager>();
      IWorkspace workspace = _container.Resolve<WorkspaceBase>();

      _container.RegisterType<IPropertyGrid, Magnum.Tools.PropertyGrid.ViewModels.PropertyGridViewModel>(new ContainerControlledLifetimeManager());
      IModule propertyGridModule = _container.Resolve<PropertyGridModule>();
      propertyGridModule.Initialize();

      var propertyGridCommand = new UndoableCommand<object>(TogglePropertyGrid);
      commandManager.RegisterCommand("PROPERTYGRIDSHOW", propertyGridCommand);

      ToolViewModel propertyGrid = workspace.Tools.First(f => f.ContentId == "Properties");
      propertyGrid.OpenCommand = commandManager.GetCommand("PROPERTYGRIDSHOW");
      /*var menuService = _container.Resolve<IMenuService>();
      if (propertyGrid != null)
        menuService.Get("_TOOLS").Add(new MenuItemViewModel("_Properties", 2,
                                                  BitmapImages.LoadBitmapFromResourceKey("Error_16x16"),
                                                  commandManager.GetCommand("PROPERTYGRIDSHOW"), "PROPERTYGRIDSHOW",
                                                  new KeyGesture(Key.P, ModifierKeys.Control, "Ctrl + P")) { IsCheckable = true, IsChecked = propertyGrid.IsVisible });*/
    }

    private void LoadResourceBrowser()
    {
      _eventAggregator.GetEvent<SplashMessageUpdateEvent>().Publish(new SplashMessageUpdateEvent { Message = "Loading Resource Browser" });
      var commandManager = _container.Resolve<ICommandManager>();
      IWorkspace workspace = _container.Resolve<WorkspaceBase>();

      _container.RegisterType<IResourceBrowser, Magnum.Tools.ResourceBrowser.ViewModels.ResourceBrowserViewModel>(new ContainerControlledLifetimeManager());
      IModule resourceBrowserModule = _container.Resolve<ResourceBrowserModule>();
      resourceBrowserModule.Initialize();

      var resourceBrowserCommand = new UndoableCommand<object>(ToggleResourceBrowser);
      commandManager.RegisterCommand("RESOURCEBROWSERSHOW", resourceBrowserCommand);

      ToolViewModel propertyGrid = workspace.Tools.First(f => f.ContentId == "ResourceBrowser");
      propertyGrid.OpenCommand = commandManager.GetCommand("RESOURCEBROWSERSHOW");
      /*var menuService = _container.Resolve<IMenuService>();
      if (propertyGrid != null)
        menuService.Get("_TOOLS").Add(new MenuItemViewModel("_Properties", 2,
                                                  BitmapImages.LoadBitmapFromResourceKey("Error_16x16"),
                                                  commandManager.GetCommand("PROPERTYGRIDSHOW"), "PROPERTYGRIDSHOW",
                                                  new KeyGesture(Key.P, ModifierKeys.Control, "Ctrl + P")) { IsCheckable = true, IsChecked = propertyGrid.IsVisible });*/
    }

    private void LoadWorldEditor()
    {
      _eventAggregator.GetEvent<SplashMessageUpdateEvent>().Publish(new SplashMessageUpdateEvent { Message = "Loading World Editor" });
      var commandManager = _container.Resolve<ICommandManager>();
      IWorkspace workspace = _container.Resolve<WorkspaceBase>();

      _container.RegisterType<IWorldEditor, Revolver.Tools.WorldEditor.ViewModels.WorldEditorViewModel>(new ContainerControlledLifetimeManager());
      IModule worldEditorModule = _container.Resolve<WorldEditorModule>();
      worldEditorModule.Initialize();

      var worldEditorCommand = new UndoableCommand<object>(ToggleWorldEditor);
      commandManager.RegisterCommand("WORLDEDITORSHOW", worldEditorCommand);

      ToolViewModel worldEditor = workspace.Tools.First(f => f.ContentId == "WorldEditor");
      worldEditor.OpenCommand = commandManager.GetCommand("WORLDEDITORSHOW");
      /*var menuService = _container.Resolve<IMenuService>();
      if (worldEditor != null)
        menuService.Get("_TOOLS").Add(new MenuItemViewModel("_World Editor", 3,
                                                  BitmapImages.LoadBitmapFromResourceKey("Game_32x32"),
                                                  commandManager.GetCommand("WORLDEDITORSHOW"), "WORLDEDITORSHOW",
                                                  new KeyGesture(Key.W, ModifierKeys.Control, "Ctrl + W")) { IsCheckable = true, IsChecked = worldEditor.IsVisible });*/
    }

    private void LoadViewer()
    {
      _eventAggregator.GetEvent<SplashMessageUpdateEvent>().Publish(new SplashMessageUpdateEvent { Message = "Loading Viewer" });
      var commandManager = _container.Resolve<ICommandManager>();
      IWorkspace workspace = _container.Resolve<WorkspaceBase>();

      _container.RegisterType<IViewer, Revolver.Tools.Viewer.ViewModels.ViewerViewModel>(new ContainerControlledLifetimeManager());
      IModule viewerModule = _container.Resolve<ViewerModule>();
      viewerModule.Initialize();

      var viewerCommand = new UndoableCommand<object>(ToggleViewer);
      commandManager.RegisterCommand("VIEWERSHOW", viewerCommand);

      ToolViewModel viewer = workspace.Tools.First(f => f.ContentId == "Viewer");
      viewer.OpenCommand = commandManager.GetCommand("VIEWERSHOW");
      /*var menuService = _container.Resolve<IMenuService>();
      if (viewer != null)
        menuService.Get("_TOOLS").Add(new MenuItemViewModel("_Viewer", 4,
                                                  BitmapImages.LoadBitmapFromResourceKey("Map_16x16"),
                                                  commandManager.GetCommand("VIEWERSHOW"), "VIEWERSHOW",
                                                  new KeyGesture(Key.Q, ModifierKeys.Control, "Ctrl + Q")) { IsCheckable = true, IsChecked = viewer.IsVisible });*/
    }

    private void LoadTileEditor()
    {
      _eventAggregator.GetEvent<SplashMessageUpdateEvent>().Publish(new SplashMessageUpdateEvent { Message = "Loading Tile Editor" });
      var commandManager = _container.Resolve<ICommandManager>();
      IWorkspace workspace = _container.Resolve<WorkspaceBase>();

      _container.RegisterType<ITileEditor, Revolver.Tools.TileEditor.ViewModels.TileEditorViewModel>(new ContainerControlledLifetimeManager());
      IModule tileEditorModule = _container.Resolve<TileEditorModule>();
      tileEditorModule.Initialize();

      var tileEditorCommand = new UndoableCommand<object>(ToggleTileEditor);
      commandManager.RegisterCommand("TILEEDITORSHOW", tileEditorCommand);

      ToolViewModel tileEditor = workspace.Tools.First(f => f.ContentId == "TileEditor");
      tileEditor.OpenCommand = commandManager.GetCommand("TILEEDITORSHOW");
      /*var menuService = _container.Resolve<IMenuService>();
      if (tileEditor != null)
        menuService.Get("_TOOLS").Add(new MenuItemViewModel("_Tile Editor", 4,
                                                  BitmapImages.LoadBitmapFromResourceKey("Map_16x16"),
                                                  commandManager.GetCommand("TILEEDITORSHOW"), "TILEEDITORSHOW",
                                                  new KeyGesture(Key.T, ModifierKeys.Control, "Ctrl + T")) { IsCheckable = true, IsChecked = tileEditor.IsVisible });*/
    }

    private void LoadPatternCreator()
    {
      _eventAggregator.GetEvent<SplashMessageUpdateEvent>().Publish(new SplashMessageUpdateEvent { Message = "Loading Pattern Creator" });
      var commandManager = _container.Resolve<ICommandManager>();
      IWorkspace workspace = _container.Resolve<WorkspaceBase>();

      _container.RegisterType<IPatternCreator, Revolver.Tools.PatternCreator.ViewModels.PatternCreatorViewModel>(new ContainerControlledLifetimeManager());
      IModule patternCreatorModule = _container.Resolve<PatternCreatorModule>();
      patternCreatorModule.Initialize();

      var patternCreatorCommand = new UndoableCommand<object>(TogglePatternCreator);
      commandManager.RegisterCommand("PATTERNCREATORSHOW", patternCreatorCommand);

      ToolViewModel patternCreator = workspace.Tools.First(f => f.ContentId == "PatternCreator");
      patternCreator.OpenCommand = commandManager.GetCommand("PATTERNCREATORSHOW");
    }

    /// <summary>
    /// TODO(ndistefano) Add these loadings to the module initializations
    /// </summary>
    private void LoadEntityCreator()
    {
      _eventAggregator.GetEvent<SplashMessageUpdateEvent>().Publish(new SplashMessageUpdateEvent { Message = "Loading Entity Creator" });
      var commandManager = _container.Resolve<ICommandManager>();
      IWorkspace workspace = _container.Resolve<WorkspaceBase>();

      _container.RegisterType<IEntityCreator, Revolver.Tools.EntityCreator.ViewModels.EntityCreatorViewModel>(new ContainerControlledLifetimeManager());
      IModule patternCreatorModule = _container.Resolve<EntityCreatorModule>();
      patternCreatorModule.Initialize();

      var entityCreatorCommand = new UndoableCommand<object>(ToggleEntityCreator);
      commandManager.RegisterCommand("ENTITYCREATORSHOW", entityCreatorCommand);

      ToolViewModel entityCreator = workspace.Tools.First(f => f.ContentId == "EntityCreator");
      entityCreator.OpenCommand = commandManager.GetCommand("ENTITYCREATORSHOW");
    }
    
    /// <summary>
    /// TODO(ndistefano) Add these loadings to the module initializations
    /// </summary>
    private void LoadFightEditor()
    {
      _eventAggregator.GetEvent<SplashMessageUpdateEvent>().Publish(new SplashMessageUpdateEvent { Message = "Loading Fight Editor" });
      var commandManager = _container.Resolve<ICommandManager>();
      IWorkspace workspace = _container.Resolve<WorkspaceBase>();

      _container.RegisterType<IFightEditor, Revolver.Tools.FightEditor.ViewModels.FightEditorViewModel>(new ContainerControlledLifetimeManager());
      IModule fightEditorModule = _container.Resolve<FightEditorModule>();
      fightEditorModule.Initialize();

      var fightEditorCommand = new UndoableCommand<object>(ToggleFightEditor);
      commandManager.RegisterCommand("FIGHTEDITORSHOW", fightEditorCommand);

      ToolViewModel fightEditor = workspace.Tools.First(f => f.ContentId == "FightEditor");
      fightEditor.OpenCommand = commandManager.GetCommand("FIGHTEDITORSHOW");
    }

    /// <summary>
    /// TODO(ndistefano) Add these loadings to the module initializations
    /// </summary>
    private void LoadDialogEditor()
    {
      _eventAggregator.GetEvent<SplashMessageUpdateEvent>().Publish(new SplashMessageUpdateEvent { Message = "Loading Dialog Editor" });
      var commandManager = _container.Resolve<ICommandManager>();
      IWorkspace workspace = _container.Resolve<WorkspaceBase>();

      _container.RegisterType<IDialogEditor, Revolver.Tools.DialogEditor.ViewModels.DialogEditorViewModel>(new ContainerControlledLifetimeManager());
      IModule dialogEditorModule = _container.Resolve<DialogEditorModule>();
      dialogEditorModule.Initialize();

      var dialogEditorCommand = new UndoableCommand<object>(ToggleDialogEditor);
      commandManager.RegisterCommand("DIALOGEDITORSHOW", dialogEditorCommand);

      ToolViewModel dialogEditor = workspace.Tools.First(f => f.ContentId == "DialogEditor");
      dialogEditor.OpenCommand = commandManager.GetCommand("DIALOGEDITORSHOW");
    }
    
    /// <summary>
    /// TODO(ndistefano) Add these loadings to the module initializations
    /// </summary>
    private void LoadBehaviorTreeEditor()
    {
      _eventAggregator.GetEvent<SplashMessageUpdateEvent>().Publish(new SplashMessageUpdateEvent { Message = "Loading Behavior Tree Editor" });
      var commandManager = _container.Resolve<ICommandManager>();
      IWorkspace workspace = _container.Resolve<WorkspaceBase>();

      _container.RegisterType<IBehaviorTree, Revolver.Tools.BehaviorTreeEditor.ViewModels.BehaviorTreeEditorViewModel>(new ContainerControlledLifetimeManager());
      IModule behaviorTreeEditorModule = _container.Resolve<BehaviorTreeEditorModule>();
      behaviorTreeEditorModule.Initialize();

      var behaviorTreeEditorCommand = new UndoableCommand<object>(ToggleBehaviorTreeEditor);
      commandManager.RegisterCommand("BEHAVIORTREEEDITORSHOW", behaviorTreeEditorCommand);

      ToolViewModel behaviorTreeEditor = workspace.Tools.First(f => f.ContentId == "BehaviorTreeEditor");
      behaviorTreeEditor.OpenCommand = commandManager.GetCommand("BEHAVIORTREEEDITORSHOW");
    }

    /// <summary>
    /// TODO(ndistefano) Add these loadings to the module initializations
    /// </summary>
    private void LoadFeedbackEditor()
    {
      _eventAggregator.GetEvent<SplashMessageUpdateEvent>().Publish(new SplashMessageUpdateEvent { Message = "Loading Feedback Editor" });
      var commandManager = _container.Resolve<ICommandManager>();
      IWorkspace workspace = _container.Resolve<WorkspaceBase>();

      _container.RegisterType<IFeedback, Revolver.Tools.FeedbackEditor.ViewModels.FeedbackEditorViewModel>(new ContainerControlledLifetimeManager());
      IModule feedbackEditorModule = _container.Resolve<FeedbackEditorModule>();
      feedbackEditorModule.Initialize();

      var feedbackEditorCommand = new UndoableCommand<object>(ToggleFeedbackEditor);
      commandManager.RegisterCommand("FEEDBACKEDITORSHOW", feedbackEditorCommand);

      ToolViewModel feedbackEditor = workspace.Tools.First(f => f.ContentId == "FeedbackEditor");
      feedbackEditor.OpenCommand = commandManager.GetCommand("FEEDBACKEDITORSHOW");
    }

    private void LoadTheme()
    {
      _eventAggregator.GetEvent<SplashMessageUpdateEvent>().Publish(new SplashMessageUpdateEvent { Message = "Loading Themes" });
      var manager = _container.Resolve<IThemeManager>();
      var themeSettings = _container.Resolve<ThemeSettings>();
      var statusBar = _container.Resolve<IStatusBarService>();
      manager.AddTheme(new LightTheme());
      manager.AddTheme(new DarkTheme());
      manager.SetCurrent(themeSettings.SelectedTheme);
      statusBar.SelectedTheme = manager.CurrentTheme.Name;
    }

    private void LoadMenus()
    {
      _eventAggregator.GetEvent<SplashMessageUpdateEvent>().Publish(new SplashMessageUpdateEvent { Message = "Loading Menus" });
      var manager = _container.Resolve<ICommandManager>();
      var menuService = _container.Resolve<IMenuService>();
      var themeSettings = _container.Resolve<ThemeSettings>();
      var statusBarSettings = _container.Resolve<StatusBarSettings>();
      var statusBar = _container.Resolve<IStatusBarService>();
      IWorkspace workspace = _container.Resolve<WorkspaceBase>();
      ToolViewModel logger = workspace.Tools.First(f => f.ContentId == "Logger");

      menuService.Add(new MenuItemViewModel("_FILE", 1));

      menuService.Get("_FILE").Add(new MenuItemViewModel("_Save", 5,
                                    BitmapImages.LoadBitmapFromResourceKey("Save_32x32"),
                                    manager.GetCommand("SAVE"), "SAVE",
                                    new KeyGesture(Key.S, ModifierKeys.Control, "Ctrl + S")));
      menuService.Get("_FILE").Add(MenuItemViewModel.Separator(6));
      menuService.Get("_FILE").Add(new MenuItemViewModel("Close Window", 8,
                                    null,
                                    manager.GetCommand("CLOSE"), "CLOSE",
                                    new KeyGesture(Key.F4, ModifierKeys.Control, "Ctrl + F4")));

      menuService.Get("_FILE").Add(new MenuItemViewModel("E_xit Revolver", 101,
                                    null,
                                    manager.GetCommand("EXIT"), "EXIT",
                                    new KeyGesture(Key.F4, ModifierKeys.Alt, "Alt + F4")));


      menuService.Add(new MenuItemViewModel("_EDIT", 2));
      menuService.Get("_EDIT").Add(new MenuItemViewModel("_Undo", 1,
                                    BitmapImages.LoadBitmapFromResourceKey("Undo_32x32"),
                                    manager.GetCommand("UNDO"), "UNDO",
                                    new KeyGesture(Key.Z, ModifierKeys.Control, "Ctrl + Z")));
      menuService.Get("_EDIT").Add(new MenuItemViewModel("_Redo", 2,
                                    BitmapImages.LoadBitmapFromResourceKey("Redo_32x32"),
                                    manager.GetCommand("REDO"), "REDO",
                                    new KeyGesture(Key.Y, ModifierKeys.Control, "Ctrl + Y")));
      /*menuService.Get("_EDIT").Add(MenuItemViewModel.Separator(15));
      menuService.Get("_EDIT").Add(new MenuItemViewModel("Cut", 20,
                                    BitmapImages.LoadBitmapFromResourceKey("Cut_32x32"),
                                    ApplicationCommands.Cut));
      menuService.Get("_EDIT").Add(new MenuItemViewModel("Copy", 21,
                                    BitmapImages.LoadBitmapFromResourceKey("Copy_32x32"),
                                    ApplicationCommands.Copy));
      menuService.Get("_EDIT").Add(new MenuItemViewModel("_Paste", 22,
                                    BitmapImages.LoadBitmapFromResourceKey("Paste_32x32"),
                                    ApplicationCommands.Paste));*/

      // TODO(ndistefano): Find a way to highlight the menuItem even though a popup is hidden behind
      menuService.Add(new MenuItemViewModel("_TOOLS", 3, null, manager.GetCommand("TOGGLETOOLSLIBRARY")) { IsCheckable = true, IsChecked = workspace.IsToolsLibraryOpened });
      /*menuService.Get("_TOOLS").Add(new MenuItemViewModel("_Script Editor", 1,
                                     null,
                                     manager.GetCommand("NEW"), "NEW",
                                     new KeyGesture(Key.D, ModifierKeys.Control, "Ctrl + D"), false, null,
                                     "Script Editor", "Manage scripts to be run in the game."));
      if (logger != null)
        menuService.Get("_TOOLS").Add(new MenuItemViewModel("_Logger", 10,
                                       BitmapImages.LoadBitmapFromResourceKey("Information_32x32"),
                                       manager.GetCommand("LOGSHOW"), "LOGSHOW",
                                       new KeyGesture(Key.L, ModifierKeys.Control, "Ctrl + L"), false, null,
                                       "Logger", "Show different logs fired from the application and specific editors."));*/

      menuService.Add(new MenuItemViewModel("_WINDOW", 4));
      menuService.Get("_WINDOW").Add(new MenuItemViewModel("Dark Theme", 1,
                                                null,
                                                manager.GetCommand("THEMECHANGE")) { IsCheckable = true, IsChecked = (themeSettings.SelectedTheme == "Dark"), CommandParameter = "Dark" });
      menuService.Get("_WINDOW").Add(new MenuItemViewModel("Light Theme", 2,
                                                null,
                                                manager.GetCommand("THEMECHANGE")) { IsCheckable = true, IsChecked = (themeSettings.SelectedTheme == "Light"), CommandParameter = "Light" });
      menuService.Get("_WINDOW").Add(new MenuItemViewModel("Save Layout", 3,
                                                null,
                                                manager.GetCommand("SAVELAYOUT")));
      menuService.Get("_WINDOW").Add(MenuItemViewModel.Separator(15));
      menuService.Get("_WINDOW").Add(new MenuItemViewModel("Show Status Bar Background", 16,
                                                null,
                                                manager.GetCommand("TOGGLESTATUSBARBACKGROUND")) { IsCheckable = true, IsChecked = (statusBarSettings.ShowStatusBarBackground) });

      menuService.Add(new MenuItemViewModel("_HELP", 5));
      menuService.Get("_HELP").Add(new MenuItemViewModel("About Revolver", 1));
    }

    private void LoadRibbon()
    {
      _eventAggregator.GetEvent<SplashMessageUpdateEvent>().Publish(new SplashMessageUpdateEvent { Message = "Loading Ribbon" });
      var manager = _container.Resolve<ICommandManager>();
      var ribbonService = _container.Resolve<IRibbonService>();
      var menuService = _container.Resolve<IMenuService>();

      ribbonService.Add(new RibbonItemViewModel("ENGINE", 50, "E"));
      ribbonService.Get("ENGINE").Add(new RibbonItemViewModel("Status", 1));

      ribbonService.Add(new RibbonItemViewModel("GENERAL", 100, "G"));
      ribbonService.Get("GENERAL").Add(new RibbonItemViewModel("Edit", 1));
      ribbonService.Get("GENERAL").Get("Edit").Add(new MenuItemViewModel("Undo", 1,
                                                            BitmapImages.LoadBitmapFromResourceKey("Undo_32x32"),
                                                            manager.GetCommand("UNDO"), "UNDO", null, false, null,
                                                            string.Empty, "Undo a previous action"));
      ribbonService.Get("GENERAL").Get("Edit").Add(new MenuItemViewModel("Redo", 2,
                                                            BitmapImages.LoadBitmapFromResourceKey("Redo_32x32"),
                                                            manager.GetCommand("REDO"), "REDO", null, false, null,
                                                            string.Empty, "Redo a previous action"));
    }

    private void LoadSettings()
    {
      //Set the position of the window based on previous session values
      WindowPositionSettings position = _container.Resolve<WindowPositionSettings>();
      MainWindow.MainWindow mainWindow = _container.Resolve<IShell>() as MainWindow.MainWindow;
      if (mainWindow != null)
      {
        mainWindow.Top = position.Top;
        mainWindow.Left = position.Left;
        mainWindow.Width = position.Width;
        mainWindow.Height = position.Height;
        mainWindow.WindowState = WindowState.Minimized; // we need to set the WindowState after the window is loaded or else it won't take multiple screen setups into account
        mainWindow.Topmost = true; // we set it at false after the window is loaded
      }

      //Set the workspace settings based on previous session values
      StatusBarSettings statusBarSettings = _container.Resolve<StatusBarSettings>();
      var statusBar = _container.Resolve<IStatusBarService>();
      if (statusBar != null)
      {
        statusBar.ShowStatusBarBackground = statusBarSettings.ShowStatusBarBackground;
      }
    }

    private void LoadToolsLibrary()
    {
      _eventAggregator.GetEvent<SplashMessageUpdateEvent>().Publish(new SplashMessageUpdateEvent { Message = "Loading Tools Library Module" });
      _container.RegisterType<IToolsLibrary, ToolsLibraryViewModel>(new ContainerControlledLifetimeManager());

      IToolsLibrary toolsLibrary = _container.Resolve<ToolsLibraryViewModel>();
      WorkspaceBase workspace = _container.Resolve<WorkspaceBase>() as WorkspaceBase;
      workspace.ToolsLibrary = new ToolsLibraryViewModel(workspace);
    }

    private void LoadQuickLaunch()
    {
      _eventAggregator.GetEvent<SplashMessageUpdateEvent>().Publish(new SplashMessageUpdateEvent { Message = "Loading Quick Launch Module" });
      _container.RegisterType<IQuickLaunch, QuickLaunchViewModel>(new ContainerControlledLifetimeManager());

      IQuickLaunch quickLaunch = _container.Resolve<QuickLaunchViewModel>();
      WorkspaceBase workspace = _container.Resolve<WorkspaceBase>() as WorkspaceBase;
      workspace.QuickLaunch = new QuickLaunchViewModel(workspace);
    }

    private void LoadCommands()
    {
      _eventAggregator.GetEvent<SplashMessageUpdateEvent>().Publish(new SplashMessageUpdateEvent { Message = "Loading Commands" });
      var manager = _container.Resolve<ICommandManager>();

      var openCommand = new UndoableCommand<object>(OpenModule);
      var exitCommand = new UndoableCommand<object>(CloseCommandExecute);
      var saveCommand = new UndoableCommand<object>(Save, CanExecuteSave);
      var loggerCommand = new UndoableCommand<object>(ToggleLogger);
      var changeThemeCommand = new UndoableCommand<string>(ToggleTheme);
      var saveLayoutCommand = new UndoableCommand<object>(SaveLayout);
      var closeCommand = new UndoableCommand<object>(CloseDocument, CanExecuteCloseDocument);
      var toggleStatusBarBackgroundCommand = new UndoableCommand<object>(ToggleStatusBarBackground);
      var toggleToolsLibrary = new UndoableCommand<object>(ToggleToolsLibrary);

      manager.RegisterCommand("CLOSE", closeCommand);
      manager.RegisterCommand("OPEN", openCommand);
      manager.RegisterCommand("SAVE", saveCommand);
      manager.RegisterCommand("EXIT", exitCommand);
      manager.RegisterCommand("LOGSHOW", loggerCommand);
      manager.RegisterCommand("THEMECHANGE", changeThemeCommand);
      manager.RegisterCommand("SAVELAYOUT", saveLayoutCommand);
      manager.RegisterCommand("TOGGLESTATUSBARBACKGROUND", toggleStatusBarBackgroundCommand);
      manager.RegisterCommand("TOGGLETOOLSLIBRARY", toggleToolsLibrary);

      // Register other commands from the Revolver.Shell.Commands folder
      _container.RegisterType<IShellCommands, ShellCommands>(new ContainerControlledLifetimeManager());
    }

    public void SaveLayout(object obj)
    {
      var layoutSerializer = new XmlLayoutSerializer(Magnum.Core.ApplicationModel.Application.DockingManager);
      layoutSerializer.Serialize(@".\AvalonDock.Layout.config");
    }

    private void CloseCommandExecute(object obj)
    {
      IShell shell = _container.Resolve<IShell>();
      shell.Close();
    }

    #region Open

    private void OpenModule(object obj)
    {
      var service = _container.Resolve<IOpenFileService>();
      service.Open();
    }

    #endregion

    #region Save
    private bool CanExecuteSave(object obj)
    {
      IWorkspace workspace = _container.Resolve<WorkspaceBase>();
      if (workspace.ActiveTool != null)
      {
        return workspace.ActiveTool.Model.IsDirty;
      }
      return false;
    }

    private void Save(object obj)
    {
      IWorkspace workspace = _container.Resolve<WorkspaceBase>();
      //workspace.ActiveDocument.Handler.SaveContent(workspace.ActiveDocument);
      workspace.ActiveTool.SaveCommand.Execute(null);
    }

    private void CloseDocument(object obj)
    {
      //IWorkspace workspace = _container.Resolve<WorkspaceBase>();
      //workspace.ActiveDocument.Handler.CloseDocument(obj);
    }

    /// <summary>
    /// Can the close command execute? Checks if there is an ActiveDocument - if present, returns true.
    /// </summary>
    /// <param name="obj">The obj.</param>
    /// <returns><c>true</c> if this instance can execute close document; otherwise, <c>false</c>.</returns>
    private bool CanExecuteCloseDocument(object obj)
    {
      DocumentViewModel vm = obj as DocumentViewModel;
      if (vm != null)
        return true;

      IWorkspace workspace = _container.Resolve<WorkspaceBase>();
      return workspace.ActiveDocument != null;
    }
    #endregion

    private void ToggleStatusBarBackground(object obj)
    {
      var statusBar = _container.Resolve<IStatusBarService>();
      statusBar.ShowStatusBarBackground = !statusBar.ShowStatusBarBackground;
    }

    private void ToggleToolsLibrary(object obj)
    {
      IWorkspace workspace = _container.Resolve<WorkspaceBase>();
      workspace.IsToolsLibraryOpened = !workspace.IsToolsLibraryOpened;
    }

    #region Logger
    private void ToggleLogger(object obj)
    {
      IWorkspace workspace = _container.Resolve<WorkspaceBase>();
      var menuService = _container.Resolve<IMenuService>();
      ToolViewModel logger = workspace.Tools.First(f => f.ContentId == "Logger");
      if (logger != null)
      {
        logger.IsVisible = !logger.IsVisible;
        //var mi = menuService.Get("_TOOLS").Get("_Logger") as MenuItemBase;
        //mi.IsChecked = logger.IsVisible;
        //logger.IsActive = mi.IsChecked;
        logger.IsActive = logger.IsVisible;
      }
      workspace.IsToolsLibraryOpened = false;

      // Force a ActiveContent changed here in case the ribbon header doesn't collapse for some reason
      Magnum.Core.ApplicationModel.Application.DockingManager.ActiveContent = logger;
    }
    #endregion

    #region Property Grid
    private void TogglePropertyGrid(object obj)
    {
      IWorkspace workspace = _container.Resolve<WorkspaceBase>();
      var menuService = _container.Resolve<IMenuService>();
      ToolViewModel propertyGrid = workspace.Tools.First(f => f.ContentId == "Properties");
      if (propertyGrid != null)
      {
        propertyGrid.IsVisible = !propertyGrid.IsVisible;
        //var mi = menuService.Get("_TOOLS").Get("_Properties") as MenuItemBase;
        //mi.IsChecked = propertyGrid.IsVisible;
        //propertyGrid.IsActive = mi.IsChecked;
        propertyGrid.IsActive = propertyGrid.IsVisible;
      }
      workspace.IsToolsLibraryOpened = false;

      // Force a ActiveContent changed here in case the ribbon header doesn't collapse for some reason
      Magnum.Core.ApplicationModel.Application.DockingManager.ActiveContent = propertyGrid;
    }
    #endregion

    #region Resource Browser
    private void ToggleResourceBrowser(object obj)
    {
      IWorkspace workspace = _container.Resolve<WorkspaceBase>();
      var menuService = _container.Resolve<IMenuService>();
      ToolViewModel resourceBrowser = workspace.Tools.First(f => f.ContentId == "ResourceBrowser");
      if (resourceBrowser != null)
      {
        resourceBrowser.IsVisible = !resourceBrowser.IsVisible;
        //var mi = menuService.Get("_TOOLS").Get("_Properties") as MenuItemBase;
        //mi.IsChecked = propertyGrid.IsVisible;
        //propertyGrid.IsActive = mi.IsChecked;
        resourceBrowser.IsActive = resourceBrowser.IsVisible;
      }
      workspace.IsToolsLibraryOpened = false;

      // Force a ActiveContent changed here in case the ribbon header doesn't collapse for some reason
      Magnum.Core.ApplicationModel.Application.DockingManager.ActiveContent = resourceBrowser;
    }
    #endregion

    #region World Editor
    private void ToggleWorldEditor(object obj)
    {
      IWorkspace workspace = _container.Resolve<WorkspaceBase>();
      var menuService = _container.Resolve<IMenuService>();
      ToolViewModel worldEditor = workspace.Tools.First(f => f.ContentId == "WorldEditor");
      if (worldEditor != null)
      {
        worldEditor.IsVisible = !worldEditor.IsVisible;
        //var mi = menuService.Get("_TOOLS").Get("_World Editor") as MenuItemBase;
        //mi.IsChecked = worldEditor.IsVisible;
        //worldEditor.IsActive = mi.IsChecked;
        worldEditor.IsActive = worldEditor.IsVisible;
      }
      workspace.IsToolsLibraryOpened = false;

      // Force a ActiveContent changed here in case the ribbon header doesn't collapse for some reason
      Magnum.Core.ApplicationModel.Application.DockingManager.ActiveContent = worldEditor;
    }
    #endregion

    #region Viewer
    private void ToggleViewer(object obj)
    {
      IWorkspace workspace = _container.Resolve<WorkspaceBase>();
      var menuService = _container.Resolve<IMenuService>();
      ToolViewModel viewer = workspace.Tools.First(f => f.ContentId == "Viewer");
      if (viewer != null)
      {
        viewer.IsVisible = !viewer.IsVisible;
        //var mi = menuService.Get("_TOOLS").Get("_Viewer") as MenuItemBase;
        //mi.IsChecked = viewer.IsVisible;
        //viewer.IsActive = mi.IsChecked;
        viewer.IsActive = viewer.IsVisible;
      }
      workspace.IsToolsLibraryOpened = false;

      // Force a ActiveContent changed here in case the ribbon header doesn't collapse for some reason
      Magnum.Core.ApplicationModel.Application.DockingManager.ActiveContent = viewer;
    }
    #endregion

    #region Tile Editor
    private void ToggleTileEditor(object obj)
    {
      IWorkspace workspace = _container.Resolve<WorkspaceBase>();
      var menuService = _container.Resolve<IMenuService>();
      ToolViewModel tileEditor = workspace.Tools.First(f => f.ContentId == "TileEditor");
      if (tileEditor != null)
      {
        tileEditor.IsVisible = !tileEditor.IsVisible;
        //var mi = menuService.Get("_TOOLS").Get("_Tile Editor") as MenuItemBase;
        //mi.IsChecked = tileEditor.IsVisible;
        //tileEditor.IsActive = mi.IsChecked;
        tileEditor.IsActive = tileEditor.IsVisible;
      }
      workspace.IsToolsLibraryOpened = false;

      // Force a ActiveContent changed here in case the ribbon header doesn't collapse for some reason
      Magnum.Core.ApplicationModel.Application.DockingManager.ActiveContent = tileEditor;
    }
    #endregion

    #region Pattern Creator
    private void TogglePatternCreator(object obj)
    {
      IWorkspace workspace = _container.Resolve<WorkspaceBase>();
      var menuService = _container.Resolve<IMenuService>();
      ToolViewModel patternCreator = workspace.Tools.First(f => f.ContentId == "PatternCreator");
      if (patternCreator != null)
      {
        patternCreator.IsVisible = !patternCreator.IsVisible;
        patternCreator.IsActive = patternCreator.IsVisible;
      }
      workspace.IsToolsLibraryOpened = false;

      // Force a ActiveContent changed here in case the ribbon header doesn't collapse for some reason
      Magnum.Core.ApplicationModel.Application.DockingManager.ActiveContent = patternCreator;
    }
    #endregion
    
    #region Entity Creator
    private void ToggleEntityCreator(object obj)
    {
      IWorkspace workspace = _container.Resolve<WorkspaceBase>();
      var menuService = _container.Resolve<IMenuService>();
      ToolViewModel entityCreator = workspace.Tools.First(f => f.ContentId == "EntityCreator");
      if (entityCreator != null)
      {
        entityCreator.IsVisible = !entityCreator.IsVisible;
        entityCreator.IsActive = entityCreator.IsVisible;
      }
      workspace.IsToolsLibraryOpened = false;

      // Force a ActiveContent changed here in case the ribbon header doesn't collapse for some reason
      Magnum.Core.ApplicationModel.Application.DockingManager.ActiveContent = entityCreator;
    }
    #endregion

    #region Fight Editor
    private void ToggleFightEditor(object obj)
    {
      IWorkspace workspace = _container.Resolve<WorkspaceBase>();
      var menuService = _container.Resolve<IMenuService>();
      ToolViewModel fightEditor = workspace.Tools.First(f => f.ContentId == "FightEditor");
      if (fightEditor != null)
      {
        fightEditor.IsVisible = !fightEditor.IsVisible;
        fightEditor.IsActive = fightEditor.IsVisible;
      }
      workspace.IsToolsLibraryOpened = false;

      // Force a ActiveContent changed here in case the ribbon header doesn't collapse for some reason
      Magnum.Core.ApplicationModel.Application.DockingManager.ActiveContent = fightEditor;
    }
    #endregion

    #region Dialog Editor
    private void ToggleDialogEditor(object obj)
    {
      IWorkspace workspace = _container.Resolve<WorkspaceBase>();
      var menuService = _container.Resolve<IMenuService>();
      ToolViewModel dialogEditor = workspace.Tools.First(f => f.ContentId == "DialogEditor");
      if (dialogEditor != null)
      {
        dialogEditor.IsVisible = !dialogEditor.IsVisible;
        dialogEditor.IsActive = dialogEditor.IsVisible;
      }
      workspace.IsToolsLibraryOpened = false;

      // Force a ActiveContent changed here in case the ribbon header doesn't collapse for some reason
      Magnum.Core.ApplicationModel.Application.DockingManager.ActiveContent = dialogEditor;
    }
    #endregion

    #region Behavior Tree Editor
    private void ToggleBehaviorTreeEditor(object obj)
    {
      IWorkspace workspace = _container.Resolve<WorkspaceBase>();
      var menuService = _container.Resolve<IMenuService>();
      ToolViewModel behaviorTreeEditor = workspace.Tools.First(f => f.ContentId == "BehaviorTreeEditor");
      if (behaviorTreeEditor != null)
      {
        behaviorTreeEditor.IsVisible = !behaviorTreeEditor.IsVisible;
        behaviorTreeEditor.IsActive = behaviorTreeEditor.IsVisible;
      }
      workspace.IsToolsLibraryOpened = false;

      // Force a ActiveContent changed here in case the ribbon header doesn't collapse for some reason
      Magnum.Core.ApplicationModel.Application.DockingManager.ActiveContent = behaviorTreeEditor;
    }
    #endregion

    #region Feedback Editor
    private void ToggleFeedbackEditor(object obj)
    {
      IWorkspace workspace = _container.Resolve<WorkspaceBase>();
      var menuService = _container.Resolve<IMenuService>();
      ToolViewModel feedbackEditor = workspace.Tools.First(f => f.ContentId == "FeedbackEditor");
      if (feedbackEditor != null)
      {
        feedbackEditor.IsVisible = !feedbackEditor.IsVisible;
        feedbackEditor.IsActive = feedbackEditor.IsVisible;
      }
      workspace.IsToolsLibraryOpened = false;

      // Force a ActiveContent changed here in case the ribbon header doesn't collapse for some reason
      Magnum.Core.ApplicationModel.Application.DockingManager.ActiveContent = feedbackEditor;
    }
    #endregion

    #region Themes
    private void ToggleTheme(string s)
    {
      var statusBar = _container.Resolve<IStatusBarService>();
      var manager = _container.Resolve<IThemeManager>();
      var menuService = _container.Resolve<IMenuService>();
      MenuItemViewModel mvm = menuService.Get("_WINDOW").Get(manager.CurrentTheme.Name + " Theme") as MenuItemViewModel;

      if (manager.CurrentTheme.Name != s)
      {
        if (mvm != null)
          mvm.IsChecked = false;
        manager.SetCurrent(s);
        statusBar.SelectedTheme = manager.CurrentTheme.Name;
      }
      else
      {
        if (mvm != null)
          mvm.IsChecked = true;
      }
    }
    #endregion
  }
}
