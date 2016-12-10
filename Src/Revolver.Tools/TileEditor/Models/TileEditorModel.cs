using Engine.System.Entities;
using Engine.Wrappers.SelectionTool;
using Magnum.Core.Models;
using Magnum.Core.Services;
using Magnum.Core.ViewModels;
using Magnum.IconLibrary;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Unity;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Revolver.Tools.TileEditor.Models
{
  class TileEditorModel : ToolModel
  {
    #region Fields
    Views.TileEditor _tileEditor;
    bool _isLoaded;
    string _lastLoadedResource;
    int _aspectRatioWidth, _aspectRatioHeight;
    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="TileEditorModel"/> class.
    /// </summary>
    public TileEditorModel(string displayName, IUnityContainer container, string category, string description, bool isPinned = true, string shortcut = null)
      : base(displayName, container, category, description, isPinned, shortcut)
    {
      Container = container;
      _tileEditor = new Views.TileEditor();
      _tileEditor.OnIsTileEditorLoadedChanged += _tileEditor_OnIsTileEditorLoadedChanged;
      _isLoaded = false;
      _lastLoadedResource = String.Empty;

      InitializeCommands();
    }
    #endregion

    #region Properties
    public Views.TileEditor TileEditor
    {
      get { return _tileEditor; }
      set { _tileEditor = value; }
    }

    public bool IsTileEditorLoaded
    {
      get { return _isLoaded; }
      set
      {
        _isLoaded = value;
        RaisePropertyChanged("IsTileEditorLoaded");
      }
    }

    public event EventHandler OnLastLoadedResourceChanged;
    public string LastLoadedResource
    {
      get { return _lastLoadedResource; }
      set
      {
        if (_lastLoadedResource != value)
        {
          _lastLoadedResource = value;
          RaisePropertyChanged("LastLoadedResource");

          var handler = OnLastLoadedResourceChanged;
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
          RaisePropertyChanged("AspectRatioHeight");
        }
      }
    }
    #endregion

    #region Methods
    private void InitializeCommands()
    {
      var commandManager = Container.Resolve<ICommandManager>();
      var ribbonService = Container.Resolve<IRibbonService>();

      var loadSelectedTileCommand = new UndoableCommand<Tile>(LoadSelectedTile);
      commandManager.RegisterCommand("LOADSELECTEDTILE", loadSelectedTileCommand);
    }

    public void LoadLastLoadedResource()
    {
      var logger = Container.Resolve<ILoggerService>();
      var progressBar = Container.Resolve<IProgressBarService>();
      progressBar.Progress(true, 1, 2);
      try
      {
        TileEditor.Load(LastLoadedResource);
        var commandManager = Container.Resolve<ICommandManager>();
        commandManager.Refresh();
        logger.Log("Tile Editor: " + LastLoadedResource + " loaded.", LogCategory.Info, LogPriority.None);
      }
      catch (Exception ex)
      {
        logger.Log("Tile Editor: Error loading Last Loaded Resource. Exception message: " + ex.Message, LogCategory.Error, LogPriority.High);
      }
      progressBar.Progress(false, 2, 2);
    }

    private void LoadSelectedTile(Tile tile)
    {
      var logger = Container.Resolve<ILoggerService>();
      var progressBar = Container.Resolve<IProgressBarService>();
      if (tile != null)
      {
        progressBar.Progress(true, 1, 2);

        try
        {
          TileEditor.Load(tile);
          LastLoadedResource = tile.LinkID;
          var commandManager = Container.Resolve<ICommandManager>();
          commandManager.Refresh();
          logger.Log("Tile Editor: " + tile.Name + " loaded.", LogCategory.Info, LogPriority.None);
        }
        catch (Exception ex)
        {
          logger.Log("Tile Editor: The file " + tile.Name + " is not supported. Exception message: " + ex.Message, LogCategory.Error, LogPriority.High);
        }
      }
      progressBar.Progress(false, 2, 2);
    }

    public void OnClosing(object info)
    {
      Selector.Instance.SelectedTileRegion = Rectangle.Empty;
    }
    #endregion

    #region Events
    private void _tileEditor_OnIsTileEditorLoadedChanged(object sender, EventArgs e)
    {
      IsTileEditorLoaded = _tileEditor.IsTileEditorLoaded;
      AspectRatioWidth = _tileEditor.TileWidth > 0 ? _tileEditor.TileWidth : 1000;
      double newHeight = (1440.0 / 1920.0) * _tileEditor.ImageWidth;
      AspectRatioHeight = (int)newHeight;
    }
    #endregion
  }
}
