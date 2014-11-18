using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WizardGame.Helpers;

namespace WizardGame
{
    public partial class Home : System.Web.UI.Page
    {
        protected override void OnLoad(EventArgs e)
        {
            // validate function
            if (!Functions.IsValidSession())
            {
                // redirect to login page
                Response.Redirect("~/Default.aspx");
            }

            base.OnLoad(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            
        }
    }
}