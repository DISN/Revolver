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

namespace Revolver.Tools.WorldEditor.Models
{
  class WorldEditorModel : ToolModel
  {
    Game _game;
    private ProgressBar.ProgressBar _progressBar;
    private ObservableCollection<GameItem> _Games;
    string _lastLoadedGame;
    ContextMenu _firstNodeContextMenu, _worldNodeContextMenu;
    XmlManager<Game> _gameXmlManager;

    public WorldEditorModel(string displayName, IUnityContainer container, string category, string description, bool isPinned = true, string shortcut = null)
      : base(displayName, container, category, description, isPinned, shortcut)
    {
      Container = container;
      _progressBar = new ProgressBar.ProgressBar();
      _Games = new ObservableCollection<GameItem>();
      _lastLoadedGame = String.Empty;
      _game = new Game();

      // Generate XmlManager with overrides, just so we don't save unwanted properties in XML files
      var overrides = _game.GenerateXmlAttributeOverrides();
      _gameXmlManager = new XmlManager<Game>(overrides);

      InitializeCommands();
      InitializeFirstNodeContextMenu();
      InitializeWorldContextMenu();

      OnCurrentlySelectedItemChanged += WorldEditorModel_OnCurrentlySelectedItemChanged;
    }
    
    #region Properties
    
    public event EventHandler OnGamesChanged;
    public ObservableCollection<GameItem> Games
    {
      get { return _Games; }
      set
      {
        if (_Games != value)
        {
          _Games = value;
          RaisePropertyChanged("Games");

          var handler = OnGamesChanged;
          if (handler != null)
            handler(this, null);
        }
      }
    }

    public event EventHandler OnLastLoadedGameChanged;
    public string LastLoadedGame
    {
      get { return _lastLoadedGame; }
      set
      {
        if (_lastLoadedGame != value)
        {
          _lastLoadedGame = value;
          RaisePropertyChanged("LastLoadedGame");

          var handler = OnLastLoadedGameChanged;
          if (handler != null)
            handler(this, null);
        }
      }
    }

    /// <summary>
    /// Gets the progress bar.
    /// </summary>
    /// <value>The progress bar.</value>
    public ProgressBar.ProgressBar ProgressBar
    {
      get { return _progressBar; }
    }
    #endregion

    #region Methods
    public override void ApplyFilter()
    {
      foreach (var node in Games)
        node.ApplyCriteria(FilterText, new Stack<ITreeNodeItem>());
    }

    private void InitializeCommands()
    {
      var commandManager = Container.Resolve<ICommandManager>();
      var ribbonService = Container.Resolve<IRibbonService>();
      var menuService = Container.Resolve<IMenuService>();

      // Ribbon
      var loadGameCommand = new UndoableCommand<object>(LoadGame);
      commandManager.RegisterCommand("LOADGAME", loadGameCommand);

      ribbonService.Add(new RibbonItemViewModel("WORLD EDITOR", 1, "W", true));
      ribbonService.Get("WORLD EDITOR").Add(new RibbonItemViewModel("Game", 1));
      ribbonService.Get("WORLD EDITOR").Get("Game").Add(new MenuItemViewModel("New Game", 1,
                                                            null,
                                                            null, "",
                                                            new KeyGesture(Key.N, ModifierKeys.Control, "Ctrl + N"), false, null,
                                                            "New Game", "Create a new game."));
      ribbonService.Get("WORLD EDITOR").Get("Game").Add(new MenuItemViewModel("Open Game", 1,
                                                            BitmapImages.LoadBitmapFromResourceKey("Open_32x32"),
                                                            commandManager.GetCommand("LOADGAME"), "LOADGAME",
                                                            new KeyGesture(Key.O, ModifierKeys.Control, "Ctrl + O"), false, null,
                                                            "Open Game", "Open an already existing game."));

      var editSettingsCommand = new UndoableCommand<object>(EditSettings);
      commandManager.RegisterCommand("EDITWORLDEDITORSETTINGS", editSettingsCommand);

      ribbonService.Get("WORLD EDITOR").Add(new RibbonItemViewModel("Settings", 3));
      ribbonService.Get("WORLD EDITOR").Get("Settings").Add(new MenuItemViewModel("Edit Settings", 1,
                                                            BitmapImages.LoadBitmapFromResourceKey("Settings_16x16"),
                                                            commandManager.GetCommand("EDITWORLDEDITORSETTINGS"), "EDITWORLDEDITORSETTINGS", null, false, null,
                                                            String.Empty, "Edit tool settings."));

      // Menus
      menuService.Get("_FILE").Add(new MenuItemViewModel("_New Game", 2,
                                    null,
                                    null, "",
                                    new KeyGesture(Key.N, ModifierKeys.Control, "Ctrl + N"), false, null,
                                    "New Game", "Create a new game."));
      menuService.Get("_FILE").Add(new MenuItemViewModel("_Open Game", 3,
                                    BitmapImages.LoadBitmapFromResourceKey("Open_32x32"),
                                    commandManager.GetCommand("LOADGAME"), "LOADGAME",
                                    new KeyGesture(Key.O, ModifierKeys.Control, "Ctrl + O"), false, null,
                                    "Open Game", "Open an already existing game."));
      menuService.Get("_FILE").Add(MenuItemViewModel.Separator(4));
    }

    public void LoadLastLoadedGame()
    {
      var logger = Container.Resolve<ILoggerService>();
      ProgressBar.Progress(true, 0, 2);
      try
      {
        Games.Clear();

        _game = _gameXmlManager.Load("Content/XML/" + LastLoadedGame);
        _game.Load();

        Magnum.Core.ApplicationModel.Application.IsGameLoaded = true;

        ProgressBar.Progress(true, 1, 2);

        InitializeTreeview();
        logger.Log("World Editor: " + LastLoadedGame + " loaded.", LogCategory.Info, LogPriority.None);
      }
      catch (Exception ex)
      {
        logger.Log("World Editor: Error loading Last Loaded Game. Exception message: " + ex.Message, LogCategory.Error, LogPriority.High);
      }
      ProgressBar.Progress(false, 2, 2);
    }

    public void LoadGame(object obj)
    {
      var statusBar = Container.Resolve<IStatusBarService>();
      var logger = Container.Resolve<ILoggerService>();
      IWorkspace workspace = Container.Resolve<WorkspaceBase>();

      ProgressBar.Progress(true, 0, 2);
      workspace.IsBusy = true;

      // Show open file dialog box
      var dialog = new OpenFileDialog();
      dialog.DefaultExt = ".xml"; // Default file extension
      dialog.Filter = "eXtensible Markup Language (.xml)|*.xml"; // Filter files by extension
      statusBar.Text = "Please select a game information file to open with one of the following supported extensions: " + dialog.Filter;
      Nullable<bool> result = dialog.ShowDialog();

      // Process open file dialog box results
      if (result == true)
      {
        ProgressBar.Progress(true, 1, 2);

        try
        {
          Games.Clear();
          _game = _gameXmlManager.Load(dialog.FileName);
          _game.Load();

          Magnum.Core.ApplicationModel.Application.IsGameLoaded = true;
          LastLoadedGame = dialog.SafeFileName;

          InitializeTreeview();
          logger.Log("World Editor: " + dialog.SafeFileName + " loaded.", LogCategory.Info, LogPriority.None);

          ToolViewModel worldEditor = workspace.Tools.First(f => f.ContentId == "WorldEditor");
          if (worldEditor != null)
          {
            worldEditor.FocusOnRibbonOnClick = false;
            worldEditor.IsVisible = true;
            var menuService = Container.Resolve<IMenuService>();
            var mi = menuService.Get("_TOOLS").Get("_World Editor") as MenuItemBase;
            mi.IsChecked = true;
            worldEditor.IsActive = true;
          }
        }
        catch (Exception ex)
        {
          logger.Log("World Editor: The file " + dialog.SafeFileName + " is not supported. Exception message: " + ex.Message, LogCategory.Error, LogPriority.High);
        }
      }

      workspace.IsBusy = false;
      statusBar.Clear();
      ProgressBar.Progress(false, 2, 2);
    }

    private void SaveGameTemp(string destFileName)
    {
      _gameXmlManager.Save(destFileName, _game);
    }

    public void EditSettings(object obj)
    {
      var selectionManager = Container.Resolve<ISelectionManagerService>();
      if (selectionManager != null)
        selectionManager.SelectObject(Container.Resolve<IWorldEditor>());
    }

    public void InitializeTreeview()
    {
      // First node gets a special context menu (for ItemType.GAME types)
      GameItem gameItem = new GameItem(_game.Name, _game.GameID, IItemType.Game, BitmapImages.LoadBitmapFromIconType(IconType.Game), Container);
      gameItem.ContextMenu = _firstNodeContextMenu;

      // Load resources folder and all its resources for said game
      if (_game.LoadedResources.Count > 0)
      {
        // The rest is initialized normally (Resource Item Types, ItemType.WORLD and ItemType.MAP)
        // We use the FolderOpen image at first because we expand the node on loading
        GameItem resourceBaseItem = new GameItem("Resources", -1, IItemType.Resource, null, Container);

        foreach (ResourceBase resource in _game.LoadedResources)
        {
          GameItem resourceItem = new GameItem(resource.Name, resource.ResourceID, IItemType.Tile, BitmapImages.LoadBitmapFromIconType(IconType.Map), Container);
          resourceItem.ContextMenu = CreateTileNodeContextMenu(resourceItem);
          resourceBaseItem.Add(resourceItem);
        }

        gameItem.Add(resourceBaseItem);
      }

      // Load worlds for said game
      if (_game.World.Count > 0)
        foreach (World world in _game.World)
        {
          GameItem worldItem = new GameItem(world.Name, world.WorldID, IItemType.World, BitmapImages.LoadBitmapFromIconType(IconType.World), Container);
          worldItem.ContextMenu = _worldNodeContextMenu;
          if (world.Map.Count > 0)
            foreach (Map map in world.Map)
            {
              GameItem mapItem = new GameItem(map.Name, map.MapID, IItemType.Map, BitmapImages.LoadBitmapFromIconType(IconType.Map), Container);
              mapItem.ContextMenu = CreateMapNodeContextMenu(mapItem);
              worldItem.Add(mapItem);
            }
          gameItem.Add(worldItem);
        }

      Games.Add(gameItem);

      var handler = OnGamesChanged;
      if (handler != null)
        handler(this, null);

      // Assign the Root for the Breadcrumb
      Root = gameItem;
    }

    private void InitializeFirstNodeContextMenu()
    {
      _firstNodeContextMenu = new ContextMenu();

      MenuItem newWorldMenuItem = new MenuItem();
      newWorldMenuItem.Header = "Create New World";
      newWorldMenuItem.Icon = new System.Windows.Controls.Image
      {
        Source = BitmapImages.LoadBitmapFromIconType(IconType.World)
      };
      newWorldMenuItem.Command = new DelegateCommand(CreateNewWorld);

      _firstNodeContextMenu.Items.Add(newWorldMenuItem);
    }

    private void InitializeWorldContextMenu()
    {
      _worldNodeContextMenu = new ContextMenu();

      MenuItem newMapMenuItem = new MenuItem();
      newMapMenuItem.Header = "Create New Map";
      newMapMenuItem.Icon = new System.Windows.Controls.Image
      {
        Source = BitmapImages.LoadBitmapFromIconType(IconType.Map)
      };
      newMapMenuItem.Command = new DelegateCommand(CreateNewMap);

      _worldNodeContextMenu.Items.Add(newMapMenuItem);
    }

    private ContextMenu CreateMapNodeContextMenu(GameItem mapItem)
    {
      ContextMenu mapContextMenu = new ContextMenu();

      MenuItem openMapMenuItem = new MenuItem();
      openMapMenuItem.Header = "Open Map in Viewer";
      openMapMenuItem.Icon = new System.Windows.Controls.Image
      {
        Source = BitmapImages.LoadBitmapFromIconType(IconType.Open)
      };
      openMapMenuItem.Command = new DelegateCommand<GameItem>(OpenMap);
      openMapMenuItem.CommandParameter = mapItem;

      mapContextMenu.Items.Add(openMapMenuItem);

      return mapContextMenu;
    }

    private ContextMenu CreateTileNodeContextMenu(GameItem tileItem)
    {
      ContextMenu tileContextMenu = new ContextMenu();

      MenuItem openTileMenuItem = new MenuItem();
      openTileMenuItem.Header = "Open Tile in Tile Editor";
      openTileMenuItem.Icon = new System.Windows.Controls.Image
      {
        Source = BitmapImages.LoadBitmapFromIconType(IconType.Open)
      };
      openTileMenuItem.Command = new DelegateCommand<GameItem>(OpenTile);
      openTileMenuItem.CommandParameter = tileItem;

      tileContextMenu.Items.Add(openTileMenuItem);

      return tileContextMenu;
    }

    public void CreateNewWorld()
    {
    }

    public void CreateNewMap()
    {

    }

    private void OpenMap(GameItem mapItem)
    {
      if (mapItem != null)
        foreach (World world in _game.World)
          foreach (Map map in world.Map)
            if (map.MapID == mapItem.Id)
            {
              IWorkspace workspace = Container.Resolve<WorkspaceBase>();
              var commandManager = Container.Resolve<ICommandManager>();
              IUndoableCommand loadSelectedMapCommand = commandManager.GetCommand("LOADSELECTEDMAP");
              IUndoableCommand onSwitchingMapCommand = commandManager.GetCommand("VIEWERONSWITCHINGMAP");
              CancelEventArgs canceled = new CancelEventArgs();
              onSwitchingMapCommand.Execute(canceled);
              if (!canceled.Cancel)
              {
                loadSelectedMapCommand.Execute(map);
                ViewerViewModel viewer = workspace.Tools.First(f => f.ContentId == "Viewer") as ViewerViewModel;
                if (viewer != null)
                {
                  viewer.IsVisible = true;
                  viewer.IsActive = true;
                  viewer.IsSelected = true;
                }
              }
            }
    }

    private void OpenTile(GameItem tileItem)
    {
      if (tileItem != null)
        foreach (Tile tile in _game.LoadedResources)
          if (tile.ResourceID == tileItem.Id)
          {
            IWorkspace workspace = Container.Resolve<WorkspaceBase>();
            var commandManager = Container.Resolve<ICommandManager>();
            IUndoableCommand command = commandManager.GetCommand("LOADSELECTEDTILE");
            command.Execute(tile);
            ToolViewModel tileEditor = workspace.Tools.First(f => f.ContentId == "TileEditor");
            if (tileEditor != null)
            {
              tileEditor.IsVisible = true;
              tileEditor.IsActive = true;
              tileEditor.IsSelected = true;
            }
          }
    }
    #endregion

    #region Events

    void WorldEditorModel_OnCurrentlySelectedItemChanged(object sender, EventArgs e)
    {
      var selectionManager = Container.Resolve<ISelectionManagerService>();
      if (selectionManager != null)
        selectionManager.SelectObject(CurrentlySelectedItem);
    }

    #endregion
  }
}
