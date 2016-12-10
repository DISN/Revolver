using Magnum.Core.Services;
using Magnum.Core.ViewModels;
using Magnum.Core.Windows;
using Microsoft.Practices.Prism.Events;
using Revolver.Tools.TileEditor.Events;
using Revolver.Tools.TileEditor.Models;
using Revolver.Tools.TileEditor.ViewModels;
using Revolver.Tools.Viewer.Events;
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

namespace Revolver.Tools.TileEditor
{
  public class TileEditorToolSettings : ToolSettings
  {
    public TileEditorToolSettings(IEventAggregator eventAggregator)
      : base()
    {
      eventAggregator.GetEvent<TileEditorSettingsChangeEvent>().Subscribe(SaveToolSettings);
    }

    public void SaveToolSettings(TileEditorViewModel tileEditorViewModel)
    {
      LastLoadedResource = (tileEditorViewModel.Model as TileEditorModel).LastLoadedResource;
      base.SaveToolSettings(tileEditorViewModel);
    }

    [UserScopedSetting()]
    [DefaultSettingValue("")]
    public string LastLoadedResource
    {
      get { return (string)this["LastLoadedResource"]; }
      set { this["LastLoadedResource"] = value; }
    }

    [UserScopedSetting()]
    [DefaultSettingValue("390")]
    public override double PreferredWidth
    {
      get { return (double)this["PreferredWidth"]; }
      set { this["PreferredWidth"] = value; }
    }

    [UserScopedSetting()]
    [DefaultSettingValue("Right")]
    public override PaneLocation PreferredLocation
    {
      get { return (PaneLocation)this["PreferredLocation"]; }
      set { this["PreferredLocation"] = value; }
    }
  }
}
