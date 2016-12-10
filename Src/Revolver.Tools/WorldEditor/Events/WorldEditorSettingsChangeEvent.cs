using Magnum.Core.Services;
using Magnum.Core.ViewModels;
using Microsoft.Practices.Prism.Events;
using Revolver.Tools.WorldEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revolver.Tools.WorldEditor.Events
{
  /// <summary>
  /// Class WorldEditorSettingsChangeEvent - This event happens when the world editor tool setting is changed.
  /// </summary>
  public class WorldEditorSettingsChangeEvent : CompositePresentationEvent<WorldEditorViewModel>
  {
  }
}
