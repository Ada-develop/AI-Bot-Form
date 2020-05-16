using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace FormBot.Models
{
    public class UserProfile
    {
        public string Name { get; set; }

        public string Organization { get; set; }

        public string Developement { get; set; }
        public string Branches { get; set; }
        public string Budget { get; set; }
        public DateTime Duration { get; set; }
        public DateTime Start { get; set; }
        public string PriceH { get; set; }
        public string Comment { get; set; }
        public string Email { get; set; }
        public string Policy { get; set; }

        public Attachment Attachment { get; set; }
        public string Tech { get; set; }

        public string Website { get; set; }




    }
}
