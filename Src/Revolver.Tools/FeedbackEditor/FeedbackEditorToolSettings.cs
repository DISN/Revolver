using Magnum.Core.Services;
using Magnum.Core.ViewModels;
using Magnum.Core.Windows;
using Microsoft.Practices.Prism.Events;
using Revolver.Tools.FeedbackEditor.Events;
using Revolver.Tools.FeedbackEditor.Models;
using Revolver.Tools.FeedbackEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revolver.Tools.FeedbackEditor
{
  public class FeedbackEditorToolSettings : ToolSettings
  {
    public FeedbackEditorToolSettings(IEventAggregator eventAggregator)
      : base()
    {
      eventAggregator.GetEvent<FeedbackEditorSettingsChangeEvent>().Subscribe(SaveToolSettings);
    }

    public void SaveToolSettings(FeedbackEditorViewModel feedbackEditorViewModel)
    {
      DataPaneWidth = (feedbackEditorViewModel.Model as FeedbackEditorModel).DataPaneWidth;
      base.SaveToolSettings(feedbackEditorViewModel);
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
