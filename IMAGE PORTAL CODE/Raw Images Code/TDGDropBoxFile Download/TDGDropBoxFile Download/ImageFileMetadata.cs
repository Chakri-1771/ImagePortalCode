using Dropbox.Api;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace TDGDropBoxFile_Download
    {
     public class ImageFileMetadata
        {
        public string Name { get; private set; }

        public string DisplayName { get; private set; }

        public string Rev { get; private set; }

        public DateTime Date { get; private set; }

        public string Filename
            {
            get
                {
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}.{1:yyyyMMdd}.md",
                    this.Name,
                    this.Date);
                }
            }

      

        }
    }
