using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Pipeline;
using Octokit;

namespace ThroughputThrottlingDemo
{
    public class ThrottlingBehavior : Behavior<IInvokeHandlerContext>
    {
        // note this is no threadsafe implementation, only use this with 'configuration.LimitMessageProcessingConcurrencyTo(1);'!
        private static DateTime? nextRateLimitReset = null;

        public override async Task Invoke(IInvokeHandlerContext context, Func<Task> next)
        {
            if (nextRateLimitReset.HasValue && nextRateLimitReset >= DateTime.UtcNow)
            {
                Console.WriteLine($"rate limit already exceeded. Retry after {nextRateLimitReset} UTC");
                await DelayMessage(context);
                return;
            }

            try
            {
                await next();
            }
            catch (RateLimitExceededException ex)
            {
                nextRateLimitReset = ex.Reset.UtcDateTime;
                Console.WriteLine($"rate limit exceeded. Limit resets resets at {nextRateLimitReset} UTC");
                await DelayMessage(context);
            }
        }

        private static async Task DelayMessage(IInvokeHandlerContext context)
        {
            var sendOptions = new SendOptions();
            sendOptions.RouteToLocalEndpointInstance();
            sendOptions.DoNotDeliverBefore(nextRateLimitReset.Value);
            await context.Send(context.MessageBeingHandled, sendOptions);
        }
    }
}