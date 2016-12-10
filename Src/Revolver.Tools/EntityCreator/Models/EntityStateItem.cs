using Magnum.Core.Services;
using Magnum.Core.Utils;
using Magnum.Core.ViewModels;
using Magnum.Core.Windows;
using Magnum.IconLibrary;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Revolver.Tools.EntityCreator.Models
{
  public class EntityStateItem : ViewModelBase
  {
    #region Constructors
    public EntityStateItem(string stateName)
    {
      StateName = StateName;
      _Components = new ObservableCollection<EntityComponentItem>();
      _Actions = new ObservableCollection<EntityActionItem>();
    }
    #endregion

    #region Properties

    private string _StateName = "New State";
    public string StateName
    {
      get { return _StateName; }
      set
      {
        if (_StateName != value)
        {
          _StateName = value;
          RaisePropertyChanged("StateName");
        }
      }
    }

    private ObservableCollection<EntityComponentItem> _Components;
    public ObservableCollection<EntityComponentItem> Components
    {
      get { return _Components; }
      set
      {
        if (_Components != value)
        {
          _Components = value;
        }
      }
    }

    private ObservableCollection<EntityActionItem> _Actions;
    public ObservableCollection<EntityActionItem> Actions
    {
      get { return _Actions; }
      set
      {
        if (_Actions != value)
        {
          _Actions = value;
        }
      }
    }

    #endregion
  }
}
