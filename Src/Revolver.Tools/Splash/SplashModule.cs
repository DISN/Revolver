using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Unity;
using Magnum.Core.Windows;
using Magnum.Core.Events;
using Revolver.Tools.Splash.ViewModels;
using Revolver.Tools.Splash.Views;

namespace Revolver.Tools.Splash
{
  [Module(ModuleName = "Revolver.Tools.Splash")]
  public class SplashModule : IModule
  {
    #region Constructors
    public SplashModule(IUnityContainer container_, IEventAggregator eventAggregator_, IShell shell_)
    {
      Container = container_;
      EventAggregator = eventAggregator_;
      Shell = shell_;
    }
    #endregion

    #region Properties

    private IUnityContainer Container { get; set; }

    private IEventAggregator EventAggregator { get; set; }

    private IShell Shell { get; set; }

    private AutoResetEvent WaitForCreation { get; set; }

    #endregion

    #region IModule Members
    public void Initialize()
    {
      Dispatcher.CurrentDispatcher.BeginInvoke(
          (Action)(() =>
                        {
                          var shell = Shell as Window;
                          if (shell != null)
                          {
                            shell.Show();
                            EventAggregator.GetEvent<SplashCloseEvent>().Publish(new SplashCloseEvent());
                          }
                        }));

      WaitForCreation = new AutoResetEvent(false);

      ThreadStart showSplash =
          () =>
          {
            Dispatcher.CurrentDispatcher.BeginInvoke(
                (Action)(() =>
                              {
                                Container.RegisterType<SplashViewModel, SplashViewModel>();
                                ISplashView iSplashView;
                                try
                                {
                                  //The end user might have set a splash view - try to use that
                                  iSplashView = Container.Resolve<ISplashView>();
                                }
                                catch (Exception)
                                {
                                  Container.RegisterType<ISplashView, SplashView>();
                                  iSplashView = Container.Resolve<ISplashView>();
                                }
                                var splash = iSplashView as Window;
                                if (splash != null)
                                {
                                  EventAggregator.GetEvent<SplashCloseEvent>().Subscribe(
                                      e_ => splash.Dispatcher.BeginInvoke((Action)iSplashView.Close),
                                      ThreadOption.PublisherThread, true);

                                  splash.Show();
                                  WaitForCreation.Set();
                                }
                              }));

            Dispatcher.Run();
          };

      var thread = new Thread(showSplash) { Name = "Splash Thread", IsBackground = true };
      thread.SetApartmentState(ApartmentState.STA);
      thread.Start();

      WaitForCreation.WaitOne();
    }
    #endregion
  }
}