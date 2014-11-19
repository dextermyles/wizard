using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WizardGame.Services;

namespace WizardGame.Auto
{
    public partial class Cron : System.Web.UI.Page
    {
        // service
        WizardService wizWS = new WizardService();

        protected void Page_Load(object sender, EventArgs e)
        {
            // remove old sessions
            DeleteOldSessions();
        }

        public void DeleteOldSessions()
        {
            // output
            Response.Write("removing old sessions...");

            // remove old sessions
            wizWS.DeleteOldSessions();

            // output
            Response.Write("done.\r\n");
        }
    }
}