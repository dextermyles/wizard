using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Web;
using WizardGame.Helpers;
using WizardGame.Services;

namespace WizardGame.Helpers
{
    public static class Functions
    {
        /// <summary>
        /// Perform a deep Copy of the object.
        /// </summary>
        /// <typeparam name="T">The type of object being copied.</typeparam>
        /// <param name="source">The object instance to copy.</param>
        /// <returns>The copied object.</returns>
        public static T Clone<T>(this T source)
        {
            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", "source");
            }

            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();
            using (stream)
            {
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }

        public static string CleanText(string strIn)
        {
            // Replace invalid characters with empty strings. 
            try
            {
                return Regex.Replace(strIn, @"[^\w\.@ -]", "",
                                     RegexOptions.None, TimeSpan.FromSeconds(1.5));
            }
            // If we timeout when replacing invalid characters,  
            // we should return Empty. 
            catch (RegexMatchTimeoutException)
            {
                return String.Empty;
            }
        }

        public static string CleanInput(string strIn)
        {
            // Replace invalid characters with empty strings. 
            try
            {
                return Regex.Replace(strIn, @"[^\w\.@-]", "",
                                     RegexOptions.None, TimeSpan.FromSeconds(1.5));
            }
            // If we timeout when replacing invalid characters,  
            // we should return Empty. 
            catch (RegexMatchTimeoutException)
            {
                return String.Empty;
            }
        }

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

            // service
            WizardService wizWS = new WizardService();

            // validate
            if (cookie != null)
            {
                // get secret from cookie
                string secret = cookie.Values["secret"];
                bool remember = (Convert.ToString(cookie.Values["remember"]) == "true") ? true : false;

                if (!string.IsNullOrEmpty(secret))
                {
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

            // get referring page
            var referrer = HttpContext.Current.Request.UrlReferrer;

            if (referrer != null)
            {
                // get session
                var currentSession = HttpContext.Current.Session;

                // set referring page
                currentSession["referencePage"] = referrer.AbsolutePath;
            }

            return false;
        }
    }
}