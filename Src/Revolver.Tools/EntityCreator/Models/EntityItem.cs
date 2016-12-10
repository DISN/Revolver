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
  public class EntityItem : ViewModelBase
  {
    #region Constructors
    public EntityItem(string entityName)
    {
      EntityName = entityName;
      _States = new ObservableCollection<EntityStateItem>();
    }
    #endregion

    #region Properties

    private string _EntityName = "New Entity";
    public string EntityName
    {
      get { return _EntityName; }
      set
      {
        if (_EntityName != value)
        {
          _EntityName = value;
          RaisePropertyChanged("EntityName");
        }
      }
    }

    private ObservableCollection<EntityStateItem> _States;
    public ObservableCollection<EntityStateItem> States
    {
      get { return _States; }
      set
      {
        if (_States != value)
        {
          _States = value;
        }
      }
    }

    #endregion
  }
}
