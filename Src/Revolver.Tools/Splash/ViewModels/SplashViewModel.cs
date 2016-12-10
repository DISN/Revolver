using System;
using Microsoft.Practices.Prism.Events;
using Magnum.Core;
using Magnum.Core.Models;
using Magnum.Core.Events;
using Magnum.Core.ViewModels;
using System.Windows.Media.Imaging;

namespace Revolver.Tools.Splash.ViewModels
{
  public class SplashViewModel : ViewModelBase
  {
    #region Fields
    private string _status;
    private string _version;
    private BitmapImage _background;
    #endregion

    #region Constructors
    public SplashViewModel(IEventAggregator eventAggregator_)
    {
      eventAggregator_.GetEvent<SplashMessageUpdateEvent>().Subscribe(e_ => UpdateMessage(e_.Message));

      _version = "v2.0";

      Random rand = new Random((int)DateTime.Now.Ticks);
      int num = rand.Next(1, 6);
      _background = new BitmapImage(new Uri(@"Content/Splash/Concept" + num + "BlackTransparent.png", UriKind.RelativeOrAbsolute));
    }
    #endregion

    #region Properties
    public string Status
    {
      get { return _status; }
      set
      {
        _status = value;
        RaisePropertyChanged("Status");
      }
    }

    public string Version
    {
      get { return _version; }
      set
      {
        _version = value;
        RaisePropertyChanged("Version");
      }
    }

    public BitmapImage Background
    {
      get { return _background; }
      set
      {
        _background = value;
        RaisePropertyChanged("Background");
      }
    }
    #endregion

    #region Methods
    private void UpdateMessage(string message)
    {
      if (string.IsNullOrEmpty(message))
      {
        return;
      }

      Status = string.Concat(Environment.NewLine, message, "...");
    }
    #endregion
  }
}