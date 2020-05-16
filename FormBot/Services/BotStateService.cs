using FormBot.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FormBot.Services
{
    public class BotStateService
    {
        #region Variables

        //State Variables
        public ConversationState ConversationState { get; }

        //User State variable
        public UserState UserState { get; }

        //Id's

        public static string UserProfileId { get; } = $"{nameof(BotStateService)}.UserProfile";

        public static string ConversationDataId { get; } = $"{nameof(BotStateService)}.ConversationData";

        public static string DialogStateId { get; } = $"{nameof(BotStateService)}.DialogState";

        //Accessor:

        public IStatePropertyAccessor<UserProfile> UserProfileAccessor { get; set; }

        public IStatePropertyAccessor<ConversationData> ConversationDataAccessor { get; set; }

        public IStatePropertyAccessor<DialogState> DialogStateAccessor { get; set; }

        #endregion

        public BotStateService(ConversationState conversationState,UserState userState)
        {

            ConversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));

            UserState = userState ?? throw new ArgumentNullException(nameof(userState));

            IntializeAccessors();
        }

        public void IntializeAccessors()
        {
            //Initialize ConversationState accessors:

            ConversationDataAccessor = ConversationState.CreateProperty<ConversationData>(ConversationDataId);
            DialogStateAccessor = ConversationState.CreateProperty<DialogState>(DialogStateId);

            //Initialize User State

            UserProfileAccessor = UserState.CreateProperty<UserProfile>(UserProfileId);
        }
    }
}
