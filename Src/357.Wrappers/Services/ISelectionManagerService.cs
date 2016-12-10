using Engine.System.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Wrappers.Services
{
  /// <summary>
  /// Interface ISelectionManagerService - the application's selection manager is returned by this service
  /// </summary>
  public interface ISelectionManagerService
  {
    object SelectedObject { get; set; }
    List<object> SelectedObjects { get; set; }

    void SelectObject(object objectToSelect);
    void SelectObject(ISelectable objectToSelect);
    void SelectObjects(List<ISelectable> objectsToSelect);
    void SelectObjects(List<object> objectsToSelect);
    void SelectObjects(params ISelectable[] objectsToSelect);
    void SelectObjects(params object[] objectsToSelect);

    event EventHandler OnSelectedObjectChanged;
    event EventHandler OnSelectedObjectsChanged;
  }
}
