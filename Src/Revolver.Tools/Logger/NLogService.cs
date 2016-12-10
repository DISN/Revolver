using Magnum.Core.Services;
using Microsoft.Practices.Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using System.Diagnostics;
using System.Reflection;
using Magnum.Core.Events;

namespace Revolver.Tools.Logger
{
  /// <summary>
  /// The NLogService for logging purposes
  /// </summary>
  public class NLogService : ILoggerService
  {
    private static readonly NLog.Logger Logger = LogManager.GetLogger("Revolver");
    private readonly IEventAggregator _aggregator;

    /// <summary>
    /// Private constructor of NLogService
    /// </summary>
    private NLogService()
    {
    }

    /// <summary>
    /// The NLogService constructor
    /// </summary>
    /// <param name="aggregator">The injected event aggregator</param>
    public NLogService(IEventAggregator aggregator)
    {
      _aggregator = aggregator;
    }

    #region ILoggerService Members
    /// <summary>
    /// The logging function
    /// </summary>
    /// <param name="message">A message to log</param>
    /// <param name="category">The category of logging</param>
    /// <param name="priority">The priority of logging</param>
    public void Log(string message, LogCategory category, LogPriority priority)
    {
      Message = message;
      Category = category;
      Priority = priority;

      var trace = new StackTrace();
      StackFrame frame = trace.GetFrame(1); // 0 will be the inner-most method
      MethodBase method = frame.GetMethod();

      Logger.Log(LogLevel.Error, method.DeclaringType + ": " + message);

      _aggregator.GetEvent<LogEvent>().Publish(new NLogService { Message = Message, Category = Category, Priority = Priority });
    }

    /// <summary>
    /// The message which was last logged using the service
    /// </summary>
    public string Message { get; internal set; }

    /// <summary>
    /// The log message's category
    /// </summary>
    public LogCategory Category { get; internal set; }

    /// <summary>
    /// The log message's priority
    /// </summary>
    public LogPriority Priority { get; internal set; }

    #endregion
  }
}
