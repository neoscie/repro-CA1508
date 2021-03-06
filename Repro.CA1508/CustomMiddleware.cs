﻿namespace Repro.CA1508
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    public class CustomMiddleware
    {
        private readonly ILogger<CustomMiddleware> logger;

        public CustomMiddleware(ILogger<CustomMiddleware> logger)
        {
            this.logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, string tenantName)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // CA1508 raised when using the `CreateTenantScope` extension method
            using (this.logger.CreateTenantScope(tenantName))
            {
                var sr = new StreamReader(context.Request.Body);
                var bodyText = await sr.ReadToEndAsync().ConfigureAwait(false);
                this.logger.LogInformation($"Body Text: {bodyText}");
            }

            // Using a discard variable does not raise CA1508
            using (var _ = this.logger.CreateTenantScope(tenantName))
            {
                var sr = new StreamReader(context.Request.Body);
                var bodyText = await sr.ReadToEndAsync().ConfigureAwait(false);
                this.logger.LogInformation($"Body Text: {bodyText}");
            }

            // Apparently it is fine to do the extension method is doing
            using (this.logger.BeginScope(new[] { new KeyValuePair<string, object>("tenant", tenantName) }))
            {
                var sr = new StreamReader(context.Request.Body);
                var bodyText = await sr.ReadToEndAsync().ConfigureAwait(false);
                this.logger.LogInformation($"Body Text: {bodyText}");
            }
        }
    }

    public static class LoggerExtensions
    {
        public static IDisposable CreateTenantScope(this ILogger logger, string tenantName)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            return logger.BeginScope(new[] { new KeyValuePair<string, object>("tenant", tenantName) });
        }
    }
}
