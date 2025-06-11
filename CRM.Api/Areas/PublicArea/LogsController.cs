using CRM.Model.ApplicationModels;
using Microsoft.AspNetCore.Mvc;
using Serilog.Context;
using Serilog.Events;
using System.ComponentModel;

namespace CRM.Api.Areas.PublicArea
{
    [Area("PublicArea")]
    [DisplayName("Logs Controller")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class LogsController(ILogger<LogsController> logger) : ControllerBase
    {
        [HttpPost]
        public IActionResult ReceiveLog([FromBody] List<LogModel> logs)
        {
            foreach (var log in logs)
            {
                var logLevel = LogEventLevel.Information;

                if (Enum.TryParse(log.Level, ignoreCase: true, out LogEventLevel parsedLevel))
                {
                    logLevel = parsedLevel;
                }

                var message = log.MessageTemplate ?? "No message";
                var exceptionInfo = log.Exception;
                using (LogContext.PushProperty("UserEmail", log.UserEmail ?? "Unknown"))
                {
                    switch (logLevel)
                    {
                        case LogEventLevel.Verbose:
                            logger.LogTrace(message);
                            break;
                        case LogEventLevel.Debug:
                            logger.LogDebug(message);
                            break;
                        case LogEventLevel.Information:
                            logger.LogInformation(message);
                            break;
                        case LogEventLevel.Warning:
                            logger.LogWarning(message);
                            break;
                        case LogEventLevel.Error:
                            logger.LogError("{Message} | Exception: {Exception}", message, exceptionInfo);
                            break;
                        case LogEventLevel.Fatal:
                            logger.LogCritical("{Message} | Exception: {Exception}", message, exceptionInfo);
                            break;
                        default:
                            logger.LogInformation(message);
                            break;
                    }
                }
            }

            return Ok();
        }
    }
}
