using Magnum.Core.Models;
using Magnum.Core.Services;
using Magnum.Core.ViewModels;
using Magnum.Core.Views;
using Revolver.Tools.Viewer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.AvalonDock.Layout;

namespace Revolver.Shell.AvalonDock
{
  public class AvalonDockLayoutInitializer : ILayoutUpdateStrategy
  {
    private enum InsertPosition
    {
      Start,
      End
    }

    private static string GetPaneName(PaneLocation location)
    {
      switch (location)
      {
        case PaneLocation.Left:
          return "LeftPane";
        case PaneLocation.Right:
          return "RightPane";
        case PaneLocation.Bottom:
          return "BottomPane";
        case PaneLocation.Center:
          return "CenterPane";
        default:
          throw new ArgumentOutOfRangeException("location");
      }
    }

    private static LayoutAnchorablePane CreateAnchorablePane(LayoutRoot layout, Orientation orientation,
            string paneName, InsertPosition position)
    {
      var parent = layout.Descendents().OfType<LayoutPanel>().First(d => d.Orientation == orientation);
      var toolsPane = new LayoutAnchorablePane { Name = paneName };
      if (position == InsertPosition.Start)
        parent.InsertChildAt(0, toolsPane);
      else
        parent.Children.Add(toolsPane);
      return toolsPane;
    }

    #region ILayoutUpdateStrategy Members
    public bool BeforeInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableToShow,
                                       ILayoutContainer destinationContainer)
    {
      var tool = anchorableToShow.Content as ToolViewModel;
      if (tool != null)
      {
        var preferredLocation = tool.PreferredLocation;
        string paneName = GetPaneName(preferredLocation);

        if (paneName == "CenterPane")
        {
          var documentPane = layout.Descendents().OfType<LayoutDocumentPane>().FirstOrDefault();
          if ((tool is ViewerViewModel && !documentPane.ContainsChildOfType<ViewerViewModel>()) ||
              (!tool.CanOpenMultipleInstances && !documentPane.ContainsChildOfType<ToolViewModel>()))
          {
            documentPane.InsertChildAt(0, anchorableToShow);
          }
          else
          {
            documentPane.InsertChildAt(0, anchorableToShow);
          }
        }
        else
        {
          var toolsPane = layout.Descendents().OfType<LayoutAnchorablePane>().FirstOrDefault(d => d.Name == paneName);
          if (toolsPane == null)
          {
            switch (preferredLocation)
            {
              case PaneLocation.Left:
                toolsPane = CreateAnchorablePane(layout, Orientation.Horizontal, paneName, InsertPosition.Start);
                break;
              case PaneLocation.Right:
                toolsPane = CreateAnchorablePane(layout, Orientation.Horizontal, paneName, InsertPosition.End);
                break;
              case PaneLocation.Bottom:
                toolsPane = CreateAnchorablePane(layout, Orientation.Vertical, paneName, InsertPosition.End);
                break;
              default:
                throw new ArgumentOutOfRangeException();
            }
          }
          if (!tool.CanOpenMultipleInstances && !toolsPane.ContainsChildOfType<ToolViewModel>())
          {
            toolsPane.Children.Insert(0, anchorableToShow);
          }
          else
          {
            toolsPane.Children.Insert(0, anchorableToShow);
          }
        }
        return true;
      }

      return false;
    }


    public void AfterInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableShown)
    {
      // If this is the first anchorable added to this pane, then use the preferred size.
      var tool = anchorableShown.Content as ToolViewModel;
      if (tool != null)
      {
        var anchorablePane = anchorableShown.Parent as LayoutAnchorablePane;
        if (anchorablePane != null && anchorablePane.ChildrenCount == 1)
        {
          switch (tool.PreferredLocation)
          {
            case PaneLocation.Left:
            case PaneLocation.Right:
              anchorablePane.DockWidth = new GridLength(tool.PreferredWidth, GridUnitType.Pixel);
              break;
            case PaneLocation.Bottom:
              anchorablePane.DockHeight = new GridLength(tool.PreferredHeight, GridUnitType.Pixel);
              break;
            default:
              throw new ArgumentOutOfRangeException();
          }
        }
      }
    }


    public bool BeforeInsertDocument(LayoutRoot layout, LayoutDocument anchorableToShow,
                                     ILayoutContainer destinationContainer)
    {
      return false;
    }

    public void AfterInsertDocument(LayoutRoot layout, LayoutDocument anchorableShown)
    {
    }
    #endregion
  }
}
