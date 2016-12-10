using Revolver.Tools.Splash.ViewModels;

namespace Revolver.Tools.Splash.Views
{
    /// <summary>
    /// Interaction logic for SplashView.xaml
    /// </summary>
    public partial class SplashView : ISplashView
    {
        public SplashView(SplashViewModel model)
        {
            InitializeComponent();
            DataContext = model;
        }
    }
}