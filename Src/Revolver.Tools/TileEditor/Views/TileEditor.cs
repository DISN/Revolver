using Engine.System.Entities;
using Engine.Wrappers.Hosting;
using Engine.Wrappers.Input;
using Engine.Wrappers.SelectionTool;
using Magnum.Controls.Themes.Styles;
using Magnum.Core.Managers;
using Magnum.IconLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Serialization;

namespace Revolver.Tools.TileEditor.Views
{
  public class TileEditor : D3D11Host
  {
    #region Fields
    Image _image;
    SpriteBatch _spriteBatch;
    Tile _tile;
    public bool _isMouseDown, _isLoaded;
    public Vector2 _mousePosition, _clickPosition;
    List<Image> _temporarySelectorImages;
    XmlManager<Tile> _tileXmlManager;

    // the input helpers that replace the XNA/MonoGame classes (Keyboard and Mouse).
    //private readonly NativeKeyboardInput _keyboardInputHandler;
    private readonly WPFMouseInput _mouseInputHandler;
    //private bool _focused;
    //private KeyboardState _previous;
    #endregion

    #region Constructors
    public TileEditor()
    {
      _temporarySelectorImages = new List<Image>();
      _tile = new Tile();
      _isMouseDown = _isLoaded = false;
      _tileXmlManager = new XmlManager<Tile>();

      // keyboard input simply just works once you pass in an IInputElement.
      //_keyboardInputHandler = new NativeKeyboardInput(this);
      // mouse input requires hooking into the WPF events and calling them manually (see "Mouse Event Handlers" region at bottom of this file)
      _mouseInputHandler = new WPFMouseInput(this);
    }
    #endregion

    #region Properties
    public int TileWidth
    {
      get { return _tile.Image.SourceRect.Width; }
    }

    public int TileHeight
    {
      get { return _tile.Image.SourceRect.Height; }
    }

    public Vector2 CurrentTileDimensions
    {
      get { return _tile.TileDimensions; }
    }

    public event EventHandler OnIsTileEditorLoadedChanged;

    public bool IsTileEditorLoaded
    {
      get { return _isLoaded; }
      set
      {
        if (_isLoaded != value)
        {
          _isLoaded = value;

          var handler = OnIsTileEditorLoadedChanged;
          if (handler != null)
            handler(this, null);
        }
      }
    }
    #endregion

    #region Methods

    public override void Load()
    {
      /*_spriteBatch = new SpriteBatch(GraphicsDevice);
      Selector.Instance.Initialize(GraphicsDevice);

      XmlSerializer xml = new XmlSerializer(_tile.GetType());
      using (Stream stream = File.Open("Content/XML/Tile1.xml", FileMode.Open))
        _tile = (Tile)xml.Deserialize(stream);
      _tile.Initialize(GraphicsDevice);

      _image = _tile.Image;*/
    }

    public void Load(string filePath)
    {
      _spriteBatch = new SpriteBatch(GraphicsDevice);

      _tile = _tileXmlManager.Load(filePath);
      _tile.Initialize(GraphicsDevice);

      _image = _tile.Image;

      // D3D11Host stuff
      //ImageWidth = _tile.Image.SourceRect.Width;
      //ImageHeight = _tile.Image.SourceRect.Height;

      //Selector.Instance.CurrentTileDimensions = CurrentTileDimensions;
      Selector.Instance.Initialize(GraphicsDevice);
      Selector.Instance.CurrentTile = _tile;

      IsTileEditorLoaded = true;
      Draw(TimeSpan.Zero);
      InvalidateVisual();
    }

    public void Load(Tile tile)
    {
      _spriteBatch = new SpriteBatch(GraphicsDevice);

      _tile = _tileXmlManager.Load("Content/XML/" + tile.Name + ".xml");
      _tile.Initialize(GraphicsDevice);

      _image = _tile.Image;

      // D3D11Host stuff
      //ImageWidth = _tile.Image.SourceRect.Width;
      //ImageHeight = _tile.Image.SourceRect.Height;

      //Selector.Instance.CurrentTileDimensions = CurrentTileDimensions;
      Selector.Instance.Initialize(GraphicsDevice);
      Selector.Instance.CurrentTile = _tile;

      IsTileEditorLoaded = true;
      Draw(TimeSpan.Zero);
      InvalidateVisual();
    }

    public override void Unload()
    {
    }

    public override void Update(TimeSpan dt)
    {
      // only when we focus, keyboard input is received
      // but make sure to focus only when it is actually needed
      /*if (!_focused && IsMouseDirectlyOver && _mouseInputHandler.MouseState.LeftButton == ButtonState.Pressed)
      {
        Focus();
        _focused = true;
      }
      else
        _focused = false;

      KeyboardState keys = _keyboardInputHandler.GetState();

      // figure out all the keys that switched state inbetween the last two updates - and make them print out text
      IEnumerable<Keys> deltaKeys = keys.GetPressedKeys().Where(k => !_previous.GetPressedKeys().Contains(k));
      _previous = keys;*/
    }

    public override void Draw(TimeSpan dt)
    {
      //System.Drawing.Color color = Brushes.ConvertBrushToDrawingColor(Brushes.LoadBrushFromResourceKey("MainWindowInnerBackgroundBrush"));
      //Microsoft.Xna.Framework.Color xnaColor = new Microsoft.Xna.Framework.Color(color.R, color.G, color.B, color.A);

      if (_isLoaded)
      {
        GraphicsDevice.Clear(Color.Black);
        _spriteBatch.Begin();
        _image.Draw(_spriteBatch);
        if (Selector.Instance.OnTileEditor)
          foreach (Image image in Selector.Instance.Images)
            image.Draw(_spriteBatch);
        else
          foreach (Image image in _temporarySelectorImages)
            image.Draw(_spriteBatch);
        _spriteBatch.End();
      }
    }

    private bool AnyMouseEventTriggered()
    {
      MouseState m = _mouseInputHandler.MouseState;
      return m.LeftButton == ButtonState.Pressed ||
             m.MiddleButton == ButtonState.Pressed ||
             m.RightButton == ButtonState.Pressed;
    }
    #endregion

    #region Events
    // we can easily handle the keyboard input via native methods
    // but mouse input has proven to be very tricky to handle
    // evaluating 5+ samples, I concluded this the easiest (and so far best working!) solution for WPF
    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
      if (_isLoaded)
      {
        _mouseInputHandler.HandleMouseButtons(e);

        if (!_isMouseDown)
        {
          _clickPosition = _mousePosition;
          foreach (Image image in Selector.Instance.Images)
            image.Position = _mousePosition;
        }

        _isMouseDown = true;
        InvalidateVisual();

        base.OnMouseDown(e);
      }
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
      if (_isLoaded)
      {
        _mouseInputHandler.HandleMouseButtons(e);

        _isMouseDown = false;

        Selector.Instance.SelectedTileRegion = new Rectangle((int)Selector.Instance.Images[0].Position.X, (int)Selector.Instance.Images[0].Position.Y,
          (int)(Selector.Instance.Images[1].Position.X - Selector.Instance.Images[0].Position.X), (int)(Selector.Instance.Images[2].Position.Y - Selector.Instance.Images[0].Position.Y));

        Selector.Instance.SelectedTileRegion.X /= (int)CurrentTileDimensions.X;
        Selector.Instance.SelectedTileRegion.Y /= (int)CurrentTileDimensions.Y;
        Selector.Instance.SelectedTileRegion.Width /= (int)CurrentTileDimensions.X;
        Selector.Instance.SelectedTileRegion.Height /= (int)CurrentTileDimensions.Y;

        base.OnMouseUp(e);
      }
    }

    protected override void OnMouseEnter(MouseEventArgs e)
    {
      if (_isLoaded)
      {
        _mouseInputHandler.HandleMouseMove(e);

        Selector.Instance.OnTileEditor = true;
        if (_temporarySelectorImages.Count > 0)
        {
          Selector.Instance.Images.Clear();
          foreach (Image image in _temporarySelectorImages)
            Selector.Instance.Images.Add(image);
          _temporarySelectorImages.Clear();
        }

        base.OnMouseEnter(e);
      }
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
      if (_isLoaded)
      {
        _mouseInputHandler.HandleMouseMove(e);

        Selector.Instance.OnTileEditor = false;
        _temporarySelectorImages.Clear();
        foreach (Image image in Selector.Instance.Images)
        {
          Image newImage = new Image();
          newImage.Path = image.Path;
          newImage.Position = image.Position;
          newImage.SourceRect = image.SourceRect;
          newImage.Texture = image.Texture;
          _temporarySelectorImages.Add(newImage);
        }

        base.OnMouseLeave(e);
      }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
      if (_isLoaded)
      {
        _mouseInputHandler.HandleMouseMove(e);

        _mousePosition = new Vector2((int)(e.GetPosition(this).X / _tile.TileDimensions.X), (int)(e.GetPosition(this).Y / _tile.TileDimensions.Y));
        _mousePosition *= CurrentTileDimensions;

        if (_mousePosition != _clickPosition && _isMouseDown)
        {
          for (int i = 0; i < 4; i++)
          {
            if (i % 2 == 0 && _mousePosition.X < _clickPosition.X)
              Selector.Instance.Images[i].Position.X = _mousePosition.X;
            else if (i % 2 != 0 && _mousePosition.X > _clickPosition.X)
              Selector.Instance.Images[i].Position.X = _mousePosition.X;

            if (i < 2 && _mousePosition.Y < _clickPosition.Y)
              Selector.Instance.Images[i].Position.Y = _mousePosition.Y;
            else if (i >= 2 && _mousePosition.Y > _clickPosition.Y)
              Selector.Instance.Images[i].Position.Y = _mousePosition.Y;
          }

          InvalidateVisual();
        }
        else if (_isMouseDown)
        {
          foreach (Image image in Selector.Instance.Images)
            image.Position = _mousePosition;

          InvalidateVisual();
        }

        base.OnMouseMove(e);
      }
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
      if (_isLoaded)
      {
        _mouseInputHandler.HandleMouseWheel(e);
        base.OnMouseWheel(e);
      }
    }
    #endregion
  }
}
