using Magnum.Core;
using Magnum.Core.Models;
using Magnum.Core.Services;
using Magnum.Core.ViewModels;
using Magnum.IconLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Revolver.Tools.StatusBar
{
  public class StatusBar : ViewModelBase, IStatusBarService
  {
    #region Fields
    private int? _LineNumber;
    private bool? _InsertMode;
    private int? _ColPosition;
    private int? _CharPosition;
    private Brush _foreground;
    private Brush _background, _border;
    private Image _animImage;
    private BitmapImage _engineImage;
    private bool _isFrozen, _showStatusBarBackground;
    private string _text, _mousePosition, _currentLayerName;
    private EngineStates _engineState;
    private string _selectedTheme;

    public static class EngineColors
    {
      public static SolidColorBrush NotLoaded = (SolidColorBrush)new BrushConverter().ConvertFrom("#FF3F3F46");
      public static SolidColorBrush Stopped = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFb30d21");
      public static SolidColorBrush Paused = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFd5ab3d");
      public static SolidColorBrush Playing = (SolidColorBrush)new BrushConverter().ConvertFrom("#FF4e8f31");
    }
    #endregion

    #region Constructors
    public StatusBar()
    {
      Clear();

      Magnum.Core.ApplicationModel.Application.OnIsGameLoadedChanged += Application_OnIsGameLoadedChanged;
      Application_OnIsGameLoadedChanged(null, null);
    }
    #endregion

    #region Methods
    public bool Animation(Image image)
    {
      AnimationImage = image;
      return true;
    }

    public bool Clear()
    {
      ChangeForegroundColor();
      UpdateEngineInfo();
      MousePosition = String.Empty;
      IsFrozen = false;
      InsertMode = null;
      LineNumber = null;
      CharPosition = null;
      ColPosition = null;
      AnimationImage = null;
      return true;
    }

    void Application_OnIsGameLoadedChanged(object sender, EventArgs e)
    {
      if (Magnum.Core.ApplicationModel.Application.IsGameLoaded)
        EngineState = EngineStates.Stopped;
      else
        EngineState = EngineStates.NotLoaded;
    }

    private void ChangeForegroundColor()
    {
      if (Magnum.Core.ApplicationModel.Application.IsGameLoaded && ShowStatusBarBackground)
        Foreground = Brushes.White;
      else
        if (SelectedTheme == "Light")
          Foreground = Brushes.Black;
        else
          Foreground = Brushes.White;
    }

    public void UpdateEngineInfo()
    {
      if (EngineState == EngineStates.Stopped)
      {
        if (ShowStatusBarBackground)
        {
          Background = EngineColors.Stopped;
          BorderColor = EngineColors.Stopped;
        }
        else
        {
          Background = Brushes.Transparent;
          BorderColor = EngineColors.NotLoaded;
        }
        Text = "Ready";
        EngineImage = BitmapImages.LoadBitmapFromIconType(IconType.Stop);
      }
      else if (EngineState == EngineStates.Playing)
      {
        if (ShowStatusBarBackground)
        {
          Background = EngineColors.Playing;
          BorderColor = EngineColors.Playing;
        }
        else
        {
          Background = Brushes.Transparent;
          BorderColor = EngineColors.NotLoaded;
        }
        Text = "Running";
        EngineImage = BitmapImages.LoadBitmapFromIconType(IconType.Play);
      }
      else if (EngineState == EngineStates.Paused)
      {
        if (ShowStatusBarBackground)
        {
          Background = EngineColors.Paused;
          BorderColor = EngineColors.Paused;
        }
        else
        {
          Background = Brushes.Transparent;
          BorderColor = EngineColors.NotLoaded;
        }
        Text = "Paused";
        EngineImage = BitmapImages.LoadBitmapFromIconType(IconType.Pause);
      }
      else if (EngineState == EngineStates.NotLoaded)
      {
        Text = "Ready";
        Background = Brushes.Transparent;
        BorderColor = EngineColors.NotLoaded;
        EngineImage = null;
      }
    }
    public bool FreezeOutput()
    {
      return IsFrozen;
    }
    #endregion

    #region Properties
    public bool IsFrozen
    {
      get { return _isFrozen; }
      set
      {
        _isFrozen = value;
        RaisePropertyChanged("IsFrozen");
      }
    }

    public bool ShowStatusBarBackground
    {
      get
      {
        return _showStatusBarBackground;
      }
      set
      {
        _showStatusBarBackground = value;
        ChangeForegroundColor();
        UpdateEngineInfo();

        RaisePropertyChanged("ShowStatusBarBackground");
      }
    }

    public string Text
    {
      get { return _text; }
      set
      {
        _text = value;
        RaisePropertyChanged("Text");
      }
    }

    public string MousePosition
    {
      get { return _mousePosition; }
      set
      {
        _mousePosition = value;
        RaisePropertyChanged("MousePosition");
      }
    }

    public string CurrentLayerName
    {
      get { return _currentLayerName; }
      set
      {
        _currentLayerName = value;
        RaisePropertyChanged("CurrentLayerName");
      }
    }

    public string SelectedTheme
    {
      get { return _selectedTheme; }
      set
      {
        _selectedTheme = value;
        ChangeForegroundColor();

        RaisePropertyChanged("SelectedTheme");
      }
    }

    public EngineStates EngineState
    {
      get { return _engineState; }
      set
      {
        _engineState = value;
        ChangeForegroundColor();
        UpdateEngineInfo();

        RaisePropertyChanged("EngineState");
      }
    }

    public Brush Foreground
    {
      get { return _foreground; }
      set
      {
        _foreground = value;
        RaisePropertyChanged("Foreground");
      }
    }

    public Brush Background
    {
      get { return _background; }
      set
      {
        _background = value;
        RaisePropertyChanged("Background");
      }
    }

    public Brush BorderColor
    {
      get { return _border; }
      set
      {
        _border = value;
        RaisePropertyChanged("BorderColor");
      }
    }

    public bool? InsertMode
    {
      get { return _InsertMode; }
      set
      {
        _InsertMode = value;
        RaisePropertyChanged("InsertMode");
      }
    }

    public int? LineNumber
    {
      get { return _LineNumber; }
      set
      {
        _LineNumber = value;
        RaisePropertyChanged("LineNumber");
      }
    }

    public int? CharPosition
    {
      get { return _CharPosition; }
      set
      {
        _CharPosition = value;
        RaisePropertyChanged("CharPosition");
      }
    }

    public int? ColPosition
    {
      get { return _ColPosition; }
      set
      {
        _ColPosition = value;
        RaisePropertyChanged("ColPosition");
      }
    }
    public Image AnimationImage
    {
      get { return _animImage; }
      set
      {
        _animImage = value;
        RaisePropertyChanged("AnimationImage");
      }
    }
    public BitmapImage EngineImage
    {
      get { return _engineImage; }
      set
      {
        _engineImage = value;
        RaisePropertyChanged("EngineImage");
      }
    }
    #endregion

    
  }
}
