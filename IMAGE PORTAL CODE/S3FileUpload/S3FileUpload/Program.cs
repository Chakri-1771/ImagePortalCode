using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace S3FileUpload
{
    class Program
    {
        private string _filePath = @"\\td-sql-01\d$\TDG_IR\Converted Images\200x200";
        private string _fileName = string.Empty;
        string _fullPath = string.Empty;
        static void Main(string[] args)
        {
            Program program = new Program();
            S3Uploader s3 = new S3Uploader();

            string[] files = Directory.GetFiles(@"\\td-sql-01\d$\TDG_IR\Converted Images\200x200", "*.jpg");

            foreach (string name in files)
            {
               program._fullPath =  name;
                program._fileName = Path.GetFileName(name);
                s3.UploadFile(program._fullPath,program._fileName);
            }
             
            
        }
    }
}
