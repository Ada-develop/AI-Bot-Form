using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FormBot.Models
{
    public class ConversationData
    {
        //Track whether we have already  asked the users name
        public bool PromptedUserForName { get; set; } = false;
    }
}
