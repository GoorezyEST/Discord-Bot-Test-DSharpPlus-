using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestBot.Commands;

namespace TestBot
{
    public class Bot
    {
        // Properties to store references to various bot components
        public DiscordClient Client { get; private set; }
        public InteractivityExtension Interactivity { get; private set; }
        public CommandsNextExtension Commands { get; private set; }

        // Method to start the bot
        public async Task RunAsync()
        {
            // Read bot configuration from file
            var json = string.Empty;
            using (var fs = File.OpenRead("config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync();

            // Deserialize bot configuration JSON into a C# object
            var configJSON = JsonConvert.DeserializeObject<ConfigJSON>(json);

            // Create Discord configuration object with bot token and settings
            var config = new DiscordConfiguration()
            {
                Token = configJSON.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                Intents = DiscordIntents.All,
            };

            // Create DiscordClient object with the configuration
            Client = new DiscordClient(config);

            // Add interactivity support to the client
            Client.UseInteractivity(new InteractivityConfiguration()
            {
                Timeout = TimeSpan.FromMinutes(2)
            });

            // Configure bot command settings
            var commandsConfig = new CommandsNextConfiguration()
            {
                StringPrefixes = new string[] { configJSON.Prefix },
                EnableMentionPrefix = true,
                EnableDefaultHelp = false,
            };

            // Add command handling support to the client
            Commands = Client.UseCommandsNext(commandsConfig);
            //Registering the slash command config to be used in our bot
            var slashCommandsConfig = Client.UseSlashCommands();
            //Registering the slash commands
            slashCommandsConfig.RegisterCommands<SlashCommand>(1080307319776235570);

            // Register the commands classes
            Commands.RegisterCommands<BasicCommands>();

            //Register a method for the situation where a command fails
            Commands.CommandErrored += OnCommandError;

            //Subscribing to the our ready event
            Client.Ready += OnConnectionReady;
            //Adding method to handle components interactivity
            Client.ComponentInteractionCreated += OnComponentInteract;
            // Connect to Discord
            await Client.ConnectAsync();
            // Wait for the bot to be stopped
            await Task.Delay(-1);
        }
        //The method that handles the interactivity of each component
        private async Task OnComponentInteract(DiscordClient sender, ComponentInteractionCreateEventArgs e)
        {
            if (e.Interaction.Data.CustomId == "btn_One")
            {
                await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, 
                    new DiscordInteractionResponseBuilder().WithContent("Hello!"));
            }
            if (e.Interaction.Data.CustomId == "btn_Two")
            {
                await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
                    new DiscordInteractionResponseBuilder().WithContent("Goodbye!"));
            }
            else
            {
            return;
            }
        }

        // Method to handle when the client is ready to start receiving events
        private Task OnConnectionReady(DiscordClient sender, ReadyEventArgs e)
        {
            return Task.CompletedTask;
        }

        // Method to handle when the command trying to be executed throws an error
        private async Task OnCommandError(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            // Check if the exception thrown was a ChecksFailedException, which is raised when a precondition for a command is not met.
            if (e.Exception is ChecksFailedException)
            {
                // Cast the exception to a ChecksFailedException so we can access its FailedChecks property.
                var castedException = (ChecksFailedException)e.Exception;

                // Create an empty string to hold the cooldown timer message.
                string cooldownTimer = string.Empty;

                // Loop through each failed check and retrieve the remaining cooldown time for any CooldownAttributes.
                foreach (var item in castedException.FailedChecks)
                {
                    var cooldown = (CooldownAttribute)item;
                    TimeSpan timeleft = cooldown.GetRemainingCooldown(e.Context);
                    cooldownTimer = timeleft.ToString(@"hh\:mm\:ss");
                }

                // Create a new DiscordEmbedBuilder to display the cooldown message to the user.
                var cooldownMessage = new DiscordEmbedBuilder()
                {
                    Title = "Wait for the cooldown to end",
                    Description = $"Remaining time {cooldownTimer}",
                    Color = DiscordColor.Red
                };

                // Send the cooldown message to the user's channel.
                await e.Context.Channel.SendMessageAsync(embed: cooldownMessage);
            }
            // If the exception was not a ChecksFailedException, create a new DiscordEmbedBuilder to display the error message to the user.
            else
            {
                var errorMessage = new DiscordEmbedBuilder()
                {
                    Title = "An unexpected error ocurred",
                    Description = $"{e.Exception.Message}",
                    Color = DiscordColor.Red
                };

                // Send the error message to the user's channel.
                await e.Context.Channel.SendMessageAsync(embed: errorMessage);
            }
        }
    }
}
