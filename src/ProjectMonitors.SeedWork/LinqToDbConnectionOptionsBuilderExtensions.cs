using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using LinqToDB.Configuration;
using LinqToDB.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;

namespace ProjectMonitors.SeedWork
{
  public static class LinqToDbConnectionOptionsBuilderExtensions
  {
    public static LinqToDbConnectionOptionsBuilder ConfigureTracesWithDefaults(
      this LinqToDbConnectionOptionsBuilder self, IServiceProvider serviceProvider)
    {
      var activitySource = serviceProvider.GetRequiredService<ActivitySource>();
      var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
      var logger = loggerFactory.CreateLogger("Linq2Db");

      DataConnection.TurnTraceSwitchOn(TraceLevel.Verbose);
      var b = self.WithTracing(info =>
      {
        switch (info.TraceInfoStep)
        {
          case TraceInfoStep.BeforeExecute:
            break;
          case TraceInfoStep.AfterExecute:
            TryCaptureTraceInfo(info, activitySource);
            var level = info.TraceLevel switch
            {
              TraceLevel.Verbose => LogLevel.Debug,
              TraceLevel.Info => LogLevel.Information,
              TraceLevel.Warning => LogLevel.Warning,
              TraceLevel.Error => LogLevel.Error,
              TraceLevel.Off => LogLevel.None,
              _ => throw new ArgumentOutOfRangeException(nameof(info.TraceLevel),
                "Not supported level provided " + info.TraceLevel)
            };
            logger.Log(level, "Total execution time: {ExecutionTime}\n{Message}", info.ExecutionTime,
              info.CommandText ?? info.SqlText);
            break;
          case TraceInfoStep.Error:
          {
            TryCaptureTraceInfo(info, activitySource);
            break;
          }
          case TraceInfoStep.Completed:
          {
            break;
          }

          default:
            throw new ArgumentOutOfRangeException();
        }
      });

      return b;
    }

    private static void TryCaptureTraceInfo(TraceInfo info, ActivitySource activitySource)
    {
      if (info.StartTime == null || info.ExecutionTime == null)
      {
        return;
      }

      using var a = activitySource.StartActivity(info.DataConnection.Command.CommandText);
      if (a == null)
      {
        return;
      }

      a.SetStartTime(info.StartTime.Value);
      a.SetEndTime(info.StartTime.Value + info.ExecutionTime.Value);
      if (info.Exception != null)
      {
        a.RecordException(info.Exception);
      }
    }
  }
}