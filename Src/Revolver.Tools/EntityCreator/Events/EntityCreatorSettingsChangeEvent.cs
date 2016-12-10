using Magnum.Core.Services;
using Magnum.Core.ViewModels;
using Microsoft.Practices.Prism.Events;
using Revolver.Tools.EntityCreator.ViewModels;
using Revolver.Tools.Viewer.ViewModels;
using Revolver.Tools.WorldEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revolver.Tools.EntityCreator.Events
{
  /// <summary>
  /// Class EntityCreatorSettingsChangeEvent - This event happens when the entity creator tool setting is changed.
  /// </summary>
  public class EntityCreatorSettingsChangeEvent : CompositePresentationEvent<EntityCreatorViewModel>
  {
  }
}
