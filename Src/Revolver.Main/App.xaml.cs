using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ExceptionReporting;
using System.Windows.Threading;
using System.Reflection;
using Magnum.Core.Windows;
using Microsoft.Practices.Unity;

namespace Revolver
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    private static Revolver.Shell.Bootstrapper b;
    static ExceptionReporter reporter;
    static object _lock = new object();

    protected override void OnStartup(StartupEventArgs e)
    {
      base.OnStartup(e);
      try
      {
        b = new Revolver.Shell.Bootstrapper();
        b.Run();
      }
      catch (Exception exception)
      {
        if (reporter == null)
          lock (_lock)
            reporter = new ExceptionReporter();
        else
          return;

        b.CloseShell();
        SetupErrorReporter();
        reporter.Show(exception);
      }
    }

    private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
      IUnityContainer container = b.Container;
      IWorkspace workspace = container.Resolve<WorkspaceBase>();
      if (workspace != null)
      {
        var toolSendingError = workspace.Tools.Where(x => x.ContentId == workspace.ActiveTool.ContentId).FirstOrDefault();
        if (toolSendingError != null)
        {
          toolSendingError.CloseCommand.Execute(null);
          return;
        }
      }

      if (reporter == null)
        lock (_lock)
          reporter = new ExceptionReporter();
      else
        return;

      b.CloseShell();
      SetupErrorReporter();
      reporter.Show(e.Exception);
    }

    private void SetupErrorReporter()
    {
      // read properties from app config file 
      reporter.ReadConfig();

      // set configuration via code (TODO: Get it from AssemblyInfo.cs)
      reporter.Config.AppName = "Revolver Game Editor";
      reporter.Config.AppVersion = "v2.0";
      reporter.Config.CompanyName = "Magnum Games";
      reporter.Config.TitleText = "Revolver Game Editor Error Report";
      reporter.Config.EmailReportAddress = "nicolas.distefano@hotmail.com";
      reporter.Config.TakeScreenshot = true;   // attached if sending email
    }
  }
}
