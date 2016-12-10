using Magnum.Core.Services;
using Magnum.Core.ViewModels;
using Microsoft.Practices.Prism.Events;
using Revolver.Tools.BehaviorTreeEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revolver.Tools.BehaviorTreeEditor.Events
{
  /// <summary>
  /// Class BehaviorTreeEditorSettingsChangeEvent - This event happens when the behavior tree editor tool setting is changed.
  /// </summary>
  public class BehaviorTreeEditorSettingsChangeEvent : CompositePresentationEvent<BehaviorTreeEditorViewModel>
  {
  }
}
