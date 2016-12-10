using Magnum.Core.Services;
using Magnum.Core.ViewModels;
using Microsoft.Practices.Prism.Events;
using Revolver.Tools.TileEditor.ViewModels;
using Revolver.Tools.Viewer.ViewModels;
using Revolver.Tools.WorldEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revolver.Tools.TileEditor.Events
{
  /// <summary>
  /// Class TileEditorSettingsChangeEvent - This event happens when the tile editor tool setting is changed.
  /// </summary>
  public class TileEditorSettingsChangeEvent : CompositePresentationEvent<TileEditorViewModel>
  {
  }
}
