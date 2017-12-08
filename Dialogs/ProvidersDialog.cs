namespace MultiDialogsBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.FormFlow;
    using Microsoft.Bot.Connector;

    [Serializable]
    public class ProvidersDialog : IDialog<object>
    {
        public List<string> specialties = new List<string>()
        {
            "Podiatry",
            "Cariologist",
            "Orthopedic",
            "Urology",
            "Kinesiology",
            "Neurology",
            "Reflexology"
        };

        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("Welcome to the provider finder! We're glad you're here.");

            var providersFormDialog = FormDialog.FromForm(this.BuildProvidersForm, FormOptions.PromptInStart);

            context.Call(providersFormDialog, this.ResumeAfterProvidersFormDialog);
        }

        private IForm<ProvidersQuery> BuildProvidersForm()
        {
            OnCompletionAsyncDelegate<ProvidersQuery> processProvidersSearch = async (context, state) =>
            {
                await context.PostAsync($"Ok. Searching for Providers in {state.City} who specialize in { state.Speciality }.");
            };

            return new FormBuilder<ProvidersQuery>()
                .Field(nameof(ProvidersQuery.City))
                .Message("Looking for providers in {City}...")
                .AddRemainingFields()
                .OnCompletion(processProvidersSearch)
                .Build();
        }

        private async Task ResumeAfterProvidersFormDialog(IDialogContext context, IAwaitable<ProvidersQuery> result)
        {
            try
            {
                var searchQuery = await result;

                var providers = await this.GetProvidersAsync(searchQuery);

                await context.PostAsync($"I found in total { providers.Count() } providers:");

                var resultMessage = context.MakeMessage();
                resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                resultMessage.Attachments = new List<Attachment>();

                foreach (var provider in providers)
                {
                    HeroCard heroCard = new HeroCard()
                    {
                        Title = provider.Name,
                        Subtitle = $"{provider.Name} specializes in {provider.Speciality} and has a {provider.Rating} star rating.",
                        Images = new List<CardImage>()
                        {
                            new CardImage() { Url = provider.Image }
                        },
                        Buttons = new List<CardAction>()
                        {
                            new CardAction()
                            {
                                Title = "View Provider details",
                                Type = ActionTypes.OpenUrl,
                                Value = $"https://findadoctor.spectrumhealth.org/physician/profile/" + provider.Id
                            }
                        }
                    };

                    resultMessage.Attachments.Add(heroCard.ToAttachment());
                }

                await context.PostAsync(resultMessage);
            }
            catch (FormCanceledException ex)
            {
                string reply;

                if (ex.InnerException == null)
                {
                    reply = "You have canceled the operation. Quitting from the HotelsDialog";
                }
                else
                {
                    reply = $"Oops! Something went wrong :( Technical Details: {ex.InnerException.Message}";
                }

                await context.PostAsync(reply);
            }
            finally
            {
                context.Done<object>(null);
            }
        }

        private async Task<IEnumerable<Provider>> GetProvidersAsync(ProvidersQuery searchQuery)
        {
            var providers = new List<Provider>();

            var random = new Random(5);
            Provider provider1 = new Provider()
            {
                Id = 2500,
                Name = "Doctor",
                Speciality = "Podiatry",
                Image = "https://s3.amazonaws.com/findadoc/prod/providers/Michael_Meyers701.png",
                Location = searchQuery.City,
                Rating = random.Next(1, 5),
            };

            Provider provider2 = new Provider()
            {
                Id = 1998,
                Name = "Doctor",
                Speciality = "Podiatry",
                Image = "https://s3.amazonaws.com/findadoc/prod/providers/Carrie_Roberson851.png",
                Location = searchQuery.City,
                Rating = random.Next(1, 5),
            };

            Provider provider3 = new Provider()
            {
                Id = 2500,
                Name = "Doctor",
                Speciality = "Podiatry",
                Image = "https://s3.amazonaws.com/findadoc/prod/providers/Carrie_Roberson851.png",
                Location = searchQuery.City,
                Rating = random.Next(1, 5),
            };

            providers.Add(provider1);
            providers.Add(provider2);
            providers.Add(provider3);

            return providers;
        }
    }
}