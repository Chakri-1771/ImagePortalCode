using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;

namespace DownloadFile
    {
    class Program
        {
        static void Main(string[] args)
            {

            string RESTTokenUrl = ConfigurationManager.AppSettings["RESTTokenUrl"].ToString();
            string RESTInvoiceListUrl = ConfigurationManager.AppSettings["RESTInvoiceListUrl"].ToString();
            string RESTInvoiceDownloadUrl = ConfigurationManager.AppSettings["RESTInvoiceDownloadUrl"].ToString();
            string ClientID = ConfigurationManager.AppSettings["clientID"].ToString();
            string ClientKey = ConfigurationManager.AppSettings["clientKey"].ToString();
            string downloadLogPath = ConfigurationManager.AppSettings["downloadLogPath"];

            string token = string.Empty;
            string InvoicesList = string.Empty;

            bool isValidJSONString = false;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Getting authorization for the supplied Client ID : {0}  and Client Key : {1} ", ClientID, ClientKey);
            Console.WriteLine("\n");
            token = GetToken(RESTTokenUrl, ClientID, ClientKey);
            bool isValiJSONTokenString = IsValidJson(token);

            if (isValiJSONTokenString)
                {
                Console.WriteLine("Authorization successful and token applied successfully.......");
                }
            else
                {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Authorization successful and token applied successfully.......");
                return;
                }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Checking for the files to be downloaded");
            Console.WriteLine("\n");
            InvoicesList = GetInvoiceFiles(RESTInvoiceListUrl, token);

            isValidJSONString = IsValidJson(InvoicesList);

            if (isValidJSONString)
                {
                JArray jsonArray = JArray.Parse(InvoicesList);

                foreach (JObject json in jsonArray)
                    {   
                    JObject o = JObject.Parse(json.ToString());
                    Console.WriteLine("\rDownloading file(s) : {0}", o["Name"].ToString());
                    DownloadInvoiceFiles(RESTInvoiceDownloadUrl, token, o["Name"].ToString());
                    }
                Console.WriteLine("\n");
                Console.WriteLine("Downloading file(s) completed succesfully.......");
                Console.ForegroundColor = ConsoleColor.White;
                }
            else
                {
                Console.WriteLine("\n");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("{0}.......", InvoicesList);
                Console.ForegroundColor = ConsoleColor.White;
                Console.ReadLine();
                WriteToFile(downloadLogPath, InvoicesList);
                }
          
            Console.WriteLine("\n");
            Console.WriteLine("Please press any key to exit");
            Console.ReadLine();
           

            }  
        
      
        static string GetToken(string url, string userName, string password)
            {
            string token = string.Empty;
            try
                {
                var pairs = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>( "grant_type", "password" ),
                        new KeyValuePair<string, string>( "username", userName ),
                        new KeyValuePair<string, string> ( "Password", password )
                    };
                var content = new FormUrlEncodedContent(pairs);
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                using (var client = new HttpClient())
                    {
                     var response = client.PostAsync(url, content).Result;
                     token =  response.Content.ReadAsStringAsync().Result;
                    }
                }
            catch (Exception ex)
                {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);                
                }
            return token;
            }

        static string GetInvoiceFiles(string url, string token)
            {
            
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            using (var client = new HttpClient())
                {
                if (!string.IsNullOrWhiteSpace(token))
                    {
                    var t = JValue.Parse(token);

                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + t["access_token"].ToString());
                    }
                var response = client.GetAsync(url).Result;             
                    
                return response.Content.ReadAsStringAsync().Result;                              

                }
            }

        static void DownloadInvoiceFiles(string url, string token, string fileName)
            {
            if (fileName.Contains("&"))
                {                
                fileName = fileName.Replace("&", " and ");
                }

            string path = ConfigurationManager.AppSettings["downloadPath"].ToString();
            string fullPath = path + fileName;
            string logsPath = ConfigurationManager.AppSettings["downloadLogPath"].ToString();
            var client = new HttpClient();

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url + "?" + "invoiceFileName=" + fileName);
            request.Method = "GET";
            var t = JValue.Parse(token);

            request.Headers.Add("Authorization", "Bearer "  + t["access_token"].ToString());

            var response = request.GetResponse() as HttpWebResponse;

            


            if (response.StatusCode == HttpStatusCode.OK)
                {
                using (FileStream stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
                    {

                    byte[] bytes = ReadFully(response.GetResponseStream());

                    stream.Write(bytes, 0, bytes.Length);
                    //fix the file name issue
                    int totalResponseLength = response.Headers[1].Length;
                    int length = totalResponseLength - 21;
                    WriteToFile(logsPath,response.StatusCode.ToString(),  response.Headers[1].Substring(21, length));
                                       
                    }
                }
            else
                {

                }

           

            }

        public static byte[] ReadFully(Stream input)
            {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
                {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                    {
                    ms.Write(buffer, 0, read);
                    }
                return ms.ToArray();
                }
            }

        static void WriteToFile(string directory, string responseCode,string response)
            {
            
            string textFilename = string.Empty;
            textFilename = String.Format("{0:yyyy-MM-dd}" + ".txt", DateTime.Now);
            string fullPath = directory + textFilename;
            if (!File.Exists(fullPath))
                {
                File.Create(fullPath).Dispose();
                string filename = String.Format("{0:yyyy-MM-dd}" + ".txt", DateTime.Now);
                string path = Path.Combine(directory, filename);
                using (StreamWriter sw = File.CreateText(path))
                    {
                    sw.WriteLine("=============================");
                    sw.WriteLine("StatusCode : " + responseCode);
                    sw.WriteLine("Filename : " + response);
                    sw.WriteLine("=============================");
                    }
                }
            else if(File.Exists(fullPath))
                {
                string filename = String.Format("{0:yyyy-MM-dd}" + ".txt", DateTime.Now);
                string path = Path.Combine(directory, filename);
                using (StreamWriter sw = File.AppendText(path))
                    {
                    sw.WriteLine("=============================");
                    sw.WriteLine("StatusCode : " + responseCode);
                    sw.WriteLine("Filename : " + response);
                    sw.WriteLine("=============================");
                    }
                }
           
            }

        static void WriteToFile(string directory, string response)
            {

            string textFilename = string.Empty;
            textFilename = String.Format("{0:yyyy-MM-dd}" + ".txt", DateTime.Now);
            string fullPath = directory + textFilename;
            if (!File.Exists(fullPath))
                {
                File.Create(fullPath).Dispose();
                string filename = String.Format("{0:yyyy-MM-dd}" + ".txt", DateTime.Now);
                string path = Path.Combine(directory, filename);
                using (StreamWriter sw = File.CreateText(path))
                    {
                    sw.WriteLine("=============================");                  
                    sw.WriteLine("Message : " + response);
                    sw.WriteLine("=============================");
                    }
                }
            else if (File.Exists(fullPath))
                {
                string filename = String.Format("{0:yyyy-MM-dd}" + ".txt", DateTime.Now);
                string path = Path.Combine(directory, filename);
                using (StreamWriter sw = File.AppendText(path))
                    {
                    sw.WriteLine("=============================");                 
                    sw.WriteLine("Message : " + response);
                    sw.WriteLine("=============================");
                    }
                }

            }


        private static bool IsValidJson(string strInput)
            {
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
                {
                try
                    {
                    var obj = JToken.Parse(strInput);
                    return true;
                    }
                catch (JsonReaderException jex)
                    {
                    //Exception in parsing json
                    Console.WriteLine(jex.Message);
                    return false;
                    }
                catch (Exception ex) //some other exception
                    {
                    Console.WriteLine(ex.ToString());
                    return false;
                    }
                }
            else
                {
                return false;
                }
            }

        private static void DeleteThumbsFile(string fileName)
            {
            if (System.IO.File.Exists(fileName))
                {
                try
                    {
                    System.IO.File.Delete(fileName);
                    }
                catch (System.IO.IOException ex)
                    {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.Message);
                    }
                }
            }
        }
     }
    
