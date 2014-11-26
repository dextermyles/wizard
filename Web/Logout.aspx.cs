using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WizardGame.Services;

namespace WizardGame
{
    public partial class Logout : System.Web.UI.Page
    {
        // service
        WizardService wizWS = new WizardService();

        protected void Page_Load(object sender, EventArgs e)
        {
            // check for existing cookie
            HttpCookie cookie = Request.Cookies["OfficeWizard"];
            
            // cookie exists
            if (cookie != null)
            {
                // get secret from cookie
                string secret = cookie.Values["secret"];

                if (!string.IsNullOrEmpty(secret))
                {
                    // delete db session
                    wizWS.DeleteSession(secret);
                }

                // force cookie expiry
                cookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(cookie);
            }

            // redirect user
            Response.Redirect("~/Default.aspx");
        }
    }
}