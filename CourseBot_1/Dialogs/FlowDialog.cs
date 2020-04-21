using CourseBot_1.Models;
using CourseBot_1.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Net.Mail;
using Microsoft.Bot.Schema;
using Microsoft.BotBuilderSamples;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using Microsoft.Bot.Builder.AI.QnA;

namespace CourseBot_1.Dialogs
{
    public class FlowDialog : ComponentDialog
    {

        private readonly BotStateService _botStateService;
        private readonly BotServices _botServices;
        private BotStateService botStateService;

        public FlowDialog(string dialogId, BotStateService botStateService, BotServices botServices) : base(dialogId)
        {
            _botStateService = botStateService ?? throw new System.ArgumentNullException(nameof(botStateService));
            _botServices = botServices ?? throw new System.ArgumentNullException(nameof(botServices));

            InitializeWaterfallDialog();
        }



        private void InitializeWaterfallDialog()
        {
            //Create waterfall steps
            var waterfallSteps = new WaterfallStep[]
            {
                BudgetStepAsync,
                BudgetLuisAsync,
                DurationStepAsync,
                DurationLuisAsync,
                StartworkStepAsync,
                PriceHStepAsync,
                PriceHLuisAsync,
                CommentStepAsync,
                EmailStepAsync,
                PolicyStepAsync,
                
            };

            //Types of subdialogs
            AddDialog(new WaterfallDialog($"{nameof(FlowDialog)}.mainFlow", waterfallSteps));
            AddDialog(new TextPrompt($"{nameof(FlowDialog)}.budget", BudgetValidatorAsync));
            AddDialog(new TextPrompt($"{nameof(FlowDialog)}.duration", DurationValidatorAsync));
            AddDialog(new DateTimePrompt($"{nameof(FlowDialog)}.start" ));
            AddDialog(new TextPrompt($"{nameof(FlowDialog)}.priceH", BudgetValidatorAsync));
            AddDialog(new TextPrompt($"{nameof(FlowDialog)}.comment"));
            AddDialog(new TextPrompt($"{nameof(FlowDialog)}.email", EmailStepValidatorAsync));
            AddDialog(new ChoicePrompt($"{nameof(FlowDialog)}.policy"));


            //Set the starting Dialog
            InitialDialogId = $"{nameof(FlowDialog)}.mainFlow";

        }

        #region Waterfall Steps

        private async Task<DialogTurnResult> BudgetStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync($"{nameof(FlowDialog)}.budget",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Do you have a budget in mind?"),
                    RetryPrompt = MessageFactory.Text("Invalid, till QnA")
                }, cancellationToken);

        }


        private async Task<DialogTurnResult> BudgetLuisAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Dispatch model to determine which cognitive service to use LUIS or QnA

            var result = await _botServices.Dispatch.RecognizeAsync(stepContext.Context, cancellationToken);
            var luisResult = result.Properties["luisResult"] as LuisResult;
            var entities = luisResult.Entities;
            var recognizerResult = await _botServices.Dispatch.RecognizeAsync(stepContext.Context, cancellationToken);
            //Top Intent tell us which cognitive service to use
            var topIntent = recognizerResult.GetTopScoringIntent();
            // OrganizationType.Any(s => s.Equals(entity.Entity, StringComparison.OrdinalIgnoreCase

            if (topIntent.intent == "QueryBudget")
            {
                
                foreach (var entity in luisResult.Entities)
                {
                    if (entity.Type == "money")
                    {
                       stepContext.Values["budget"] = entity.Entity;
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format("Okey, I understood your's budget!")), cancellationToken);


                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format(" I dont understand , and here should be QnA :D")), cancellationToken);
                    }

                }
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }



        private async Task<DialogTurnResult> DurationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        { 

            return await stepContext.PromptAsync($"{nameof(FlowDialog)}.duration",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("What is the anticipated duration of the project?"),
                    RetryPrompt = MessageFactory.Text("Invalid, till QnA"),
                }, cancellationToken);
        }

        private async Task<DialogTurnResult> DurationLuisAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Dispatch model to determine which cognitive service to use LUIS or QnA

            var result = await _botServices.Dispatch.RecognizeAsync(stepContext.Context, cancellationToken);
            var luisResult = result.Properties["luisResult"] as LuisResult;
            var entities = luisResult.Entities;
            var recognizerResult = await _botServices.Dispatch.RecognizeAsync(stepContext.Context, cancellationToken);
            //Top Intent tell us which cognitive service to use
            var topIntent = recognizerResult.GetTopScoringIntent();


            if (topIntent.intent == "QueryDuration")
            {
                foreach (var entity in luisResult.Entities)
                {
                    if (entity.Type == "Duration")
                    {
                        stepContext.Values["duration"] =  entity.Entity;
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format("Okey, I got it!")), cancellationToken);


                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format(" I dont understand , and here should be QnA :D")), cancellationToken);
                    }

                }
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> StartworkStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {


            //Convertig string to the DateTime format
            stepContext.Values["duration"] = (string)stepContext.Result;

            return await stepContext.PromptAsync($"{nameof(FlowDialog)}.start",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("How soon do you need your dev team to start working? Just give me the exact date "),
                    RetryPrompt = MessageFactory.Text("Value is not valid, try again."),
                }, cancellationToken);
        }

        private async Task<DialogTurnResult> PriceHStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //Convertig string to the DateTime format
            stepContext.Values["start"] = Convert.ToDateTime(((List<DateTimeResolution>)stepContext.Result).FirstOrDefault().Value);

            // Dispatch model to determine which cognitive service to use LUIS or QnA

            var result = await _botServices.Dispatch.RecognizeAsync(stepContext.Context, cancellationToken);
            var luisResult = result.Properties["luisResult"] as LuisResult;
            var entities = luisResult.Entities;
            var recognizerResult = await _botServices.Dispatch.RecognizeAsync(stepContext.Context, cancellationToken);
            //Top Intent tell us which cognitive service to use
            var topIntent = recognizerResult.GetTopScoringIntent();



            return await stepContext.PromptAsync($"{nameof(FlowDialog)}.priceH",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("What price per hour are you expecting to pay to your future development team? "),

                }, cancellationToken);


        }

        private async Task<DialogTurnResult> PriceHLuisAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Dispatch model to determine which cognitive service to use LUIS or QnA

            var result = await _botServices.Dispatch.RecognizeAsync(stepContext.Context, cancellationToken);
            var luisResult = result.Properties["luisResult"] as LuisResult;
            var entities = luisResult.Entities;
            var recognizerResult = await _botServices.Dispatch.RecognizeAsync(stepContext.Context, cancellationToken);
            //Top Intent tell us which cognitive service to use
            var topIntent = recognizerResult.GetTopScoringIntent();

            if (topIntent.intent == "QueryBudget")
            {

                foreach (var entity in luisResult.Entities)
                {
                    if (entity.Type == "money")
                    {
                        stepContext.Values["priceH"] = entity.Entity;
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format("Cool! Get it!")), cancellationToken);


                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format(" I dont understand , and here should be QnA :D")), cancellationToken);
                    }

                }
            }


          

            return await stepContext.NextAsync(null, cancellationToken);
        }



        private async Task<DialogTurnResult> CommentStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {


            return await stepContext.PromptAsync($"{nameof(FlowDialog)}.comment",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Any words about your project you'd like to leave?")
                }, cancellationToken);

        }

        private async Task<DialogTurnResult> EmailStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["comment"] = (string)stepContext.Result;

            return await stepContext.PromptAsync($"{nameof(FlowDialog)}.email",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("How should we get in touch? Give me your E-mail :) "),
                    RetryPrompt = MessageFactory.Text("Value is not valid, try again."),
                }, cancellationToken);

        }

        private async Task<DialogTurnResult> PolicyStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //Convertig string to the DateTime format
            stepContext.Values["email"] = (string)stepContext.Result;

            return await stepContext.PromptAsync($"{nameof(FlowDialog)}.policy",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("By clicking the button below, you agree that we, will process your personal information in accordance with our 'Privacy Policy' "),
                    Choices = ChoiceFactory.ToChoices(new List<string> { "Agree", "Cancel" }),
                }, cancellationToken);
        }







        #region Summary

        private async Task<DialogTurnResult> SummaryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            
            //Get the current profile object from userState.

            var userProfile = await _botStateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

            //Save all of the data inside the user profile

            userProfile.Budget = (string)stepContext.Values["budget"];
            userProfile.Duration = (DateTime)stepContext.Values["duration"];
            userProfile.Start = (DateTime)stepContext.Values["start"];
            userProfile.PriceH = (string)stepContext.Values["priceH"];
            userProfile.Comment = (string)stepContext.Values["comment"];
            userProfile.Email = (string)stepContext.Values["email"];
            userProfile.Policy = (string)stepContext.Values["polciy"];

            //Show the summary to the user
            
            //await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Here is a summary of your form : "), cancellationToken);
            //await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format("Budget : {0}",userProfile.Budget)), cancellationToken);
           // await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format("Duration : {0}", userProfile.Duration)), cancellationToken);
           // await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format("We will start in : {0}", userProfile.Start)), cancellationToken);
           // await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format("Price per hour : {0}", userProfile.PriceH)), cancellationToken);
           // await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format("Comment  : {0}", userProfile.Comment)), cancellationToken);
           // await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format("E-mail : {0}", userProfile.Email)), cancellationToken);
           // await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format("Policy status : {0}", userProfile.Policy)), cancellationToken);

            //Save data in userState

            await _botStateService.UserProfileAccessor.SetAsync(stepContext.Context, userProfile);

            //WaterfallStep always finishes with the end of the Waterfall or with another dialog, here is is the end

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);


        }
        #endregion

        #region Validators

        private Task<bool> EmailStepValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var valid = false;

            if (promptContext.Recognized.Succeeded)
            {
                valid = Regex.Match(promptContext.Recognized.Value, @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$").Success;
            }

            return Task.FromResult(valid);
        }



        private async Task<bool> BudgetValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            // Dispatch model to determine which cognitive service to use LUIS or QnA

            var recognizerResult = await _botServices.Dispatch.RecognizeAsync(promptContext.Context, cancellationToken);

            //Top Intent tell us which cognitive service to use
            var topIntent = recognizerResult.GetTopScoringIntent();

            var valid = false;

            if (promptContext.Recognized.Succeeded)
            {
                valid = topIntent.intent == "QueryBudget" && topIntent.score >= 0.45;
            }
            return valid;
        }

        private async Task<bool> DurationValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            // Dispatch model to determine which cognitive service to use LUIS or QnA

            var recognizerResult = await _botServices.Dispatch.RecognizeAsync(promptContext.Context, cancellationToken);

            //Top Intent tell us which cognitive service to use
            var topIntent = recognizerResult.GetTopScoringIntent();

            var valid = false;

            if (promptContext.Recognized.Succeeded)
            {
                valid = topIntent.intent == "QueryDuration" && topIntent.score >= 0.45;
            }
            return valid;
        }





        #endregion











        #endregion





    }
}
