using Engine.Main;
using Engine.System.Entities;
using Engine.Wrappers.Hosting;
using Engine.Wrappers.Input;
using Engine.Wrappers.SelectionTool;
using Engine.Wrappers.Services;
using Magnum.Core.Managers;
using Magnum.Core.Services;
using Magnum.Core.ViewModels;
using Magnum.Core.Windows;
using Microsoft.Practices.Unity;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;
using System.Windows.Input;

namespace Revolver.Tools.Viewer.Views
{
  public class Viewer : D3D11Host
  {
    #region Fields
    IUnityContainer _container;
    GameManager _gameManager;
    TimeSpan _lastTime;
    bool _isLoaded, _isPlaying, _isPaused, _isDirty, _isFocused, _isGridActivated;
    string _mapName, _layerName;

    SpriteBatch _spriteBatch;
    Map _map;
    ResourceBase _selectedObject;
    public int _layerNumber;
    public int _lastAddedLayerNumber;

    public bool _isMouseDown, _isMouseOver;
    public Vector2 _mousePosition;

    XmlManager<Map> _mapXmlManager;

    // the input helpers that replace the XNA/MonoGame classes (Keyboard and Mouse).
    private readonly NativeKeyboardInput _keyboardInputHandler;
    private readonly WPFMouseInput _mouseInputHandler;
    private bool _focused;
    private KeyboardState _previous;
    #endregion

    #region Constructors
    public Viewer(IUnityContainer container)
    {
      _container = container;
      _map = new Map();
      _layerNumber = 0;
      _isLoaded = _isPlaying = _isMouseDown = _isMouseOver = _isFocused = _focused = _isGridActivated = false;
      _mapXmlManager = new XmlManager<Map>();
      _mapName = _layerName = String.Empty;

      _gameManager = new GameManager();
      _gameManager.Load();
      FireOnLoaded(_gameManager.GraphicsDevice);
      IsViewerLoaded = true;

      // keyboard input simply just works once you pass in an IInputElement.
      _keyboardInputHandler = new NativeKeyboardInput(this);
      this.Focusable = true; // needed so we can receive keyboard inputs ("this" is set in a ContentControl).
      // mouse input requires hooking into the WPF events and calling them manually (see "Mouse Event Handlers" region at bottom of this file)
      _mouseInputHandler = new WPFMouseInput(this);
    }
    #endregion

    #region Properties
    public Map Map
    {
      get { return _map; }
      set { _map = value; }
    }

    public event EventHandler OnIsPlayingChanged;

    public bool IsPlaying
    {
      get { return _isPlaying; }
      set
      {
        if (_isPlaying != value)
        {
          _isPlaying = value;

          var handler = OnIsPlayingChanged;
          if (handler != null)
            handler(this, null);
        }
      }
    }

    public event EventHandler OnIsPausedChanged;

    public bool IsPaused
    {
      get { return _isPaused; }
      set
      {
        if (_isPaused != value)
        {
          _isPaused = value;

          var handler = OnIsPausedChanged;
          if (handler != null)
            handler(this, null);
        }
      }
    }

    public Layer CurrentLayer
    {
      get
      {
        if (Map.Layer.Count > 0)
          return Map.Layer[_layerNumber];
        else
          return null;
      }
    }

    public Vector2 CurrentTileDimensions
    {
      get { return Map.TileDimensions; }
    }

    public int MapWidth
    {
      get { return Map.MapWidth; }
    }

    public int MapHeight
    {
      get { return Map.MapHeight; }
    }

    public event EventHandler OnSelectedObjectChanged;
    public ResourceBase SelectedObject
    {
      get { return _selectedObject; }
      set
      {
        _selectedObject = value;

        var handler = OnSelectedObjectChanged;
        if (handler != null)
          handler(this, null);
      }
    }

    public event EventHandler OnIsDirtyChanged;
    public bool IsDirty
    {
      get { return _isDirty; }
      set
      {
        _isDirty = value;

        var handler = OnIsDirtyChanged;
        if (handler != null)
          handler(this, null);
      }
    }

    public bool IsViewerFocused
    {
      get { return _isFocused; }
      set
      {
        if (_isFocused != value)
          _isFocused = value;
      }
    }

    public event EventHandler OnIsViewerLoadedChanged;

    public bool IsViewerLoaded
    {
      get { return _isLoaded; }
      set
      {
        _isLoaded = value;

        var handler = OnIsViewerLoadedChanged;
        if (handler != null)
          handler(this, null);
      }
    }

    public event EventHandler OnMapNameChanged;
    public string MapName
    {
      get { return _mapName; }
      set
      {
        _mapName = value;

        var handler = OnMapNameChanged;
        if (handler != null)
          handler(this, null);
      }
    }

    public event EventHandler OnCurrentLayerNameChanged;
    public string CurrentLayerName
    {
      get { return _layerName; }
      set
      {
        _layerName = value;

        var handler = OnCurrentLayerNameChanged;
        if (handler != null)
          handler(this, null);
      }
    }

    public event EventHandler OnMousePositionChanged;
    public Vector2 MousePosition
    {
      get { return _mousePosition; }
      set
      {
        _mousePosition = value;

        var handler = OnMousePositionChanged;
        if (handler != null)
          handler(this, null);
      }
    }

    public bool IsGridActivated
    {
      get { return _isGridActivated; }
      set
      {
        _isGridActivated = value;

        var ribbonService = _container.Resolve<IRibbonService>();
        MenuItemViewModel mvm = ribbonService.Get("VIEWER").Get("Grid").Get("Show Grid") as MenuItemViewModel;

        if (mvm != null)
          mvm.IsChecked = _isGridActivated;
      }
    }

    public GameManager GameManager
    {
      get { return _gameManager; }
      set { _gameManager = value; }
    }
    #endregion

    #region Methods
    public override void Load()
    {
      /*_spriteBatch = new SpriteBatch(GraphicsDevice);
      Selector.Instance.Initialize(GraphicsDevice);

      XmlSerializer xml = new XmlSerializer(_map.GetType());
      using (Stream stream = File.Open("Content/XML/Map1.xml", FileMode.Open))
        _map = (Map)xml.Deserialize(stream);
      _map.Initialize(GraphicsDevice);

      Selector.Instance.CurrentLayer = CurrentLayer;

      if (!_isLoaded)
      {
        _gameManager = new GameManager(GraphicsDevice);
        _gameManager.Load();
        _isLoaded = true;
      }*/
    }

    public void Load(string filePath)
    {
      if (!IsViewerLoaded)
      {
        GameManager = new GameManager();
        GameManager.Load();
        FireOnLoaded(GameManager.GraphicsDevice);
      }
      else
        GameManager.Reload();

      _spriteBatch = new SpriteBatch(GraphicsDevice);

      Map = _mapXmlManager.Load(filePath);
      Map.OnIsDirtyChanged += _map_OnIsDirtyChanged;
      Map.Initialize(GraphicsDevice);
      MapName = Map.Name;
      _lastAddedLayerNumber = Map.Layer.Count - 1; // we start at 0

      if (_layerNumber > Map.Layer.Count - 1)
        _layerNumber = 0;

      /*//Selector.Instance.CurrentTileDimensions = CurrentTileDimensions;
      Selector.Instance.CurrentLayer = CurrentLayer;
      CurrentLayerName = CurrentLayer.Name;
      CurrentLayer.OnIsDirtyChanged += CurrentLayer_OnIsDirtyChanged;
      Selector.Instance.Initialize(GraphicsDevice);

      // D3D11Host stuff
      if (Map.MapWidth > 0)
        ImageWidth = Map.MapWidth;
      else
        ImageWidth = 1000;
      if (Map.MapHeight > 0)
        ImageHeight = Map.MapHeight;
      else
        ImageHeight = 1000;*/

      IsViewerLoaded = true;
    }

    public void Load(Map map)
    {
      if (!IsViewerLoaded)
      {
        GameManager = new GameManager();
        GameManager.Load();
        FireOnLoaded(GameManager.GraphicsDevice);
      }
      else
        GameManager.Reload();

      _spriteBatch = new SpriteBatch(GraphicsDevice);

      Map = _mapXmlManager.Load("Content/XML/" + map.Name + ".xml");
      Map.OnIsDirtyChanged += _map_OnIsDirtyChanged;
      Map.Initialize(GraphicsDevice);
      MapName = Map.Name;
      _lastAddedLayerNumber = Map.Layer.Count - 1; // we start at 0

      if (_layerNumber > Map.Layer.Count - 1)
        _layerNumber = 0;

      /*//Selector.Instance.CurrentTileDimensions = CurrentTileDimensions;
      Selector.Instance.CurrentLayer = CurrentLayer;
      CurrentLayerName = CurrentLayer.Name;
      Selector.Instance.Initialize(GraphicsDevice);

      //// D3D11Host stuff
      if (Map.MapWidth > 0)
        ImageWidth = Map.MapWidth;
      else
        ImageWidth = 1000;
      if (Map.MapHeight > 0)
        ImageHeight = Map.MapHeight;
      else
        ImageHeight = 1000;*/

      IsViewerLoaded = true;
    }

    public override void Unload()
    {
      if (IsViewerLoaded)
      {
        GameManager.Unload();
        IsViewerLoaded = false;
      }
    }

    public void Close()
    {
      GameManager.Close();
    }

    public void Save()
    {
      Map.Save();
    }

    public void SaveAs(string filePath)
    {
      Map.SaveAs(filePath);
    }

    public override void Draw(TimeSpan dt)
    {
      if (IsViewerLoaded)
      {
        //System.Drawing.Color color = Brushes.ConvertBrushToDrawingColor(Brushes.LoadBrushFromResourceKey("MainWindowInnerBackgroundBrush"));
        //Microsoft.Xna.Framework.Color xnaColor = new Microsoft.Xna.Framework.Color(color.R, color.G, color.B, color.A);

        GraphicsDevice.Clear(Color.White);

        //_spriteBatch.Begin();
        ////Map.Draw(_spriteBatch);
        //if (IsGridActivated)
          //Map.DrawGrid(_spriteBatch);
        //if (IsPlaying)
          GameManager.Draw(dt, dt - _lastTime);
        //if (_isMouseOver && Selector.Instance.CurrentTile != null)
          //foreach (Image image in Selector.Instance.Images)
            //image.Draw(_spriteBatch);
        //if (Selector.Instance.SelectedMapTileRegion != Rectangle.Empty)
          //foreach (Image selectedImage in Selector.Instance.SelectedImages)
            //selectedImage.Draw(_spriteBatch);
        //_spriteBatch.End();
      }
    }


    public override void Update(TimeSpan dt)
    {
      if (IsViewerLoaded)
      {
        if (IsPlaying && !IsPaused)
        {
          if (IsViewerFocused)
            Focus();
          _focused = true;
          KeyboardState keys = _keyboardInputHandler.GetState();
          GameManager.Update(dt, dt - _lastTime, keys);
          _previous = keys;
        }
        else
          _focused = false;

        _lastTime = dt;
      }
    }

    private bool AnyMouseEventTriggered()
    {
      MouseState m = _mouseInputHandler.MouseState;
      return m.LeftButton == ButtonState.Pressed ||
             m.MiddleButton == ButtonState.Pressed ||
             m.RightButton == ButtonState.Pressed;
    }

    public void Play()
    {
      IsPlaying = true;
      if (!IsPaused)
        GameManager.Reload();
      else
        IsPaused = false;
    }

    public void Stop()
    {
      IsPlaying = false;
      IsPaused = false;
      GameManager.Reload();
    }

    public void Pause()
    {
      IsPaused = true;
    }
    #endregion

    #region Events
    // we can easily handle the keyboard input via native methods
    // but mouse input has proven to be very tricky to handle
    // evaluating 5+ samples, I concluded this the easiest (and so far best working!) solution for WPF
    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
      if (IsViewerLoaded && Selector.Instance.CurrentTile != null)
      {
        _mouseInputHandler.HandleMouseButtons(e);

        if (e.LeftButton == MouseButtonState.Pressed)
        {
          for (int i = 0; i < Engine.System.Managers.ModelManager.Instance.Models.Count; i++)
          {
            if (Map.Layer[i].Image.Path == Selector.Instance.CurrentTile.Image.Path)
            {
              _layerNumber = Map.Layer[i].LayerNumber;
              break;
            }
          }
          if (_layerNumber == -1)
            _layerNumber = 0;
          if (CurrentLayer.Image.Path != Selector.Instance.CurrentTile.Image.Path)
          {
            // Il faut créer un nouveau layer qui va contenir l'image du tile sélectionné
            Layer newLayer = new Layer();
            newLayer.Image = Selector.Instance.CurrentTile.Image;
            newLayer.TileDimensions = Selector.Instance.CurrentTile.TileDimensions;
            newLayer.Tile = new Engine.System.Entities.Layer.TileMap();

            // This needs to be fixed. The engine doesn't support empty TileMaps for now...
            newLayer.Tile.Row.Add("[x:x]");

            newLayer.LayerNumber = _lastAddedLayerNumber + 1;
            newLayer.Initialize(GraphicsDevice, Selector.Instance.CurrentTile.TileDimensions);
            Map.Layer.Add(newLayer);
            _lastAddedLayerNumber++;
          }

          // Ici on change/ajoute le nouveau "tile" sélectionné (via le TileEditor par exemple)
          CurrentLayerName = CurrentLayer.Name;
          CurrentLayer.ReplaceTile(_mousePosition, Selector.Instance.SelectedTileRegion);
          _isMouseDown = true;
          IsDirty = CurrentLayer.IsDirty;
        }
        else if (e.RightButton == MouseButtonState.Pressed)
        {
          SelectedObject = CurrentLayer.SelectTile(_mousePosition);

          Selector.Instance.SelectedImages[0].Position = Selector.Instance.Images[0].Position;
          Selector.Instance.SelectedImages[1].Position = Selector.Instance.Images[1].Position;
          Selector.Instance.SelectedImages[2].Position = Selector.Instance.Images[2].Position;
          Selector.Instance.SelectedImages[3].Position = Selector.Instance.Images[3].Position;

          Selector.Instance.SelectedMapTileRegion = (SelectedObject as Tile).SourceRect;
        }

        base.OnMouseDown(e);
      }
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
      if (IsViewerLoaded)
      {
        _mouseInputHandler.HandleMouseButtons(e);

        _isMouseDown = false;

        base.OnMouseUp(e);
      }
    }

    protected override void OnMouseEnter(MouseEventArgs e)
    {
      if (IsViewerLoaded)
      {
        _mouseInputHandler.HandleMouseMove(e);

        _isMouseOver = true;

        base.OnMouseEnter(e);
      }
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
      if (IsViewerLoaded)
      {
        _mouseInputHandler.HandleMouseMove(e);

        _isMouseOver = false;
        Draw(TimeSpan.Zero);
        InvalidateVisual();

        base.OnMouseLeave(e);
      }
      MousePosition = -Vector2.One;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
      if (IsViewerLoaded && Selector.Instance.CurrentTile != null)
      {
        _mouseInputHandler.HandleMouseMove(e);

        _mousePosition = new Vector2((int)(e.GetPosition(this).X / CurrentLayer.TileDimensions.X), (int)(e.GetPosition(this).Y / CurrentLayer.TileDimensions.Y));
        _mousePosition *= CurrentTileDimensions;
        MousePosition = _mousePosition;

        int width = (int)(Selector.Instance.SelectedTileRegion.Width * CurrentLayer.TileDimensions.X);
        int height = (int)(Selector.Instance.SelectedTileRegion.Height * CurrentLayer.TileDimensions.Y);

        Selector.Instance.Images[0].Position = _mousePosition;
        Selector.Instance.Images[1].Position = new Vector2(_mousePosition.X + width, _mousePosition.Y);
        Selector.Instance.Images[2].Position = new Vector2(_mousePosition.X, _mousePosition.Y + height);
        Selector.Instance.Images[3].Position = new Vector2(_mousePosition.X + width, _mousePosition.Y + height);

        if (_isMouseDown)
          OnMouseDown(new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Right) { RoutedEvent = e.RoutedEvent });

        InvalidateVisual();

        base.OnMouseMove(e);
      }
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
      if (IsViewerLoaded)
      {
        _mouseInputHandler.HandleMouseWheel(e);

        Engine.System.Managers.CameraManager.Instance.Zoom(e.Delta/1000f);

        base.OnMouseWheel(e);
      }
    }

    public void _map_OnIsDirtyChanged(object sender, EventArgs e)
    {
      var saveManager = _container.Resolve<ISaveManagerService>();
      saveManager.Add(Map, _container.Resolve<WorkspaceBase>().Tools.First(f => f.ContentId == "Viewer") as ToolViewModel);
      IsDirty = true;
    }

    void CurrentLayer_OnIsDirtyChanged(object sender, EventArgs e)
    {
      // TODO: Actually, this should go in the TileEditor at one point
      /*var saveManager = _container.Resolve<ISaveManagerService>();
      if (!saveManager.ObjectsToSave.Contains(CurrentLayer))
      {
        saveManager.ObjectsToSave.Add(CurrentLayer);
        saveManager.Refresh();
      }
      IsDirty = true;*/
    }
    #endregion
  }
}
