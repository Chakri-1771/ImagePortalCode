using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using Microsoft.Owin.Security.OAuth;
using System.Diagnostics;
using System.Web;
using System.Linq;

namespace TheDistributorsWebService.Controllers
{
    public class FileController : ApiController
    {
        string invoicesDownloadPath = string.Empty;
        string fullPath = string.Empty;
        string ordersUploadPath = string.Empty;
        string logsPath = string.Empty;


        [Authorize]
        [HttpGet]
        [Route("api/File/DownloadFile/{*FileName}")]
        public HttpResponseMessage DownloadFile(string invoiceFileName)
        {
            string FileToBeDeleted = string.Empty;
            FileToBeDeleted = ConfigurationManager.AppSettings["FileToBeDeleted"];
            DeleteOrderFile(FileToBeDeleted);

            invoicesDownloadPath = ConfigurationManager.AppSettings["finalDownloadPath"];
            logsPath = ConfigurationManager.AppSettings["logsPath"];

            fullPath = invoicesDownloadPath + invoiceFileName;


            var dataBytes = File.ReadAllBytes(fullPath);

            var dataStream = new MemoryStream(dataBytes);



            HttpResponseMessage httpResponseMessage = Request.CreateResponse(HttpStatusCode.OK);
            httpResponseMessage.Content = new StreamContent(dataStream);
            httpResponseMessage.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
            httpResponseMessage.Content.Headers.ContentDisposition.FileName = invoiceFileName;

            httpResponseMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                WriteToFile(logsPath, invoiceFileName, "  Downloaded Successfully.......!");
                MoveDownloadedFiles(invoiceFileName);
            }
            

            return httpResponseMessage;
        }

       

        [Authorize]
        [HttpPost]

        public Task<HttpResponseMessage> UploadFile()
        {
            List<string> savedFilePath = new List<string>();
            string fileName = string.Empty;

            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }


            ordersUploadPath = ConfigurationManager.AppSettings["finalUploadPath"]; // Prod Invoices Path
            logsPath = ConfigurationManager.AppSettings["logsPath"];
            var provider = new MultipartFileStreamProvider(ordersUploadPath);

            var task = Request.Content.ReadAsMultipartAsync(provider).
                ContinueWith<HttpResponseMessage>(t =>
                {
                    if (t.IsCanceled || t.IsFaulted)
                    {
                        Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        WriteToFile(logsPath, fileName, HttpStatusCode.InternalServerError.ToString());
                    }
                    foreach (MultipartFileData dataIltem in provider.FileData)
                    {
                        try
                        {
                            string name = dataIltem.Headers.ContentDisposition.FileName.Replace("\"", "");
                            // string newFileName = Guid.NewGuid() + Path.GetExtension(name);                           
                            File.Move(dataIltem.LocalFileName, Path.Combine(ordersUploadPath, name));
                            fileName = name;
                        }
                        catch (Exception ex)
                        {

                            string message = ex.Message;
                        }
                    }

                    savedFilePath.Add(fileName + "  Uploaded Successfully.......!");
                    WriteToFile(logsPath,fileName, "  Uploaded Successfully.......!");
                    return Request.CreateResponse(HttpStatusCode.Created, savedFilePath);

                });
            return task;
        }
       

       




        public void MoveDownloadedFiles(string _fileName)
        {

            try
            {
                #region Move files to Processed folder

                // string sourcePath = @"\\teapot\D$\ORDERS_IN\SPOTLESS";
                string sourcePathFromAppSettings = ConfigurationManager.AppSettings["finalDownloadPath"].ToString();
                string sourcePath = sourcePathFromAppSettings + _fileName;


                //string targetPath = @"\\teapot\D$\ORDERS_IN\SPOTLESS\Processed";
                string targetPathFromAppSettings = ConfigurationManager.AppSettings["finalDownloadedFilesPath"].ToString();
                string targetPath = targetPathFromAppSettings + _fileName;
                if (!Directory.Exists(targetPathFromAppSettings))

                {
                    Directory.CreateDirectory(targetPathFromAppSettings);
                }

                string logsPath = ConfigurationManager.AppSettings["logsPath"].ToString();

                WriteToFile(logsPath, sourcePath, targetPath, _fileName);

                //Copy the file from sourcepath and place into mentioned target path, 
                //Overwrite the file if same file is exist in target path
                foreach (var srcPath in Directory.GetFiles(sourcePathFromAppSettings))
                {
                    //Copy the file from sourcepath and place into mentioned target path, 
                    //Overwrite the file if same file is exist in target path
                    File.Move(sourcePath, targetPath);
                    File.Delete(sourcePath);
                    break;
                }

                #endregion
            }
            catch (Exception ex)
            {
              
            }

        }

        static void WriteToFile(string logPath, string sourcePath, string targetPath, string fileName)
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
                    sw.WriteLine("Source Path : " + sourcePath);
                    sw.WriteLine("Target Path : " + targetPath);
                    sw.WriteLine("fileName : " + fileName);
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
                    sw.WriteLine("Source Path : " + sourcePath);
                    sw.WriteLine("Target Path : " + targetPath);
                    sw.WriteLine("fileName : " + fileName);
                    sw.WriteLine("=============================");
                }
            }

        }

        static void WriteToFile(string logPath, string fileName, string message)
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
                    sw.WriteLine("File Name : " + fileName);
                    sw.WriteLine("Message : " + message);                 
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
                    sw.WriteLine("File Name : " + fileName);
                    sw.WriteLine("Message : " + message);
                    sw.WriteLine("=============================");
                }
            }

        }

        static void DeleteOrderFile(string fileName)
        {
            string finalDownloadPath = ConfigurationManager.AppSettings["finalDownloadPath"];
            DirectoryInfo dir = new DirectoryInfo(finalDownloadPath);


            if (System.IO.File.Exists(fileName))
            {
                try
                {
                    System.IO.File.Delete(fileName);
                }
                catch (System.IO.IOException ex)
                {
                    return;
                }
            }

        }
    }
}
