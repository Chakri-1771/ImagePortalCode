using Dropbox.Api;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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
            string hashCode;
            string oldHashCode = string.Empty;
            DateTime CreatedDate;
            DateTime updatedDateTimeForInsert;
            string updatedDateTimeForUpdate;
            int Downloaded = 0;
            int Updated = 0;
            int AccountNumber = 0;
            int seqID = 0;
            DataRow[] hasSameImage;
            DataRow[] hasSameHashCode;


            try
                {

                DataTable allFileDetails = new DataTable();
                allFileDetails = program.GetAllFileDetails();

                string brandzAccessKey = ConfigurationManager.AppSettings["BrandzAccessKey"].ToString();
                string imageRootPath = ConfigurationManager.AppSettings["ImageRootPath"].ToString();
                string logFilePath = ConfigurationManager.AppSettings["LogFilePath"].ToString();
                string fileDownloadPath = ConfigurationManager.AppSettings["FileDownloadPath"].ToString();

                AccountName = program.ConnectToDropbox(brandzAccessKey);



                List<string> paths = new List<string>();

                var allPathsList = program.dbx.Files.ListFolderAsync(imageRootPath).Result;


                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                    {
                    writer.WriteLine("Process StartTime : " + DateTime.Now);
                    }

                foreach (var allPaths in allPathsList.Entries.Where(i => i.IsFolder))
                    {


                    paths.Add(allPaths.PathDisplay);

                    var list = program.dbx.Files.ListFolderAsync(allPaths.PathDisplay, recursive: true).Result;

                    foreach (var item in list.Entries.Where(i => i.IsFile))
                        {


                     

                        paths.Add(item.PathDisplay);

                      

                        if (item.Name.Contains("'"))
                            {
                            string escAphostrophe = item.Name;

                            escAphostrophe = escAphostrophe.Replace("'", "''");
                            hasSameImage = allFileDetails.Select("ImageFileName = " + "'" + escAphostrophe + "'");
                            hasSameHashCode = allFileDetails.Select(" HashCode = " + "'" + item.AsFile.ContentHash.ToString() + "'");
                            }
                        else
                            {
                            hasSameImage = allFileDetails.Select("ImageFileName = " + "'" + item.Name + "'");
                            hasSameHashCode = allFileDetails.Select(" HashCode = " + "'" + item.AsFile.ContentHash.ToString() + "'");
                            }


                        if (hasSameImage.Length == 0 && hasSameHashCode.Length == 0)
                            {
                            if (item.Name.EndsWith(".jpg") || item.Name.EndsWith(".jpeg") || item.Name.EndsWith(".png") || item.Name.EndsWith(".JPG"))
                                {


                                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                                    {
                                    writer.WriteLine(item.PathDisplay);
                                    }

                                var downloadTask = Task.Run(() => program.Download(item.PathDisplay));
                                downloadTask.Wait();
                                var data = downloadTask.Result;
                                MemoryStream ms = new MemoryStream(data);
                                Image im = Image.FromStream(ms);
                                im.Save(fileDownloadPath + item.Name);
                                hashCode = item.AsFile.ContentHash.ToString();
                                updatedDateTimeForInsert = item.AsFile.ServerModified;
                                CreatedDate = item.AsFile.ClientModified;
                                Downloaded = 1;
                                AccountNumber = 9;
                                program.InsertIntoDropboxTable(AccountName, item.PathDisplay, item.Name, hashCode, CreatedDate, Downloaded, Updated, updatedDateTimeForInsert, AccountNumber);

                                }
                            else if (item.Name.EndsWith(".tif"))
                                {

                                var downloadTask = Task.Run(() => program.Download(item.PathDisplay));
                                downloadTask.Wait();
                                var data = downloadTask.Result;

                                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                                    {
                                    writer.WriteLine(item.PathDisplay);
                                    }

                                using (FileStream stream = File.OpenWrite(fileDownloadPath + item.Name))
                                    {
                                    stream.Write(data, 0, data.Length);
                                    }

                                hashCode = item.AsFile.ContentHash.ToString();
                                updatedDateTimeForInsert = item.AsFile.ServerModified;
                                CreatedDate = item.AsFile.ClientModified;
                                Downloaded = 1;
                                AccountNumber = 9;

                                program.InsertIntoDropboxTable(AccountName, item.PathDisplay, item.Name, hashCode, CreatedDate, Downloaded, Updated, updatedDateTimeForInsert, AccountNumber);

                                }

                            }
                        else if (hasSameImage.Count() != 0 && hasSameHashCode.Length == 0)
                            {
                            if (item.Name.EndsWith(".jpg") || item.Name.EndsWith(".jpeg") || item.Name.EndsWith(".png") || item.Name.EndsWith(".JPG"))
                                {
                           

                                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                                    {
                                    writer.WriteLine(item.PathDisplay);
                                    }

                                var downloadTask = Task.Run(() => program.Download(item.PathDisplay));
                                downloadTask.Wait();
                                var data = downloadTask.Result;
                                MemoryStream ms = new MemoryStream(data);
                                Image im = Image.FromStream(ms);
                                im.Save(fileDownloadPath + item.Name);
                                hashCode = item.AsFile.ContentHash.ToString();
                                updatedDateTimeForUpdate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");  //item.AsFile.ServerModified;
                                CreatedDate = item.AsFile.ClientModified;
                                Downloaded = 1;
                                Updated = 1;
                                AccountNumber = 9;

                                DataSet dsUpdateValues = new DataSet();
                                if (item.Name.Contains("'"))
                                    {
                                    string escApho = item.Name;
                                    escApho = escApho.Replace("'", "''");
                                    dsUpdateValues = program.GetSeq_ID(escApho);

                                    program.UpdateImageDetails(hashCode, updatedDateTimeForUpdate, int.Parse(dsUpdateValues.Tables[0].Rows[0]["Updated"].ToString()), int.Parse(dsUpdateValues.Tables[0].Rows[0]["Seq_ID"].ToString()));
                                    }
                                else
                                    {
                                    dsUpdateValues = program.GetSeq_ID(item.Name);

                                    program.UpdateImageDetails(hashCode, updatedDateTimeForUpdate, int.Parse(dsUpdateValues.Tables[0].Rows[0]["Updated"].ToString()), int.Parse(dsUpdateValues.Tables[0].Rows[0]["Seq_ID"].ToString()));
                                    }

                                }
                            else if (item.Name.EndsWith(".tif"))
                                {

                                var downloadTask = Task.Run(() => program.Download(item.PathDisplay));
                                downloadTask.Wait();
                                var data = downloadTask.Result;


                                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                                    {
                                    writer.WriteLine(item.PathDisplay);
                                    }

                                using (FileStream stream = File.OpenWrite(fileDownloadPath + item.Name))
                                    {
                                    stream.Write(data, 0, data.Length);
                                    }

                                hashCode = item.AsFile.ContentHash.ToString();
                                updatedDateTimeForUpdate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); //item.AsFile.ServerModified;
                                CreatedDate = item.AsFile.ClientModified;
                                Downloaded = 1;
                                Updated = 1;
                                AccountNumber = 9;
                                DataSet dsUpdateValues = new DataSet();
                                if (item.Name.Contains("'"))
                                    {
                                    string escApho = item.Name;
                                    escApho = escApho.Replace("'", "''");
                                    dsUpdateValues = program.GetSeq_ID(escApho);

                                    program.UpdateImageDetails(hashCode, updatedDateTimeForUpdate, int.Parse(dsUpdateValues.Tables[0].Rows[0]["Updated"].ToString()), int.Parse(dsUpdateValues.Tables[0].Rows[0]["Seq_ID"].ToString()));
                                    }
                                else
                                    {
                                    dsUpdateValues = program.GetSeq_ID(item.Name);

                                    program.UpdateImageDetails(hashCode, updatedDateTimeForUpdate, int.Parse(dsUpdateValues.Tables[0].Rows[0]["Updated"].ToString()), int.Parse(dsUpdateValues.Tables[0].Rows[0]["Seq_ID"].ToString()));
                                    }


                                }

                            }
                        

                        }


                    }
                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                    {
                    writer.WriteLine("Process EndTime : " + DateTime.Now);
                    }
                }
            catch (Exception ex)
                {
                ExceptionLogging.SendExcepToDB(ex, "Download Web Images");
                }


            }

        private Bitmap BytesToBmp_Serialized(byte[] bmpBytes)
            {
            BinaryFormatter bf = new BinaryFormatter();
            // copy the bytes to the memory
            MemoryStream ms = new MemoryStream(bmpBytes);
            return (Bitmap)bf.Deserialize(ms);
            }




        public void InsertIntoDropboxTable(string _AccountName, string _DropboxPath, string _ImageFileName, string _HashCode, DateTime _CreatedDate, int _Downloaded, int _Updated, DateTime _UpdatedDate, int _AccountNumber)
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
                    cmd.Parameters.AddWithValue("@HashCode", _HashCode);
                    cmd.Parameters.AddWithValue("@CreatedDate", _CreatedDate);
                    cmd.Parameters.AddWithValue("@Downloaded", _Downloaded);
                    cmd.Parameters.AddWithValue("@Updated", _Updated);
                    cmd.Parameters.AddWithValue("@UpdatedDate", _UpdatedDate);
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

                ExceptionLogging.SendExcepToDB(ex, "TDGDropBoxFile_Download");
                }
            }

        public void UpdateImageDetails(string _HashCode, string _UpdatedDate, int _Updated,int Seq_Id)
            {
            try
                {
                SqlConnection conn = new SqlConnection(connectionString);
                string query = ConfigurationManager.AppSettings["UpdateQuery"].ToString();
                int update = _Updated + 1 ;
                string updateQuery = String.Format(query, _HashCode, update, _UpdatedDate, Seq_Id);
                SqlCommand cmd = new SqlCommand(updateQuery, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();

                }
            catch (Exception ex)
                {
                ExceptionLogging.SendExcepToDB(ex, "TDGDropBoxFile_Download - UpdateImageDetails");
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

        public DataSet GetSeq_ID(string _ImageName)
            {
            DataSet dsUpdateValues = new DataSet();
            try
                {

                SqlConnection conn = new SqlConnection(connectionString);
                string query = ConfigurationManager.AppSettings["GetValuesForUpdateQuery"].ToString();
                query = String.Format(query, _ImageName);
                SqlDataAdapter dtUpdateValues = new SqlDataAdapter(query, conn);
                dtUpdateValues.Fill(dsUpdateValues);

                }
            catch (Exception ex)
                {
                ExceptionLogging.SendExcepToDB(ex, "TDGDropBoxFile_Download - GetSeq_ID");
                }

            return dsUpdateValues;

            }


        public DataTable GetAllFileDetails()
            {
            DataTable fileDetails = new DataTable();
            string GetFileDetails = ConfigurationManager.AppSettings["FileDetails"].ToString();
            string query = GetFileDetails;
            SqlDataAdapter dtFileDetails = new SqlDataAdapter(query, connectionString);
            DataSet dsFileDetails = new DataSet();
            dtFileDetails.Fill(dsFileDetails);

            fileDetails = dsFileDetails.Tables[0];

            return fileDetails;
            }
        }
    }
