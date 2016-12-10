using Engine.Wrappers.Services;
using Magnum.Core.Views;
using Revolver.Tools.WorldEditor.Models;
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
using Microsoft.Practices.Unity;

namespace Revolver.Tools.WorldEditor.Views
{
  /// <summary>
  /// Interaction logic for WorldEditorView.xaml
  /// </summary>
  public partial class WorldEditorView : UserControl, IDocumentView
  {
    #region Fields
    WorldEditorModel _model;
    #endregion

    public WorldEditorView()
    {
      InitializeComponent();
    }

    private void WorldEditorView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      _model = DataContext as WorldEditorModel;
    }
  }
}
