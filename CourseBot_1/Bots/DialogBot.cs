using CourseBot_1.Helpers;
using CourseBot_1.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CourseBot_1.Bots
{
    public class DialogBot<T> : ActivityHandler where T : Dialog
    {
        #region Variables
        //Variables 
        //Dialog = that we going to implement
        protected readonly Dialog _dialog;
        //Instance of BotService : 
        protected readonly BotStateService _botStateService;
        //Logger object:
        protected readonly ILogger _logger;
        #endregion


        //Injecting Variables botservice, dialog type and logger object, checking for nulls and setting the to private variables
        public DialogBot(BotStateService botStateService, T dialog, ILogger<DialogBot<T>> logger)
        {
            _botStateService = botStateService ?? throw new System.ArgumentNullException(nameof(botStateService));
            _dialog = dialog ?? throw new System.ArgumentNullException(nameof(dialog));
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            //Base class implementation, responsible for forrwarding all of the message types 
            //If it wasnt here OnMessageActivity will never be called
            await base.OnTurnAsync(turnContext, cancellationToken);

            //State data saves:
            await _botStateService.UserState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _botStateService.ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            //Log info to logger

            _logger.LogInformation("Running dialog with Message Activity.");

            //Run the dialog with the new message Activity
            await _dialog.Run(turnContext, _botStateService.DialogStateAccessor, cancellationToken);
        }



    }
}
