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
  public class EntityComponentItem : ViewModelBase
  {
    #region Constructors
    public EntityComponentItem(string componentName, ComponentType componentType)
    {
      ComponentName = componentName;
      ComponentType = componentType;
    }
    #endregion

    #region Properties
    
    private string _ComponentName = "Empty Component";
    public string ComponentName
    {
      get { return _ComponentName; }
      set
      {
        if (_ComponentName != value)
        {
          _ComponentName = value;
          RaisePropertyChanged("ComponentName");
        }
      }
    }

    private ComponentType _ComponentType = ComponentType.None;
    public ComponentType ComponentType
    {
      get { return _ComponentType; }
      set
      {
        if (_ComponentType != value)
        {
          _ComponentType = value;
          RaisePropertyChanged("ComponentType");
        }
      }
    }

    #endregion
  }
}
