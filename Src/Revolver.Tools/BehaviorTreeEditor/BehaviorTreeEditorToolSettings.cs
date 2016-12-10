using Magnum.Core.Services;
using Magnum.Core.ViewModels;
using Magnum.Core.Windows;
using Microsoft.Practices.Prism.Events;
using Revolver.Tools.BehaviorTreeEditor.Events;
using Revolver.Tools.BehaviorTreeEditor.Models;
using Revolver.Tools.BehaviorTreeEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revolver.Tools.BehaviorTreeEditor
{
  public class BehaviorTreeEditorToolSettings : ToolSettings
  {
    public BehaviorTreeEditorToolSettings(IEventAggregator eventAggregator)
      : base()
    {
      eventAggregator.GetEvent<BehaviorTreeEditorSettingsChangeEvent>().Subscribe(SaveToolSettings);
    }

    public void SaveToolSettings(BehaviorTreeEditorViewModel behaviorTreeEditorViewModel)
    {
      DataPaneWidth = (behaviorTreeEditorViewModel.Model as BehaviorTreeEditorModel).DataPaneWidth;
      base.SaveToolSettings(behaviorTreeEditorViewModel);
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
