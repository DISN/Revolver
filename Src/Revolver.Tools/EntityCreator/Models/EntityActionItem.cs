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
  public class EntityActionItem : ViewModelBase
  {
    #region Constructors
    public EntityActionItem(string actionName, ActionType actionType)
    {
      ActionName = actionName;
      ActionType = actionType;
    }
    #endregion

    #region Properties
    
    private string _ActionName = "Empty Action";
    public string ActionName
    {
      get { return _ActionName; }
      set
      {
        if (_ActionName != value)
        {
          _ActionName = value;
          RaisePropertyChanged("ActionName");
        }
      }
    }

    private ActionType _ActionType = ActionType.None;
    public ActionType ActionType
    {
      get { return _ActionType; }
      set
      {
        if (_ActionType != value)
        {
          _ActionType = value;
          RaisePropertyChanged("ActionType");
        }
      }
    }

    #endregion
  }
}
