using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestBot.Commands
{
    public class BasicCommands : BaseCommandModule
    {

        [Command("testbot")]
        public async Task GreetsCommand(CommandContext ctx)
        {
            //Send to the channel where the command was triggered a simple mesagge
            await ctx.Channel.SendMessageAsync("I'm a bot developed by Goorezy");
        }
        [Command("whoami")]
        public async Task WhoAmICommand(CommandContext ctx)
        {
            // Creates a new DiscordMessageBuilder object
            var embeddedMessage = new DiscordMessageBuilder()
                // Adds a new DiscordEmbedBuilder object to the message builder
                .AddEmbed(new DiscordEmbedBuilder()
                    // Sets the title of the embed to the user's username
                    .WithTitle($"**You're {ctx.User.Username}**")
                    // Sets the thumbnail of the embed to the user's avatar
                    .WithThumbnail($"{ctx.User.AvatarUrl}")
                    // Sets the color of the embed to white
                    .WithColor(DiscordColor.White)
                );
            // Sends the message to the channel where the command was executed
            await ctx.Channel.SendMessageAsync(embeddedMessage);
        }
        [Command("roll-dice")]
        [Cooldown(3, 20, CooldownBucketType.User)]
        public async Task RollDiceCommand(CommandContext ctx)
        {
            // Generate a random number between 1 and 6 for the bot's roll
            int botRoll = new Random().Next(1, 7);

            // Generate a random number between 1 and 6 for the user's roll
            int userRoll = new Random(Guid.NewGuid().GetHashCode()).Next(1, 7);

            // Determine the winner based on the roll results
            DiscordMessageBuilder result = new DiscordMessageBuilder();
            if (userRoll > botRoll)
            {
                result.AddEmbed(new DiscordEmbedBuilder()
                    .WithColor(DiscordColor.Green)
                    .WithTitle($"Result of roll dice by {ctx.User.Username}")
                    .WithDescription($"**Winner:** {ctx.User.Username} with {userRoll}")
                    );
            }
            else if (userRoll < botRoll)
            {
                result.AddEmbed(new DiscordEmbedBuilder()
                    .WithColor(DiscordColor.Red)
                    .WithTitle($"Result of roll dice by {ctx.User.Username}")
                    .WithDescription($"**Winner:** TestBot with {botRoll}")
                    );
            }
            else
            {
                result.AddEmbed(new DiscordEmbedBuilder()
                    .WithColor(DiscordColor.White)
                    .WithTitle($"Result of roll dice by {ctx.User.Username}")
                    .WithDescription($"**Winner:** ¡Tie! both numbers are {userRoll}")
                    );
            }

            // Send the result message to the Discord channel
            await ctx.Channel.SendMessageAsync(result);
        }
        [Command("yes-or-no")]
        public async Task YesOrNoCommand(CommandContext ctx, int timeLimit, params string[] question)
        {
            // Get the interactivity service from the Discord client
            var interactivity = ctx.Client.GetInteractivity();
            // Create two DiscordEmoji objects, one for thumbs up and one for thumbs down
            DiscordEmoji[] discordEmojis = {
                DiscordEmoji.FromName(ctx.Client, ":thumbsup:", false),
                DiscordEmoji.FromName(ctx.Client, ":thumbsdown:", false)
            };

            // Create a DiscordMessageBuilder object for the poll message
            var yesOrNoPoll = new DiscordMessageBuilder()
                .AddEmbed(new DiscordEmbedBuilder()
                // Add the poll title, using the user's name and the question string array
                .WithTitle($"{ctx.User.Username} says: {string.Join(" ", question)}")
                // Add the description of the poll, including the thumbs up and thumbs down emojis
                .WithDescription($"{discordEmojis[0]} Yes.\n\n{discordEmojis[1]} No.")
                // Set the color of the poll embed
                .WithColor(DiscordColor.White)

                );
            // Send the poll message to the channel and wait for it to be sent
            var messageToReact = await ctx.Channel.SendMessageAsync(yesOrNoPoll);
            // Add the thumbs up and thumbs down emojis as reactions to the poll message
            foreach (var emoji in discordEmojis)
            {
                await messageToReact.CreateReactionAsync(emoji);
            }
            // Wait for the specified amount of time (timeLimit) for users to vote on the poll
            var pollResult = await interactivity.CollectReactionsAsync(messageToReact, TimeSpan.FromSeconds(timeLimit));
            // Create variables to keep track of how many times each option was voted for
            int timesVotedYes = 0;
            int timesVotedNo = 0;
            // Loop through each reaction in the poll result
            foreach (var emoji in pollResult)
            {
                // If the reaction is the thumbs up emoji, increment the times voted yes variable
                if (emoji.Emoji == discordEmojis[0])
                {
                    timesVotedYes++;
                }
                // If the reaction is the thumbs down emoji, increment the times voted no variable
                if (emoji.Emoji == discordEmojis[1])
                {
                    timesVotedNo++;
                }
            }
            // Calculate the total number of votes
            int timesVoted = timesVotedNo + timesVotedYes;
            // Create a string to represent the winning option of the poll
            string pollOptionWinner = string.Empty;
            // Determine which option won the poll and set the pollOptionWinner string accordingly
            if (timesVotedYes > timesVotedNo)
            {
                pollOptionWinner = "The poll result is: Yes!";
            }
            if (timesVotedYes < timesVotedNo)
            {
                pollOptionWinner = "The poll result is: No!";
            }
            if (timesVotedYes == timesVotedNo)
            {
                pollOptionWinner = "The number of votes is equal.";
            }
            // Create a new DiscordMessageBuilder object for the result message
            var resultMessage = new DiscordMessageBuilder()
                .AddEmbed(new DiscordEmbedBuilder()
                // Add the question string array as the title of the result message
                .WithTitle($"{string.Join(" ", question)}\nResults")
                // Add the emojis wiht the count of each one, also the result of the poll as the description
                .WithDescription($"{pollOptionWinner}\n\n{discordEmojis[0]} {timesVotedYes}.\n\n{discordEmojis[1]} {timesVotedNo}.")
                //Add white color to the embed message
                .WithColor(DiscordColor.White)
                //Add a footer with the total count of votes
                .WithFooter($"Total votes: {timesVoted}.")

                );
            //Send the result of the poll as an embed message to the channel where the command was triggered
            await ctx.Channel.SendMessageAsync(resultMessage);
        }
        [Command("yes-or-no")]
        public async Task YesOrNoCommand(CommandContext ctx, params string[] question)
        {
            //Set the default value countdown in seconds of the poll at 30 (this is when the user doesn't insert a time limit by himself)
            int timeLimit = 30;

            // Get the interactivity service from the Discord client
            var interactivity = ctx.Client.GetInteractivity();
            // Create two DiscordEmoji objects, one for thumbs up and one for thumbs down
            DiscordEmoji[] discordEmojis = {
                DiscordEmoji.FromName(ctx.Client, ":thumbsup:", false),
                DiscordEmoji.FromName(ctx.Client, ":thumbsdown:", false)
            };

            // Create a DiscordMessageBuilder object for the poll message
            var yesOrNoPoll = new DiscordMessageBuilder()
                .AddEmbed(new DiscordEmbedBuilder()
                // Add the poll title, using the user's name and the question string array
                .WithTitle($"{ctx.User.Username} says: {string.Join(" ", question)}")
                // Add the description of the poll, including the thumbs up and thumbs down emojis
                .WithDescription($"{discordEmojis[0]} Yes.\n\n{discordEmojis[1]} No.")
                // Set the color of the poll embed
                .WithColor(DiscordColor.White)

                );
            // Send the poll message to the channel and wait for it to be sent
            var messageToReact = await ctx.Channel.SendMessageAsync(yesOrNoPoll);
            // Add the thumbs up and thumbs down emojis as reactions to the poll message
            foreach (var emoji in discordEmojis)
            {
                await messageToReact.CreateReactionAsync(emoji);
            }
            // Wait for the specified amount of time (timeLimit) for users to vote on the poll
            var pollResult = await interactivity.CollectReactionsAsync(messageToReact, TimeSpan.FromSeconds(timeLimit));
            // Create variables to keep track of how many times each option was voted for
            int timesVotedYes = 0;
            int timesVotedNo = 0;
            // Loop through each reaction in the poll result
            foreach (var emoji in pollResult)
            {
                // If the reaction is the thumbs up emoji, increment the times voted yes variable
                if (emoji.Emoji == discordEmojis[0])
                {
                    timesVotedYes++;
                }
                // If the reaction is the thumbs down emoji, increment the times voted no variable
                if (emoji.Emoji == discordEmojis[1])
                {
                    timesVotedNo++;
                }
            }
            // Calculate the total number of votes
            int timesVoted = timesVotedNo + timesVotedYes;
            // Create a string to represent the winning option of the poll
            string pollOptionWinner = string.Empty;
            // Determine which option won the poll and set the pollOptionWinner string accordingly
            if (timesVotedYes > timesVotedNo)
            {
                pollOptionWinner = "The poll result is: Yes!";
            }
            if (timesVotedYes < timesVotedNo)
            {
                pollOptionWinner = "The poll result is: No!";
            }
            if (timesVotedYes == timesVotedNo)
            {
                pollOptionWinner = "The number of votes is equal.";
            }
            // Create a new DiscordMessageBuilder object for the result message
            var resultMessage = new DiscordMessageBuilder()
                .AddEmbed(new DiscordEmbedBuilder()
                // Add the question string array as the title of the result message
                .WithTitle($"{string.Join(" ", question)}\nResults")
                // Add the emojis wiht the count of each one, also the result of the poll as the description
                .WithDescription($"{pollOptionWinner}\n\n{discordEmojis[0]} {timesVotedYes}.\n\n{discordEmojis[1]} {timesVotedNo}.")
                //Add white color to the embed message
                .WithColor(DiscordColor.White)
                //Add a footer with the total count of votes
                .WithFooter($"Total votes: {timesVoted}.")

                );
            //Send the result of the poll as an embed message to the channel where the command was triggered
            await ctx.Channel.SendMessageAsync(resultMessage);
        }
    }
}
