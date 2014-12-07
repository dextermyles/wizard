using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WizardGame.Helpers
{
    [Serializable]
    public class NewUserResult
    {
        public bool Result = false;
        public string Secret = string.Empty;
        public string Message = string.Empty;
    }
}