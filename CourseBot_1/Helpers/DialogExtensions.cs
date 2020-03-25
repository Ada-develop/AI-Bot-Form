using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CourseBot_1.Helpers
{
    public static class DialogExtensions
    {
        //ITurnContext Istatepropertyaccessor and cancellationtoken will be use to create dialog context
        //Use to create acces to dialogs
        public static async Task Run(this Dialog dialog, ITurnContext turnContext, IStatePropertyAccessor<DialogState> accessor, CancellationToken cancellationToken)
        {
            //DialogSet
            var dialogSet = new DialogSet(accessor);
            dialogSet.Add(dialog);

            //Createing context used to interact with the dialogSet variable :
            var dialogContext = await dialogSet.CreateContextAsync(turnContext, cancellationToken);
            var results = await dialogContext.ContinueDialogAsync(cancellationToken);
            if (results.Status == DialogTurnStatus.Empty)
            {
                //Allows to start dialog with the ID
                await dialogContext.BeginDialogAsync(dialog.Id, null, cancellationToken);
            }
        }


    }
}
