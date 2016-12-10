using Magnum.Core.Views;
using Revolver.Tools.Viewer.Models;
using Revolver.Tools.WorldEditor.ViewModels;
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

namespace Revolver.Tools.Viewer.Views
{
  /// <summary>
  /// Interaction logic for ViewerView.xaml
  /// </summary>
  public partial class ViewerView : UserControl, IDocumentView
  {
    #region Fields
    ViewerModel _model;
    bool _initialized = false;
    #endregion

    public ViewerView()
    {
      InitializeComponent();
    }

    private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      if (_model != null)
      {
        _model.ViewerControlActualWidth = this.ActualWidth;
        _model.ViewerControlActualHeight = this.ActualHeight;
        _viewerStackPanel.Width = this.ActualWidth;
        _viewerStackPanel.Height = this.ActualHeight;

        if (!_initialized)
        {
          _model.ToggleRatioInit();
          _initialized = true;
        }
        else
          _model.RatioChanged(null);
      }
    }

    private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      _model = DataContext as ViewerModel;
    }
  }
}
