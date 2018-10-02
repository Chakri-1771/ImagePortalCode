using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Configuration;
using System.Drawing.Imaging;
using System.Data.SqlClient;
using System.Data;

namespace ImageProcessingEngine
    {
    class Program
        {
        Image img;
        Image img1;
        string sourcePath = ConfigurationManager.AppSettings["SourcePath"];

        string imagePath = ConfigurationManager.AppSettings["ImagePath"];

        string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ToString();


        static void Main(string[] args)
            {

           
            string[] extn;

           
            string finalDestination = string.Empty;

            List<Image> imagelist = new List<Image>();
            DataSet dsImages = new DataSet();

            string fullImagePath = string.Empty;

            string[] sizes;
            int width = 0;
            int height = 0;



            Program thisProgram = new Program();

            
            try
                {

               

                    using (SqlConnection con = new SqlConnection(thisProgram.connectionString))
                        {
                        string imagesToBeConverted = ConfigurationManager.AppSettings["viewImagesToBeConverted"].ToString();
                        SqlDataAdapter daImages = new SqlDataAdapter(imagesToBeConverted, con);
                        dsImages = new DataSet();
                        daImages.Fill(dsImages);
                        }

                    

                    DirectoryInfo dir = new DirectoryInfo(thisProgram.imagePath);

                    ImageDestinations destinations = new ImageDestinations();
                    string[] destination = destinations.GetDestinationsFromConfig();

                    ImageDimensions dimensions = new ImageDimensions();
                    string[] dimension = dimensions.GetDimensionsFromConfig();


                    ImageExtensions extns = new ImageExtensions();
                    extn = extns.GetExtensionsFromConfig();


                for (int i = 0; i < dsImages.Tables[0].Rows.Count; i++)
                    {

                    fullImagePath = thisProgram.imagePath + @"\" + dsImages.Tables[0].Rows[i]["IMAGE_FILE_NAME"].ToString();

                    FileInfo file = new FileInfo(fullImagePath);

                    //string fileName = Path.GetFileNameWithoutExtension(file.FullName);
                    //    Bitmap.FromFile(file.FullName)
                    //               .Save(file.DirectoryName + "\\" + fileName + ".jpg", ImageFormat.Jpeg);

                
                            if (imagelist.Count != 0)
                                {
                                imagelist.Clear();
                                }
                            imagelist.Add(Image.FromFile(file.FullName));

                            //  extn = Path.GetExtension(file.ToString());
                            foreach (var dims in dimension)
                                {
                                sizes = dims.Split('x');
                                width = int.Parse(sizes[0]);
                                height = int.Parse(sizes[1]);
                                finalDestination = Array.Find(destination, s => s.Contains(width.ToString()));
                                foreach (Image img in imagelist)
                                    {
                                    if (!Directory.Exists(finalDestination))
                                        {
                                        DirectoryInfo di = Directory.CreateDirectory(finalDestination);
                                        }

                                    thisProgram.img1 = thisProgram.Resize(img, width, height);
                                    thisProgram.img1.Save(finalDestination + file.Name);
                            thisProgram.UpdateImageFlags(int.Parse(dsImages.Tables[0].Rows[i]["SeqID"].ToString()));
                                    }
                                }
                          
                    }

                 
                    
              
                }
            catch (Exception ex)
                {
                ExceptionLogging.SendExcepToDB(ex, "Image Processing Engine");
                }
            }

        Image Resize(Image image, int w, int h)
            {
            Bitmap bmp = new Bitmap(w, h);
            try
                {                
                Graphics graphic = Graphics.FromImage(bmp);                
                graphic.DrawImage(image, 0, 0, w, h);
                graphic.Dispose();
                }
            catch (Exception ex)
                {
                ExceptionLogging.SendExcepToDB(ex, "Image Processing Engine");
                }            
            return bmp;
            }

        public void UpdateImageFlags(int seqID)
            {
            try
                {
                SqlConnection conn = new SqlConnection(connectionString);
                string query = ConfigurationManager.AppSettings["updateFlagsInImageTable"].ToString();
                string updateDate = DateTime.Now.ToString("yyyy-MM-dd");
                string updateQuery = String.Format(query, 1,0, updateDate, seqID);
                SqlCommand cmd = new SqlCommand(updateQuery, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();

                }
            catch (Exception ex)
                {
                ExceptionLogging.SendExcepToDB(ex, "ImageProcessingEngine - UpdateImageFlags");
                }



            }

        }
    }
