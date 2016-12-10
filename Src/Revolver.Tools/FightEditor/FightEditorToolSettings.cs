using Magnum.Core.Services;
using Magnum.Core.ViewModels;
using Magnum.Core.Windows;
using Microsoft.Practices.Prism.Events;
using Revolver.Tools.FightEditor.Events;
using Revolver.Tools.FightEditor.Models;
using Revolver.Tools.FightEditor.ViewModels;
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

namespace Revolver.Tools.FightEditor
{
  public class FightEditorToolSettings : ToolSettings
  {
    public FightEditorToolSettings(IEventAggregator eventAggregator)
      : base()
    {
      eventAggregator.GetEvent<FightEditorSettingsChangeEvent>().Subscribe(SaveToolSettings);
    }

    public void SaveToolSettings(FightEditorViewModel fightEditorViewModel)
    {
      DataPaneWidth = (fightEditorViewModel.Model as FightEditorModel).DataPaneWidth;
      ContextualPaneHeight = (fightEditorViewModel.Model as FightEditorModel).ContextualPaneHeight;
      base.SaveToolSettings(fightEditorViewModel);
    }

    [UserScopedSetting()]
    [DefaultSettingValue("350")]
    public double DataPaneWidth
    {
      get { return (double)this["DataPaneWidth"]; }
      set { this["DataPaneWidth"] = value; }
    }

    [UserScopedSetting()]
    [DefaultSettingValue("300")]
    public double ContextualPaneHeight
    {
      get { return (double)this["ContextualPaneHeight"]; }
      set { this["ContextualPaneHeight"] = value; }
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
