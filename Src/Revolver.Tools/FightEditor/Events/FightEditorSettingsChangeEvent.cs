using Magnum.Core.Services;
using Magnum.Core.ViewModels;
using Microsoft.Practices.Prism.Events;
using Revolver.Tools.FightEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revolver.Tools.FightEditor.Events
{
  /// <summary>
  /// Class FightEditorSettingsChangeEvent - This event happens when the fight editor tool setting is changed.
  /// </summary>
  public class FightEditorSettingsChangeEvent : CompositePresentationEvent<FightEditorViewModel>
  {
  }
}
