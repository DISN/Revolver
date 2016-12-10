using Magnum.Core.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revolver.Tools.TileEditor.Models
{
  public interface ITileEditor : IToolWindow
  {
    /// <summary>
    /// Closes the tile editor.
    /// </summary>
    void CloseTileEditor(object info);
  }
}
