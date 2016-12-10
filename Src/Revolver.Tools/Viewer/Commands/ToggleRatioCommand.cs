using Magnum.Core.Services;
using Revolver.Tools.Viewer.Models;
using Revolver.Tools.Viewer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AspectRatios = Revolver.Tools.Viewer.Models.ViewerModel.AspectRatios;

namespace Revolver.Tools.Viewer.Commands
{
  public class ToggleRatioCommand : UndoableCommand<object>
  {
    AspectRatios _previousAspectRatio;
    AspectRatios _selectedAspectRatio;

    public ToggleRatioCommand(AspectRatios selectedAspectRatio, ViewerModel model)
      : base(model, true, true)
    {
      this._selectedAspectRatio = selectedAspectRatio;

      this._executeMethod = new Action<object>(ToggleRatio);
      this._undoMethod = new Action<object>(RetoggleLastRatio);
      this._canExecuteMethod = new Func<object, bool>(CanChangeRatio);
    }

    public ViewerModel Model { get { return _model as ViewerModel; } }

    public override bool Initialize()
    {
      _previousAspectRatio = Model.LastRatio;

      return true;
    }

    public void ToggleRatio(object parameter)
    {
      if (_selectedAspectRatio == AspectRatios._Full)
      {
        Model.LastRatio = AspectRatios._Full;
        Model.AspectRatioWidth = (int)Model.ViewerControlActualWidth;
        Model.AspectRatioHeight = (int)Model.ViewerControlActualHeight;
        float aspectRatio = Model.AspectRatioWidth > Model.AspectRatioHeight ? (float)(Model.AspectRatioWidth / Model.AspectRatioHeight) : (float)(Model.AspectRatioHeight / Model.AspectRatioWidth);
        Model.Viewer.GameManager.ToggleRatio(aspectRatio, (float)Model.AspectRatioWidth > Model.AspectRatioHeight ? Model.AspectRatioWidth : Model.AspectRatioHeight, (float)Model.AspectRatioHeight < Model.AspectRatioWidth ? Model.AspectRatioHeight : Model.AspectRatioWidth);
      }
      if (_selectedAspectRatio == AspectRatios._43)
      {
        Model.LastRatio = AspectRatios._43;
        Model.AspectRatioWidth = 1920;
        Model.AspectRatioHeight = 1440;
        Model.Viewer.GameManager.ToggleRatio((float)(Model.AspectRatioWidth / Model.AspectRatioHeight), (float)Model.AspectRatioWidth, (float)Model.AspectRatioHeight);
      }
      if (_selectedAspectRatio == AspectRatios._54)
      {
        Model.LastRatio = AspectRatios._54;
        Model.AspectRatioWidth = 1280;
        Model.AspectRatioHeight = 1024;
        Model.Viewer.GameManager.ToggleRatio((float)(Model.AspectRatioWidth / Model.AspectRatioHeight), (float)Model.AspectRatioWidth, (float)Model.AspectRatioHeight);
      }
      if (_selectedAspectRatio == AspectRatios._169)
      {
        Model.LastRatio = AspectRatios._169;
        Model.AspectRatioWidth = 1920;
        Model.AspectRatioHeight = 1080;
        Model.Viewer.GameManager.ToggleRatio((float)(Model.AspectRatioWidth / Model.AspectRatioHeight), (float)Model.AspectRatioWidth, (float)Model.AspectRatioHeight);
      }
      if (_selectedAspectRatio == AspectRatios._1610)
      {
        Model.LastRatio = AspectRatios._1610;
        Model.AspectRatioWidth = 1920;
        Model.AspectRatioHeight = 1200;
        Model.Viewer.GameManager.ToggleRatio((float)(Model.AspectRatioWidth / Model.AspectRatioHeight), (float)Model.AspectRatioWidth, (float)Model.AspectRatioHeight);
      }

      Model.SetupViewport();
    }

    public bool CanChangeRatio(object parameter)
    {
      return Model.IsViewerLoaded;
    }

    public void RetoggleLastRatio(object parameter)
    {
      if (_previousAspectRatio == AspectRatios._Full)
      {
        Model.LastRatio = AspectRatios._Full;
        Model.AspectRatioWidth = (int)Model.ViewerControlActualWidth;
        Model.AspectRatioHeight = (int)Model.ViewerControlActualHeight;
        float aspectRatio = Model.AspectRatioWidth > Model.AspectRatioHeight ? (float)(Model.AspectRatioWidth / Model.AspectRatioHeight) : (float)(Model.AspectRatioHeight / Model.AspectRatioWidth);
        Model.Viewer.GameManager.ToggleRatio(aspectRatio, (float)Model.AspectRatioWidth > Model.AspectRatioHeight ? Model.AspectRatioWidth : Model.AspectRatioHeight, (float)Model.AspectRatioHeight < Model.AspectRatioWidth ? Model.AspectRatioHeight : Model.AspectRatioWidth);
      }
      if (_previousAspectRatio == AspectRatios._43)
      {
        Model.LastRatio = AspectRatios._43;
        Model.AspectRatioWidth = 1920;
        Model.AspectRatioHeight = 1440;
        Model.Viewer.GameManager.ToggleRatio((float)(Model.AspectRatioWidth / Model.AspectRatioHeight), (float)Model.AspectRatioWidth, (float)Model.AspectRatioHeight);
      }
      if (_previousAspectRatio == AspectRatios._54)
      {
        Model.LastRatio = AspectRatios._54;
        Model.AspectRatioWidth = 1280;
        Model.AspectRatioHeight = 1024;
        Model.Viewer.GameManager.ToggleRatio((float)(Model.AspectRatioWidth / Model.AspectRatioHeight), (float)Model.AspectRatioWidth, (float)Model.AspectRatioHeight);
      }
      if (_previousAspectRatio == AspectRatios._169)
      {
        Model.LastRatio = AspectRatios._169;
        Model.AspectRatioWidth = 1920;
        Model.AspectRatioHeight = 1080;
        Model.Viewer.GameManager.ToggleRatio((float)(Model.AspectRatioWidth / Model.AspectRatioHeight), (float)Model.AspectRatioWidth, (float)Model.AspectRatioHeight);
      }
      if (_previousAspectRatio == AspectRatios._1610)
      {
        Model.LastRatio = AspectRatios._1610;
        Model.AspectRatioWidth = 1920;
        Model.AspectRatioHeight = 1200;
        Model.Viewer.GameManager.ToggleRatio((float)(Model.AspectRatioWidth / Model.AspectRatioHeight), (float)Model.AspectRatioWidth, (float)Model.AspectRatioHeight);
      }

      Model.SetupViewport();
    }
  }
}
