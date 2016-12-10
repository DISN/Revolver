using Magnum.Core.Services;
using System;
using System.Windows.Input;

namespace Revolver.Shell.Commands
{
  class EngineCommands
  {
    #region ICommand Members
    public bool CanExecute(object parameter)
    {
      throw new NotImplementedException();
    }

    public event EventHandler CanExecuteChanged
    {
      add { throw new NotImplementedException(); }
      remove { throw new NotImplementedException(); }
    }

    public void Execute(object parameter)
    {
      throw new NotImplementedException();
    }
    #endregion

    public void Undo(object parameter)
    {
      throw new NotImplementedException();
    }
  }
}
