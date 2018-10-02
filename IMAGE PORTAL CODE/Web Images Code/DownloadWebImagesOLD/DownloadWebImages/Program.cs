using Dropbox.Api;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;


namespace DownloadWebImages
    {
    class Program
        {
        private DropboxClient dbx;
        string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ToString();
        public static void Main(string[] args)
            {


            Program program = new Program();

            string AccountName = string.Empty;
            string CreatedDate = null;
            int Downloaded = 0;
            int AccountNumber = 0;

            try
                {
                string brandzAccessKey = ConfigurationManager.AppSettings["BrandzAccessKey"].ToString();
                string imageRootPath = ConfigurationManager.AppSettings["ImageRootPath"].ToString();
                string logFilePath = ConfigurationManager.AppSettings["LogFilePath"].ToString();
                string fileDownloadPath = ConfigurationManager.AppSettings["FileDownloadPath"].ToString();

                AccountName = program.ConnectToDropbox(brandzAccessKey);

               

                var list = program.dbx.Files.ListFolderAsync(imageRootPath, recursive: true).Result;



                List<string> paths = new List<string>();
                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                    {
                    writer.WriteLine("Process Start Time : " + DateTime.Now);
                    }


                foreach (var item in list.Entries.Where(i => i.IsFile))
                    {

                    paths.Add(item.PathDisplay);

                    using (StreamWriter writer = new StreamWriter(logFilePath, true))
                        {
                        writer.WriteLine(item.PathDisplay);
                        }


                    if (item.Name.EndsWith(".jpg") || item.Name.EndsWith(".jpeg") || item.Name.EndsWith(".png"))
                        {
                        var downloadTask = Task.Run(() => program.Download(item.PathDisplay));
                        downloadTask.Wait();
                        var data = downloadTask.Result;
                        MemoryStream ms = new MemoryStream(data);
                        Image im = Image.FromStream(ms);
                        im.Save(fileDownloadPath + item.Name);
                        CreatedDate = DateTime.Now.ToString("yyyy-MM-dd");
                        Downloaded = 1;
                        AccountNumber = 9;
                        program.InsertIntoDropboxTable(AccountName, item.PathDisplay, item.Name, CreatedDate, Downloaded, AccountNumber);

                        }
                    else if (item.Name.EndsWith(".tif"))
                        {

                        var downloadTask = Task.Run(() => program.Download(item.PathDisplay));
                        downloadTask.Wait();
                        var data = downloadTask.Result;


                        using (FileStream stream = File.OpenWrite(fileDownloadPath + item.Name))
                            {
                            stream.Write(data, 0, data.Length);
                            }

                        //  tiffImage.Save(@"C:\Testing\Download\DownloadAll\" + item.Name);

                        CreatedDate = DateTime.Now.ToString("yyyy-MM-dd");
                        Downloaded = 1;
                        AccountNumber = 9;

                        program.InsertIntoDropboxTable(AccountName, item.PathDisplay, item.Name, CreatedDate, Downloaded, AccountNumber);

                        }

                    }
                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                    {
                    writer.WriteLine("Process End Time : " + DateTime.Now);
                    }
                }
            catch (Exception ex)
                {

                ExceptionLogging.SendExcepToDB(ex, "DownloadWebImages");
                }


            }

        private Bitmap BytesToBmp_Serialized(byte[] bmpBytes)
            {
            BinaryFormatter bf = new BinaryFormatter();
            // copy the bytes to the memory
            MemoryStream ms = new MemoryStream(bmpBytes);
            return (Bitmap)bf.Deserialize(ms);
            }




        public void InsertIntoDropboxTable(string _AccountName, string _DropboxPath, string _ImageFileName, string _CreatedDate, int _Downloaded, int _AccountNumber)
            {

            try
                {

                string InsertIntoDropboxTable = ConfigurationManager.AppSettings["InsertIntoDropboxTable"].ToString();
                string query = InsertIntoDropboxTable;

                // create connection and command
                using (SqlConnection cn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(query, cn))
                    {
                    // define parameters and their values
                    cmd.Parameters.AddWithValue("@AccountName", _AccountName);
                    cmd.Parameters.AddWithValue("@DropboxPath", _DropboxPath);

                    cmd.Parameters.AddWithValue("@ImageFileName", _ImageFileName);
                    cmd.Parameters.AddWithValue("@CreatedDate", _CreatedDate);
                    cmd.Parameters.AddWithValue("@Downloaded", _Downloaded);
                    cmd.Parameters.AddWithValue("@AccountNumber", _AccountNumber);


                    foreach (SqlParameter parameter in cmd.Parameters)
                        {
                        if (parameter.Value == null)
                            {
                            parameter.Value = DBNull.Value;
                            }
                        }
                    // open connection, execute INSERT, close connection
                    cn.Open();
                    cmd.ExecuteNonQuery();
                    cn.Close();

                    }
                }
            catch (Exception ex)
                {

                ExceptionLogging.SendExcepToDB(ex, "DownloadWebImages");
                }
            }






        public async Task DeleteIfExists(string path, string name, bool folder)
            {
            var exists = await Task.Run(() => DropboxExists(path, name, folder));

            if (exists)
                {
                Console.WriteLine((folder ? "Folder" : "File") + ": " + name + " already exists in '" + path + "' Folder. " + (folder ? "Folder" : "File") + " will be overwritten");
                await dbx.Files.DeleteV2Async(path + "/" + name);
                }
            }

        private string ConnectToDropbox(string _accessKey)
            {

            string accountName = string.Empty;
            bool connected = false;
            //  string accessKey = "IXPTAm6aHmcAAAAAAAABW_UAxAK3nsg7UE4mkoXzJFEvquIXh5s_8e4kbK7StXfU"; test access key
           // string brandzAccessKey = "ojDLkPHH5DEAAAAAAAUREigqOWgcZ37cjZk251jk97OxgTtK8Nxmb6Trvkokm4kq"; // prod access key (Brandz access key)
            string brandzAccessKey = _accessKey;
           //   string distProdAccessKey = "JFNL9tCItmAAAAAAAAAAGO99iHkd2_3DL5qqGIu2afqRDgEmG18Wm1LGv7kBA7lm";

            // string accessKey = "IXPTAm6aHmcAAAAAAAABhRWtH13V7rNZdqhqBdRBZzQ";
            if (brandzAccessKey != string.Empty)
                {
                //Establish the dropbox connection
                Console.WriteLine("Attempting dropbox connection...");
                dbx = new DropboxClient(brandzAccessKey);

                //Check if the previously established connection is valid by making a small request of the users account name
                var getAccount = Task.Run(dbx.Users.GetCurrentAccountAsync);
                getAccount.Wait();
                Console.WriteLine("Dropbox connection established. Connected as {0}!\n", getAccount.Result.Name.DisplayName);
                connected = true;

                accountName = getAccount.Result.Name.DisplayName;
                }

            return accountName;
            }



        public async Task<bool> DropboxExists(string path, string name, bool folder)
            {
            var list = await dbx.Files.ListFolderAsync(path);
            foreach (var item in list.Entries.Where(i => (folder ? i.IsFolder : i.IsFile)))
                {
                if (item.Name == name)
                    //  Console.WriteLine("{0} Folder Exists.......", name);
                    return true;
                }
            //Console.WriteLine("{0} Folder Doesn't Exist.......", name);
            return false;
            }

        public async Task<byte[]> Download(string path)
            {
            using (var response = await dbx.Files.DownloadAsync(path))
                {
                return await response.GetContentAsByteArrayAsync();
                }
            }
        }
    }
