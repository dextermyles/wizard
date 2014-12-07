using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WizardGame.Helpers
{
    [Serializable]
    public class User
    {
        public int UserId = 0;
        public string Username = string.Empty;
        public string Password = string.Empty;
        public string EmailAddress = string.Empty;
        public DateTime? DateCreated = null;
        public bool Active = false;
        public string FB_UserId = string.Empty;
        public DateTime? FB_SyncDate = null;
    }
}