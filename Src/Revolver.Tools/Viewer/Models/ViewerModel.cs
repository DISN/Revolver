using Engine.System.Entities;
using Engine.Wrappers.Services;
using Magnum.Core.Args;
using Magnum.Core.Models;
using Magnum.Core.Services;
using Magnum.Core.ViewModels;
using Magnum.Core.Windows;
using Magnum.IconLibrary;
using Microsoft.Practices.Unity;
using Microsoft.Win32;
using Microsoft.Xna.Framework;
using Revolver.Tools.Viewer.Commands;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Revolver.Tools.Viewer.Models
{
  public class ViewerModel : ToolModel
  {
    #region Fields
    Views.Viewer _viewer;
    //Views.Player _player;
    bool _isPlaying, _isPaused, _isLoaded, _isDirty;
    string _loadedMapName, _lastLoadedMap;
    int _aspectRatioWidth, _aspectRatioHeight;
    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="ViewerModel"/> class.
    /// </summary>
    public ViewerModel(string displayName, IUnityContainer container, string category, string description, bool isPinned = true, string shortcut = null)
      : base(displayName, container, category, description, isPinned, shortcut)
    {
      Container = container;
      _viewer = new Views.Viewer(container);
      _viewer.OnIsPlayingChanged += _viewer_OnIsPlayingChanged;
      _viewer.OnIsPausedChanged += _viewer_OnIsPausedChanged;
      _viewer.OnIsViewerLoadedChanged += _viewer_OnIsViewerLoadedChanged;
      _viewer.OnMapNameChanged += _viewer_OnMapNameChanged;
      _viewer.OnCurrentLayerNameChanged += _viewer_OnCurrentLayerNameChanged;
      _viewer.OnIsDirtyChanged += _viewer_OnIsDirtyChanged;
      _viewer.OnSelectedObjectChanged += _viewer_OnSelectedObjectChanged;
      _viewer.OnMousePositionChanged += _viewer_OnMousePositionChanged;
      //_player = new Views.Player();
      _isPlaying = _isPaused = _isLoaded = _isDirty = false;

      _loadedMapName = _lastLoadedMap = String.Empty;

      InitializeCommands();
    }
    #endregion

    #region Properties
    public Views.Viewer Viewer
    {
      get { return _viewer; }
      set { _viewer = value; }
    }

    /*public Views.Player Player
    {
      get { return _player; }
      set { _player = value; }
    }*/

    public bool IsPlaying
    {
      get { return _isPlaying; }
      set
      {
        _isPlaying = value;
        RaisePropertyChanged("IsPlaying");
      }
    }

    public bool IsPaused
    {
      get { return _isPaused; }
      set
      {
        _isPaused = value;
        RaisePropertyChanged("IsPaused");
      }
    }

    public bool IsViewerLoaded
    {
      get { return _isLoaded; }
      set
      {
        _isLoaded = value;

        var commandManager = Container.Resolve<ICommandManager>();
        commandManager.Refresh();

        RaisePropertyChanged("IsViewerLoaded");
      }
    }

    public event EventHandler OnLoadedMapNameChanged;
    public string LoadedMapName
    {
      get { return _loadedMapName; }
      set
      {
        _loadedMapName = value;

        var handler = OnLoadedMapNameChanged;
        if (handler != null)
          handler(this, null);
      }
    }

    public event EventHandler OnLastLoadedMapChanged;
    public string LastLoadedMap
    {
      get { return _lastLoadedMap; }
      set
      {
        if (_lastLoadedMap != value)
        {
          _lastLoadedMap = value;
          RaisePropertyChanged("LastLoadedMap");

          var handler = OnLastLoadedMapChanged;
          if (handler != null)
            handler(this, null);
        }
      }
    }

    /// <summary>
    /// Is the map dirty - does it need to be saved?
    /// </summary>
    /// <value><c>true</c> if this instance is dirty; otherwise, <c>false</c>.</value>
    public event EventHandler OnIsDirtyChanged;
    public override bool IsDirty
    {
      get { return _isDirty; }
      set
      {
        if (_isDirty != value)
        {
          _isDirty = value;

          var handler = OnIsDirtyChanged;
          if (handler != null)
            handler(this, null);
        }
      }
    }

    public int AspectRatioWidth
    {
      get { return _aspectRatioWidth; }
      set
      {
        if (_aspectRatioWidth != value)
        {
          _aspectRatioWidth = value;
          RaisePropertyChanged("AspectRatioWidth");
        }
      }
    }

    public int AspectRatioHeight
    {
      get { return _aspectRatioHeight; }
      set
      {
        if (_aspectRatioHeight != value)
        {
          _aspectRatioHeight = value;
        }
      }
    }
    #endregion

    #region Methods
    /// <summary>
    /// Determines whether this instance can be played.
    /// </summary>
    /// <returns><c>true</c> if this instance can be played; otherwise, <c>false</c>.</returns>
    protected virtual bool CanPlayGame(object obj)
    {
      return (IsViewerLoaded && (!IsPlaying || IsPaused)) ? true : false;
    }

    /// <summary>
    /// Determines whether this instance can be stopped.
    /// </summary>
    /// <returns><c>true</c> if this instance can be stopped; otherwise, <c>false</c>.</returns>
    protected virtual bool CanStopGame(object obj)
    {
      return (IsViewerLoaded && (IsPlaying || IsPaused)) ? true : false;
    }

    /// <summary>
    /// Determines whether this instance can be paused.
    /// </summary>
    /// <returns><c>true</c> if this instance can be paused; otherwise, <c>false</c>.</returns>
    protected virtual bool CanPauseGame(object obj)
    {
      return (IsViewerLoaded && IsPlaying && !IsPaused) ? true : false;
    }

    /// <summary>
    /// Determines whether this instance can be saved.
    /// </summary>
    /// <returns><c>true</c> if this instance can be saved; otherwise, <c>false</c>.</returns>
    protected virtual bool CanSave(object obj)
    {
      return IsViewerLoaded ? true : false;
    }

    private void InitializeCommands()
    {
      var commandManager = Container.Resolve<ICommandManager>();
      var ribbonService = Container.Resolve<IRibbonService>();

      // Ribbon
      var showGridCommand = new UndoableCommand<object>(ToggleGrid);
      commandManager.RegisterCommand("SHOWGRID", showGridCommand);

      ribbonService.Add(new RibbonItemViewModel("VIEWER", 2, "V", true));
      ribbonService.Get("VIEWER").Add(new RibbonItemViewModel("Grid", 1));
      ribbonService.Get("VIEWER").Get("Grid").Add(new MenuItemViewModel("Show Grid", 1,
                                                            BitmapImages.LoadBitmapFromResourceKey("Map_16x16"),
                                                            commandManager.GetCommand("SHOWGRID"), "SHOWGRID",
                                                            new KeyGesture(Key.G, ModifierKeys.Control, "Ctrl + G"), false, null,
                                                            "Show Grid", "Shows a grid over the map.") { IsCheckable = true, IsChecked = _viewer.IsGridActivated });

      var toggleFullRatioCommand = new ToggleRatioCommand(AspectRatios._Full, this);
      commandManager.RegisterCommand("TOGGLEFULLRATIO", toggleFullRatioCommand);
      var toggle43RatioCommand = new ToggleRatioCommand(AspectRatios._43, this);
      commandManager.RegisterCommand("TOGGLE43RATIO", toggle43RatioCommand);
      var toggle54RatioCommand = new ToggleRatioCommand(AspectRatios._54, this);
      commandManager.RegisterCommand("TOGGLE54RATIO", toggle54RatioCommand);
      var toggle169RatioCommand = new ToggleRatioCommand(AspectRatios._169, this);
      commandManager.RegisterCommand("TOGGLE169RATIO", toggle169RatioCommand);
      var toggle1610RatioCommand = new ToggleRatioCommand(AspectRatios._1610, this);
      commandManager.RegisterCommand("TOGGLE1610RATIO", toggle1610RatioCommand);

      ribbonService.Get("VIEWER").Add(new RibbonItemViewModel("Viewport", 1));
      ribbonService.Get("VIEWER").Get("Viewport").Add(new MenuItemViewModel("Full", 1,
                                                            BitmapImages.LoadBitmapFromResourceKey("AspectRatio_Full_32x32"),
                                                            commandManager.GetCommand("TOGGLEFULLRATIO"), "TOGGLEFULLRATIO",
                                                            new KeyGesture(Key.NumPad0, ModifierKeys.Control, "Ctrl + NumPad0"), false, null,
                                                            "Full", "Set the graphics device viewport to full ratio."));
      ribbonService.Get("VIEWER").Get("Viewport").Add(new MenuItemViewModel("4:3", 2,
                                                            BitmapImages.LoadBitmapFromResourceKey("AspectRatio_43_32x32"),
                                                            commandManager.GetCommand("TOGGLE43RATIO"), "TOGGLE43RATIO",
                                                            new KeyGesture(Key.NumPad1, ModifierKeys.Control, "Ctrl + NumPad1"), false, null,
                                                            "4:3", "Set the graphics device viewport to 4:3 ratio."));
      ribbonService.Get("VIEWER").Get("Viewport").Add(new MenuItemViewModel("16:9", 3,
                                                            BitmapImages.LoadBitmapFromResourceKey("AspectRatio_54_32x32"),
                                                            commandManager.GetCommand("TOGGLE54RATIO"), "TOGGLE54RATIO",
                                                            new KeyGesture(Key.NumPad2, ModifierKeys.Control, "Ctrl + NumPad2"), false, null,
                                                            "5:4", "Set the graphics device viewport to 5:4 ratio."));
      ribbonService.Get("VIEWER").Get("Viewport").Add(new MenuItemViewModel("16:9", 4,
                                                            BitmapImages.LoadBitmapFromResourceKey("AspectRatio_169_32x32"),
                                                            commandManager.GetCommand("TOGGLE169RATIO"), "TOGGLE169RATIO",
                                                            new KeyGesture(Key.NumPad3, ModifierKeys.Control, "Ctrl + NumPad3"), false, null,
                                                            "16:9", "Set the graphics device viewport to 16:9 ratio."));
      ribbonService.Get("VIEWER").Get("Viewport").Add(new MenuItemViewModel("16:10", 5,
                                                            BitmapImages.LoadBitmapFromResourceKey("AspectRatio_1610_32x32"),
                                                            commandManager.GetCommand("TOGGLE1610RATIO"), "TOGGLE1610RATIO",
                                                            new KeyGesture(Key.NumPad4, ModifierKeys.Control, "Ctrl + NumPad4"), false, null,
                                                            "16:10", "Set the graphics device viewport to 16:10."));

      var toggleSolidCommand = new UndoableCommand<object>(ToggleSolid);
      commandManager.RegisterCommand("TOGGLESOLID", toggleSolidCommand);
      var toggleWireframeCommand = new UndoableCommand<object>(ToggleWireframe);
      commandManager.RegisterCommand("TOGGLEWIREFRAME", toggleWireframeCommand);
      
      ribbonService.Get("VIEWER").Add(new RibbonItemViewModel("Rasterizer State", 2));
      ribbonService.Get("VIEWER").Get("Rasterizer State").Add(new MenuItemViewModel("Fill", 1,
                                                            null,
                                                            commandManager.GetCommand("TOGGLESOLID"), "TOGGLESOLID",
                                                            new KeyGesture(Key.Q, ModifierKeys.Control, "Ctrl + Q"), false, null,
                                                            "Fill", "Set the graphics device rasterization to draw solid faces for each primitive."));
      ribbonService.Get("VIEWER").Get("Rasterizer State").Add(new MenuItemViewModel("Wireframe", 2,
                                                            null,
                                                            commandManager.GetCommand("TOGGLEWIREFRAME"), "TOGGLEWIREFRAME",
                                                            new KeyGesture(Key.W, ModifierKeys.Control, "Ctrl + W"), false, null,
                                                            "Wireframe", "Set the graphics device rasterization to draw the lines connecting the vertices defining each primitive face."));

      var editSettingsCommand = new UndoableCommand<object>(EditSettings);
      commandManager.RegisterCommand("EDITVIEWERSETTINGS", editSettingsCommand);

      ribbonService.Get("VIEWER").Add(new RibbonItemViewModel("Settings", 5));
      ribbonService.Get("VIEWER").Get("Settings").Add(new MenuItemViewModel("Edit Settings", 2,
                                                            BitmapImages.LoadBitmapFromResourceKey("Settings_16x16"),
                                                            commandManager.GetCommand("EDITVIEWERSETTINGS"), "EDITVIEWERSETTINGS", null, false, null,
                                                            String.Empty, "Edit tool settings."));

      var playGameCommand = new UndoableCommand<object>(PlayGame, CanPlayGame);
      commandManager.RegisterCommand("PLAYGAME", playGameCommand);
      var stopGameCommand = new UndoableCommand<object>(StopGame, CanStopGame);
      commandManager.RegisterCommand("STOPGAME", stopGameCommand);
      var pauseGameCommand = new UndoableCommand<object>(PauseGame, CanPauseGame);
      commandManager.RegisterCommand("PAUSEGAME", pauseGameCommand);

      ribbonService.Get("ENGINE").Get("Status").Add(new MenuItemViewModel("Play", 3,
                                                            BitmapImages.LoadBitmapFromResourceKey("Play_32x32"),
                                                            commandManager.GetCommand("PLAYGAME"), "PLAYGAME", null, false, null,
                                                            "Play", "Play the engine"));
      ribbonService.Get("ENGINE").Get("Status").Add(new MenuItemViewModel("Stop", 4,
                                                            BitmapImages.LoadBitmapFromResourceKey("Stop_43x32"),
                                                            commandManager.GetCommand("STOPGAME"), "STOPGAME", null, false, null,
                                                            string.Empty, "Stop the engine"));
      ribbonService.Get("ENGINE").Get("Status").Add(new MenuItemViewModel("Pause", 5,
                                                            BitmapImages.LoadBitmapFromResourceKey("Pause_32x32"),
                                                            commandManager.GetCommand("PAUSEGAME"), "PAUSEGAME", null, false, null,
                                                            string.Empty, "Pause the engine"));

      var loadMapCommand = new UndoableCommand<object>(LoadMap);
      commandManager.RegisterCommand("LOADMAP", loadMapCommand);
      var loadSelectedMapCommand = new UndoableCommand<Map>(LoadSelectedMap);
      commandManager.RegisterCommand("LOADSELECTEDMAP", loadSelectedMapCommand);
      var saveMapCommand = new UndoableCommand<object>(SaveMap, CanSave);
      commandManager.RegisterCommand("SAVEMAP", saveMapCommand);
      var saveMapAsCommand = new UndoableCommand<object>(SaveMapAs, CanSave);
      commandManager.RegisterCommand("SAVEMAPAS", saveMapAsCommand);

      /*ribbonService.Get("WORLD EDITOR").Add(new RibbonItemViewModel("Map", 2));
      ribbonService.Get("WORLD EDITOR").Get("Map").Add(new MenuItemViewModel("Load a Map", 1,
                                                            BitmapImages.LoadBitmapFromResourceKey("Game_32x32"),
                                                            commandManager.GetCommand("LOADMAP"), "LOADMAP", null, false, null,
                                                            "Load a Map", "Load a Map"));
      ribbonService.Get("WORLD EDITOR").Get("Map").Add(new MenuItemViewModel("Save Current Map", 2,
                                                            BitmapImages.LoadBitmapFromResourceKey("Game_32x32"),
                                                            commandManager.GetCommand("SAVEMAP"), "SAVEMAP", null, false, null,
                                                            "Save Current Map", "Saves the current changes made to the currently loaded map in the Viewer."));
      ribbonService.Get("WORLD EDITOR").Get("Map").Add(new MenuItemViewModel("Save Map As", 3,
                                                            BitmapImages.LoadBitmapFromResourceKey("Game_32x32"),
                                                            commandManager.GetCommand("SAVEMAPAS"), "SAVEMAPAS", null, false, null,
                                                            "Save Map As", "Saves the current changes made to a *.xml file."));*/

      var viewerOnSwitchingMapCommand = new UndoableCommand<object>(OnSwitchingMap);
      commandManager.RegisterCommand("VIEWERONSWITCHINGMAP", viewerOnSwitchingMapCommand);
    }

    private void ToggleGrid(object obj)
    {
      _viewer.IsGridActivated = !_viewer.IsGridActivated;
    }

    public void ToggleRatioInit()
    {
      RatioChanged(null);
    }

    public void RatioChanged(object obj)
    {
      if (LastRatio == AspectRatios._Full)
        ToggleFullRatio(AspectRatios._Full);
      else if (LastRatio == AspectRatios._43)
        Toggle43Ratio(AspectRatios._43);
      else if (LastRatio == AspectRatios._54)
        Toggle54Ratio(AspectRatios._54);
      else if (LastRatio == AspectRatios._169)
        Toggle169Ratio(AspectRatios._169);
      else if (LastRatio == AspectRatios._1610)
        Toggle1610Ratio(AspectRatios._1610);
    }

    public void SetupViewport()
    {
      bool invertScale = false;
      var targetAspectRatio = (int)AspectRatioWidth / (float)AspectRatioHeight;
      // figure out the largest area that fits in this resolution at the desired aspect ratio
      var width = (int)ViewerControlActualWidth;
      var height = (int)((int)ViewerControlActualWidth / targetAspectRatio + .5f);

      if (height > (int)ViewerControlActualHeight)
      {
        invertScale = true;
        // PillarBox
        width = (int)((int)ViewerControlActualHeight * targetAspectRatio + .5f);
      }

      // set up the new viewport centered in the backbuffer
      int x;
      int y;
      if (invertScale)
      {
        x = (int)((int)((int)ViewerControlActualWidth / 2) - (width / 2));
        y = 0;

        Viewer.ImageWidth = (int)ViewerControlActualWidth - x;
        height = (int)((int)(ViewerControlActualWidth - x) / targetAspectRatio + .5f);
        Viewer.ImageHeight = (int)ViewerControlActualHeight + (int)((int)((int)ViewerControlActualHeight / 2) - (height / 2));

        // set up the new viewport centered in the backbuffer
        LetterBoxWidth = x;
        LetterBoxHeight = y;
        RaisePropertyChanged("LetterBoxWidth");
        RaisePropertyChanged("LetterBoxHeight");
      }
      else
      {
        x = 0;
        y = (int)((int)((int)ViewerControlActualHeight / 2) - (height / 2));

        Viewer.ImageWidth = (int)width;
        Viewer.ImageHeight = (int)height;

        // set up the new viewport centered in the backbuffer
        LetterBoxWidth = x;
        LetterBoxHeight = y;
        RaisePropertyChanged("LetterBoxWidth");
        RaisePropertyChanged("LetterBoxHeight");
      }
      /*Viewer.GraphicsDevice.Viewport = new Viewport
      {
        X = x,
        Y = y,
        Width = (int)width,
        Height = height
      };*/

      Viewer.ResetBackBuffer = true;
    }

    public int LetterBoxHeight { get; set; }
    public int LetterBoxWidth { get; set; }
    public AspectRatios PreviousRatio { get; set; }
    public AspectRatios LastRatio { get; set; }

    public enum AspectRatios
    {
      _Full,
      _43,
      _54,
      _169,
      _1610
    }

    private void ToggleFullRatio(object obj)
    {
      PreviousRatio = LastRatio;
      LastRatio = AspectRatios._Full;
      AspectRatioWidth = (int)ViewerControlActualWidth;
      AspectRatioHeight = (int)ViewerControlActualHeight;
      float aspectRatio = AspectRatioWidth > AspectRatioHeight ? (float)(AspectRatioWidth / AspectRatioHeight) : (float)(AspectRatioHeight / AspectRatioWidth);
      Viewer.GameManager.ToggleRatio(aspectRatio, (float)AspectRatioWidth > AspectRatioHeight ? AspectRatioWidth : AspectRatioHeight, (float)AspectRatioHeight < AspectRatioWidth ? AspectRatioHeight : AspectRatioWidth);

      SetupViewport();
    }

    private void Toggle43Ratio(object obj)
    {
      PreviousRatio = LastRatio;
      LastRatio = AspectRatios._43;
      AspectRatioWidth = 1920;
      AspectRatioHeight = 1440;
      Viewer.GameManager.ToggleRatio((float)(AspectRatioWidth / AspectRatioHeight), (float)AspectRatioWidth, (float)AspectRatioHeight);

      SetupViewport();
    }

    private void Toggle54Ratio(object obj)
    {
      PreviousRatio = LastRatio;
      LastRatio = AspectRatios._54;
      AspectRatioWidth = 1280;
      AspectRatioHeight = 1024;
      Viewer.GameManager.ToggleRatio((float)(AspectRatioWidth / AspectRatioHeight), (float)AspectRatioWidth, (float)AspectRatioHeight);

      SetupViewport();
    }

    private void Toggle169Ratio(object obj)
    {
      PreviousRatio = LastRatio;
      LastRatio = AspectRatios._169;
      AspectRatioWidth = 1920;
      AspectRatioHeight = 1080;
      Viewer.GameManager.ToggleRatio((float)(AspectRatioWidth / AspectRatioHeight), (float)AspectRatioWidth, (float)AspectRatioHeight);

      SetupViewport();
    }

    private void Toggle1610Ratio(object obj)
    {
      PreviousRatio = LastRatio;
      LastRatio = AspectRatios._1610;
      AspectRatioWidth = 1920;
      AspectRatioHeight = 1200;
      Viewer.GameManager.ToggleRatio((float)(AspectRatioWidth / AspectRatioHeight), (float)AspectRatioWidth, (float)AspectRatioHeight);

      SetupViewport();
    }

    private void ToggleSolid(object obj)
    {
      Viewer.GameManager.ToggleSolid();
    }

    private void ToggleWireframe(object obj)
    {
      Viewer.GameManager.ToggleWireframe();
    }

    public void EditSettings(object obj)
    {
      var selectionManager = Container.Resolve<ISelectionManagerService>();
      if (selectionManager != null)
        selectionManager.SelectObject(Container.Resolve<IViewer>());
    }

    public void OnSwitchingMap(object info)
    {
      IWorkspace workspace = Container.Resolve<WorkspaceBase>();
      var saveManager = Container.Resolve<ISaveManagerService>();

      if (IsDirty)
      {
        CancelEventArgs e = info as CancelEventArgs;

        //means the map is dirty - show a message box and then handle based on the user's selection
        workspace.IsMessageBoxShowing = true;
        Magnum.Controls.MessageBox messageBox = new Magnum.Controls.MessageBox();
        messageBox.Owner = Application.Current.MainWindow;
        var res = messageBox.Show("Proceed to save?",
          "You did not save the currently modified map",
          MessageBoxButton.YesNoCancel);
        workspace.IsMessageBoxShowing = false;

        //Pressed Yes
        if (res == MessageBoxResult.Yes)
        {
          // Save map to folder
          string filePath = "Content/XML";
          if (!Directory.Exists(filePath))
            Directory.CreateDirectory(filePath);
          SaveMapTemp(Path.Combine(filePath, Viewer.MapName) + ".xml");
        }

        //Pressed Cancel
        if (res == MessageBoxResult.Cancel)
        {
          //Cancel was pressed - so, we cant close
          if (e != null)
          {
            e.Cancel = true;
          }
          return;
        }

        // TODO: Change this. It is just for the presentation, but usually we should go through all of the ObjectsToSave's objets' Save() methods
        saveManager.Clear();
      }
      IsDirty = false;
    }

    public WindowClosingEventArgs OnClosing(object info)
    {
      IWorkspace workspace = Container.Resolve<WorkspaceBase>();
      var statusBar = Container.Resolve<IStatusBarService>();
      var saveManager = Container.Resolve<ISaveManagerService>();
      WindowClosingEventArgs e = info as WindowClosingEventArgs;

      if (IsDirty)
      {
        //means the map is dirty - show a message box and then handle based on the user's selection
        workspace.IsMessageBoxShowing = true;
        Magnum.Controls.MessageBox messageBox = new Magnum.Controls.MessageBox();
        messageBox.Owner = Application.Current.MainWindow;
        var res = messageBox.Show("Proceed to save (this will also close the Viewer)?",
          "You did not save the currently modified map",
          MessageBoxButton.YesNoCancel);
        workspace.IsMessageBoxShowing = false;

        //Pressed Yes
        if (res == MessageBoxResult.Yes)
        {
          // Save map to folder
          string filePath = "Content/XML";
          if (!Directory.Exists(filePath))
            Directory.CreateDirectory(filePath);
          SaveMapTemp(Path.Combine(filePath, Viewer.MapName) + ".xml");

          // Remove a temp map, if it exists
          string tempFilePath = Path.Combine("Content/XML/Temp", Viewer.MapName);
          if (File.Exists(tempFilePath))
            File.Delete(tempFilePath);
        }

        //Pressed Cancel
        if (res == MessageBoxResult.Cancel)
        {
          //Cancel was pressed - so, we cant close
          if (e != null)
            e.Cancel = true;
          return e;
        }

        // TODO: Change this. It is just for the presentation, but usually we should go through all of the ObjectsToSave's objets' Save() methods
        saveManager.Clear();
        IsDirty = false;
      }

      if (e == null) // This means we closed the Viewer via the X button and not by closing the MainWindow
      {
        e = new WindowClosingEventArgs(true);

        if (!CanPlayGame(null))
        {
          ToolViewModel viewer = workspace.Tools.First(f => f.ContentId == "Viewer");
          if (viewer != null)
          {
            viewer.IsVisible = false;
            //var menuService = Container.Resolve<IMenuService>();
            //var mi = menuService.Get("_TOOLS").Get("_Viewer") as MenuItemBase;
            //mi.IsChecked = false;
            viewer.IsActive = false;
          }
        }
        else
          _viewer.Close();

        IsViewerLoaded = false;
      }
      statusBar.CurrentLayerName = String.Empty;
      return e;
    }

    public void PlayGame(object obj)
    {
      var statusBar = Container.Resolve<IStatusBarService>();
      var logger = Container.Resolve<ILoggerService>();
      IWorkspace workspace = Container.Resolve<WorkspaceBase>();
      if (IsDirty)
      {
        // Save map to temporary folder
        string filePath = "Content/XML/Temp";
        if (!Directory.Exists(filePath))
          Directory.CreateDirectory(filePath);
        SaveMapTemp(Path.Combine(filePath, Viewer.MapName) + ".xml");

        Viewer.Play();
      }
      else
        Viewer.Play();

      ToolViewModel viewer = workspace.Tools.First(f => f.ContentId == "Viewer");
      if (viewer != null)
      {
        viewer.IsVisible = true;
        //var menuService = Container.Resolve<IMenuService>();
        //var mi = menuService.Get("_TOOLS").Get("_Viewer") as MenuItemBase;
        //mi.IsChecked = true;
        viewer.IsActive = true;
      }

      statusBar.EngineState = EngineStates.Playing;
      var commandManager = Container.Resolve<ICommandManager>();
      commandManager.Refresh();
      logger.Log("Viewer: Engine Running", LogCategory.Info, LogPriority.None);
    }

    public void StopGame(object obj)
    {
      var statusBar = Container.Resolve<IStatusBarService>();
      var logger = Container.Resolve<ILoggerService>();
      IWorkspace workspace = Container.Resolve<WorkspaceBase>();

      statusBar.EngineState = EngineStates.Stopped;
      Viewer.Stop();

      var commandManager = Container.Resolve<ICommandManager>();
      commandManager.Refresh();
      logger.Log("Viewer: Engine Stopped", LogCategory.Info, LogPriority.None);
    }

    public void PauseGame(object obj)
    {
      var statusBar = Container.Resolve<IStatusBarService>();
      var logger = Container.Resolve<ILoggerService>();
      IWorkspace workspace = Container.Resolve<WorkspaceBase>();

      statusBar.EngineState = EngineStates.Paused;
      Viewer.Pause();

      var commandManager = Container.Resolve<ICommandManager>();
      commandManager.Refresh();
      logger.Log("Viewer: Engine Paused", LogCategory.Info, LogPriority.None);
    }

    public void LoadMap(object obj)
    {
      var statusBar = Container.Resolve<IStatusBarService>();
      var logger = Container.Resolve<ILoggerService>();
      var progressBar = Container.Resolve<IProgressBarService>();
      IWorkspace workspace = Container.Resolve<WorkspaceBase>();

      progressBar.Progress(true, 0, 2);
      workspace.IsBusy = true;

      // Show open file dialog box
      var dialog = new OpenFileDialog();
      dialog.DefaultExt = ".xml"; // Default file extension
      dialog.Filter = "eXtensible Markup Language (.xml)|*.xml"; // Filter files by extension
      statusBar.Text = "Please select a map information file to open with one of the following supported extensions: " + dialog.Filter;
      Nullable<bool> result = dialog.ShowDialog();

      // Process open file dialog box results
      if (result == true)
      {
        progressBar.Progress(true, 1, 2);

        try
        {
          Viewer.Load(dialog.FileName);
          LastLoadedMap = dialog.FileName;
          var commandManager = Container.Resolve<ICommandManager>();
          commandManager.Refresh();
          logger.Log("Viewer: " + dialog.SafeFileName + " loaded.", LogCategory.Info, LogPriority.None);

          ToolViewModel viewer = workspace.Tools.First(f => f.ContentId == "Viewer");
          if (viewer != null)
          {
            viewer.FocusOnRibbonOnClick = false;
            viewer.IsVisible = true;
            //var menuService = Container.Resolve<IMenuService>();
            //var mi = menuService.Get("_TOOLS").Get("_Viewer") as MenuItemBase;
            //mi.IsChecked = true;
            viewer.IsActive = true;
          }
        }
        catch (Exception ex)
        {
          logger.Log("Viewer: The file " + dialog.SafeFileName + " is not supported. Exception message: " + ex.Message, LogCategory.Error, LogPriority.High);
        }
      }

      workspace.IsBusy = false;
      statusBar.Clear();
      progressBar.Progress(false, 2, 2);
    }

    public void LoadLastLoadedMap()
    {
      var logger = Container.Resolve<ILoggerService>();
      var progressBar = Container.Resolve<IProgressBarService>();
      IWorkspace workspace = Container.Resolve<WorkspaceBase>();
      progressBar.Progress(true, 1, 2);

      try
      {
        Viewer.Load(LastLoadedMap);
        var commandManager = Container.Resolve<ICommandManager>();
        commandManager.Refresh();
        logger.Log("Viewer: " + LastLoadedMap + " loaded.", LogCategory.Info, LogPriority.None);
      }
      catch (Exception ex)
      {
        logger.Log("Viewer: Error loading Last Loaded Map. Exception message: " + ex.Message, LogCategory.Error, LogPriority.High);
      }

      progressBar.Progress(false, 2, 2);
    }

    private void LoadSelectedMap(Map map)
    {
      var logger = Container.Resolve<ILoggerService>();
      var progressBar = Container.Resolve<IProgressBarService>();
      IWorkspace workspace = Container.Resolve<WorkspaceBase>();
      if (map != null)
      {
        progressBar.Progress(true, 1, 2);

        try
        {
          Viewer.Load(map);
          LastLoadedMap = map.LinkID;
          var commandManager = Container.Resolve<ICommandManager>();
          commandManager.Refresh();
          logger.Log("Viewer: " + map.Name + " loaded.", LogCategory.Info, LogPriority.None);

          ToolViewModel viewer = workspace.Tools.First(f => f.ContentId == "Viewer");
          if (viewer != null)
          {
            viewer.FocusOnRibbonOnClick = false;
            viewer.IsVisible = true;
            //var menuService = Container.Resolve<IMenuService>();
            //var mi = menuService.Get("_TOOLS").Get("_Viewer") as MenuItemBase;
            //mi.IsChecked = true;
            viewer.IsActive = true;
          }
        }
        catch (Exception ex)
        {
          logger.Log("Viewer: Error loading " + map.Name + ". Exception message: " + ex.Message, LogCategory.Error, LogPriority.High);
        }
      }
      progressBar.Progress(false, 2, 2);
    }

    public void SaveMap(object obj)
    {
      var saveManager = Container.Resolve<ISaveManagerService>();

      Viewer.Save();
      IsDirty = false;

      saveManager.Remove(Viewer.Map);
    }

    public void SaveMapTemp(string filePath)
    {
      Viewer.SaveAs(filePath);
    }

    public void SaveMapAs(object obj)
    {
      var statusBar = Container.Resolve<IStatusBarService>();
      var logger = Container.Resolve<ILoggerService>();
      var progressBar = Container.Resolve<IProgressBarService>();
      progressBar.Progress(true, 1, 2);
      IWorkspace workspace = Container.Resolve<WorkspaceBase>();
      workspace.IsBusy = true;

      SaveFileDialog dlg = new SaveFileDialog();
      if (!String.IsNullOrEmpty(_viewer.MapName))
        dlg.FileName = _viewer.MapName;
      else
        dlg.FileName = "Map";  // Default file name
      dlg.DefaultExt = ".xml"; // Default file extension
      dlg.Filter = "eXtensible Markup Langage (.xml)|*.xml"; // Filter files by extension

      // Show save file dialog box
      statusBar.Text = "Please select a location to save the map as";
      Nullable<bool> result = dlg.ShowDialog();

      if (result == true)
      {
        Viewer.SaveAs(dlg.FileName);
        logger.Log("Viewer: " + dlg.SafeFileName + " saved.", LogCategory.Info, LogPriority.None);
      }

      statusBar.Clear();
      progressBar.Progress(false, 2, 2);
      workspace.IsBusy = false;
    }
    #endregion

    #region Events
    void _viewer_OnIsPlayingChanged(object sender, EventArgs e)
    {
      IsPlaying = !IsPlaying;
    }

    void _viewer_OnIsPausedChanged(object sender, EventArgs e)
    {
      IsPaused = !IsPaused;
    }

    void _viewer_OnIsViewerLoadedChanged(object sender, EventArgs e)
    {
      IsViewerLoaded = _viewer.IsViewerLoaded;
      //AspectRatioWidth = _viewer.MapWidth > 0 ? _viewer.MapWidth : 1000;
      //double newHeight = (1440.0 / 1920.0) * _viewer.ImageWidth;
      //AspectRatioHeight = (int)newHeight;
    }

    void _viewer_OnMapNameChanged(object sender, EventArgs e)
    {
      LoadedMapName = _viewer.MapName;
    }

    void _viewer_OnCurrentLayerNameChanged(object sender, EventArgs e)
    {
      var statusBar = Container.Resolve<IStatusBarService>();
      statusBar.CurrentLayerName = "Current layer: " + _viewer.CurrentLayerName;
    }

    void _viewer_OnIsDirtyChanged(object sender, EventArgs e)
    {
      IsDirty = _viewer.IsDirty;
    }

    void _viewer_OnSelectedObjectChanged(object sender, EventArgs e)
    {
      IWorkspace workspace = Container.Resolve<WorkspaceBase>();
      if (_viewer.SelectedObject.Name != String.Empty)
      {
        var statusBar = Container.Resolve<IStatusBarService>();
        statusBar.Text = _viewer.SelectedObject.Name + " selected";
      }

      var selectionManager = Container.Resolve<ISelectionManagerService>();
      if (selectionManager != null)
        selectionManager.SelectObject(_viewer.SelectedObject);
    }

    void _viewer_OnMousePositionChanged(object sender, EventArgs e)
    {
      var statusBar = Container.Resolve<IStatusBarService>();
      if (_viewer.MousePosition != -Vector2.One)
        statusBar.MousePosition = "X: " + _viewer.MousePosition.X + ", Y: " + _viewer.MousePosition.Y + "";
      else
        statusBar.MousePosition = String.Empty;
    }
    #endregion

    public double ViewerControlActualWidth { get; set; }

    public double ViewerControlActualHeight { get; set; }
  }
}
