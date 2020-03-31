
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

namespace BotAttachment.Dialogs
{

    public class AttachmentDialog : ComponentDialog
    {
        private readonly BotStateService _botStateService; //Importing and injecting BotService to constructor down below


        //DialogId = each dialog would have ID to indicate
        public AttachmentDialog(string dialogId, BotStateService botStateService) : base(dialogId)
        {
            _botStateService = botStateService ?? throw new System.ArgumentNullException(nameof(botStateService));

            InitializeWaterfallDialog();
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
            AddDialog(new FlowDialog($"{nameof(AttachmentDialog)}.flowDialog", _botStateService));
            AddDialog(new WaterfallDialog($"{nameof(AttachmentDialog)}.mainFlow", waterfallSteps));
            AddDialog(new TextPrompt($"{nameof(AttachmentDialog)}.attachement"));
            AddDialog(new AttachmentPrompt(nameof(AttachmentPrompt), PicturePromptValidatorAsync));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));

            // Set the starting Dialog 

            InitialDialogId = $"{nameof(AttachmentDialog)}.mainFlow";


        }


        private static async Task<DialogTurnResult> AttachmentAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            
            // We can send messages to the user at any point in the WaterfallStep.
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("That is awesome! You can upload your screens below."), cancellationToken);

            if (stepContext.Context.Activity.ChannelId == Channels.Msteams)
            {
                // This attachment prompt example is not designed to work for Teams attachments, so skip it in this case
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Skipping attachment prompt in Teams channel..."), cancellationToken);
                return await stepContext.NextAsync(null, cancellationToken);
            }
            else
            {
                // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text("Please attach a profile picture (or type any message to skip)."),
                    RetryPrompt = MessageFactory.Text("The attachment must be a jpeg/png image file."),
                };

                return await stepContext.PromptAsync(nameof(AttachmentPrompt), promptOptions, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

           
            stepContext.Values["attachment"] = ((IList<Attachment>)stepContext.Result)?.FirstOrDefault();

            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text("Is this ok?") }, cancellationToken);

        }

        private async Task<DialogTurnResult> NextStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            return await stepContext.EndDialogAsync(null, cancellationToken);
            //return await stepContext.BeginDialogAsync($"{nameof(FinalDevDialog)}.flowDialog", null, cancellationToken);

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

                // If none of the attachments are valid images, the retry prompt should be sent.
                return validImages.Any();
            }
            else
            {
                await promptContext.Context.SendActivityAsync("No attachments received. Proceeding without a picture...");

                // We can return true from a validator function even if Recognized.Succeeded is false.
                return true;
            }
        }


    }

}
