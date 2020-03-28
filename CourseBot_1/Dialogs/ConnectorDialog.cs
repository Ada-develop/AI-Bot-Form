using BotAttachment.Dialogs;
using CourseBot_1.Dialogs;
using CourseBot_1.Models;
using CourseBot_1.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CourseBot_1.Dialogs
{
    public class ConnectorDialog : ComponentDialog
    {
        private readonly BotStateService _botStateService;


        public ConnectorDialog(BotStateService botStateService) : base(nameof(ConnectorDialog))
        {
            _botStateService = botStateService ?? throw new System.ArgumentNullException(nameof(botStateService));

            InitializeWaterfallDialog();
        }



        private void InitializeWaterfallDialog()
        {
            var waterfallSteps = new WaterfallStep[]
            {
                Initial,
                Branch,
                //Question,
                Final
            };

            AddDialog(new GreetingDialog($"{nameof(ConnectorDialog)}.greeting", _botStateService));
            AddDialog(new FlowDialog($"{nameof(ConnectorDialog)}.flowDialog", _botStateService));
            AddDialog(new FinalDevDialog($"{nameof(ConnectorDialog)}.final", _botStateService));
            AddDialog(new WaterfallDialog($"{nameof(ConnectorDialog)}.mainFlow", waterfallSteps));
            AddDialog(new AttachmentDialog($"{nameof(ConnectorDialog)}.attach", _botStateService));
            AddDialog(new TextPrompt($"{nameof(ConnectorDialog)}.branch"));


            InitialDialogId = $"{nameof(ConnectorDialog)}.mainFlow";
        }




        private async Task<DialogTurnResult> Initial(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
            {
                foreach (var member in membersAdded)
                {
                    var welcomeText = "Hi there! Bot from BgTeams greetings you!";
                    if (member.Id != turnContext.Activity.Recipient.Id)
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);

                    }
                }


            }

            return await stepContext.BeginDialogAsync($"{nameof(ConnectorDialog)}.greeting", null, cancellationToken);




        }

        private async Task<DialogTurnResult> Question(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync($"{nameof(ConnectorDialog)}.branch",
               new PromptOptions
               {
                   Prompt = MessageFactory.Text("OK. What is the current stage of your project? "),
                   RetryPrompt = MessageFactory.Text("Value is not valid, try again."),

               }, cancellationToken);

        }







        private async Task<DialogTurnResult> Branch(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {


            if (Regex.Match(stepContext.Context.Activity.Text.ToLower(), "final").Success)
            {
                return await stepContext.BeginDialogAsync($"{nameof(ConnectorDialog)}.final", null, cancellationToken);
            }
            else if (Regex.Match(stepContext.Context.Activity.Text.ToLower(), "mid").Success)
            {
                return await stepContext.BeginDialogAsync($"{nameof(ConnectorDialog)}.attach", null, cancellationToken);
            }
            else
            {
                return await stepContext.BeginDialogAsync($"{nameof(ConnectorDialog)}.flowDialog", null, cancellationToken);
            };
        }





        private async Task<DialogTurnResult> Final(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync($"{nameof(ConnectorDialog)}.flowDialog", null, cancellationToken);
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
