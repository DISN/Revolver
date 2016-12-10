using Engine.System.Interfaces;
using Engine.Wrappers.Services;
using Magnum.Core.ViewModels;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Engine.Wrappers.Managers
{
  public sealed class SaveManager : ViewModelBase, ISaveManagerService
  {
    #region Fields
    private IUnityContainer _container;
    private readonly ObservableCollection<SaveManagerObject> _objectsToSave;

    #endregion

    #region Constructors
    public SaveManager(IUnityContainer container)
    {
      _container = container;
      _objectsToSave = new ObservableCollection<SaveManagerObject>();

      _ObjectsToSaveViewSource = (ListCollectionView)CollectionViewSource.GetDefaultView(_objectsToSave);
      _ObjectsToSaveViewSource.SortDescriptions.Add(new SortDescription("DisplayName", ListSortDirection.Ascending));
      _ObjectsToSaveViewSource.GroupDescriptions.Add(new PropertyGroupDescription("ObjectType"));
    }
    #endregion

    #region Properties

    private readonly ListCollectionView _ObjectsToSaveViewSource;

    public ListCollectionView ObjectsToSaveViewSource
    {
      get { return _ObjectsToSaveViewSource; }
    }

    public ObservableCollection<SaveManagerObject> ObjectsToSave
    {
      get { return _objectsToSave; }
    } 

    /// <summary>
    /// Returns the number of objects to save
    /// </summary>
    public int NumberOfObjectsToSave
    {
      get
      {
        return ObjectsToSave.Count;
      }
    }
    
    /// <summary>
    /// Returns a value indicating if we have objects to save or not
    /// </summary>
    public bool NotifySaveIsPossible
    {
      get
      {
        return ObjectsToSave.Count > 0 ? true : false;
      }
    }
    #endregion

    #region Methods
    public void Add(ISaveable objectToSave)
    {
      foreach (SaveManagerObject item in ObjectsToSave)
	    {
        if (item.ObjectToSave == objectToSave)
          return;
	    }
      ObjectsToSave.Add(new SaveManagerObject(objectToSave, _container));
      Refresh();
    }

    public void Add(ISaveable objectToSave, ToolViewModel tvm)
    {
      foreach (SaveManagerObject item in ObjectsToSave)
      {
        if (item.ObjectToSave == objectToSave)
          return;
      }
      ObjectsToSave.Add(new SaveManagerObject(objectToSave, tvm, _container));
      Refresh();
    }

    public void Add(object objectToSave)
    {
      foreach (SaveManagerObject item in ObjectsToSave)
      {
        if (item.ObjectToSave == objectToSave)
          return;
      }
      ObjectsToSave.Add(new SaveManagerObject(objectToSave, _container));
      Refresh();
    }

    public void Add(object objectToSave, ToolViewModel tvm)
    {
      foreach (SaveManagerObject item in ObjectsToSave)
      {
        if (item.ObjectToSave == objectToSave)
          return;
      }
      ObjectsToSave.Add(new SaveManagerObject(objectToSave, tvm, _container));
      Refresh();
    }

    public void Remove(ISaveable objectToRemove)
    {
      foreach (SaveManagerObject item in ObjectsToSave)
      {
        if (item.ObjectToSave == objectToRemove)
        {
          ObjectsToSave.Remove(item);
          Refresh();
          return;
        }
      }
    }

    public void Remove(object objectToRemove)
    {
      foreach (SaveManagerObject item in ObjectsToSave)
      {
        if (item.ObjectToSave == objectToRemove)
        {
          ObjectsToSave.Remove(item);
          Refresh();
          return;
        }
      }
    }

    public void Clear()
    {
      ObjectsToSave.Clear();
      Refresh();
    }

    /// <summary>
    /// Calls RaisePropertyChanged on all properties.
    /// </summary>
    public void Refresh()
    {
      RaisePropertyChanged("NumberOfObjectsToSave");
      RaisePropertyChanged("NotifySaveIsPossible");
    }
    #endregion
  }
}
