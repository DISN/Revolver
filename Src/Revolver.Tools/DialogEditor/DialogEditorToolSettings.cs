using Magnum.Core.Services;
using Magnum.Core.ViewModels;
using Magnum.Core.Windows;
using Microsoft.Practices.Prism.Events;
using Revolver.Tools.FightEditor.Events;
using Revolver.Tools.FightEditor.Models;
using Revolver.Tools.FightEditor.ViewModels;
using Revolver.Tools.DialogEditor.Events;
using Revolver.Tools.DialogEditor.Models;
using Revolver.Tools.DialogEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revolver.Tools.DialogEditor
{
  public class DialogEditorToolSettings : ToolSettings
  {
    public DialogEditorToolSettings(IEventAggregator eventAggregator)
      : base()
    {
      eventAggregator.GetEvent<DialogEditorSettingsChangeEvent>().Subscribe(SaveToolSettings);
    }

    public void SaveToolSettings(DialogEditorViewModel dialogEditorViewModel)
    {
      DataPaneWidth = (dialogEditorViewModel.Model as DialogEditorModel).DataPaneWidth;
      base.SaveToolSettings(dialogEditorViewModel);
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
