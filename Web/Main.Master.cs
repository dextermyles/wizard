using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WizardGame.Helpers;
namespace WizardGame
{
    public partial class Main : System.Web.UI.MasterPage
    {
        private bool isValidSession = false;

        public bool IsSessionValid()
        {
            // is valid session
            if (!isValidSession)
                isValidSession = Functions.IsValidSession();

            return isValidSession;
        }
    }
}