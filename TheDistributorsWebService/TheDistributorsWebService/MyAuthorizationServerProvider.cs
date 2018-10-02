using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using TheDistributorsWebService.Models;

namespace TheDistributorsWebService
{
    public class MyAuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            string clientID = string.Empty;
            string clientKey = string.Empty;
            string customerName = string.Empty;
            List<ClientModel> lstClientDetails = new List<ClientModel>();
            lstClientDetails = GetClientDtails(context);
            clientID = lstClientDetails[0].ClientID;
            clientKey = lstClientDetails[0].ClientKey;
            customerName = GetCustomerName(clientID, context);

            CreateFoldersInServer(customerName, context);

            var identity = new ClaimsIdentity(context.Options.AuthenticationType);

            if (context.UserName == "Admin" && context.Password == "Admin")
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, "Admin"));
                identity.AddClaim(new Claim("username", "Admin"));
                identity.AddClaim(new Claim(ClaimTypes.Name, "Chakravarthy Sudha"));
                context.Validated(identity);
            }
            else if (context.UserName == clientID && context.Password == clientKey)
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, "user"));
                identity.AddClaim(new Claim("username", clientID));
                identity.AddClaim(new Claim(ClaimTypes.Name, clientID));
                context.Validated(identity);
            }
            else
            {
                context.SetError("invalid_grant", "Provided username and password is incorrect");
                return;
            }
        }

        public List<ClientModel> GetClientDtails(OAuthGrantResourceOwnerCredentialsContext context)
        {
            
            string clientKey = string.Empty;

            List<ClientModel> lstClientDetails = new List<ClientModel>();

            using (POC_DBEntities entities = new POC_DBEntities())
            {
                clientKey = (from s in entities.tbl_REST_TokenDetails
                            where (s.ClientID == context.UserName && s.Active == true)                            
                            select s.ClientKey).First();
                lstClientDetails.Add(new ClientModel(context.UserName, clientKey));
            }
                return lstClientDetails;
        }

        public string GetCustomerName(string clientID, OAuthGrantResourceOwnerCredentialsContext context)
        {

            string customerName = string.Empty;

            

            using (POC_DBEntities entities = new POC_DBEntities())
            {
                customerName = (from s in entities.tbl_REST_TokenDetails
                             where (s.ClientID == context.UserName && s.Active == true)
                             select s.CustomerName).First();
                
            }
            return customerName;
        }

        public void CreateFoldersInServer(string customerName, OAuthGrantResourceOwnerCredentialsContext context)
        {
            string foldername = customerName;
            string folderNames = ConfigurationManager.AppSettings["FolderNames"];
            string subFolderNames = ConfigurationManager.AppSettings["SubFolderNames"];
            string basePath = ConfigurationManager.AppSettings["BasePath"];            
            string downloadedInvoicePath = string.Empty;
            string downloadInvoicePath = string.Empty;
            string orderUploadPath = string.Empty;
            string logsPath = string.Empty;

            string[] mainFolders = folderNames.Split(',');

            if (!Directory.Exists(@"\\td-ws-01\d$\WebApps\TDG WebApi\" + foldername))
            {

                Directory.CreateDirectory(@"\\td-ws-01\d$\WebApps\TDG WebApi\" + foldername);
                foreach (string var in mainFolders)
                {
                    if (!Directory.Exists(@"\\td-ws-01\d$\WebApps\TDG WebApi\" + @"\" + foldername + @"\" + var))
                    {
                        Directory.CreateDirectory(@"\\td-ws-01\d$\WebApps\TDG WebApi\" + @"\" + foldername + @"\" + var);
                    }
                }
                 
                string[] subFolders = subFolderNames.Split(',');

                foreach (string var in subFolders)
                {
                    if (!Directory.Exists(@"\\td-ws-01\d$\WebApps\TDG WebApi\" + @"\" + foldername + @"\" + var))
                    {
                        Directory.CreateDirectory(@"\\td-ws-01\d$\WebApps\TDG WebApi\" + @"\" + foldername + @"\" + var);
                    }
                }


            }

           
                    downloadInvoicePath = basePath + customerName + @"\" + mainFolders[0] + @"\";
                    orderUploadPath = basePath + customerName + @"\" + mainFolders[1] + @"\";
                    logsPath = basePath + customerName + @"\" + mainFolders[2] + @"\";
                    downloadedInvoicePath = basePath + customerName + @"\" + subFolderNames + @"\";
              
           
               
            
              
            

            //Helps to open the Root level web.config file.
            Configuration webConfigApp = WebConfigurationManager.OpenWebConfiguration("~");

            webConfigApp.AppSettings.Settings["finalDownloadPath"].Value = downloadInvoicePath;
            webConfigApp.AppSettings.Settings["finalDownloadedFilesPath"].Value = downloadedInvoicePath;
            webConfigApp.AppSettings.Settings["finalUploadPath"].Value = orderUploadPath;
            webConfigApp.AppSettings.Settings["logsPath"].Value = logsPath;

            webConfigApp.Save();


            WriteToFile(logsPath, downloadInvoicePath, downloadedInvoicePath, downloadInvoicePath, downloadedInvoicePath, orderUploadPath);


        }

        static void WriteToFile(string logPath, string sourcePath, string targetPath, string downloadInvoicePath, string downloadedInvoicePath, string orderUploadPath)
        {

            string textFilename = string.Empty;
            textFilename = String.Format("{0:yyyy-MM-dd}" + ".txt", DateTime.Now);
            string fullPath = logPath + textFilename;
            DateTime dt = DateTime.UtcNow;
            if (!File.Exists(fullPath))
            {
                File.Create(fullPath).Dispose();
                string filename = String.Format("{0:yyyy-MM-dd}" + ".txt", DateTime.Now);
                string path = Path.Combine(logPath, filename);
               

                using (StreamWriter sw = File.CreateText(path))
                {
                    sw.WriteLine("=============================");
                    sw.WriteLine("Date & Time : " + dt.ToString());
                    sw.WriteLine("**DYNAMICALLY CREATED PATHS");
                    sw.WriteLine("Source Path : " + downloadInvoicePath);
                    sw.WriteLine("Target Path : " + downloadedInvoicePath);
                    sw.WriteLine("Order Upload Path : " + orderUploadPath);
                    sw.WriteLine("Logs Path : " + logPath);
                    sw.WriteLine("=============================");
                }
            }
            else if (File.Exists(fullPath))
            {
                string filename = String.Format("{0:yyyy-MM-dd}" + ".txt", DateTime.Now);
                string path = Path.Combine(logPath, filename);
                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.WriteLine("=============================");
                    sw.WriteLine("Date & Time : " + dt.ToString());
                    sw.WriteLine("**DYNAMICALLY CREATED PATHS");
                    sw.WriteLine("Source Path : " + downloadInvoicePath);
                    sw.WriteLine("Target Path : " + downloadedInvoicePath);
                    sw.WriteLine("Order Upload Path : " + orderUploadPath);
                    sw.WriteLine("Logs Path : " + logPath);
                    sw.WriteLine("=============================");
                }
            }

        }













    }
}