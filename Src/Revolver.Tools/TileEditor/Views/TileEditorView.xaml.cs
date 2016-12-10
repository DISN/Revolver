using Magnum.Core.Views;
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

namespace Revolver.Tools.TileEditor.Views
{
  /// <summary>
  /// Interaction logic for TileEditorView.xaml
  /// </summary>
  public partial class TileEditorView : UserControl, IDocumentView
  {
    public TileEditorView()
    {
      InitializeComponent();
    }
  }
}
