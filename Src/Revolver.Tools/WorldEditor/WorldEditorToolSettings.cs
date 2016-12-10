using Magnum.Core.Services;
using Magnum.Core.ViewModels;
using Magnum.Core.Windows;
using Microsoft.Practices.Prism.Events;
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

namespace Revolver.Tools.WorldEditor
{
  public class WorldEditorToolSettings : ToolSettings
  {
    public WorldEditorToolSettings(IEventAggregator eventAggregator)
      : base()
    {
      eventAggregator.GetEvent<WorldEditorSettingsChangeEvent>().Subscribe(SaveToolSettings);
    }

    public void SaveToolSettings(WorldEditorViewModel worldEditorViewModel)
    {
      LastLoadedGame = (worldEditorViewModel.Model as WorldEditorModel).LastLoadedGame;
      base.SaveToolSettings(worldEditorViewModel);
    }

    [UserScopedSetting()]
    [DefaultSettingValue("")]
    public string LastLoadedGame
    {
      get { return (string)this["LastLoadedGame"]; }
      set { this["LastLoadedGame"] = value; }
    }

    [UserScopedSetting()]
    [DefaultSettingValue("Left")]
    public override PaneLocation PreferredLocation
    {
      get { return (PaneLocation)this["PreferredLocation"]; }
      set { this["PreferredLocation"] = value; }
    }
  }
}
