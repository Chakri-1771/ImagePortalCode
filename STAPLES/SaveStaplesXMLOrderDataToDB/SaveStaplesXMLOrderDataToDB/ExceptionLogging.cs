using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;


namespace SaveStaplesXMLOrderDataToDB
    {
    class ExceptionLogging
        {
        static SqlConnection con;
        private static void connecttion()
            {
            string constr = ConfigurationManager.ConnectionStrings["sqlConn"].ToString();
            con = new SqlConnection(constr);
            con.Open();
            }
        public static void SendExcepToDB(Exception exdb, string _Module)
            {

            connecttion();
            SqlCommand com = new SqlCommand("ExceptionLoggingToDataBase", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@ExceptionMessage", exdb.Message.ToString());
            com.Parameters.AddWithValue("@ExceptionType", exdb.GetType().Name.ToString());
            com.Parameters.AddWithValue("@ExceptionSource", exdb.StackTrace.ToString());
            com.Parameters.AddWithValue("@Module", _Module);
            com.Parameters.AddWithValue("@Logdate", DateTime.Now.ToString("yyyy-MM-dd"));
            com.ExecuteNonQuery();

            }
        }
    }
