using Magnum.Core.Services;
using Magnum.Core.ViewModels;
using Microsoft.Practices.Prism.Events;
using Revolver.Tools.Viewer.ViewModels;
using Revolver.Tools.WorldEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revolver.Tools.Viewer.Events
{
  /// <summary>
  /// Class ViewerSettingsChangeEvent - This event happens when the viewer tool setting is changed.
  /// </summary>
  public class ViewerSettingsChangeEvent : CompositePresentationEvent<ViewerViewModel>
  {
  }
}
