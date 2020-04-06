
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CourseBot_1.Dialogs;
using CourseBot_1.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.BotBuilderSamples;

namespace BotAttachment.Dialogs
{

    public class AttachmentDialog : ComponentDialog
    {
        private readonly BotStateService _botStateService;
        private readonly BotServices _botServices;
        private BotStateService botStateService;

        public AttachmentDialog(string v, BotStateService botStateService, BotServices botServices) : base(nameof(AttachmentDialog))
        {
            _botStateService = botStateService ?? throw new System.ArgumentNullException(nameof(botStateService));
            _botServices = botServices ?? throw new System.ArgumentNullException(nameof(botServices));

            InitializeWaterfallDialog();
        }

        public AttachmentDialog(string dialogId, BotStateService botStateService = null) : base(dialogId)
        {
            this.botStateService = botStateService;
        }

        private void InitializeWaterfallDialog()
        {

            //Create Waterfall Steps | Bot's state bag
            var waterfallSteps = new WaterfallStep[]
            {

                AttachmentAsync,
                ConfirmStepAsync,
                NextStepAsync


            };

            //Add Named Dialogs , adding to the bot's state bag
            AddDialog(new FlowDialog($"{nameof(AttachmentDialog)}.flowDialog", _botStateService, _botServices));
            AddDialog(new WaterfallDialog($"{nameof(AttachmentDialog)}.mainFlow", waterfallSteps));
            AddDialog(new TextPrompt($"{nameof(AttachmentDialog)}.attachement"));
            AddDialog(new AttachmentPrompt(nameof(AttachmentPrompt), PicturePromptValidatorAsync));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));

            // Set the starting Dialog 

            InitialDialogId = $"{nameof(AttachmentDialog)}.mainFlow";


        }


        private static async Task<DialogTurnResult> AttachmentAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("That is awesome! You can upload your screens below."), cancellationToken);

            if (stepContext.Context.Activity.ChannelId == Channels.Msteams)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Skipping attachment prompt in Teams channel..."), cancellationToken);
                return await stepContext.NextAsync(null, cancellationToken);
            }
            else
            {
                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text("Please attach a project picture (or type any message to skip)."),
                    RetryPrompt = MessageFactory.Text("The attachment must be a jpeg/png image file."),
                };

                return await stepContext.PromptAsync(nameof(AttachmentPrompt), promptOptions, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

           
            stepContext.Values["attachment"] = ((IList<Attachment>)stepContext.Result)?.FirstOrDefault();

            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text("Is this ok?") }, cancellationToken);

        }

        private async Task<DialogTurnResult> NextStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            return await stepContext.EndDialogAsync(null, cancellationToken);
           
        }


        private static async Task<bool> PicturePromptValidatorAsync(PromptValidatorContext<IList<Attachment>> promptContext, CancellationToken cancellationToken)
        {
            if (promptContext.Recognized.Succeeded)
            {
                var attachments = promptContext.Recognized.Value;
                var validImages = new List<Attachment>();

                foreach (var attachment in attachments)
                {
                    if (attachment.ContentType == "image/jpeg" || attachment.ContentType == "image/png")
                    {
                        validImages.Add(attachment);
                    }
                }

                promptContext.Recognized.Value = validImages;

           
                return validImages.Any();
            }
            else
            {
                await promptContext.Context.SendActivityAsync("No attachments received. Proceeding without a picture...");

         
                return true;
            }
        }


    }

}
