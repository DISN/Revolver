using Magnum.Controls.SearchBox;
using Magnum.Core.Utils;
using Magnum.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Revolver.Shell.Views
{
  /// <summary>
  /// Interaction logic for ToolsLibraryView.xaml
  /// </summary>
  public partial class ToolsLibraryView : UserControl
  {
    #region Fields
    ToolsLibraryViewModel _viewModel;
    #endregion

    public ToolsLibraryView()
    {
      InitializeComponent();
    }

    private void ToolsLibraryView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      _viewModel = DataContext as ToolsLibraryViewModel;
    }

    private void _SearchBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
      var assets = (ListCollectionView)_ListBox.ItemsSource;

      switch (e.Key)
      {
        case Key.Up:
          assets.MoveCurrentToPrevious();
          if (assets.IsCurrentBeforeFirst)
            assets.MoveCurrentToFirst();
          e.Handled = true;
          break;
        case Key.Down:
          assets.MoveCurrentToNext();
          if (assets.IsCurrentAfterLast)
            assets.MoveCurrentToLast();
          _ListBox.ScrollIntoView(assets.CurrentItem);
          e.Handled = true;
          break;
        case Key.Tab:
          e.Handled = true;
          break;
        case Key.Enter:
          var currentItem = assets.CurrentItem as ToolViewModel;

          if (currentItem != null)
          {
            currentItem.OpenCommand.Execute(null);
            e.Handled = true;
          }
          break;
      }
    }

    private void _SearchBox_Search(object sender, RoutedEventArgs e)
    {
      _viewModel.FilterText = _SearchBox.Text;
    }

    public void FocusSearchBox()
    {
      _viewModel.FilterText = String.Empty;
      _SearchBox.Text = String.Empty;
      _SearchBox.Focus();
    }
  }
}
