using Magnum.Core.Services;
using Magnum.Core.ViewModels;
using Microsoft.Practices.Prism.Events;
using Revolver.Tools.DialogEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revolver.Tools.DialogEditor.Events
{
  /// <summary>
  /// Class DialogEditorSettingsChangeEvent - This event happens when the dialog editor tool setting is changed.
  /// </summary>
  public class DialogEditorSettingsChangeEvent : CompositePresentationEvent<DialogEditorViewModel>
  {
  }
}
