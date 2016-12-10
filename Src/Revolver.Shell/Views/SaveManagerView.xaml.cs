using Engine.Wrappers.Managers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
  /// Interaction logic for SaveManagerView.xaml
  /// </summary>
  public partial class SaveManagerView : UserControl
  {
    #region Fields
    SaveManager _viewModel;
    #endregion

    public SaveManagerView()
    {
      InitializeComponent();
    }

    private void SaveManagerView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      _viewModel = DataContext as SaveManager;
    }

    private void BTNSaveAll_Click(object sender, RoutedEventArgs e)
    {
      List<SaveManagerObject> temp = new List<SaveManagerObject>(_viewModel.ObjectsToSave);
      foreach (SaveManagerObject item in temp)
        item.SaveCommand.Execute(null);
      _viewModel.Clear();
    }
  }
}
