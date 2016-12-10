using Engine.System.Interfaces;
using Engine.Wrappers.Services;
using Magnum.Core.ViewModels;
using Magnum.Core.Windows;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Wrappers.Managers
{
  public sealed class SelectionManager : ViewModelBase, ISelectionManagerService
  {
    #region Fields
    private IUnityContainer _container;
    private IEventAggregator _eventAggregator;
    private object _selectedObject;
    private List<object> _selectedObjects;
    #endregion

    #region Constructors
    /// <summary>
    /// Constructor of Selection
    /// </summary>
    /// <param name="container">The injected container of the application</param>
    public SelectionManager(IUnityContainer container, IEventAggregator eventAggregator)
    {
      _container = container;
      _eventAggregator = eventAggregator;
      _selectedObjects = new List<object>();
    }
    #endregion

    #region Properties
    public event EventHandler OnSelectedObjectChanged;
    public object SelectedObject
    {
      get
      {
        return _selectedObject;
      }
      set
      {
        if (_selectedObject != value)
        {
          _selectedObject = value;
          RaisePropertyChanged("SelectedObject");
          SelectedObjectChanged();
        }
      }
    }

    public event EventHandler OnSelectedObjectsChanged;
    public List<object> SelectedObjects
    {
      get
      {
        return _selectedObjects;
      }
      set
      {
        if (_selectedObjects != value)
        {
          _selectedObjects = value;
          RaisePropertyChanged("SelectedObjects");
          SelectedObjectsChanged();
        }
      }
    }
    #endregion

    #region Methods
    public void SelectObject(ISelectable objectToSelect)
    {
      SelectedObject = objectToSelect.Selection;
    }

    public void SelectObject(object objectToSelect)
    {
      SelectedObject = objectToSelect;
    }

    public void SelectObjects(List<ISelectable> objectsToSelect)
    {
      SelectedObjects.Clear();
      foreach (ISelectable objectToAdd in objectsToSelect)
          SelectedObjects.Add(objectToAdd.Selection);
      SelectFirstObjectFromSelectedObjects();
    }

    public void SelectObjects(List<object> objectsToSelect)
    {
      SelectedObjects = objectsToSelect;
      SelectFirstObjectFromSelectedObjects();
    }

    public void SelectObjects(params ISelectable[] objectsToSelect)
    {
      SelectedObjects.Clear();
      SelectedObjects.AddRange(objectsToSelect);
      SelectFirstObjectFromSelectedObjects();
    }

    public void SelectObjects(params object[] objectsToSelect)
    {
      SelectedObjects.Clear();
      SelectedObjects.AddRange(objectsToSelect);
      SelectFirstObjectFromSelectedObjects();
    }

    /// <summary>
    /// Selects the first object from the list SelectedObjects.
    /// The first one should be the very first object clicked during multi-selection, for example.
    /// </summary>
    private void SelectFirstObjectFromSelectedObjects()
    {
      if (SelectedObjects.Count > 0)
        SelectedObject = SelectedObjects[0];
    }
    #endregion

    #region Events
    private void SelectedObjectChanged()
    {
      // Shows the properties grid, if not already done
      IWorkspace workspace = _container.Resolve<WorkspaceBase>();
      ToolViewModel propertyGrid = workspace.Tools.First(f => f.ContentId == "Properties");
      if (propertyGrid != null && !propertyGrid.IsVisible)
      {
        propertyGrid.IsVisible = true;
        propertyGrid.IsSelected = true;
      }

      // Call the other assigned events from other classes
      var handler = OnSelectedObjectChanged;
      if (handler != null)
        handler(this, EventArgs.Empty);
    }

    private void SelectedObjectsChanged()
    {
      // Shows the properties grid, if not already done
      IWorkspace workspace = _container.Resolve<WorkspaceBase>();
      ToolViewModel propertyGrid = workspace.Tools.First(f => f.ContentId == "Properties");
      if (propertyGrid != null && !propertyGrid.IsVisible)
      {
        propertyGrid.IsVisible = true;
        propertyGrid.IsSelected = true;
      }

      // Call the other assigned events from other classes
      var handler = OnSelectedObjectsChanged;
      if (handler != null)
        handler(this, EventArgs.Empty);
    }
    #endregion
  }
}
