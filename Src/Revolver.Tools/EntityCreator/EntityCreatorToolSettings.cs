using Magnum.Core.Services;
using Magnum.Core.ViewModels;
using Magnum.Core.Windows;
using Microsoft.Practices.Prism.Events;
using Revolver.Tools.EntityCreator.Events;
using Revolver.Tools.EntityCreator.Models;
using Revolver.Tools.EntityCreator.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revolver.Tools.EntityCreator
{
  public class EntityCreatorToolSettings : ToolSettings
  {
    public EntityCreatorToolSettings(IEventAggregator eventAggregator)
      : base()
    {
      eventAggregator.GetEvent<EntityCreatorSettingsChangeEvent>().Subscribe(SaveToolSettings);
    }

    public void SaveToolSettings(EntityCreatorViewModel patternCreatorViewModel)
    {
      base.SaveToolSettings(patternCreatorViewModel);
    }
    
    [UserScopedSetting()]
    [DefaultSettingValue("Center")]
    public override PaneLocation PreferredLocation
    {
      get { return (PaneLocation)this["PreferredLocation"]; }
      set { this["PreferredLocation"] = value; }
    }
  }
}
