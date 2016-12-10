using Magnum.Core.Services;
using Magnum.Core.ViewModels;
using Microsoft.Practices.Prism.Events;
using Revolver.Tools.PatternCreator.ViewModels;
using Revolver.Tools.Viewer.ViewModels;
using Revolver.Tools.WorldEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revolver.Tools.PatternCreator.Events
{
  /// <summary>
  /// Class PatternCreatorSettingsChangeEvent - This event happens when the pattern creator tool setting is changed.
  /// </summary>
  public class PatternCreatorSettingsChangeEvent : CompositePresentationEvent<PatternCreatorViewModel>
  {
  }
}
