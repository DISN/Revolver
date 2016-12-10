using Magnum.Core.Services;
using Magnum.Core.ViewModels;
using Microsoft.Practices.Prism.Events;
using Revolver.Tools.FeedbackEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revolver.Tools.FeedbackEditor.Events
{
  /// <summary>
  /// Class FeedbackEditorSettingsChangeEvent - This event happens when the feedback editor tool setting is changed.
  /// </summary>
  public class FeedbackEditorSettingsChangeEvent : CompositePresentationEvent<FeedbackEditorViewModel>
  {
  }
}
