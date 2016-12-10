using Magnum.Core.Attributes;
using Magnum.Core.Models;
using Magnum.Core.Services;
using Magnum.Core.ViewModels;
using Magnum.Core.Windows;
using Magnum.Tools.TextDocument.Models;
using Magnum.Tools.TextDocument.ViewModels;
using Magnum.Tools.TextDocument.Views;
using Microsoft.Practices.Unity;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace Revolver.Shell.Services
{
  /// <summary>
  /// AllFileHandler class that supports opening of any file on the computer
  /// </summary>
  [FileContent("All files", "*.*", 1000)]
  internal class AllFileHandler : IContentHandler
  {
    /// <summary>
    /// The injected container
    /// </summary>
    private readonly IUnityContainer _container;
    /// <summary>
    /// The injected logger service
    /// </summary>
    private readonly ILoggerService _loggerService;

    /// <summary>
    /// Constructor of AllFileHandler - all parameters are injected
    /// </summary>
    /// <param name="container">The injected container of the application</param>
    /// <param name="loggerService">The injected logger service of the application</param>
    public AllFileHandler(IUnityContainer container, ILoggerService loggerService)
    {
      _container = container;
      _loggerService = loggerService;
    }

    #region IContentHandler Members

    public ITool NewContent(object parameter)
    {
      var vm = _container.Resolve<TextViewModel>();
      var model = _container.Resolve<TextModel>();
      var view = _container.Resolve<TextView>();

      //Model details
      model.PropertyChanged += vm.ModelOnPropertyChanged;
      _loggerService.Log("Creating a new simple file using AllFileHandler", LogCategory.Info, LogPriority.Low);

      //Clear the undo stack
      model.Document.UndoStack.ClearAll();

      //Set the model and view
      vm.Model = model;
      vm.View = view;
      vm.Title = "untitled";
      vm.View.DataContext = model;
      vm.Handler = this;
      vm.Model.IsDirty = true;

      return vm;
    }

    /// <summary>
    /// Validates the content by checking if a file exists for the specified location
    /// </summary>
    /// <param name="info">The string containing the file location</param>
    /// <returns>True, if the file exists - false otherwise</returns>
    public bool ValidateContentType(object info)
    {
      var location = info as string;
      if (location != null)
      {
        return File.Exists(location);
      }
      return false;
    }

    /// <summary>
    /// Validates the content from an ID - the ContentID from the ContentViewModel
    /// </summary>
    /// <param name="contentId">The content ID which needs to be validated</param>
    /// <returns>True, if valid from content ID - false, otherwise</returns>
    public bool ValidateContentFromId(string contentId)
    {
      string[] split = Regex.Split(contentId, ":##:");
      if (split.Count() == 2)
      {
        string identifier = split[0];
        string path = split[1];
        if (identifier == "FILE" && File.Exists(path))
        {
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Opens a file and returns the corresponding ContentViewModel
    /// </summary>
    /// <param name="info">The string location of the file</param>
    /// <returns>The <see cref="TextViewModel"/> for the file.</returns>
    public ITool OpenContent(object info)
    {
      var location = info as string;
      if (location != null)
      {
        var vm = _container.Resolve<TextViewModel>();
        var model = _container.Resolve<TextModel>();
        var view = _container.Resolve<TextView>();

        //Model details
        model.PropertyChanged += vm.ModelOnPropertyChanged;
        model.SetLocation(info);
        try
        {
          model.Document.Text = File.ReadAllText(location);
          model.IsDirty = false;
        }
        catch (Exception exception)
        {
          _loggerService.Log(exception.Message, LogCategory.Exception, LogPriority.High);
          _loggerService.Log(exception.StackTrace, LogCategory.Exception, LogPriority.High);
          return null;
        }

        //Clear the undo stack
        model.Document.UndoStack.ClearAll();

        //Set the model and view
        vm.Model = model;
        vm.View = view;
        vm.Title = Path.GetFileName(location);
        vm.View.DataContext = model;

        return vm;
      }
      return null;
    }

    /// <summary>
    /// Opens the content from the content ID
    /// </summary>
    /// <param name="contentId">The content ID</param>
    /// <returns></returns>
    public ITool OpenContentFromId(string contentId)
    {
      string[] split = Regex.Split(contentId, ":##:");
      if (split.Count() == 2)
      {
        string identifier = split[0];
        string path = split[1];
        if (identifier == "FILE" && File.Exists(path))
        {
          return OpenContent(path);
        }
      }
      return null;
    }

    /// <summary>
    /// Saves the content of the TextViewModel
    /// </summary>
    /// <param name="contentViewModel">This needs to be a TextViewModel that needs to be saved</param>
    /// <param name="saveAs">Pass in true if you need to Save As?</param>
    /// <returns>true, if successful - false, otherwise</returns>
    public virtual bool SaveContent(ITool contentViewModel, bool saveAs = false)
    {
      var statusBar = _container.Resolve<IStatusBarService>();
      var progressBar = _container.Resolve<IProgressBarService>();
      progressBar.Progress(true, 0, 2);
      IWorkspace workspace = _container.Resolve<WorkspaceBase>();
      var textViewModel = contentViewModel as TextViewModel;

      workspace.IsBusy = true;
      if (textViewModel == null)
      {
        _loggerService.Log("ContentViewModel needs to be a TextViewModel to save details", LogCategory.Exception, LogPriority.High);
        throw new ArgumentException("ContentViewModel needs to be a TextViewModel to save details");
      }

      var textModel = textViewModel.Model as TextModel;

      if (textModel == null)
      {
        _loggerService.Log("TextViewModel does not have a TextModel which should have the text", LogCategory.Exception, LogPriority.High);
        throw new ArgumentException("TextViewModel does not have a TextModel which should have the text");
      }

      var location = textModel.Location as string;

      if (location == null)
      {
        //If there is no location, just prompt for Save As..
        saveAs = true;
      }

      progressBar.Progress(true, 1, 2);
      if (saveAs)
      {
        SaveFileDialog dlg = new SaveFileDialog();
        dlg.FileName = textViewModel.Title.Remove(textViewModel.Title.Length - 1); // Default file name
        dlg.DefaultExt = ".txt"; // Default file extension
        dlg.Filter = "Text documents (.txt)|*.txt"; // Filter files by extension

        // Show save file dialog box
        statusBar.Text = "Please select a location to save the file as";
        Nullable<bool> result = dlg.ShowDialog();

        // Process save file dialog box results
        if (result == true)
        {
          try
          {
            File.WriteAllText(dlg.FileName, textModel.Document.Text);
            textModel.SetLocation(dlg.FileName);
            textModel.IsDirty = false;
            statusBar.Clear();
            progressBar.Progress(false, 2, 2);
            workspace.IsBusy = false;
            return true;
          }
          catch (Exception exception)
          {
            _loggerService.Log(exception.Message, LogCategory.Exception, LogPriority.High);
            _loggerService.Log(exception.StackTrace, LogCategory.Exception, LogPriority.High);
          }
        }
      }
      else
      {
        try
        {
          File.WriteAllText(location, textModel.Document.Text);
          textModel.IsDirty = false;
          statusBar.Clear();
          progressBar.Progress(false, 2, 2);
          workspace.IsBusy = false;
          return true;
        }
        catch (Exception exception)
        {
          _loggerService.Log(exception.Message, LogCategory.Exception, LogPriority.High);
          _loggerService.Log(exception.StackTrace, LogCategory.Exception, LogPriority.High);
        }
      }
      workspace.IsBusy = false;
      statusBar.Clear();
      progressBar.Progress(false, 2, 2);
      return false;
    }

    /// <summary>
    /// CloseDocument method that gets called when the Close command gets executed.
    /// </summary>
    public virtual void CloseDocument(object obj)
    {
      IWorkspace workspace = _container.Resolve<WorkspaceBase>();
      ILoggerService logger = _container.Resolve<ILoggerService>();
      IMenuService menuService = _container.Resolve<IMenuService>();
      MainWindow.MainWindow mainWindow = _container.Resolve<IShell>() as MainWindow.MainWindow;

      CancelEventArgs e = obj as CancelEventArgs;
      DocumentViewModel activeDocument = obj as DocumentViewModel;

      if (activeDocument == null)
        activeDocument = workspace.ActiveDocument;

      if (activeDocument.Model.IsDirty)
      {
        //means the document is dirty - show a message box and then handle based on the user's selection
        workspace.IsMessageBoxShowing = true;
        Magnum.Controls.MessageBox messageBox = new Magnum.Controls.MessageBox();
        messageBox.Owner = mainWindow;
        var res = messageBox.Show(string.Format("Save changes for document '{0}'?", activeDocument.Title), "Are you sure?", MessageBoxButton.YesNoCancel);
        //var res = Xceed.Wpf.Toolkit.MessageBox.Show(string.Format("Save changes for document '{0}'?", activeDocument.Title), "Are you sure?", MessageBoxButton.YesNoCancel, Application.Current.MainWindow.FindResource("MessageBoxStyle") as Style);
        workspace.IsMessageBoxShowing = false;

        //Pressed Yes
        if (res == MessageBoxResult.Yes)
        {
          if (!workspace.ActiveDocument.Handler.SaveContent(workspace.ActiveDocument))
          {
            //Failed to save - return cancel
            res = MessageBoxResult.Cancel;

            //Cancel was pressed - so, we cant close
            if (e != null)
            {
              e.Cancel = true;
            }
            return;
          }
        }

        //Pressed Cancel
        if (res == MessageBoxResult.Cancel)
        {
          //Cancel was pressed - so, we cant close
          if (e != null)
          {
            e.Cancel = true;
          }
          return;
        }
      }

      if (e == null)
      {
        logger.Log("Closing document " + activeDocument.Model.Location, LogCategory.Info, LogPriority.None);
        workspace.Documents.Remove(activeDocument);
        menuService.Refresh();
      }
      else
      {
        // If the location is not there - then we can remove it.
        // This can happen when on clicking "No" in the popup and we still want to quit
        if (activeDocument.Model.Location == null)
        {
          workspace.Documents.Remove(activeDocument);
        }
      }
    }
    #endregion
  }
}
