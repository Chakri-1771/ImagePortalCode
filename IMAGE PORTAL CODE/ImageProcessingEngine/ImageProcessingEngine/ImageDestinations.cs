using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace ImageProcessingEngine
    {
    public class ImageDestinations
        {
        public string[] Destinations { get; set; }

        public string[] GetDestinationsFromConfig()
            {
            string destinations = ConfigurationManager.AppSettings["DestinationPaths"];
            string[] destination = destinations.Split(',');
            Destinations = destination;
            return Destinations;
            }
        }
    }
