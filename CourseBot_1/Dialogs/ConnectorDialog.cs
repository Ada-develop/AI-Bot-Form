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
        private readonly BotServices _botServices;


        public ConnectorDialog(BotStateService botStateService, BotServices botServices) : base(nameof(ConnectorDialog))
        {
            _botStateService = botStateService ?? throw new System.ArgumentNullException(nameof(botStateService));
            _botServices = botServices ?? throw new System.ArgumentNullException(nameof(botServices));

            InitializeWaterfallDialog();
        }



        private void InitializeWaterfallDialog()
        {
            var waterfallSteps = new WaterfallStep[]
            {
                InitialAsync,
                BranchAsync,
                //Question,
                FinalAsync,
                GoodByeAsync
            };

            AddDialog(new GreetingDialog($"{nameof(ConnectorDialog)}.greeting", _botStateService));
            AddDialog(new FlowDialog($"{nameof(ConnectorDialog)}.flowDialog", _botStateService));
            AddDialog(new FinalDevDialog($"{nameof(ConnectorDialog)}.final", _botStateService));
            AddDialog(new WaterfallDialog($"{nameof(ConnectorDialog)}.mainFlow", waterfallSteps));
            AddDialog(new AttachmentDialog($"{nameof(ConnectorDialog)}.attach", _botStateService));
            AddDialog(new TextPrompt($"{nameof(ConnectorDialog)}.branch"));


            InitialDialogId = $"{nameof(ConnectorDialog)}.mainFlow";
        }




        private async Task<DialogTurnResult> InitialAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync($"{nameof(ConnectorDialog)}.greeting", null, cancellationToken);




        }

        private async Task<DialogTurnResult> QuestionAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync($"{nameof(ConnectorDialog)}.branch",
               new PromptOptions
               {
                   Prompt = MessageFactory.Text("OK. What is the current stage of your project? "),
                   RetryPrompt = MessageFactory.Text("Value is not valid, try again."),

               }, cancellationToken);

        }







        private async Task<DialogTurnResult> BranchAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Dispatch model to determine which cognitive service to use LUIS or QnA

            var recognizerResult = await _botServices.Dispatch.RecognizeAsync(stepContext.Context, cancellationToken);

            //Top Intent tell us which cognitive service to use
            var topIntent = recognizerResult.GetTopScoringIntent();

            switch (topIntent.intent)
            {
                case "QueryStageFinal":
                    return await stepContext.BeginDialogAsync($"{nameof(ConnectorDialog)}.final", null, cancellationToken);
                case "QueryStageMid":
                    return await stepContext.BeginDialogAsync($"{nameof(ConnectorDialog)}.attach", null, cancellationToken);
                case "QueryStage":
                    return await stepContext.BeginDialogAsync($"{nameof(ConnectorDialog)}.flowDialog", null, cancellationToken);
                default:
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"I'm sorry I don't know what you mean"), cancellationToken);
                    break;
            }
            return await stepContext.NextAsync(null, cancellationToken);
        }





        private async Task<DialogTurnResult> FinalAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync($"{nameof(ConnectorDialog)}.flowDialog", null, cancellationToken);
            
        }
        private async Task<DialogTurnResult> GoodByeAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
             return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
