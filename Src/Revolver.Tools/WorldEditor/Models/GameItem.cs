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

namespace Revolver.Tools.WorldEditor.Models
{
  public class GameItem : TreeNodeViewModel
  {
    #region Constructors
    public GameItem(string title, int id, IItemType itemType, BitmapImage icon, IUnityContainer container)
      : base(title, id, itemType, icon, container, new ObservableCollection<GameItem>())
    {
      InitializeGameItem();
    }
    #endregion

    #region Methods

    private void InitializeGameItem()
    {
      if ((IItemType)this.ItemType == IItemType.Game ||
          (IItemType)this.ItemType == IItemType.Resource)
        IsExpanded = true;
    }

    public override bool IsCriteriaMatched(string criteria)
    {
      if (StringUtils.SimpleMatch(this.DisplayName, criteria))
        return true;

      return false;
    }

    #endregion
  }
}
