using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace ImageProcessingEngine
    {
    public class ImageDimensions
        {
        public string[] Dimensions { get; set; }
        public string[] GetDimensionsFromConfig()
            {
            string dimensions = ConfigurationManager.AppSettings["Dimensions"];
            string[] dimension = dimensions.Split(',');
            Dimensions = dimension;
            return Dimensions;
            }
        }
    }
