namespace Revolver.Tools.Splash.Views
{
    /// <summary>
    /// Interface for SplashView
    /// </summary>
  public interface ISplashView
  {
    /// <summary>
    /// Closes this instance of the splash.
    /// </summary>
    void Close();

    /// <summary>
    /// Shows this instance of the splash
    /// </summary>
    void Show();

    /// <summary>
    /// Gets the top.
    /// </summary>
    /// <value>The top.</value>
    double Top { get; }

    /// <summary>
    /// Gets the left.
    /// </summary>
    /// <value>The left.</value>
    double Left { get; }

    /// <summary>
    /// Gets the width.
    /// </summary>
    /// <value>The width.</value>
    double Width { get; }

    /// <summary>
    /// Gets the height.
    /// </summary>
    /// <value>The height.</value>
    double Height { get; }
  }
}