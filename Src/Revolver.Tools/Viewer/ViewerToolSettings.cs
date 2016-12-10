using Magnum.Core.ViewModels;
using Magnum.Core.Windows;
using Microsoft.Practices.Prism.Events;
using Revolver.Tools.Viewer.Events;
using Revolver.Tools.Viewer.Models;
using Revolver.Tools.Viewer.ViewModels;
using Revolver.Tools.WorldEditor.Events;
using Revolver.Tools.WorldEditor.Models;
using Revolver.Tools.WorldEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revolver.Tools.Viewer
{
  public class ViewerToolSettings : ToolSettings
  {
    public ViewerToolSettings(IEventAggregator eventAggregator)
      : base()
    {
      eventAggregator.GetEvent<ViewerSettingsChangeEvent>().Subscribe(SaveToolSettings);
    }

    public void SaveToolSettings(ViewerViewModel viewerViewModel)
    {
      LastLoadedMap = (viewerViewModel.Model as ViewerModel).LastLoadedMap;
      IsGridActivated = (viewerViewModel.Model as ViewerModel).Viewer.IsGridActivated;
      LastRatio = (viewerViewModel.Model as ViewerModel).LastRatio;
      base.SaveToolSettings(viewerViewModel);
    }

    [UserScopedSetting()]
    [DefaultSettingValue("")]
    public string LastLoadedMap
    {
      get { return (string)this["LastLoadedMap"]; }
      set { this["LastLoadedMap"] = value; }
    }

    [UserScopedSetting()]
    [DefaultSettingValue("false")]
    public bool IsGridActivated
    {
      get { return (bool)this["IsGridActivated"]; }
      set { this["IsGridActivated"] = value; }
    }

    [UserScopedSetting()]
    [DefaultSettingValue("_169")]
    public Revolver.Tools.Viewer.Models.ViewerModel.AspectRatios LastRatio
    {
      get { return (Revolver.Tools.Viewer.Models.ViewerModel.AspectRatios)this["LastRatio"]; }
      set { this["LastRatio"] = value; }
    }
  }
}
