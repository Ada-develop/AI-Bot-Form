using CourseBot_1.Models;
using CourseBot_1.Services;
using Microsoft.Bot.Builder;
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

    public class FinalDevDialog : ComponentDialog
    {


        private readonly BotStateService _botStateService;
        private readonly BotServices _botServices;
        private BotStateService botStateService;

        public FinalDevDialog(string dialogId, BotStateService botStateService, BotServices botServices) : base(dialogId)
        {
            _botStateService = botStateService ?? throw new System.ArgumentNullException(nameof(botStateService));
            _botServices = botServices ?? throw new System.ArgumentNullException(nameof(botServices));

            InitializeWaterfallDialog();
        }


        private void InitializeWaterfallDialog()
        {

            //Create Waterfall Steps | Bot's state bag
            var waterfallSteps = new WaterfallStep[]
            {

                TechStepAsync,
                WebsiteStepAsync,
                NextStepAsync,
                //SumStepAsync,


            };

            //Add Named Dialogs , adding to the bot's state bag
            AddDialog(new FlowDialog($"{nameof(FinalDevDialog)}.flowDialog", _botStateService,_botServices));
            AddDialog(new WaterfallDialog($"{nameof(FinalDevDialog)}.mainFlow", waterfallSteps));
            AddDialog(new TextPrompt($"{nameof(FinalDevDialog)}.tech"));
            AddDialog(new TextPrompt($"{nameof(FinalDevDialog)}.website", WebValidatorAsync));
            // Set the starting Dialog 

            InitialDialogId = $"{nameof(FinalDevDialog)}.mainFlow";


        }


        //Waterfall inject, checking or we have users name in userProfile , if don't have kicks PromptAsync  and Prompt for name
        //Else NextAsync




        private async Task<DialogTurnResult> TechStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync($"{nameof(FinalDevDialog)}.tech",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("That is awesome! Can you describe the technologies you are using and the dev team you have.")
                }, cancellationToken);


        }




        private async Task<DialogTurnResult> WebsiteStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["tech"] = (string)stepContext.Result;





            return await stepContext.PromptAsync($"{nameof(FinalDevDialog)}.website",
                
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Thanks!\nCan you share a link to your project or website, please :)"),
                    RetryPrompt = MessageFactory.Text("Write link , for example https://www.google.com/ , or just write 'Skip' word :)"),
                }, cancellationToken);


        }

        private async Task<DialogTurnResult> NextStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            

                stepContext.Values["website"] = (string)stepContext.Result;


            


            return await stepContext.EndDialogAsync(null, cancellationToken);






        }

        private async Task<DialogTurnResult> NextDialogasync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {


            var userProfile = await _botStateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);


          

            userProfile.Tech = (string)stepContext.Values["tech"];
            userProfile.Website = (string)stepContext.Values["website"];

            //Save data in userState

            await _botStateService.UserProfileAccessor.SetAsync(stepContext.Context, userProfile);

            //WaterfallStep always finishes with the end of the Waterfall or with another dialog, here is is the end

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }



        private Task<bool> WebValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var valid = false;

            if (promptContext.Recognized.Succeeded)
            {
                valid = Regex.Match(promptContext.Recognized.Value, @"^http(s) ?://([\w-]+.)+[\w-]+(/[\w- ./?%&=])?$").Success;
            }

            return Task.FromResult(valid);
        }

    }
}
