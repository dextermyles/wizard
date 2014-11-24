using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WizardGame.Services;

namespace WizardGame.Helpers
{
    public static class Functions
    {
        public static Session GetSessionFromCookie()
        {
            // session
            Session session = null;

            // check for existing cookie
            HttpCookie cookie = HttpContext.Current.Request.Cookies["OfficeWizard"];

            // validate
            if (cookie != null)
            {
                // get secret from cookie
                string secret = cookie.Values["secret"];

                // validate
                if (!string.IsNullOrEmpty(secret))
                {
                    // service
                    WizardService wizWS = new WizardService();

                    // get session data
                    session = wizWS.GetSessionBySecret(secret);
                }
            }

            return session;
        }

        public static string GetUserIPAddress()
        {
            string VisitorsIPAddr = string.Empty;
            
            if (HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] != null)
            {
                VisitorsIPAddr = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"].ToString();
            }
            else if (HttpContext.Current.Request.UserHostAddress.Length != 0)
            {
                VisitorsIPAddr = HttpContext.Current.Request.UserHostAddress;
            }

            return VisitorsIPAddr;
        }

        public static bool IsValidSession()
        {
            // check for existing cookie
            HttpCookie cookie = HttpContext.Current.Request.Cookies["OfficeWizard"];

            // validate
            if (cookie != null)
            {
                // get secret from cookie
                string secret = cookie.Values["secret"];
                bool remember = (Convert.ToString(cookie.Values["remember"]) == "true") ? true : false;

                if (!string.IsNullOrEmpty(secret))
                {
                    // service
                    WizardService wizWS = new WizardService();

                    // make sure secret is valid
                    var session = wizWS.ValidateSession(secret);

                    // validate session result
                    if (session != null && !string.IsNullOrEmpty(session.Secret))
                    {
                        // get time left on cookie
                        TimeSpan timeDiff = (cookie.Expires - DateTime.Now);

                        // if cookie expires in less than a day
                        if (timeDiff.Days < 1)
                            cookie.Expires = DateTime.Now.AddDays(1);

                        // clear values
                        cookie.Values.Clear();

                        // reassign new values
                        cookie.Values.Add("secret", secret);

                        // remember user
                        if (remember)
                        {
                            cookie.Expires = DateTime.Now.AddDays(30);
                            cookie.Values.Add("remember", "true");
                        }

                        // re-add cookie
                        HttpContext.Current.Response.Cookies.Add(cookie);
                        
                        return true;
                    }
                }
            }

            return false;
        }
    }
}