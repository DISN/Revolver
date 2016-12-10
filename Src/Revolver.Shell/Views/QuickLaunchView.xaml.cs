using Magnum.Controls.SearchBox;
using Magnum.Core.Services;
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
  /// Interaction logic for QuickLaunchView.xaml
  /// </summary>
  public partial class QuickLaunchView : UserControl
  {
    #region Fields
    QuickLaunchViewModel _viewModel;
    #endregion

    public QuickLaunchView()
    {
      InitializeComponent();
    }

    public QuickLaunchListBox ListBox
    {
      get { return _ListBox as QuickLaunchListBox; }
    }

    private void QuickLaunchView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      _viewModel = DataContext as QuickLaunchViewModel;
    }
  }
}
