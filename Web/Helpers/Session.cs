using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WizardGame.Helpers
{
    public class Session
    {
        public int SessionId = 0;
        public DateTime? DateCreated = null;
        public DateTime? DateLastActive = null;
        public int UserId = 0;
        public int PlayerId = 0;
        public string IpAddress = string.Empty;
        public string Secret = string.Empty;
        public string ConnectionId = string.Empty;
    }
}