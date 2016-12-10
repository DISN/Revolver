using Engine.System.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;
using Engine.Wrappers.Extensions.SaveManagerExtension;
using Magnum.IconLibrary;
using Magnum.Core.ViewModels;
using Microsoft.Practices.Unity;
using Magnum.Core.Services;

namespace Engine.Wrappers.Managers
{
  public class SaveManagerObject
  {
    private IUnityContainer _container;

    public SaveManagerObject(object objectToSave, ToolViewModel tvm, IUnityContainer container)
    {
      _container = container;

      ObjectToSave = objectToSave;
      IconSource = objectToSave.GetIconSourceByType();
      DisplayName = objectToSave.GetDisplayNameByType();
      ObjectType = objectToSave.GetDisplayNameByType();
      AssociatedEditor = tvm;
      AssociatedEditorOpenCommand = tvm.OpenCommand;
      SaveCommand = tvm.SaveCommand;
    }

    public SaveManagerObject(object objectToSave, IUnityContainer container)
    {
      _container = container;

      ObjectToSave = objectToSave;
      IconSource = objectToSave.GetIconSourceByType();
      DisplayName = objectToSave.GetDisplayNameByType();
      ObjectType = objectToSave.GetDisplayNameByType();
      AssociatedEditor = objectToSave.GetAssociatedEditorByType();
      AssociatedEditorOpenCommand = objectToSave.GetAssociatedEditorOpenCommandByToolViewModelType();
      SaveCommand = objectToSave.GetSaveCommandByType(_container);
    }

    public SaveManagerObject(ISaveable objectToSave, ToolViewModel tvm, IUnityContainer container)
    {
      _container = container;

      ObjectToSave = objectToSave;
      ObjectsToSave = objectToSave.ObjectsToSave;
      IconSource = objectToSave.GetIconSourceByType();
      DisplayName = objectToSave.Name;
      ObjectType = objectToSave.GetDisplayNameByType();
      AssociatedEditor = tvm;
      AssociatedEditorOpenCommand = tvm.OpenCommand;
      SaveCommand = tvm.SaveCommand;
    }

    public SaveManagerObject(ISaveable objectToSave, IUnityContainer container)
    {
      _container = container;

      ObjectToSave = objectToSave;
      ObjectsToSave = objectToSave.ObjectsToSave;
      IconSource = objectToSave.GetIconSourceByType();
      DisplayName = objectToSave.Name;
      ObjectType = objectToSave.GetDisplayNameByType();
      AssociatedEditor = objectToSave.GetAssociatedEditorByType();
      AssociatedEditorOpenCommand = objectToSave.GetAssociatedEditorOpenCommandByToolViewModelType();
      SaveCommand = objectToSave.GetSaveCommandByType(_container);
    }

    public ImageSource IconSource { get; set; }

    public string DisplayName { get; set; }

    public string ObjectType { get; set; }

    public ToolViewModel AssociatedEditor { get; set; }

    public object ObjectToSave { get; set; }

    public List<ISaveable> ObjectsToSave { get; set; }

    public IUndoableCommand AssociatedEditorOpenCommand { get; set; }

    public IUndoableCommand SaveCommand { get; set; }
  }
}

namespace Engine.Wrappers.Extensions.SaveManagerExtension
{
  public static class SaveManagerExtension
  {
    public static string GetDisplayNameByType(this object objectToSave)
    {
      string name = "Unknown";
      if (objectToSave.GetType().ToString().Contains("Map"))
        name = "Map";
      return name;
    }

    public static ToolViewModel GetAssociatedEditorByType(this object objectToSave)
    {
      return null;
    }

    public static IUndoableCommand GetAssociatedEditorOpenCommandByToolViewModelType(this object objectToSave)
    {
      return null;
    }

    public static IUndoableCommand GetSaveCommandByType(this object objectToSave, IUnityContainer container)
    {
      IUndoableCommand saveCommand = null;
      var commandManager = container.Resolve<ICommandManager>();
      if (commandManager != null)
      {
        if (objectToSave.GetType().ToString().Contains("Map"))
          saveCommand = commandManager.GetCommand("SAVEMAP");
      }
      return saveCommand;
    }

    public static ImageSource GetIconSourceByType(this object objectToSave)
    {
      ImageSource icon = BitmapImages.LoadBitmapFromIconType(IconType.None);
      if (objectToSave.GetType().ToString().Contains("Map"))
        icon = BitmapImages.LoadBitmapFromIconType(IconType.Map);
      return icon;
    }

    public static string GetDisplayNameByType(this ISaveable objectToSave)
    {
      string name = "Unknown";
      if (objectToSave.GetType().ToString().Contains("Map"))
        name = "Map";
      return name;
    }

    public static ToolViewModel GetAssociatedEditorByType(this ISaveable objectToSave)
    {
      return null;
    }

    public static IUndoableCommand GetAssociatedEditorOpenCommandByToolViewModelType(this ISaveable objectToSave)
    {
      return null;
    }

    public static IUndoableCommand GetSaveCommandByType(this ISaveable objectToSave, IUnityContainer container)
    {
      IUndoableCommand saveCommand = null;
      var commandManager = container.Resolve<ICommandManager>();
      if (commandManager != null)
      {
        if (objectToSave.GetType().ToString().Contains("Map"))
          saveCommand = commandManager.GetCommand("SAVEMAP");
      }
      return saveCommand;
    }
    
    public static ImageSource GetIconSourceByType(this ISaveable objectToSave)
    {
      ImageSource icon = BitmapImages.LoadBitmapFromIconType(IconType.None);
      if (objectToSave.GetType().ToString().Contains("Map"))
        icon = BitmapImages.LoadBitmapFromIconType(IconType.Map);
      return icon;
    }
  }
}
