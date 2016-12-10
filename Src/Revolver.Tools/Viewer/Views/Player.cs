using Engine.Main;
using Engine.System.Managers;
using Engine.Wrappers.Hosting;
using Engine.Wrappers.Input;
using Magnum.Controls.Themes.Styles;
using Magnum.Core.Windows;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Revolver.Tools.Viewer.Views
{
  /// <summary>
  /// Should only be used to play the game as if it's from the engine, it loads up the whole
  /// GameManager.
  /// </summary>
  public class Player : D3D11Host
  {
    #region Fields
    GameManager _gameManager;
    TimeSpan _lastTime;
    bool _isLoaded, _isPlaying, _isPaused;

    // the input helpers that replace the XNA/MonoGame classes (Keyboard and Mouse).
    private readonly NativeKeyboardInput _keyboardInputHandler;
    private readonly WPFMouseInput _mouseInputHandler;
    //private bool _focused;
    private KeyboardState _previous;
    #endregion

    #region Constructors
    public Player()
    {
      IsPlaying /*= _focused*/ = false;

      InitializeGraphicsDevice(); // we need to force this because D3D11Host doesn't fire the Load() until the view is drawn on the editor (some WPF call shenanigans)
      Load();

      // keyboard input simply just works once you pass in an IInputElement.
      _keyboardInputHandler = new NativeKeyboardInput(this);
      this.Focusable = true; // needed so we can receive keyboard inputs ("this" is set in a ContentControl).
      // mouse input requires hooking into the WPF events and calling them manually (see "Mouse Event Handlers" region at bottom of this file)
      _mouseInputHandler = new WPFMouseInput(this);
    }
    #endregion

    #region Properties
    public event EventHandler OnIsPlayingChanged;

    public bool IsPlaying
    {
      get
      {
        return _isPlaying;
      }
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
      get
      {
        return _isPaused;
      }
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
    #endregion

    #region Methods
    public override void Load()
    {
      if (!_isLoaded)
      {
        _gameManager = new GameManager();
        _gameManager.Load();
        _isLoaded = true;
      }
    }

    public override void Unload()
    {
      _gameManager.Unload();
    }

    public override void Draw(TimeSpan dt)
    {
      //System.Drawing.Color color = Brushes.ConvertBrushToDrawingColor(Brushes.LoadBrushFromResourceKey("MainWindowInnerBackgroundBrush"));
      //Microsoft.Xna.Framework.Color xnaColor = new Microsoft.Xna.Framework.Color(color.R, color.G, color.B, color.A);

      GraphicsDevice.Clear(Color.Black);
      if (IsPlaying)
        _gameManager.Draw(dt, dt - _lastTime);
    }

    public void Play()
    {
      IsPlaying = true;
      if (!IsPaused)
        _gameManager.Reload();
      else
        IsPaused = false;
    }

    public void Stop()
    {
      IsPlaying = false;
      IsPaused = false;
      _gameManager.Reload();
    }

    public void Pause()
    {
      IsPaused = true;
    }

    public override void Update(TimeSpan dt)
    {
      // only when we focus, keyboard input is recieved
      // but make sure to focus only when it is actually needed
      if (IsPlaying && !IsPaused)
      {
        Focus();
        //_focused = true;
        KeyboardState keys = _keyboardInputHandler.GetState();
        _gameManager.Update(dt, dt - _lastTime, keys);
        _previous = keys;
      }
      /*else
        _focused = false;*/

      _lastTime = dt;
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
      _mouseInputHandler.HandleMouseButtons(e);
      base.OnMouseDown(e);
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
      _mouseInputHandler.HandleMouseButtons(e);
      base.OnMouseUp(e);
    }

    protected override void OnMouseEnter(MouseEventArgs e)
    {
      _mouseInputHandler.HandleMouseMove(e);
      base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
      _mouseInputHandler.HandleMouseMove(e);
      base.OnMouseLeave(e);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
      _mouseInputHandler.HandleMouseMove(e);
      base.OnMouseMove(e);
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
      _mouseInputHandler.HandleMouseWheel(e);
      base.OnMouseWheel(e);
    }
    #endregion
  }
}
