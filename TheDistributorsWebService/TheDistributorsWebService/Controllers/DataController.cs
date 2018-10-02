using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using TheDistributorsWebService.Models;

namespace TheDistributorsWebService.Controllers
{
    public class DataController : ApiController
    {

        string invoiceDownloadPath = string.Empty;
        string invoiceFileName = string.Empty;
        string ordersUploadPath = string.Empty;

        [Authorize]
        [HttpGet]
        
        [Route("api/data/getallinvoices")]
        public IHttpActionResult GetAllFiles()
        {
            invoiceDownloadPath = ConfigurationManager.AppSettings["finalDownloadPath"]; // Prod Invoices Path

            List<FileModel> files = new List<FileModel>();

            string[] fileEntries = Directory.GetFiles(invoiceDownloadPath);

            if (fileEntries.Length > 0)
            {
                foreach (string fileName in fileEntries)
                {
                    invoiceFileName = Path.GetFileName(fileName);
                    files.Add(new FileModel { Name = invoiceFileName });
                }

                return Ok(files);
            }
            else
            {
                return Content(HttpStatusCode.NotFound, "No Invoice files are available for download currently, Please check later...");
            }
        }
    }
}
