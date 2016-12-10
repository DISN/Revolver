using Magnum.Core;
using Magnum.Core.Models;
using Magnum.Core.Services;
using Magnum.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revolver.Tools.ProgressBar
{
  public class ProgressBar : ViewModelBase, IProgressBarService
  {
    #region Fields
    private uint _pMax;
    private uint _pVal;
    private bool _showProgress;
    #endregion

    #region Constructors
    public ProgressBar()
    {
      Clear();
    }
    #endregion

    #region Properties
    public double ProgressValueTaskBarItemInfo
    {
      get { return ((double)_pVal / (double)_pMax); }
    }

    public uint ProgressMaximum
    {
      get { return _pMax; }
      set
      {
        _pMax = value;
        RaisePropertyChanged("ProgressMaximum");
      }
    }

    public uint ProgressValue
    {
      get { return _pVal; }
      set
      {
        _pVal = value;
        RaisePropertyChanged("ProgressValue");
        RaisePropertyChanged("ProgressValueTaskBarItemInfo");
      }
    }

    public bool ShowProgressBar
    {
      get { return _showProgress; }
      set
      {
        _showProgress = value;
        RaisePropertyChanged("ShowProgressBar");
      }
    }
    #endregion

    #region Methods
    public bool Clear()
    {
      ShowProgressBar = false;
      return true;
    }

    public bool Progress(bool On, uint current, uint total)
    {
      ShowProgressBar = On;
      ProgressMaximum = total;
      ProgressValue = current;
      return true;
    }
    #endregion
  }
}
