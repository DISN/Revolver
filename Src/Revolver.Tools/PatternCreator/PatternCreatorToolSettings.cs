using Magnum.Core.Services;
using Magnum.Core.ViewModels;
using Magnum.Core.Windows;
using Microsoft.Practices.Prism.Events;
using Revolver.Tools.PatternCreator.Events;
using Revolver.Tools.PatternCreator.Models;
using Revolver.Tools.PatternCreator.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revolver.Tools.PatternCreator
{
  public class PatternCreatorToolSettings : ToolSettings
  {
    public PatternCreatorToolSettings(IEventAggregator eventAggregator)
      : base()
    {
      eventAggregator.GetEvent<PatternCreatorSettingsChangeEvent>().Subscribe(SaveToolSettings);
    }

    public void SaveToolSettings(PatternCreatorViewModel patternCreatorViewModel)
    {
      DataPaneWidth = (patternCreatorViewModel.Model as PatternCreatorModel).DataPaneWidth;
      base.SaveToolSettings(patternCreatorViewModel);
    }

    [UserScopedSetting()]
    [DefaultSettingValue("300")]
    public double DataPaneWidth
    {
      get { return (double)this["DataPaneWidth"]; }
      set { this["DataPaneWidth"] = value; }
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
