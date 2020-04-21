using BotAttachment.Dialogs;
using CourseBot_1.Dialogs;
using CourseBot_1.Models;
using CourseBot_1.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.BotBuilderSamples;
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

            AddDialog(new GreetingDialog($"{nameof(ConnectorDialog)}.greeting", _botStateService, _botServices ));
            AddDialog(new FlowDialog($"{nameof(ConnectorDialog)}.flowDialog", _botStateService, _botServices));
            AddDialog(new FinalDevDialog($"{nameof(ConnectorDialog)}.final", _botStateService,_botServices));
            AddDialog(new WaterfallDialog($"{nameof(ConnectorDialog)}.mainFlow", waterfallSteps));
            AddDialog(new AttachmentDialog($"{nameof(ConnectorDialog)}.attach", _botStateService,_botServices));
            AddDialog(new TextPrompt($"{nameof(ConnectorDialog)}.branch", BranchValidatorAsync));


            InitialDialogId = $"{nameof(ConnectorDialog)}.mainFlow";
        }




        private async Task<DialogTurnResult> InitialAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync($"{nameof(ConnectorDialog)}.greeting", null, cancellationToken);




        }








        private async Task<DialogTurnResult> BranchAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Dispatch model to determine which cognitive service to use LUIS or QnA

            var recognizerResult = await _botServices.Dispatch.RecognizeAsync(stepContext.Context, cancellationToken);

            //Top Intent tell us which cognitive service to use
            var topIntent = recognizerResult.GetTopScoringIntent();

            var options = new QnAMakerOptions { Top = 1 };
            var qna = await _botServices.SampleQnA.GetAnswersAsync(stepContext.Context);


            if (topIntent.intent == "QueryStageFinal" && topIntent.score >= 0.15)
            {
                return await stepContext.BeginDialogAsync($"{nameof(ConnectorDialog)}.final", null, cancellationToken);
            }
            else if (topIntent.intent == "QueryStageMid" && topIntent.score >= 0.15)
            {
                return await stepContext.BeginDialogAsync($"{nameof(ConnectorDialog)}.attach", null, cancellationToken);
            }
            else if (topIntent.intent == "QueryStage" && topIntent.score >= 0.15)
            {
                return await stepContext.BeginDialogAsync($"{nameof(ConnectorDialog)}.flowDialog", null, cancellationToken);
            }
            else if (qna != null && qna.Length > 0)
            {
 
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(qna[0].Answer), cancellationToken);
                return  new DialogTurnResult(DialogTurnStatus.Waiting) ;
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




        #region Validator

        private async Task<bool> BranchValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            // Dispatch model to determine which cognitive service to use LUIS or QnA

            var recognizerResult = await _botServices.Dispatch.RecognizeAsync(promptContext.Context, cancellationToken);


            //Top Intent tell us which cognitive service to use
            var topIntent = recognizerResult.GetTopScoringIntent();

            var valid = false;

            if (promptContext.Recognized.Succeeded)
            {
                valid = topIntent.intent == "QueryStage" || topIntent.intent == "QueryStageMid" || topIntent.intent == "QueryStageFinal";
            }

            return valid;
        }

        #endregion
    }
}
