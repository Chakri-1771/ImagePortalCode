using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace ImageProcessingEngine
    {
    public class ImageExtensions
        {
        public string[] Extensions { get; set; }

        public string[] GetExtensionsFromConfig()
            {
            string extensions = ConfigurationManager.AppSettings["Extensions"];
            string[] extension = extensions.Split(',');
            Extensions = extension;
            return Extensions;
            }
        }

   
    }
