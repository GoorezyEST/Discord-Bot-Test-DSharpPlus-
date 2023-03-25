using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestBot.Commands
{
    public class SlashCommand : ApplicationCommandModule
    {

        [SlashCommand("test", "my first slash command")]
        public async Task TestSlashCommand(InteractionContext ctx,
            [Choice("Standard Title", "Default")]
            [Option("string", "Type anything")] string text)
        {
            // Respond to the user with a starting message.
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .WithContent("Starting ..."));

            // Create a new DiscordEmbedBuilder to send an embed message to the channel.
            var embedMessage = new DiscordEmbedBuilder()
            {
                Title = text
            };

            // Send the embed message to the channel.
            await ctx.Channel.SendMessageAsync(embed: embedMessage);
        }


        [SlashCommand("poll", "Create your own poll")]
        public async Task SlashCommandPoll(InteractionContext ctx, 
            [Option("question", "What's this poll about?")] string question,
            [Choice("Fast", 15)] [Choice("Standard", 30)] [Choice("Slow", 60)]
            [Option("timelimit", "The time set to this poll until finishing.")] long timelimit)
        {
            // Respond to the user with a starting message.
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .WithContent("Starting ..."));
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
            var pollResult = await interactivity.CollectReactionsAsync(messageToReact, TimeSpan.FromSeconds(timelimit));
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


        [SlashCommand("caption", "Upload an image with a caption in it")]
        public async Task SlashCommandCaption(InteractionContext ctx,
            [Option("caption", "Write your caption here")] string caption,
            [Option("image", "The image you want to upload")] DiscordAttachment image)
        {
            // Respond to the user with a starting message.
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .WithContent("Starting ..."));

            // Create caption embed to send into the channel
            var captionMessage = new DiscordEmbedBuilder()
            {
                Title = caption,
                ImageUrl = image.Url
            };

            //Sending message to the channel where the command was triggered
            await ctx.Channel.SendMessageAsync(embed: captionMessage);
        }

        [SlashCommand("button_test", "Testing the use of buttons in embeds messages")]
        public async Task SlashCommandButton(InteractionContext ctx)
        {
            // Respond to the user with a starting message.
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .WithContent("Starting ..."));

            // Create two button components with primary style, unique IDs, and labels
            DiscordButtonComponent buttonOne = new DiscordButtonComponent(ButtonStyle.Primary, "btn_One", "Send hello");
            DiscordButtonComponent buttonTwo = new DiscordButtonComponent(ButtonStyle.Primary, "btn_Two", "Send goodbye");

            // Create a message builder and add an embed with a title, description, and color
            var buttonMessage = new DiscordMessageBuilder()
                .AddEmbed(new DiscordEmbedBuilder()
                    .WithTitle("Testing buttons")
                    .WithDescription("Please pick a button")
                    .WithColor(DiscordColor.White)
                )
                // Add the two button components to the message builder
                .AddComponents(buttonOne)
                .AddComponents(buttonTwo);

            // Send the message with the embedded buttons to the channel where the command was used
            await ctx.Channel.SendMessageAsync(buttonMessage);
        }
    }
}
