using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;

namespace S3FileUpload
{
    public class S3Uploader
    {

        string connectionString = ConfigurationManager.ConnectionStrings["sqlConn"].ToString();

        string url = string.Empty;
        int? processedFlag = 0;
        int? createdFlag = 0;
        int? updatedFlag = 0;
        string createdDate = null;
        string updatedDate = null;
        /* Test S3 Bucket name
    private string bucketName = "distimagerepository";

    private string keyName = "distimages";
    */

           private string bucketName = "acr-ipad-images"; // PROD

      //  private string bucketName = "distimagerepository";

        //  private string filePath = @"C:\Personal\ImageProcessor\OriginalImage\Image\CW06\93238946.jpg";



        public void UploadFile(string _filePath, string _fileName)

    {
        
      var client = new AmazonS3Client(Amazon.RegionEndpoint.APSoutheast2);

       

      PutObjectRequest putRequest = new PutObjectRequest

      {

        BucketName = bucketName,

        Key = _fileName,

        FilePath = _filePath,

        ContentType = "image"

      };

 

      PutObjectResponse response = client.PutObject(putRequest);

            string successCode = response.HttpStatusCode.ToString();

            if (successCode == "OK")
            {
                 url = "https://s3-ap-southeast-2.amazonaws.com/acr-ipad-images/" + _fileName;
                 processedFlag = 1;
                createdDate = DateTime.Now.ToString("yyyy-MM-dd");
                InsertIntoOrderRequestItemData(_fileName, url, processedFlag, createdFlag, updatedFlag, createdDate, updatedDate);
                MoveProcessedFiles(_fileName);
            }



        }

        public void InsertIntoOrderRequestItemData(string _FileName,string _URL,int? _Processed_Flag,int? _Created_Flag,int? _Updated_Flag,string _CreatedDate,string _UpdatedDate)
        {

            try
            {

                string InsertIntoOrderRequestItemData = ConfigurationManager.AppSettings["InsertIntoOrderRequestItemData"].ToString();
                string query = InsertIntoOrderRequestItemData;

                // create connection and command
                using (SqlConnection cn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(query, cn))
                {
                    // define parameters and their values
                    cmd.Parameters.AddWithValue("@FileName", _FileName);
                    cmd.Parameters.AddWithValue("@URL", _URL);
                    cmd.Parameters.AddWithValue("@Processed_Flag", _Processed_Flag);
                    cmd.Parameters.AddWithValue("@Created_Flag", _Created_Flag);
                    cmd.Parameters.AddWithValue("@Updated_Flag", _Updated_Flag);
                    cmd.Parameters.AddWithValue("@Created_Date", _CreatedDate);
                    cmd.Parameters.AddWithValue("@Updated_Date", _UpdatedDate);
                 

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

             
            }
        }


        public void MoveProcessedFiles(string _fileName)
        {

            try
            {
                #region Move files to Processed folder

                // string sourcePath = @"\\teapot\D$\ORDERS_IN\SPOTLESS";
                string sourcePathFromAppSettings = ConfigurationManager.AppSettings["sourcePathFromAppSettings"].ToString();
                string sourcePath = sourcePathFromAppSettings;


                //string targetPath = @"\\teapot\D$\ORDERS_IN\SPOTLESS\Processed";
                string targetPathFromAppSettings = ConfigurationManager.AppSettings["targetPathFromAppSettings"].ToString();
                string targetPath = targetPathFromAppSettings;
                if (!Directory.Exists(targetPath))

                {
                    Directory.CreateDirectory(targetPath);
                }



                //Copy the file from sourcepath and place into mentioned target path, 
                //Overwrite the file if same file is exist in target path
                foreach (var srcPath in Directory.GetFiles(sourcePath))
                {
                    //Copy the file from sourcepath and place into mentioned target path, 
                    //Overwrite the file if same file is exist in target path
                    File.Copy(srcPath, srcPath.Replace(sourcePath, targetPath), true);
                    File.Delete(srcPath);
                    break;
                }

                #endregion
            }
            catch (Exception ex)
            {
               
            }

        }

    }
}
