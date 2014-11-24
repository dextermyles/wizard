using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WizardGame.WizardService;

namespace WizardGame
{
    public partial class Logout : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // check for existing cookie
            HttpCookie cookie = HttpContext.Current.Request.Cookies["OfficeWizard"];
            
            // cookie exists
            if (cookie != null)
            {
                // get secret from cookie
                string secret = cookie.Values["secret"];

                if (!string.IsNullOrEmpty(secret))
                {
                    // service
                    WizardServiceClient wizWS = new WizardServiceClient();

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