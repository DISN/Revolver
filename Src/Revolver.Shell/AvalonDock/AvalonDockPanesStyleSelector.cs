using Magnum.Core.Models;
using Magnum.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.AvalonDock.Controls;

namespace Revolver.Shell.AvalonDock
{
  public class AvalonDockPanesStyleSelector : StyleSelector
  {
    public Style ToolStyle { get; set; }

    public Style DocumentStyle { get; set; }

    public override Style SelectStyle(object item, DependencyObject container)
    {
      if (item is ToolViewModel)
      {
        (item as ToolViewModel).LayoutAnchorableItem = container as LayoutAnchorableItem;
        return ToolStyle;
      }

      if (item is DocumentViewModel)
        return DocumentStyle;

      return base.SelectStyle(item, container);
    }
  }
}
