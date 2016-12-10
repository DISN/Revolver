using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Engine.Wrappers.Hosting
{
  /// <summary>
  /// Host a Direct3D 11 scene in WPF. Override to implement.
  /// This class uses the <see cref="Image"/> class to host its content.
  /// The actual game is rendered into a DirectX image (D3D11Image) and then displayed as the source.
  /// </summary>
  public abstract class D3D11Host : Image
  {
    // The Direct3D 11 device (shared by all D3D11Host elements):
    private static GraphicsDevice _graphicsDevice;
    private static int _referenceCount;
    private static readonly object GraphicsDeviceLock = new object();

    // Image source:
    private RenderTarget2D _renderTarget;
    private D3D11Image _d3D11Image;
    private bool _resetBackBuffer;

    public bool ResetBackBuffer
    {
      get { return _resetBackBuffer; }
      set { _resetBackBuffer = value; }
    }

    // Render timing:
    private readonly Stopwatch _timer;
    private TimeSpan _lastRenderingTime;

    private bool _loaded;
    private ContentManager _content;

    /// <summary>
    /// Gets a value indicating whether the controls runs in the context of a designer (e.g.
    /// Visual Studio Designer or Expression Blend).
    /// </summary>
    /// <value>
    /// <see langword="true" /> if controls run in design mode; otherwise, 
    /// <see langword="false" />.
    /// </value>
    public static bool IsInDesignMode
    {
      get
      {
        if (!_isInDesignMode.HasValue)
          _isInDesignMode = (bool)DependencyPropertyDescriptor.FromProperty(DesignerProperties.IsInDesignModeProperty, typeof(FrameworkElement)).Metadata.DefaultValue;

        return _isInDesignMode.Value;
      }
    }
    private static bool? _isInDesignMode;

    /// <summary>
    /// Initializes a new instance of the <see cref="D3D11Host"/> class.
    /// </summary>
    protected D3D11Host()
    {
      _timer = new Stopwatch();
      Loaded += OnLoaded;
      //Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs eventArgs)
    {
      if (IsInDesignMode || _loaded)
        return;

      /*InitializeGraphicsDevice();
      _content = new ContentManager(new ServiceContainer());
      Content.RootDirectory = "Content";

      _loaded = true;
      InitializeImageSource();
      Load();
      StartRendering();*/
    }

    public void FireOnLoaded()
    {
      InitializeGraphicsDevice();
      _content = new ContentManager(new ServiceContainer());
      Content.RootDirectory = "Content";

      _loaded = true;
      InitializeImageSource();
      Load();
      StartRendering();
    }

    public void FireOnLoaded(GraphicsDevice graphicsDevice)
    {
      InitializeGraphicsDevice(graphicsDevice);

      _content = new ContentManager(new ServiceContainer());
      Content.RootDirectory = "Content";

      _loaded = true;
      InitializeImageSource();
      Load();
      StartRendering();
    }

    private void OnUnloaded(object sender, RoutedEventArgs eventArgs)
    {
      if (IsInDesignMode)
        return;

      StopRendering();
      Unload();
      UnitializeImageSource();
      UninitializeGraphicsDevice();
    }

    public void FireOnUnloaded()
    {
      OnUnloaded(null, null);
    }

    public static void InitializeGraphicsDevice()
    {
      lock (GraphicsDeviceLock)
      {
        _referenceCount++;
        if (_referenceCount == 1)
        {
          // Create Direct3D 11 device.
          var presentationParameters = new PresentationParameters
          {
            // Do not associate graphics device with window.
            DeviceWindowHandle = IntPtr.Zero,
          };
          _graphicsDevice = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, GraphicsProfile.HiDef, presentationParameters);
        }
      }
    }

    public static void InitializeGraphicsDevice(GraphicsDevice graphicsDevice)
    {
      lock (GraphicsDeviceLock)
      {
        _referenceCount++;
        if (_referenceCount == 1)
        {
          _graphicsDevice = graphicsDevice;
        }
      }
    }

    private static void UninitializeGraphicsDevice()
    {
      lock (GraphicsDeviceLock)
      {
        _referenceCount--;
        if (_referenceCount == 0)
        {
          _graphicsDevice.Dispose();
          _graphicsDevice = null;
        }
      }
    }

    private void InitializeImageSource()
    {
      _d3D11Image = new D3D11Image();
      _d3D11Image.IsFrontBufferAvailableChanged += OnIsFrontBufferAvailableChanged;
      CreateBackBuffer();

      Source = _d3D11Image;
    }

    private void UnitializeImageSource()
    {
      _d3D11Image.IsFrontBufferAvailableChanged -= OnIsFrontBufferAvailableChanged;
      Source = null;

      if (_d3D11Image != null)
      {
        _d3D11Image.Dispose();
        _d3D11Image = null;
      }
      if (_renderTarget != null)
      {
        _renderTarget.Dispose();
        _renderTarget = null;
      }
    }

    private void CreateBackBuffer()
    {
      _d3D11Image.SetBackBuffer(null);
      if (_renderTarget != null)
      {
        _renderTarget.Dispose();
        _renderTarget = null;
      }

      int width;
      int height;
      if (ImageWidth != 0 && ImageHeight != 0)
      {
        width = Math.Max(ImageWidth, 1);
        height = Math.Max(ImageHeight, 1);
      }
      else
      {
        width = Math.Max((int)ActualWidth, 1);
        height = Math.Max((int)ActualHeight, 1);
      }
      _renderTarget = new RenderTarget2D(_graphicsDevice, width, height, false, SurfaceFormat.Bgr32, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.DiscardContents, true);
      _d3D11Image.SetBackBuffer(_renderTarget);
    }

    private void StartRendering()
    {
      if (_timer.IsRunning)
        return;

      CompositionTarget.Rendering += OnRendering;
      _timer.Start();
    }

    private void StopRendering()
    {
      if (!_timer.IsRunning)
        return;

      CompositionTarget.Rendering -= OnRendering;
      _timer.Stop();
    }

    public void SuspendRendering()
    {
      if (_timer.IsRunning)
        _timer.Stop();
    }

    public void ResumeRendering()
    {
      if (!_timer.IsRunning)
        _timer.Start();
    }

    private void OnRendering(object sender, EventArgs eventArgs)
    {
      if (!_timer.IsRunning)
        return;

      // Recreate back buffer if necessary.
      if (ResetBackBuffer)
        CreateBackBuffer();

      // CompositionTarget.Rendering event may be raised multiple times per frame
      // (e.g. during window resizing).
      var renderingEventArgs = (RenderingEventArgs)eventArgs;
      if (_lastRenderingTime != renderingEventArgs.RenderingTime || ResetBackBuffer)
      {
        _lastRenderingTime = renderingEventArgs.RenderingTime;

        UpdateInternal(_timer.Elapsed);
        GraphicsDevice.SetRenderTarget(_renderTarget);

        Draw(_timer.Elapsed);
        GraphicsDevice.Flush();
      }

      _d3D11Image.Invalidate(); // Always invalidate D3DImage to reduce flickering
      // during window resizing.

      ResetBackBuffer = false;
    }

    /// <summary>
    /// Raises the <see cref="FrameworkElement.SizeChanged" /> event, using the specified 
    /// information as part of the eventual event data.
    /// </summary>
    /// <param name="sizeInfo">Details of the old and new size involved in the change.</param>
    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
      ResetBackBuffer = true;
      base.OnRenderSizeChanged(sizeInfo);
    }

    private void OnIsFrontBufferAvailableChanged(object sender, DependencyPropertyChangedEventArgs eventArgs)
    {
      if (_d3D11Image.IsFrontBufferAvailable)
      {
        StartRendering();
        ResetBackBuffer = true;
      }
      else
      {
        StopRendering();
      }
    }

    private void UpdateInternal(TimeSpan dt)
    {
      Update(dt);
    }

    public int ImageWidth { get; set; }

    public int ImageHeight { get; set; }

    public bool D3D11ImageLoaded { get { return _d3D11Image != null; } }

    #region Game members
    /// <summary>
    /// Gets the graphics device.
    /// </summary>
    /// <value>The graphics device.</value>
    public GraphicsDevice GraphicsDevice
    {
      get { return _graphicsDevice; }
    }

    /// <summary>
    /// Gets the content manager - use in <see cref="Load"/> to load your content and in <see cref="Unload"/> to unload your content!
    /// </summary>
    public ContentManager Content
    {
      get { return _content; }
    }

    // These methods mimick the XNA Game class
    public abstract void Load();

    public abstract void Unload();

    public abstract void Update(TimeSpan dt);

    public abstract void Draw(TimeSpan dt);
    #endregion
  }
}