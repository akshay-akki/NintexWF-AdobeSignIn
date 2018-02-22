using System;
using System.Globalization;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Administration.Claims;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Akki.AdobeSign.Common
{
    public  class Utilities
    {
        internal static string GetEmailAddressCurrentExecutingUser()
        {            
            string accountname = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            return GetEmailFromUser(accountname);           
        }

        public static string GetEmailFromUser(string userIdentifier)
        {
            if (string.IsNullOrEmpty(userIdentifier))
                throw new ApplicationException("[GetEmailFromUser] The input 'userIdentifier' param can not be null or empty ");

            Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
            Match match = regex.Match(userIdentifier);
            if (match.Success)
                return userIdentifier;

            // else check if its claim 
            var userLogin = string.Empty;
            try
            {
                SPClaimProviderManager mgr = SPClaimProviderManager.Local;
                userLogin = mgr.DecodeClaim(userIdentifier).Value;
            }
            catch(ArgumentException)
            {
                userLogin = userIdentifier;
            }
                      
            var rooturl = string.Empty;
            if (!SPFarm.Local.Properties.ContainsKey(Constants.KeyRootSiteColl))
                throw new ApplicationException("Farm is missing Root Site Collection property  [" + Constants.KeyRootSiteColl + "]");
            else
                rooturl = SPFarm.Local.Properties[Constants.KeyRootSiteColl] as string;

            SPUser user = null;

            using (SPSite site = new SPSite(rooturl))
            using (SPWeb web = site.OpenWeb())
            {
                //web.ValidateFormDigest();
                try
                {
                    web.AllowUnsafeUpdates = true;
                    user = web.EnsureUser(userLogin.ToLower());
                }
                catch (Exception ex)
                {
                    //TODO log 

                }
                finally
                {
                    web.AllowUnsafeUpdates = false;
                }
            }

            if (user == null)
                throw new ApplicationException(string.Format("No user with id {0} found", userIdentifier));

            return user.Email;
        }
        public static bool IsValidEmail(string strIn)
        {            
            if (String.IsNullOrEmpty(strIn))
                return false;

            // Use IdnMapping class to convert Unicode domain names.
            try
            {
                strIn = Regex.Replace(strIn, @"(@)(.+)$", DomainMapper,RegexOptions.None, TimeSpan.FromMilliseconds(200));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }

            if (strIn == null )
                return false;

            // Return true if strIn is in valid e-mail format.
            try
            {
                return Regex.IsMatch(strIn,
                      @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                      @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                      RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        /// <summary>
        /// The function loops through a comma /colon speareated string and get the users in it. The user can be mentioned as emails, login accounts or even a sharepoint group.
        /// </summary>
        /// <param name="approvers"></param>
        /// <param name="urlOfSiteWhereApplicationRuns">the url where the function should look for a group is its mentioned in the string</param>
        /// <returns>the list of emails of all the users </returns>
        public static List<string> GetUserEmails(string approvers, string urlOfSiteWhereApplicationRuns)
        {

            char Delimiter = ',';
            var lstApprovers = approvers.Split(new string[] { ",", ";" }, StringSplitOptions.None);
            List<string> lstUserEmails = new List<string>();

            try
            {
                // if string is an email 
                foreach (var user in lstApprovers)
                {
                    if (Utilities.IsValidEmail(user))
                        lstUserEmails.Add(user);
                    else if (Utilities.IsDomainName(user))
                    {
                        lstUserEmails.Add(Utilities.GetEmailFromUser(user));
                    }
                    else
                    {
                        using (SPSite spSite = new SPSite(urlOfSiteWhereApplicationRuns))
                        {
                            using (SPWeb spWeb = spSite.OpenWeb())
                            {
                                // check if its a wrong val                                
                                SPGroup grp = spWeb.Groups[user];
                                foreach (SPUser indUser in grp.Users)
                                    lstUserEmails.Add(indUser.Email);
                            }
                        }
                    }
                }
            }
            catch(Exception e)
            {
                //log.Error("[GetUserEmails]", e);
                throw;
            }
            return lstUserEmails;
        }

        public static bool IsDomainName(string user)
        {
            var DomainStyleLogin = new Regex(@"^.*(\\|/)");
            var match = DomainStyleLogin.Match(user);
            return match.Success;
        }

        private static string DomainMapper(Match match)
        {
            // IdnMapping class with default property values.
            IdnMapping idn = new IdnMapping();

            string domainName = match.Groups[2].Value;
            try
            {
                domainName = idn.GetAscii(domainName);
            }
            catch (ArgumentException)
            {
                return null;
            }
            return match.Groups[1].Value + domainName;
        }
    }
}
