using Magnum.Core.Models;
using Magnum.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Revolver.Shell.AvalonDock
{
  public class AvalonDockPanesTemplateSelector : DataTemplateSelector
  {
    public DataTemplate ToolViewTemplate { get; set; }

    public DataTemplate DocumentViewTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
      if (item is ToolViewModel)
        return ToolViewTemplate;

      if (item is DocumentViewModel)
        return DocumentViewTemplate;

      return base.SelectTemplate(item, container);
    }
  }
}
