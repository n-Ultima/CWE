﻿namespace CWE.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using CWE.Data;
    using Discord.Commands;
    using Discord.WebSocket;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Attribute used to ensure the user is promoted (a helper or higher).
    /// </summary>
    public class RequirePromoted : PreconditionAttribute
    {
        /// <summary>
        /// Checks if the attribute will pass or fail.
        /// </summary>
        /// <param name="context">The underlying context.</param>
        /// <param name="command">The command that this attribute was placed on.</param>
        /// <param name="services">The services inside our service pool.</param>
        /// <returns>Returns if a user is authorized to manage tags.</returns>
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (context.User is SocketGuildUser user)
            {
                var helperRoleId = services.GetRequiredService<IConfiguration>().GetSection("Roles").GetValue<ulong>("Helper");
                var contributorRoleId = services.GetRequiredService<IConfiguration>().GetSection("Roles").GetValue<ulong>("Contributor");

                var helperRole = context.Guild.GetRole(helperRoleId);
                var contributorRole = context.Guild.GetRole(contributorRoleId);
                if (user.Roles.Contains(helperRole) || user.Roles.Contains(contributorRole))
                {
                    return Task.FromResult(PreconditionResult.FromSuccess());
                }
                else
                {
                    return Task.FromResult(PreconditionResult.FromError("User is not authorized to manage tags."));
                }
            }
            else
            {
                return Task.FromResult(PreconditionResult.FromError("Command was run outside of a guild.")); // Should never be called, but we put this here to guarentee all code paths return a value.
            }
        }
    }
}
