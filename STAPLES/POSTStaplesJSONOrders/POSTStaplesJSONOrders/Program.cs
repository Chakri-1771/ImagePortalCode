using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Mail;
using System.Linq;
using System.Configuration;

namespace POSTStaplesJSONOrders
    {
    class Program
        {

        SqlDataAdapter daData;
        DataSet dsData;
        SqlConnection objConn;
        SqlCommand cmd;

        StringBuilder finalJSON = new StringBuilder();
        StringBuilder tokenJSON = new StringBuilder();
        //string connectionString = "Data Source=TEAPOT;Initial Catalog=SpotLess;Integrated Security=SSPI";
        string connectionString = ConfigurationManager.ConnectionStrings["sqlConn"].ToString();
        string singleResponseJSON = string.Empty;
        string fileName = "WEB_SERVICE_ONLY";

        // string clientId = "dist_spotless";
        string clientId = ConfigurationManager.AppSettings["clientId"].ToString();

        //string clientKey = "qvCbP7lmE9WkuUydP3mvcMGODM5SPbyg";
        string clientKey = ConfigurationManager.AppSettings["clientKey"].ToString();

        string lastPONumber = "";


        static void Main(string[] args)
            {

            Program thisProgram = new Program();
            string responseTokenString = string.Empty;
            thisProgram.objConn = new SqlConnection(thisProgram.connectionString);
            thisProgram.objConn.Open();
            // thisProgram.daData = new SqlDataAdapter("select distinct ponumber,member_cust_id,ACR_SITE_CODE from SPOTLESS_ORDER_VALID_DATA_VIEW", thisProgram.connectionString);
            thisProgram.daData = new SqlDataAdapter(ConfigurationManager.AppSettings["selectDataFromView"], thisProgram.connectionString);
            thisProgram.objConn.Close();
            thisProgram.dsData = new DataSet();
            thisProgram.daData.Fill(thisProgram.dsData);
            string singlePostJSON = string.Empty;


            string traceID = string.Empty;
            string jsonMessage = string.Empty;
            int jsonStatus = 0;
            string jsonSubStatus = string.Empty;
            DateTime date = DateTime.Now;
            string sDate;

            sDate = date.ToString("yyyy-MM-dd");

            try
                {

                for (int i = 0; i < thisProgram.dsData.Tables[0].Rows.Count; i++)
                    {

                    thisProgram.lastPONumber = thisProgram.dsData.Tables[0].Rows[i]["ponumber"].ToString();
                    thisProgram.tokenJSON.Clear();
                    responseTokenString = thisProgram.GetToken();
                    JObject acrKey = JObject.Parse(responseTokenString);
                    string key = string.Empty;
                    key = acrKey["Data"]["key"].ToString();
                    thisProgram.finalJSON.Clear();
                    thisProgram.AppendHeaderForSingleOrder(key, thisProgram.dsData.Tables[0].Rows[i]["ACR_SITE_CODE"].ToString());
                    thisProgram.AppendHeaderwithPartiesForSingleOrder(thisProgram.dsData.Tables[0].Rows[i]["ponumber"].ToString());
                    thisProgram.AppendHeaderDataForSingleOrder(thisProgram.dsData.Tables[0].Rows[i]["ponumber"].ToString());
                    thisProgram.AppendClosingTagsForSingleOrder();

                    singlePostJSON = thisProgram.finalJSON.ToString();

                    thisProgram.singleResponseJSON = thisProgram.postJSONOrder(thisProgram.dsData.Tables[0].Rows[i]["ponumber"].ToString(), singlePostJSON);

                    JObject o = JObject.Parse(thisProgram.singleResponseJSON);

                    traceID = (o["Data"]["trace_id"].ToString() == null ? null : o["Data"]["trace_id"].ToString());
                    jsonMessage = (o["Message"].ToString() == null ? null : o["Message"].ToString());
                    int stat = Convert.ToInt32((o["Status"].ToString()));
                    if (stat != 0)
                        {
                        jsonStatus = stat;
                        }
                    else
                        {
                        jsonStatus = 0;
                        }
                    jsonSubStatus = (o["SubStatus"].ToString() == null ? null : o["SubStatus"].ToString());




                    thisProgram.insert(thisProgram.dsData.Tables[0].Rows[i]["ponumber"].ToString(), DateTime.Now, thisProgram.fileName, traceID, thisProgram.dsData.Tables[0].Rows[i]["member_cust_id"].ToString(), jsonMessage, jsonStatus, sDate);




                    }
                }
            catch (Exception ex)// Need to implement mail funtionality
                {
                //thisProgram.sendErrorMail(ex, lastPONumber);
                thisProgram.LogErrorInDB(thisProgram.lastPONumber, ex.Message, DateTime.Now, "POSTStaplesJSONOrders");
                }
            finally
                {

                }

            }


        public void AppendHeaderForSingleOrder(string key, string acrSiteCode)
            {

            try
                {
                finalJSON.Append(@"{");
                finalJSON.AppendLine();
                //Appending ClientID
                finalJSON.Append("\"");
                finalJSON.Append(@"clientId");
                finalJSON.Append("\"");
                finalJSON.Append(":");
                finalJSON.Append("\"");
                finalJSON.Append(clientId);//ACRTest
                finalJSON.Append("\"");
                finalJSON.Append(@", ");

                //Appending ClientKey
                finalJSON.AppendLine();
                finalJSON.Append("\"");
                finalJSON.Append(@"clientKey");
                finalJSON.Append("\"");
                finalJSON.Append(":");
                finalJSON.Append("\"");
                finalJSON.Append(key);
                finalJSON.Append("\"");
                //106ef642463167e03bc6cdf5d0f6ff930ede3b875949638335e1964f2e1127c3c614650df499be12bbf0a52b344cdd3d4c2a61798c0 5b5d29c5d0a8362d16532
                finalJSON.Append(@", ");

                //Appending parameters
                finalJSON.AppendLine();
                finalJSON.Append("\"");
                finalJSON.Append(@"parameters");
                finalJSON.Append("\"");
                finalJSON.Append(":");
                finalJSON.Append(@"  {");

                //Appending sender
                finalJSON.AppendLine();
                finalJSON.Append("     \"");
                finalJSON.Append(@"sender");
                finalJSON.Append("\"");
                finalJSON.Append(":");
                finalJSON.Append(@"""staples""");
                finalJSON.Append(@",");


                //Appending recipient
                finalJSON.AppendLine();
                finalJSON.Append("     \"");
                finalJSON.Append(@"recipient");
                finalJSON.Append("\"");

                finalJSON.Append(":");
                finalJSON.Append("\"");
                // finalJSON.Append(acrSiteCode.ToLower());
                finalJSON.Append("dist");
                finalJSON.Append("\"");
                finalJSON.Append(@",");
                finalJSON.AppendLine();



                }
            catch (Exception ex)
                {
                LogErrorInDB(lastPONumber, ex.Message, DateTime.Now, "POSTStaplesJSONOrders");
                }

            }
        public void AppendHeaderwithPartiesForSingleOrder(string OrderNumber)
            {
            try
                {
                string selectPartiesDataQuery = ConfigurationManager.AppSettings["selectPartiesDataQuery"].ToString();
                //SqlDataAdapter daHeaderwithParties = new SqlDataAdapter("select ponumber, CONVERT(VARCHAR(20), podate, 103) as podate, " +
                //     " CONVERT(VARCHAR(20), Requesteddeliverydate, 103) as Requesteddeliverydate,member_cust_id, barcode, real_qty, line_no,ACR_SITE_CODE " +
                //     " from SPOTLESS_ORDER_VALID_DATA_VIEW where ponumber = " + "'" + OrderNumber + "'", connectionString);

                SqlDataAdapter daHeaderwithParties = new SqlDataAdapter(selectPartiesDataQuery + "'" + OrderNumber + "'", connectionString);
                // int lineNumber = 1;
                objConn.Close();
                DataSet dsHeaderwithParties = new DataSet();
                daHeaderwithParties.Fill(dsHeaderwithParties);
                OrderHeader();
                //Appending order from DB
                finalJSON.AppendLine();
                finalJSON.Append(@"                ""customerOrderReference""");
                finalJSON.Append(@":");
                finalJSON.Append("\"");
                finalJSON.Append(dsHeaderwithParties.Tables[0].Rows[0]["ponumber"]);
                finalJSON.Append("\"");
                finalJSON.Append(",");

                ////Appending Header Comment from DB
                //finalJSON.AppendLine();
                //finalJSON.Append(@"                ""CommentData""");
                //finalJSON.Append(@":");
                //finalJSON.Append("\"");
                //finalJSON.Append(dsHeaderwithParties.Tables[0].Rows[0]["Header_Comments"]);
                //finalJSON.Append("\"");
                //finalJSON.Append(",");


                //Appending ordertype from DB
                finalJSON.AppendLine();
                finalJSON.Append(@"                ""orderType""");
                finalJSON.Append(@":");
                finalJSON.Append(1);
                finalJSON.Append(",");

                //Appending orderdate from DB
                finalJSON.AppendLine();
                finalJSON.Append(@"                ""orderDate""");
                finalJSON.Append(@":");
                finalJSON.Append("\"");
                finalJSON.Append(dsHeaderwithParties.Tables[0].Rows[0]["podate"]);
                finalJSON.Append("\"");
                finalJSON.Append(",");

                //Appending first deliverydate
                finalJSON.AppendLine();
                finalJSON.Append(@"                ""firstDeliveryDate""");
                finalJSON.Append(@":");
                finalJSON.Append("\"");               
                finalJSON.Append(dsHeaderwithParties.Tables[0].Rows[0]["requesteddeliveryDate"]);
                finalJSON.Append("\"");
                finalJSON.Append(",");

                //Appending last deliverydate
                finalJSON.AppendLine();
                finalJSON.Append(@"                ""lastDeliveryDate""");
                finalJSON.Append(@":");
                finalJSON.Append("\"");
                finalJSON.Append(dsHeaderwithParties.Tables[0].Rows[0]["requesteddeliveryDate"]);
                finalJSON.Append("\"");
                finalJSON.Append(",");

                //SqlDataAdapter daHeaderComments = new SqlDataAdapter("SELECT  [OrderNumber],[Comments]     ,[commentType]  " +
                //   " FROM[Spotless].[dbo].[SL_ORDER_H2_VIEW] where ordernumber = " + "'" + OrderNumber + "'", connectionString);

                string selectHeaderPartiesDataQuery = ConfigurationManager.AppSettings["selectHeaderCommentsDataQuery"].ToString();
                SqlDataAdapter daHeaderComments = new SqlDataAdapter(selectHeaderPartiesDataQuery + "'" + OrderNumber + "'", connectionString);
                objConn.Close();
                DataSet dsHeaderComments = new DataSet();
                daHeaderComments.Fill(dsHeaderComments);

                //Appending Header Comments
                finalJSON.AppendLine();
                finalJSON.Append(@"                ""comment""");
                finalJSON.Append(@":");
                finalJSON.Append("\"");
                finalJSON.Append(dsHeaderComments.Tables[0].Rows[0]["Header_comments"]);
                finalJSON.Append("\"");

                finalJSON.AppendLine();
                finalJSON.Append("             }");
                finalJSON.Append(",");







                //Appending Control section
                finalJSON.AppendLine();
                finalJSON.Append(@"                ""control""");
                finalJSON.Append(@":");
                finalJSON.Append(" {");
                finalJSON.AppendLine();


                finalJSON.AppendLine();
                //Appending ClientID
                finalJSON.Append("\"");
                finalJSON.Append(@"internalClientId");
                finalJSON.Append("\"");
                finalJSON.Append(":");
                finalJSON.Append("\"");
                finalJSON.Append("staples");
                // finalJSON.Append(clientId);//ACRTest
                finalJSON.Append("\"");
                finalJSON.Append(@", ");


                finalJSON.AppendLine();
                //Appending ClientID
                finalJSON.Append("\"");
                finalJSON.Append(@"internalPartyId");
                finalJSON.Append("\"");
                finalJSON.Append(":");
                finalJSON.Append("\"");
                //  finalJSON.Append(dsHeaderwithParties.Tables[0].Rows[0]["ACR_SITE_CODE"].ToString().ToLower());
                finalJSON.Append("staples");  // change this field to set the source in ACR price validator 
                finalJSON.Append("\"");
                finalJSON.Append(@", ");
                finalJSON.AppendLine();

                finalJSON.Append("\"");
                finalJSON.Append("internalReferenceNumber");
                finalJSON.Append("\"");
                finalJSON.Append(@":");
                finalJSON.Append("\"");
                finalJSON.Append(dsHeaderwithParties.Tables[0].Rows[0]["ponumber"]);
                finalJSON.Append("\"");
                // finalJSON.Append(",");
                finalJSON.AppendLine();
                finalJSON.Append("             }");
                finalJSON.Append(",");

                //Appending parties  line
                finalJSON.AppendLine();
                finalJSON.Append(@"             ""parties""");
                finalJSON.Append(@":");
                finalJSON.Append(@"  {");
                finalJSON.AppendLine();

                //Appending parties Type "BY" Header
                finalJSON.Append("                  \"");
                finalJSON.Append("BY");
                finalJSON.Append("\"");
                finalJSON.Append(":");
                finalJSON.Append(@"  {");

                //Appending parties Type "BY"
                finalJSON.AppendLine();
                finalJSON.Append("                         \"");
                finalJSON.Append("type");
                finalJSON.Append("\"");
                finalJSON.Append(":");
                finalJSON.Append(" \"");
                finalJSON.Append("BY");
                finalJSON.Append("\"");
                finalJSON.Append(",");

                //Appending parties ID 

                finalJSON.AppendLine();
                finalJSON.Append("                         \"");
                finalJSON.Append("id");
                finalJSON.Append("\"");
                finalJSON.Append(":");
                finalJSON.Append("   \"");
                finalJSON.Append(dsHeaderwithParties.Tables[0].Rows[0]["MEMBER_CUST_ID"].ToString().Trim());
                finalJSON.Append("\"");
                finalJSON.AppendLine();
                finalJSON.Append(@"                            }");
                finalJSON.AppendLine();
                finalJSON.Append(@"             }");
                finalJSON.Append(",");
                }
            catch (Exception ex)
                {
                LogErrorInDB(lastPONumber, ex.Message, DateTime.Now, "POSTStaplesJSONOrders");
                }



            }
        public void OrderHeader()
            {
            try
                {
                //Appending order
                finalJSON.AppendLine();
                finalJSON.Append("     \"");
                finalJSON.Append(@"order");
                finalJSON.Append("\"");
                finalJSON.Append(":");
                finalJSON.Append(@"  {");
                finalJSON.AppendLine();

                //Appending header
                finalJSON.AppendLine();
                finalJSON.Append("          \"");
                finalJSON.Append(@"header");
                finalJSON.Append("\"");
                finalJSON.Append(":");
                finalJSON.Append(@"  {");
                }
            catch (Exception ex)
                {
                LogErrorInDB(lastPONumber, ex.Message, DateTime.Now, "POSTStaplesJSONOrders");
                }


            }
        public void AppendHeaderDataForSingleOrder(string OrderNumber)
            {
            try
                {
                //SqlDataAdapter daHeader = new SqlDataAdapter("select distinct ponumber from ACR_REST_JSON_TEST ", connectionString);

                //      SqlDataAdapter daHeader = new SqlDataAdapter("select ponumber, CONVERT(VARCHAR(20), podate, 101) as podate, " +
                //" member_cust_id, barcode, real_qty, line_no " +
                //" from SPOTLESS_ORDER_VALID_DATA_VIEW where ponumber = " + "'" + OrderNumber + "'", connectionString);

                string selectHeaderDataQuery = ConfigurationManager.AppSettings["selectHeaderDataQuery"].ToString();
                SqlDataAdapter daHeader = new SqlDataAdapter(selectHeaderDataQuery + "'" + OrderNumber + "'", connectionString);
                // int lineNumber = 1;
                objConn.Close();
                DataSet dsHeader = new DataSet();
                daHeader.Fill(dsHeader);




                for (int i = 0; i < dsHeader.Tables[0].Rows.Count; i++)
                    {


                    finalJSON.AppendLine();
                    finalJSON.Append("             \"");
                    finalJSON.Append("lines");
                    finalJSON.Append("\"");
                    finalJSON.Append(":");
                    finalJSON.Append(@"{");


                    //Appending Lines Header
                    finalJSON.AppendLine();
                    finalJSON.Append("                \"");
                    finalJSON.Append("toSupplyItems");
                    finalJSON.Append("\"");
                    finalJSON.Append(":");
                    finalJSON.Append(@"{");



                    AppendLineHeadersForSingleOrder(dsHeader.Tables[0].Rows[i]["ponumber"].ToString());




                    if (i == 0)
                        break;
                    }
                }
            catch (Exception ex)
                {
                LogErrorInDB(lastPONumber, ex.Message, DateTime.Now, "POSTStaplesJSONOrders");
                }



            }
        public void AppendClosingTagsForSingleOrder()
            {
            try
                {
                //Appending recipient
                finalJSON.AppendLine();
                finalJSON.Append(@"}");
                finalJSON.AppendLine();
                finalJSON.Append(@"}");
                
                }
            catch (Exception ex)
                {
                LogErrorInDB(lastPONumber, ex.Message, DateTime.Now, "POSTStaplesJSONOrders");
                }



            }

        public void AppendLineHeadersForSingleOrder(string orderNumber) //pass order number as paramaeter
            {
            try
                {
                //SqlDataAdapter daLines = new SqlDataAdapter("select description,barcode, real_qty, line_no,unitprice,comments from SPOTLESS_ORDER_VALID_DATA_VIEW where ponumber = " + "'" + orderNumber + "' order by ponumber, line_no", connectionString);
                string selectLinesDataQuery = ConfigurationManager.AppSettings["selectLinesDataQuery"].ToString();
                SqlDataAdapter daLines = new SqlDataAdapter(selectLinesDataQuery + "'" + orderNumber + "' order by ponumber, line_no", connectionString);

                objConn.Close();
                DataSet dsLines = new DataSet();
                daLines.Fill(dsLines);
                int lineNumber = 1;

                for (int i = 0; i < dsLines.Tables[0].Rows.Count; i++)
                    {

                    finalJSON.AppendLine();
                    finalJSON.Append("                   \"");
                    finalJSON.Append("L");
                    finalJSON.Append(lineNumber++);
                    finalJSON.Append("\"");
                    finalJSON.Append(":");
                    finalJSON.Append(@"{");

                    finalJSON.AppendLine();
                    finalJSON.Append("                   \"");
                    finalJSON.Append("lineNumber");
                    finalJSON.Append("\"");
                    finalJSON.Append(":");

                    finalJSON.Append(dsLines.Tables[0].Rows[i]["Line_no"].ToString().Trim());
                    finalJSON.Append(",");


                    finalJSON.AppendLine();
                    finalJSON.Append("                        \"");
                    finalJSON.Append("id");
                    finalJSON.Append("\"");
                    finalJSON.Append(":");
                    finalJSON.Append("\"");
                    finalJSON.Append(dsLines.Tables[0].Rows[i]["barcode"].ToString().Trim());
                    finalJSON.Append("\"");
                    finalJSON.Append(",");

                    finalJSON.AppendLine();
                    finalJSON.Append("                        \"");
                    finalJSON.Append("itemDescription");
                    finalJSON.Append("\"");
                    finalJSON.Append(":");
                    finalJSON.Append("\"");
                    finalJSON.Append(dsLines.Tables[0].Rows[i]["description"].ToString().Trim());
                    finalJSON.Append("\"");
                    finalJSON.Append(",");

                    finalJSON.AppendLine();
                    finalJSON.Append("                        \"");
                    finalJSON.Append("orderQuantity");
                    finalJSON.Append("\"");
                    finalJSON.Append(":");
                    finalJSON.Append(dsLines.Tables[0].Rows[i]["Real_Qty"].ToString().Trim());
                    finalJSON.Append(",");


                    finalJSON.AppendLine();
                    finalJSON.Append("                        \"");
                    finalJSON.Append("unitPriceBase");
                    finalJSON.Append("\"");
                    finalJSON.Append(":");
                    finalJSON.Append("\"");
                    finalJSON.Append("E");//modify later
                    finalJSON.Append("\"");
                    finalJSON.Append(",");


                    finalJSON.AppendLine();
                    finalJSON.Append("                        \"");
                    finalJSON.Append("unitPriceAmount");
                    finalJSON.Append("\"");
                    finalJSON.Append(":");
                    finalJSON.Append(float.Parse(dsLines.Tables[0].Rows[i]["unitprice"].ToString().Trim()));



                    string comment = dsLines.Tables[0].Rows[i]["comments"].ToString();

                    if (comment != "")
                        {
                        finalJSON.Append(",");
                        finalJSON.AppendLine();
                        finalJSON.Append("                        \"");
                        finalJSON.Append("comment");
                        finalJSON.Append("\"");
                        finalJSON.Append(":");
                        finalJSON.Append("\"");
                        finalJSON.Append(comment);//
                        finalJSON.Append("\"");
                        }


                    finalJSON.AppendLine();
                    finalJSON.Append(@"                    }");


                    if (i != dsLines.Tables[0].Rows.Count - 1)
                        {
                        //finalJSON.Append(@"                    }");
                        finalJSON.Append(",");
                        }
                    else
                        {
                        finalJSON.AppendLine();
                        finalJSON.Append(@"                    }");
                        finalJSON.Append(",");
                        }

                    }
                AppendTotalLineCountForSingleOrder(orderNumber);
                AppendClosingQuantityDataForSingleOrder(orderNumber);
                }
            catch (Exception ex)
                {
                LogErrorInDB(lastPONumber, ex.Message, DateTime.Now, "POSTStaplesJSONOrders");
                }

            }

        public void AppendClosingQuantityDataForSingleOrder(string OrderNumber)
            {
            try
                {
                decimal totalLineQuantity;
                //         string sqlGetTotalLineQuantity = " select sum(real_qty) as TotalOrderQuantity from SPOTLESS_ORDER_VALID_DATA_VIEW  " +
                //" where ponumber = " + "'" + OrderNumber + "'";
                string selectTotalLineQuantity = ConfigurationManager.AppSettings["selectTotalLineQuantity"].ToString();
                string sqlGetTotalLineQuantity = selectTotalLineQuantity + "'" + OrderNumber + "'";
                cmd = new SqlCommand(sqlGetTotalLineQuantity, objConn);

                objConn.Open();
                totalLineQuantity = (Decimal)cmd.ExecuteScalar();
                objConn.Close();

                finalJSON.AppendLine();
                finalJSON.Append("                        \"");
                finalJSON.Append("totalQuantityCount");
                finalJSON.Append("\"");
                finalJSON.Append(":");
                finalJSON.Append(totalLineQuantity);


                //lines header close
                finalJSON.AppendLine();
                finalJSON.Append(@"                            }");

                //order header close
                finalJSON.AppendLine();
                finalJSON.Append(@"                        }");
                }
            catch (Exception ex)
                {
                LogErrorInDB(lastPONumber, ex.Message, DateTime.Now, "POSTStaplesJSONOrders");
                }

            }
        public void AppendTotalLineCountForSingleOrder(string OrderNumber)
            {
            try
                {
                int totalLineQuantity;
                string selectCountOftotalLine = ConfigurationManager.AppSettings["selectCountOftotalLine"].ToString();
                //         string sqlGetTotalLineQuantity = " select count(ponumber) as TotalLineCount from SPOTLESS_ORDER_VALID_DATA_VIEW  " +
                //" where ponumber = " + "'" + OrderNumber + "'";
                string sqlGetTotalLineQuantity = selectCountOftotalLine + "'" + OrderNumber + "'";
                cmd = new SqlCommand(sqlGetTotalLineQuantity, objConn);

                objConn.Open();
                totalLineQuantity = (Int32)cmd.ExecuteScalar();
                objConn.Close();

                finalJSON.AppendLine();
                finalJSON.Append("                        \"");
                finalJSON.Append("totalLineCount");
                finalJSON.Append("\"");
                finalJSON.Append(":");
                finalJSON.Append(totalLineQuantity);
                finalJSON.Append(",");
                finalJSON.AppendLine();
                //lines header close
                }
            catch (Exception ex)
                {
                LogErrorInDB(lastPONumber, ex.Message, DateTime.Now, "POSTStaplesJSONOrders");
                }


            }


        public string postJSONOrder(string JSONOrderNumber, string JSONPostString)
            {
            try
                {
                string restURL = string.Empty;
                string res = string.Empty;

                // restURL = "https://portal.ipaccess.com.au/PostOrder/" + JSONOrderNumber;
                string orderPOSTURL = ConfigurationManager.AppSettings["orderPOSTURL"].ToString();
                restURL = orderPOSTURL + JSONOrderNumber;
                res = GetResult(JSONPostString, restURL);
                return res;
                }
            catch (Exception ex)
                {
                LogErrorInDB(lastPONumber, ex.Message, DateTime.Now, "POSTStaplesJSONOrders");
                throw;


                }

            }

        public string GetResult(string postString, string restURL)
            {
            try
                {
                string result = null;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(restURL);
                //request.Method = "POST";
                request.Method = ConfigurationManager.AppSettings["JSONMethod"].ToString();
                //request.ContentType = "application/json";
                request.ContentType = ConfigurationManager.AppSettings["JSONContentType"].ToString();
                //  string postData = "home=Cosby&favorite+flavor=flies";
                //   byte[] bytes = Encoding.UTF8.GetBytes(txtPost.ToString());
                byte[] bytes = Encoding.UTF8.GetBytes(postString);
                request.ContentLength = bytes.Length;

                Stream requestStream = request.GetRequestStream();
                requestStream.Write(bytes, 0, bytes.Length);
                WebResponse response = request.GetResponse();
                Stream stream = response.GetResponseStream();
                StreamReader reader = new StreamReader(stream);
                result = reader.ReadToEnd();
                stream.Dispose();
                reader.Dispose();
                return result;
                }
            catch (Exception ex)
                {
                LogErrorInDB(lastPONumber, ex.Message, DateTime.Now, "POSTStaplesJSONOrders");
                throw;
                }

            }

        public void insert(string ponumber, DateTime pushdate, string filename, string traceid, string sitecode, string JSONMessage, int Status,  string CreatedDate)
            {

            try
                {
                objConn.Open();
                string insertResponseData = ConfigurationManager.AppSettings["insertResponseData"].ToString();
                using (SqlCommand command = new SqlCommand(insertResponseData, objConn))
                    {
                    command.Parameters.Add(new SqlParameter("ponumber", ponumber));
                    command.Parameters.Add(new SqlParameter("pushdate", pushdate));
                    command.Parameters.Add(new SqlParameter("filename", filename));
                    command.Parameters.Add(new SqlParameter("traceid", traceid));
                    command.Parameters.Add(new SqlParameter("sitecode", sitecode));
                    command.Parameters.Add(new SqlParameter("@jsonmessage", JSONMessage));
                    command.Parameters.Add(new SqlParameter("@status", Status));
                    
                    command.Parameters.Add(new SqlParameter("@createddate", CreatedDate));
                    command.ExecuteNonQuery();
                    }
                objConn.Close();
                }
            catch (Exception ex)
                {
                LogErrorInDB(lastPONumber, ex.Message, DateTime.Now, "POSTStaplesJSONOrders");
                }
            }

        public string GetToken()
            {
            try
                {
                string acrJSONTokenString = string.Empty;
                acrJSONTokenString = ACRTokenString();
                string restURL = string.Empty;
                string acrToken = string.Empty;
                // restURL = " https://portal.ipaccess.com.au/GetPortalToken/dist_spotless";
                restURL = ConfigurationManager.AppSettings["restURL"].ToString();

                acrToken = GetTokenFromACR(acrJSONTokenString, restURL);
                return acrToken;
                }
            catch (Exception ex)
                {
                LogErrorInDB(lastPONumber, ex.Message, DateTime.Now, "POSTStaplesJSONOrders");
                throw;
                }
            }
        public string GetTokenFromACR(string acrJSONTokenString, string restURL)
            {
            try
                {


                string result = null;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(restURL);
                // request.Method = "POST";
                request.Method = ConfigurationManager.AppSettings["JSONMethod"].ToString();

                // request.ContentType = "application/json";
                request.ContentType = ConfigurationManager.AppSettings["JSONContentType"].ToString();

                //  string postData = "home=Cosby&favorite+flavor=flies";
                //   byte[] bytes = Encoding.UTF8.GetBytes(txtPost.ToString());
                byte[] bytes = Encoding.UTF8.GetBytes(acrJSONTokenString);
                request.ContentLength = bytes.Length;

                Stream requestStream = request.GetRequestStream();
                requestStream.Write(bytes, 0, bytes.Length);
                WebResponse response = request.GetResponse();
                Stream stream = response.GetResponseStream();
                StreamReader reader = new StreamReader(stream);
                result = reader.ReadToEnd();
                stream.Dispose();
                reader.Dispose();
                return result;
                }
            catch (Exception ex)
                {
                LogErrorInDB(lastPONumber, ex.Message, DateTime.Now, "POSTStaplesJSONOrders");
                throw;
                }
            }

        public string ACRTokenString()
            {
            try
                {
                tokenJSON.Append(@"{");
                tokenJSON.AppendLine();
                //Appending ClientID
                tokenJSON.Append("\"");
                tokenJSON.Append(@"clientId");
                tokenJSON.Append("\"");
                tokenJSON.Append(":");
                tokenJSON.Append("\"");
                tokenJSON.Append(clientId);//ACRTest
                tokenJSON.Append("\"");//ACRTest
                tokenJSON.Append(@", ");

                //Appending ClientKey
                tokenJSON.AppendLine();
                tokenJSON.Append("\"");
                tokenJSON.Append(@"clientKey");
                tokenJSON.Append("\"");
                tokenJSON.Append(":");
                tokenJSON.Append("\"");
                tokenJSON.Append(clientKey);
                tokenJSON.Append("\"");
                tokenJSON.Append(@", ");

                tokenJSON.AppendLine();
                tokenJSON.Append("\"");
                tokenJSON.Append(@"parameters");
                tokenJSON.Append("\"");
                tokenJSON.Append(":");
                tokenJSON.Append("[");
                tokenJSON.Append("]");
                tokenJSON.AppendLine();
                tokenJSON.Append(@"}");

                return tokenJSON.ToString();
                }
            catch (Exception ex)
                {
                LogErrorInDB(lastPONumber, ex.Message, DateTime.Now, "POSTStaplesJSONOrders");
                throw;
                }
            }

        public void LogErrorInDB(string _orderNumber, string errorMSG, DateTime _ErrorOccuredDate, string _moduleName)
            {


            //string query = "INSERT INTO [dbo].[tblOrderErrors] (OrderNumber,    ErrorMessage,       ErrorOccuredDate,       ModuleName ) " +
            //                   " VALUES (" +
            //                                                  " @OrderNumber,    @ErrorMessage,     @ErrorOccuredDate,  @ModuleName  ) ";
            string insertErrorData = ConfigurationManager.AppSettings["insertErrorData"].ToString();
            string query = insertErrorData;

            // create connection and command
            using (SqlConnection cn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, cn))
                {
                // define parameters and their values
                cmd.Parameters.AddWithValue("@OrderNumber", _orderNumber);
                cmd.Parameters.AddWithValue("@ErrorMessage", errorMSG);
                cmd.Parameters.AddWithValue("@ErrorOccuredDate", _ErrorOccuredDate);
                cmd.Parameters.AddWithValue("@ModuleName", _moduleName);


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

        }
    }
