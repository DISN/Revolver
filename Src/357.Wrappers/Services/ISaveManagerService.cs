using Engine.System.Interfaces;
using Engine.Wrappers.Managers;
using Magnum.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Wrappers.Services
{
  /// <summary>
  /// Interface ISaveManagerService - the application's save manager is returned by this service
  /// </summary>
  public interface ISaveManagerService
  {
    void Add(ISaveable objectToAdd);
    void Add(ISaveable objectToAdd, ToolViewModel tvm);
    void Add(object objectToAdd);
    void Add(object objectToAdd, ToolViewModel tvm);
    void Remove(ISaveable objectToAdd);
    void Remove(object objectToAdd);
    void Clear();

    void Refresh();
  }
}
