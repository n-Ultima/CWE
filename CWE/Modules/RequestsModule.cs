﻿namespace CWE.Modules
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using CWE.Common;
    using CWE.Data.Models;
    using CWE.Services;
    using Discord;
    using Discord.Commands;
    using Discord.WebSocket;
    using Interactivity;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// The requests module, used to create and toggle requests.
    /// </summary>
    [Name("Requests")]
    public class RequestsModule : CWEModuleBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequestsModule"/> class.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> to inject.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> to inject.</param>
        /// <param name="interactivityService">The <see cref="InteractivityService"/> to inject.</param>
        public RequestsModule(IServiceProvider serviceProvider, IConfiguration configuration, InteractivityService interactivityService)
                : base(serviceProvider, configuration, interactivityService)
        {
        }

        /// <summary>
        /// The command used to create a new request.
        /// </summary>
        /// <param name="description">The description of the request.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Command("request", RunMode = RunMode.Async)]
        public async Task Request([Remainder] string description)
        {
            if (CommandHandler.Requests == false)
            {
                var error = Embeds.GetErrorEmbed("Requests disabled", $"Requests are currently disabled, please come back later.");
                await this.Context.Channel.SendMessageAsync(embed: error);
                return;
            }

            var socketGuildUser = this.Context.User as SocketGuildUser;
            if (socketGuildUser.Roles.All(x => x.Name != "Patron"))
            {
                var error = Embeds.GetErrorEmbed("Not a Patron", $"This command can only be used by patrons. Take a look at [our Patreon page](https://www.patreon.com/codingwithefehan) to become a patron.");
                await this.Context.Channel.SendMessageAsync(embed: error);
                return;
            }

            var request = new Request
            {
                Description = description,
                Initiator = this.Context.User.Id,
            };

            var channel = this.Context.Guild.GetTextChannel(this.Configuration.GetSection("Channels").GetValue<ulong>("Requests"));
            var requestEmbed = Embeds.GetRequestEmbed(request);
            var component = new ComponentBuilder()
                .WithButton("Deny", "deny", ButtonStyle.Danger)
                .WithButton("Switch", "switch", ButtonStyle.Success)
                .Build();

            var message = await channel.SendMessageAsync(embed: requestEmbed, component: component);
            request.MessageId = message.Id;

            try
            {
                await this.DataAccessLayer.CreateRequest(request);
                var success = Embeds.GetSuccessEmbed("Request sent!", $"Your request has been sent!");
                await this.Context.Channel.SendMessageAsync(embed: success);
            }
            catch
            {
                await message.DeleteAsync();
                var error = Embeds.GetErrorEmbed("Error", $"An error occurred while sending your request.");
                await this.Context.Channel.SendMessageAsync(embed: error);
            }
        }

        /// <summary>
        /// The command used to toggle on or off the ability to send requests.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Command("togglerequests")]
        [RequireOwner]
        public async Task ToggleRequests()
        {
            CommandHandler.Requests = !CommandHandler.Requests;
            var success = Embeds.GetSuccessEmbed((CommandHandler.Requests ? "Enabled" : "Disabled") + " requests", $"Successfully {(CommandHandler.Requests ? "enabled" : "disabled")} requests!");
            await this.Context.Channel.SendMessageAsync(embed: success);
        }
    }
}
