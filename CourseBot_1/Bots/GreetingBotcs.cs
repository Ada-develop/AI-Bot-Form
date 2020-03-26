using CourseBot_1.Dialogs;
using CourseBot_1.Models;
using CourseBot_1.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CourseBot_1.Bots
{
    public class GreetingBotcs : ActivityHandler
    {
        // Import BotStateService
        #region Variables
        private readonly BotStateService _botStateService;

        #endregion

        //Inject BotStateservise
        public GreetingBotcs(BotStateService botStateService)
        {
            _botStateService = botStateService ?? throw new System.ArgumentNullException(nameof(botStateService));
        }
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await GetName(turnContext, cancellationToken);
            
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach( var member in membersAdded)
            {
                var welcomeText = "Hi there! Bot from BgTeams greetings you!";
                if(member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                    await GetName(turnContext, cancellationToken);
                }
            }

 
        }

        private async Task GetName(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            //GetAsync = if UserProfile is not instantiated yet, it's  going yo do by using function call
            //Retrieving UserProfile and ConversationData
            UserProfile userProfile = await _botStateService.UserProfileAccessor.GetAsync(turnContext, () => new UserProfile());
            ConversationData conversationData = await _botStateService.ConversationDataAccessor.GetAsync(turnContext, () => new ConversationData());
            
            if (!string.IsNullOrEmpty(userProfile.Name))
            {
                await turnContext.SendActivityAsync(MessageFactory.Text(String.Format("Hi {0}. How can I help you today?", userProfile.Name)), cancellationToken);
            }
            else
            {//If we have variable of name just Greeting, else asking for the name , Prompt = false == dont' write value to the variable

                if(conversationData.PromptedUserForName)
                {
                    //Set the name to what the user provided
                    userProfile.Name = turnContext.Activity.Text?.Trim();

                    //Acknowledge that we got their name

                    await turnContext.SendActivityAsync(MessageFactory.Text(String.Format("Nice to meet you {0}.", userProfile.Name)), cancellationToken);

                    conversationData.PromptedUserForName = false;

                }
                else
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"What is your name?"), cancellationToken);

                    conversationData.PromptedUserForName = true;
                }


                //Setting UserProfile and conversation data and save it

                await _botStateService.UserProfileAccessor.SetAsync(turnContext, userProfile);
                await _botStateService.ConversationDataAccessor.SetAsync(turnContext, conversationData);

                await _botStateService.UserState.SaveChangesAsync(turnContext);
                await _botStateService.ConversationState.SaveChangesAsync(turnContext);
            }
            


        }

    }
}
