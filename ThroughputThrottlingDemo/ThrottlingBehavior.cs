using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Pipeline;
using Octokit;

namespace ThroughputThrottlingDemo
{
    public class ThrottlingBehavior : Behavior<IInvokeHandlerContext>
    {
        public override async Task Invoke(IInvokeHandlerContext context, Func<Task> next)
        {
            try
            {
                await next();
            }
            catch (RateLimitExceededException ex)
            {
                var sendOptions = new SendOptions();
                sendOptions.RouteToLocalEndpointInstance();
                var timeSpan = ex.Reset.AddMilliseconds(1).Offset;
                sendOptions.DelayDeliveryWith(timeSpan);
                await context.Send(context.MessageBeingHandled, sendOptions);
            }
        }
    }
}