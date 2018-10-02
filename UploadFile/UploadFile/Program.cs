using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace UploadFile
    {
    class Program
        {
         
     
        static void Main(string[] args)
            {
            string logsPath = ConfigurationManager.AppSettings["uploadLogPath"].ToString();
            string RESTTokenUrl = ConfigurationManager.AppSettings["RESTTokenUrl"].ToString();
            string RESTOrderUploadUrl = ConfigurationManager.AppSettings["RESTOrderUploadUrl"].ToString();
            string ClientID = ConfigurationManager.AppSettings["clientID"].ToString();
            string ClientKey = ConfigurationManager.AppSettings["clientKey"].ToString();
            string uploadPath = ConfigurationManager.AppSettings["uploadPath"].ToString();
            string token = string.Empty;
            string[] orderFiles;
            bool isValiJSONTokenString = false;
            bool invalidGrant = false;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Getting authorization for the supplied Client ID : {0}  and Client Key : {1} ", ClientID, ClientKey);
            Console.WriteLine("\n");
            token = GetToken(RESTTokenUrl, ClientID, ClientKey);

            if (token.Contains("invalid_grant"))
                {
                invalidGrant = true;
                }
            isValiJSONTokenString = IsValidJson(token);
            if (isValiJSONTokenString && !invalidGrant)
                {              
                Console.WriteLine("Authorization successful and token applied successfully.......");
                Console.WriteLine("\n");               
                }
            else
                {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Authorization Unsuccessful and token has not been applied .......");
                Console.WriteLine("Please check if the Client ID and Client Key are correct and try agian.");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Press any key to exit.");
                Console.ReadLine();
                return;
                }         


            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Checking for the files to be upoaded");
            Console.WriteLine("\n");
            orderFiles = Directory.GetFiles(uploadPath);

            if (orderFiles.Length > 0)
                {
                foreach (string fileName in orderFiles)
                    {
                    Console.ForegroundColor = ConsoleColor.Green;
                                       
                    Console.WriteLine("Uploading file : {0}", fileName);                        
                       
                    UploadOrderFiles(RESTOrderUploadUrl, token, fileName);

                    }
                Console.WriteLine("\n");
                Console.WriteLine("Uploading file(s) completed succesfully.......");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("\n");
                }
            else
                {
                Console.WriteLine("\n");
                WriteToFile(logsPath, "No files have been uploaded, Please check if there are files in the upload folder path");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No files have been uploaded, Please check if there are files in the upload folder path");
                Console.ForegroundColor = ConsoleColor.White;
                }
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
                    token = response.Content.ReadAsStringAsync().Result;
                    }
                }
            catch (Exception ex)
                {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);

                }
            return token;
            }


        static void UploadOrderFiles(string url, string token, string fileName)
            {
            FileInfo fi = new FileInfo(fileName);
            fileName = fi.Name;
            string logsPath = ConfigurationManager.AppSettings["uploadLogPath"].ToString();
            string deleteOrderFile = ConfigurationManager.AppSettings["deleteOrderFile"].ToString();
            string moveOrderFilePath = ConfigurationManager.AppSettings["moveOrderFilePath"].ToString();
            string uploadPath = ConfigurationManager.AppSettings["uploadPath"].ToString();
            byte[] fileContents = File.ReadAllBytes(fi.FullName);
            Uri webService = new Uri(url);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, webService);
            var t = JValue.Parse(token);
            request.Headers.Add("Authorization", "Bearer " + t["access_token"].ToString());


            request.Headers.ExpectContinue = false;

            MultipartFormDataContent multiPartContent = new MultipartFormDataContent("----OrderFile");
            ByteArrayContent byteArrayContent = new ByteArrayContent(fileContents);
            byteArrayContent.Headers.Add("Content-Type", "application/octet-stream");
            multiPartContent.Add(byteArrayContent, "this is the name of the content", fileName);
            request.Content = multiPartContent;

            HttpClient httpClient = new HttpClient();
            try
                {
                Task<HttpResponseMessage> httpRequest = httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
                HttpResponseMessage httpResponse = httpRequest.Result;
                HttpStatusCode statusCode = httpResponse.StatusCode;
                HttpContent responseContent = httpResponse.Content;



                if (responseContent != null)
                    {
                    Task<string> stringContentsTask = responseContent.ReadAsStringAsync();
                    string stringContents = stringContentsTask.Result;
                    WriteToFile(logsPath, httpResponse.StatusCode.ToString(), stringContents, fileName);
                    if (deleteOrderFile == "1")
                        {
                        DeleteOrderFile(fi.FullName);
                        }
                    else
                        {
                        File.Move(uploadPath + fileName, moveOrderFilePath + fileName);
                        File.Delete(uploadPath + fileName);
                        }

                    }
               
                }
            catch (Exception ex)
                {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                }
            finally
                {
                
                }

            }

        static void WriteToFile(string directory, string responseCode,string responseMessage, string fileName)
            {
            try
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
                        sw.WriteLine("Status Code : " + responseCode);
                        sw.WriteLine("Response Message : " + responseMessage);
                        sw.WriteLine("File Name : " + fileName);
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
                        sw.WriteLine("StatusCode : " + responseCode);
                        sw.WriteLine("Response Message : " + responseMessage);
                        sw.WriteLine("Filename : " + fileName);
                        sw.WriteLine("=============================");
                        }
                    }
                }
            catch (Exception ex)
                {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
               
                }
           
       

            }

        static void WriteToFile(string directory, string message)
            {
            try
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
                        sw.WriteLine("Generic Message : " + message);
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
                        sw.WriteLine("Generic Message : " + message);
                        sw.WriteLine("=============================");
                        }
                    }
                }
            catch (Exception ex)
                {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                }

           

            }

        static void DeleteOrderFile(string directory)
            {        

            if (System.IO.File.Exists(directory))
                {               
                try
                    {
                    System.IO.File.Delete(directory);
                    }
                catch (System.IO.IOException e)
                    {                   
                    return;
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

        }
    }
