using CourseBot_1.Models;
using CourseBot_1.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
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
                Final
            };

            AddDialog(new GreetingDialog($"{nameof(ConnectorDialog)}.greeting", _botStateService));
            AddDialog(new MainDialog($"{nameof(ConnectorDialog)}.main", _botStateService));
            AddDialog(new WaterfallDialog($"{nameof(ConnectorDialog)}.mainFlow"));

            InitialDialogId = $"{nameof(ConnectorDialog)}.mainFlow";
        }


        private async Task<DialogTurnResult> Initial(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if(Regex.Match(stepContext.Context.Activity.Text.ToLower(), "hi").Success)
            {
                return await stepContext.BeginDialogAsync($"{nameof(ConnectorDialog)}.greeting", null, cancellationToken);
            }
            else
            {
                return await stepContext.BeginDialogAsync($"{nameof(ConnectorDialog)}.main", null, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> Final(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
