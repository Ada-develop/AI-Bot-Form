using BotAttachment.Dialogs;
using CourseBot_1.Helpers;
using CourseBot_1.Models;
using CourseBot_1.Services;
using LuisModel;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
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
    public class GreetingDialog : ComponentDialog //Component = for reusing dialogs like container
    {

        private readonly BotStateService _botStateService;
        private readonly BotServices _botServices;


        public GreetingDialog(string dialogId, BotStateService botStateService, BotServices botServices) : base(dialogId)
        {
            _botStateService = botStateService ?? throw new System.ArgumentNullException(nameof(botStateService));
            _botServices = botServices ?? throw new System.ArgumentNullException(nameof(botServices));

            InitializeWaterfallDialog();
        }


        #region Waterfall and dialog saved data
        private void InitializeWaterfallDialog()
        {

            //Create Waterfall Steps | Bot's state bag
            var waterfallSteps = new WaterfallStep[]
            {

                NameStepAsync,
                NiceStepAsync,
                OrganizationStepAsync,
                DevelopementStepAsync,
                BranchesStepAsync,
                NextDialogasync,              
                //SumStepAsync,


            };

            //Add Named Dialogs , adding to the bot's state bag
            AddDialog(new FlowDialog($"{nameof(GreetingDialog)}.flowDialog", _botStateService, _botServices));
            AddDialog(new WaterfallDialog($"{nameof(GreetingDialog)}.mainFlow", waterfallSteps));
            AddDialog(new FinalDevDialog($"{nameof(GreetingDialog)}.final", _botStateService, _botServices));
            AddDialog(new AttachmentDialog($"{nameof(GreetingDialog)}.attach", _botStateService, _botServices));
            AddDialog(new TextPrompt($"{nameof(GreetingDialog)}.name"));
            AddDialog(new TextPrompt($"{nameof(GreetingDialog)}.organization", OrgValidatorAsync));
            AddDialog(new TextPrompt($"{nameof(GreetingDialog)}.developement", DevValidatorAsync));
            AddDialog(new TextPrompt($"{nameof(GreetingDialog)}.branch"));

            //AddDialog(new QnADialog($"{ nameof(GreetingDialog)}.qna"));
            
            // Set the starting Dialog 

            InitialDialogId = $"{nameof(GreetingDialog)}.mainFlow";


        }
        #endregion

        #region WATERFALL



        //Waterfall inject, checking or we have users name in userProfile , if don't have kicks PromptAsync  and Prompt for name
        //Else NextAsync
        private async Task<DialogTurnResult> NameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            UserProfile userProfile = await _botStateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile());

            if (string.IsNullOrEmpty(userProfile.Name))
            {
                return await stepContext.PromptAsync($"{nameof(GreetingDialog)}.name",
                    new PromptOptions
                    {
                        Prompt = MessageFactory.Text("What is your name?")
                    }, cancellationToken);
            }
            else
            {
                return await stepContext.NextAsync(null, cancellationToken);
            }
        }


        private async Task<DialogTurnResult> NiceStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            UserProfile userProfile = await _botStateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile());
            if (string.IsNullOrEmpty(userProfile.Name))
            {
                //Set the name from previous step
                userProfile.Name = (string)stepContext.Result;

                //Save any state changes that might have occured during the turn.

                await _botStateService.UserProfileAccessor.SetAsync(stepContext.Context, userProfile);
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format("Nice to meet you {0}. ", userProfile.Name)), cancellationToken);
            }
            else
            {

                await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format("Welcome back! {0}. ", userProfile.Name)), cancellationToken);
            }
            //Comunication:
            return await stepContext.NextAsync(null, cancellationToken);
        }



    private async Task<DialogTurnResult> OrganizationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
            {         
            return await stepContext.PromptAsync($"{nameof(GreetingDialog)}.organization",
                    new PromptOptions
                  {
                      Prompt = MessageFactory.Text("What type of organization do you represent?"),
                     RetryPrompt = MessageFactory.Text("Invalid"),
                 }, cancellationToken);
        }


        private async Task<DialogTurnResult> DevelopementStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            

            var result = await _botServices.Dispatch.RecognizeAsync(stepContext.Context, cancellationToken);
            var luisResult = result.Properties["luisResult"] as LuisResult;
            var entities = luisResult.Entities;
            var recognizerResult = await _botServices.Dispatch.RecognizeAsync(stepContext.Context, cancellationToken);
            //Top Intent tell us which cognitive service to use
            var topIntent = recognizerResult.GetTopScoringIntent();
            

            foreach (var entity in luisResult.Entities)
            {
                if(entity.Type == "OrganizationType")
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format("Wow ! {0}, I guess funny to work there!", entity.Entity)), cancellationToken);
                    stepContext.Values["organization"] = (string)stepContext.Result == entity.Entity;

                }
                else
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format(" I dont understand , and here should be QnA :D")), cancellationToken);
                }

            }

            return await stepContext.PromptAsync($"{nameof(GreetingDialog)}.developement",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Great! What type of project do you want to develop?"),
                    RetryPrompt = MessageFactory.Text("Value is not valid, try again."),
                }, cancellationToken);           
        }

      

        private async Task<DialogTurnResult> BranchesStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            
            var result = await _botServices.Dispatch.RecognizeAsync(stepContext.Context, cancellationToken);
            var luisResult = result.Properties["luisResult"] as LuisResult;
            var entities = luisResult.Entities;
            



            foreach (var entity in luisResult.Entities)
            {
                if (entity.Type == "DevType")
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format("Nice ! You have kinda interesting idea, to create a {0}", entity.Entity)), cancellationToken);
                    stepContext.Values["developement"] = (string)stepContext.Result == entity.Entity;
                }
                else
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format("LOL! I dont understand , and here should be QnA :D")), cancellationToken);
                }

            }
            
            {
                return await stepContext.PromptAsync($"{nameof(ConnectorDialog)}.branch",
                   new PromptOptions
                   {
                       Prompt = MessageFactory.Text("OK. What is the current stage of your project? "),
                       RetryPrompt = MessageFactory.Text("Value is not valid, try again."),

                   }, cancellationToken);

            }
        }


        #endregion


        private async Task<DialogTurnResult> NextDialogasync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        

        #region VALIDATORS


        private async Task<bool> OrgValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            // Dispatch model to determine which cognitive service to use LUIS or QnA

            var recognizerResult = await _botServices.Dispatch.RecognizeAsync(promptContext.Context, cancellationToken);
            

            //Top Intent tell us which cognitive service to use
            var topIntent = recognizerResult.GetTopScoringIntent();

            var valid = false;

            if (promptContext.Recognized.Succeeded)
            {
                valid = topIntent.intent == "QueryOrg" && topIntent.score >= 0.45;
            }
           
            return valid;
        }

        private async Task<bool> DevValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            // Dispatch model to determine which cognitive service to use LUIS or QnA

            var recognizerResult = await _botServices.Dispatch.RecognizeAsync(promptContext.Context, cancellationToken);

            //Top Intent tell us which cognitive service to use
            var topIntent = recognizerResult.GetTopScoringIntent();

            var valid = false;

            if (promptContext.Recognized.Succeeded)
            {
                valid = topIntent.intent == "QueryDev" && topIntent.score >= 0.45;
            }
            return valid;
        }

        #endregion


        #region Don't used part 
        /// Don't used part : 
        private async Task<DialogTurnResult> SumStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["branch"] = ((FoundChoice)stepContext.Result).Value;

            //Get the current profile object from userState.

            var userProfile = await _botStateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

            //Save all of the data inside the user profile

            userProfile.Organization = (string)stepContext.Values["organiztion"];
            userProfile.Developement = (string)stepContext.Values["developement"];
            userProfile.Branches = (string)stepContext.Values["branches"];

            await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format("Organization : {0}", userProfile.Organization)), cancellationToken);

            await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format("Developement : {0}", userProfile.Developement)), cancellationToken);

            await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format("Branches : {0}", userProfile.Branches)), cancellationToken);


            await _botStateService.UserProfileAccessor.SetAsync(stepContext.Context, userProfile);

            //WaterfallStep always finishes with the end of the Waterfall or with another dialog, here is is the end

            return await stepContext.BeginDialogAsync(nameof(FlowDialog),null,cancellationToken);
        }
        #endregion

    }
}
