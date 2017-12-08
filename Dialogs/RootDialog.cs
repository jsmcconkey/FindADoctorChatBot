namespace MultiDialogsBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;
    using Microsoft.Azure.CognitiveServices.Language.TextAnalytics.Models;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;

    [Serializable]
    public class RootDialog : IDialog<object>
    {
        private const string ProvidersOption = "Providers";

        private const string LocationsOption = "Locations";

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            var client = new TextAnalyticsAPI();
            client.AzureRegion = AzureRegions.Westcentralus;
            client.SubscriptionKey = "103b80f43b844925b85446a4377e5146";

            SentimentBatchResult sentimentAnalysis = client.Sentiment(
                new MultiLanguageBatchInput(
                    new List<MultiLanguageInput>()
                    {
                        new MultiLanguageInput("en", "1", message.Text)
                    }));

            if (sentimentAnalysis.Documents[0].Score < .5)
            {
                await context.PostAsync(@"Sorry, I am not sure what you are looking for...  Please call 8675309. Or click this bad boy https://youtu.be/6WTdTwcmxyo?t=52");
                context.Wait(MessageReceivedAsync);
            }
            else
            {
                if (message.Text.ToLower().Contains("help") || message.Text.ToLower().Contains("support") || message.Text.ToLower().Contains("problem"))
                {
                    await context.Forward(new SupportDialog(), this.ResumeAfterSupportDialog, message, CancellationToken.None);
                }
                else
                {
                    this.ShowOptions(context);
                }
            }
        }

        private void ShowOptions(IDialogContext context)
        {
            PromptDialog.Choice(context, this.OnOptionSelected, new List<string>() { ProvidersOption, LocationsOption }, "Hey there! Are you looking for providers or locations? I can help!", "Not a valid option", 3);
        }

        private async Task OnOptionSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string optionSelected = await result;

                switch (optionSelected)
                {
                    case ProvidersOption:
                        context.Call(new ProvidersDialog(), this.ResumeAfterOptionDialog);
                        break;

                    case LocationsOption:
                        context.Call(new HotelsDialog(), this.ResumeAfterOptionDialog);
                        break;
                }
            }
            catch (TooManyAttemptsException ex)
            {
                await context.PostAsync($"Ooops! Too many attemps :(. But don't worry, I'm handling that exception and you can try again!");

                context.Wait(this.MessageReceivedAsync);
            }
        }

        private async Task ResumeAfterSupportDialog(IDialogContext context, IAwaitable<int> result)
        {
            var ticketNumber = await result;

            await context.PostAsync($"Thanks for contacting our support team. Your ticket number is {ticketNumber}.");
            context.Wait(this.MessageReceivedAsync);
        }

        private async Task ResumeAfterOptionDialog(IDialogContext context, IAwaitable<object> result)
        {
            try
            {
                var message = await result;
            }
            catch (Exception ex)
            {
                await context.PostAsync($"Failed with message: {ex.Message}");
            }
            finally
            {
                context.Wait(this.MessageReceivedAsync);
            }
        }
    }
}