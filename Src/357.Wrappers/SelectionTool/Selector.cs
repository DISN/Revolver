using Engine.System.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Wrappers.SelectionTool
{
  public class Selector
  {
    static Selector _instance;
    List<Image> _images;
    List<Image> _selectedImages;
    Layer _layer;
    Tile _tile;
    bool _onTileEditor;
    //Vector2 _tileDimensions;
    string[] _selectorPath = { "Tools/SelectionTool/TopLeft",
                               "Tools/SelectionTool/TopRight",
                               "Tools/SelectionTool/BottomLeft",
                               "Tools/SelectionTool/BottomRight" };
    /*string[] _selectorPath_16x16 = { "Tools/SelectionTool/TopLeft_16x16",
                                     "Tools/SelectionTool/TopRight_16x16",
                                     "Tools/SelectionTool/BottomLeft_16x16",
                                     "Tools/SelectionTool/BottomRight_16x16" };*/
    string[] _selectorSelectedPath = { "Tools/SelectionTool/TopLeftSelected",
                                       "Tools/SelectionTool/TopRightSelected",
                                       "Tools/SelectionTool/BottomLeftSelected",
                                       "Tools/SelectionTool/BottomRightSelected" };
    public bool _isInitialized = false;

    #region Constructors
    private Selector()
    {
      _images = new List<Image>();
      _selectedImages = new List<Image>();
      _onTileEditor = false;

      for (int i = 0; i < 4; i++)
      {
        _images.Add(new Image());
        _selectedImages.Add(new Image());
      }

      for (int j = 0; j < 4; j++)
      {
        _images[j].Path = _selectorPath[j];
        _selectedImages[j].Path = _selectorSelectedPath[j];
      }

      SelectedTileRegion = new Rectangle(0, 0, 0, 0);
      SelectedMapTileRegion = new Rectangle(0, 0, 0, 0);
    }
    #endregion

    #region Properties
    public static Selector Instance
    {
      get
      {
        if (_instance == null)
          _instance = new Selector();
        return _instance;
      }
    }

    public List<Image> Images
    {
      get { return _images; }
      set
      {
        if (_images != value)
          _images = value;
      }
    }

    public List<Image> SelectedImages
    {
      get { return _selectedImages; }
      set
      {
        if (_selectedImages != value)
          _selectedImages = value;
      }
    }

    public Layer CurrentLayer
    {
      get { return _layer; }
      set
      {
        if (_layer != value)
          _layer = value;
      }
    }

    public Tile CurrentTile
    {
      get { return _tile; }
      set
      {
        if (_tile != value)
          _tile = value;
      }
    }

    public bool OnTileEditor
    {
      get { return _onTileEditor; }
      set
      {
        if (_onTileEditor != value)
          _onTileEditor = value;
      }
    }

    /*public Vector2 CurrentTileDimensions
    {
      get { return _tileDimensions; }
      set
      {
        if (_tileDimensions != value)
        {
          _tileDimensions = value;
          for (int i = 0; i < 4; i++)
            if (_tileDimensions.X == 16 && _tileDimensions.Y == 16)
              Images[i].Path = _selectorPath_16x16[i];
            else
              Images[i].Path = _selectorPath[i];
          for (int j = 0; j < 4; j++)
            Images[j].SourceRect = new Rectangle((int)Images[0].Position.X, (int)Images[0].Position.Y,
                    (int)(Images[1].Position.X - Images[0].Position.X), (int)(Images[2].Position.Y - Images[0].Position.Y));
          _isInitialized = false;
        }
      }
    }*/

    public Rectangle SelectedTileRegion;
    public Rectangle SelectedMapTileRegion;
    #endregion

    #region Methods
    public void Initialize(GraphicsDevice graphicsDevice)
    {
      if (!_isInitialized)
      {
        for (int i = 0; i < Images.Count; i++)
          Images[i].Initialize(graphicsDevice);
        for (int j = 0; j < SelectedImages.Count; j++)
          SelectedImages[j].Initialize(graphicsDevice);
        _isInitialized = true;
      }
    }
    #endregion
  }
}
