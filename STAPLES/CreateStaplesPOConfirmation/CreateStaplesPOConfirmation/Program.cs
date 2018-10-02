using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;

namespace CreateStaplesPOConfirmation
    {
    class Program
        {

        XmlWriter xmlWriter;
        string connectionString = ConfigurationManager.ConnectionStrings["sqlConn"].ToString();
        string POCCreationPath = ConfigurationManager.AppSettings["POCCreationPath"].ToString();
        static void Main(string[] args)
            {
            Program thisProgram = new Program();

            //    thisProgram.PostPOC("4501795773");// testing post
                //thisProgram.MoveFiles("4501680748");

                //thisProgram.UpdatePOCFlag("4501680748");

            string orderNum = null;

            try
                {

                thisProgram.xmlWriter = null;
                SqlConnection con = new SqlConnection(thisProgram.connectionString);
                string selectOrders = ConfigurationManager.AppSettings["selectPOCOrders"].ToString();
                string strOrders = selectOrders;
                SqlDataAdapter dtOrders = new SqlDataAdapter(strOrders, con);
                DataSet dsOrders = new DataSet();
                dtOrders.Fill(dsOrders);

                if (dsOrders.Tables[0].Rows.Count == 0)
                    {
                    throw new Exception("NO Data in View_POC_Header, Please check the data and run the exe again");
                    }


                for (int i = 0; i < dsOrders.Tables[0].Rows.Count; i++)
                    {
                    orderNum = dsOrders.Tables[0].Rows[i]["OrderNumber"].ToString();                


                    #region Payload


                    string OrderNumber = null;
                    string PayloadID = null;
                    string POCPayloadID = null; // This payloadID is for the POC Document
                    string DocRefPayloadID = null; // This payloadID is for the Document reference Payload ID which should match with PO PayloadID
                    string OrderTimeStamp = null;

                   
                    string selectPOCData = ConfigurationManager.AppSettings["selectPOCData"].ToString();
                    string strPOCload = selectPOCData + "'" + orderNum + "'";
                    SqlDataAdapter dtPOCload = new SqlDataAdapter(strPOCload, con);
                    DataSet dsPOCload = new DataSet();
                    dtPOCload.Fill(dsPOCload);
                    if (dsPOCload.Tables[0].Rows.Count == 0)
                        {
                        throw new Exception("NO Data in View_POC_Header, Please check the data and run the exe again");
                        }


                    OrderNumber = orderNum;
                    PayloadID = dsPOCload.Tables[0].Rows[0]["PayloadID"].ToString();
                    //POCPayloadID = PayloadID + "3";
                    POCPayloadID = thisProgram.GeneratePaylodID(32);
                    OrderTimeStamp = dsPOCload.Tables[0].Rows[0]["noticeDate"].ToString();
                    DateTime timeStampDate = Convert.ToDateTime(dsPOCload.Tables[0].Rows[0]["noticeDate"].ToString());
                    string convTimeStampDate = String.Format("{0:s}", timeStampDate);                   
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Indent = true;
                    settings.OmitXmlDeclaration = false;



                    thisProgram.xmlWriter = XmlWriter.Create(thisProgram.POCCreationPath + OrderNumber + "_POC" + ".xml", settings);

                    thisProgram.xmlWriter.WriteStartDocument();

                    thisProgram.xmlWriter.WriteDocType("cXML", null, "http://xml.cxml.org/schemas/cXML/1.2.021/Fulfill.dtd", null);
                    thisProgram.xmlWriter.WriteStartElement("cXML");

                    thisProgram.xmlWriter.WriteAttributeString("payloadID", POCPayloadID);
                    thisProgram.xmlWriter.WriteAttributeString("timestamp", convTimeStampDate + "+11:00");
                    thisProgram.xmlWriter.WriteAttributeString("version", "1.2.024");
                    thisProgram.xmlWriter.WriteAttributeString("xml", "lang", null, "en-US");

                    #endregion Payload


                    #region Header                  

                    
                    string FromNetworkID = null;
                    string ToNetworkID = null;
                    string SenderNetworkID = null;                    
                    string SenderSharedSecret = null;
                    string SenderUserAgent = null;

                    OrderNumber = dsPOCload.Tables[0].Rows[0]["OrderNumber"].ToString();
                    FromNetworkID = dsPOCload.Tables[0].Rows[0]["From_NetworkID"].ToString();
                    ToNetworkID = dsPOCload.Tables[0].Rows[0]["To_NetworkID"].ToString();
                    SenderNetworkID = dsPOCload.Tables[0].Rows[0]["Sender_NetworkID"].ToString();
                    //SenderAribaNetworkUserID = dsPOCload.Tables[0].Rows[0]["Sender_AribaNetworkUserID"].ToString();
                    SenderSharedSecret = dsPOCload.Tables[0].Rows[0]["Sender_SharedSecret"].ToString();
                    SenderUserAgent = dsPOCload.Tables[0].Rows[0]["Sender_UserAgent"].ToString();



                    thisProgram.xmlWriter.WriteStartElement("Header");//Header Tag starting


                    thisProgram.xmlWriter.WriteStartElement("From");//Starting From tag
                    thisProgram.xmlWriter.WriteStartElement("Credential");//Starting Credential tag
                    thisProgram.xmlWriter.WriteAttributeString("domain", "NetworkID");
                    thisProgram.xmlWriter.WriteStartElement("Identity");//Starting Identity tag under Credential tag
                    thisProgram.xmlWriter.WriteString(FromNetworkID);//Hardcoding as of now 
                    thisProgram.xmlWriter.WriteEndElement();//Closing Identity tag Under Credential tag
                    thisProgram.xmlWriter.WriteEndElement();//Closing Credential tag under From Tag
                    thisProgram.xmlWriter.WriteEndElement();// Closing From Tag

                    thisProgram.xmlWriter.WriteStartElement("To");//Starting To tag
                    thisProgram.xmlWriter.WriteStartElement("Credential");//Starting Credential tag
                    thisProgram.xmlWriter.WriteAttributeString("domain", "NetworkID");
                    thisProgram.xmlWriter.WriteStartElement("Identity");//Starting Identity tag under Credential tag
                    thisProgram.xmlWriter.WriteString(ToNetworkID);
                    thisProgram.xmlWriter.WriteEndElement();//Closing Identity tag Under Credential tag
                    thisProgram.xmlWriter.WriteEndElement();//Closing Credential tag under To Tag
                    thisProgram.xmlWriter.WriteEndElement();// Closing To Tag

                    thisProgram.xmlWriter.WriteStartElement("Sender");//Starting Sender tag
                    thisProgram.xmlWriter.WriteStartElement("Credential");//Starting Credential tag
                    thisProgram.xmlWriter.WriteAttributeString("domain", "NetworkID");
                    thisProgram.xmlWriter.WriteStartElement("Identity");//Starting Identity tag under Credential tag
                    thisProgram.xmlWriter.WriteString(SenderNetworkID);
                    thisProgram.xmlWriter.WriteEndElement();//Closing Identity tag Under Credential tag
                    thisProgram.xmlWriter.WriteStartElement("SharedSecret");//Starting SharedSecret tag under Credential tag
                    thisProgram.xmlWriter.WriteString(SenderSharedSecret);
                    thisProgram.xmlWriter.WriteEndElement();//Closing SharedSecret tag Under Credential tag
                    thisProgram.xmlWriter.WriteEndElement();//Closing Credential tag under Sender Tag
                    thisProgram.xmlWriter.WriteStartElement("UserAgent");//Starting UserAgent tag
                    thisProgram.xmlWriter.WriteString(SenderUserAgent);
                    thisProgram.xmlWriter.WriteEndElement();//Closing UserAgent tag under Sender Tag
                    thisProgram.xmlWriter.WriteEndElement();// Closing Sender Tag

                    thisProgram.xmlWriter.WriteEndElement();//Closing Header Tag

                    #endregion Header


                    #region Request


                    string selectPOCSummaryData = ConfigurationManager.AppSettings["selectPOCSummaryData"].ToString();
                    string strPOCSummaryData = selectPOCSummaryData + "'" + orderNum + "'";
                    SqlDataAdapter dtPOCSummaryData = new SqlDataAdapter(strPOCSummaryData, con);
                    DataSet dsPOCSummaryData = new DataSet();
                    dtPOCSummaryData.Fill(dsPOCSummaryData);
                    if (dsPOCSummaryData.Tables[0].Rows.Count == 0)
                        {
                        throw new Exception("NO Data in View_POC_Summary_Data, Please check the data and run the exe again");
                        }

                    //DateTime timeStampDate = Convert.ToDateTime(dsRequest.Tables[0].Rows[0]["RequestedDeliveryDate"].ToString());
                    DateTime timeStampDate1 = DateTime.Now;
                    string convTimeStampDate1 = String.Format("{0:s}", timeStampDate);
                    convTimeStampDate1 = convTimeStampDate1 + "+11:00";

                    string OrderTotal = null;
                    string Tax = null;
                    string OrderType = null;

                    OrderTotal = dsPOCSummaryData.Tables[0].Rows[0]["INVOICE_NET_TOTAL"].ToString();                    

                    Tax = dsPOCSummaryData.Tables[0].Rows[0]["INVOICE_GST_TOTAL"].ToString();

                    OrderType = dsPOCSummaryData.Tables[0].Rows[0]["Order_type"].ToString();

                    thisProgram.xmlWriter.WriteStartElement("Request");
                    thisProgram.xmlWriter.WriteAttributeString("deploymentMode", "production");//Request Tag starting

                    thisProgram.xmlWriter.WriteStartElement("ConfirmationRequest");//Starting ConfirmationRequest tag

                    thisProgram.xmlWriter.WriteStartElement("ConfirmationHeader");
                    thisProgram.xmlWriter.WriteAttributeString("noticeDate", convTimeStampDate1);
                    thisProgram.xmlWriter.WriteAttributeString("type", OrderType);
                    thisProgram.xmlWriter.WriteAttributeString("operation", "new");
                                      
                    thisProgram.xmlWriter.WriteAttributeString("confirmID", dsPOCload.Tables[0].Rows[0]["confirmID"].ToString());

                    thisProgram.xmlWriter.WriteStartElement("Total"); // starting total tag
                    thisProgram.xmlWriter.WriteStartElement("Money");  //starting money tag          
                    thisProgram.xmlWriter.WriteAttributeString("currency", "AUD");
                    thisProgram.xmlWriter.WriteString(OrderTotal);
                    thisProgram.xmlWriter.WriteEndElement();//closing money tag
                    thisProgram.xmlWriter.WriteEndElement(); // Closing Total tag

                    thisProgram.xmlWriter.WriteStartElement("Shipping");

                    thisProgram.xmlWriter.WriteStartElement("Money");  //starting money tag          
                    thisProgram.xmlWriter.WriteAttributeString("currency", "AUD");
                    //thisProgram.xmlWriter.WriteString("10.00");
                    thisProgram.xmlWriter.WriteEndElement();//closing money tag

                    thisProgram.xmlWriter.WriteStartElement("Description");// starting description Tag
                    thisProgram.xmlWriter.WriteAttributeString("xml", "lang", null, "en-AU");
                    //thisProgram.xmlWriter.WriteString("FedEx 2-day");
                    thisProgram.xmlWriter.WriteEndElement();// closing Description Tag

                    thisProgram.xmlWriter.WriteEndElement();// closing Shipping Tag

                    thisProgram.xmlWriter.WriteStartElement("Tax");

                    

                    thisProgram.xmlWriter.WriteStartElement("Money");  //starting money tag          
                    thisProgram.xmlWriter.WriteAttributeString("currency", "AUD");
                    thisProgram.xmlWriter.WriteString(Tax);
                    thisProgram.xmlWriter.WriteEndElement();//closing money tag

                    thisProgram.xmlWriter.WriteStartElement("Description");// starting description Tag
                    thisProgram.xmlWriter.WriteAttributeString("xml", "lang", null, "en-AU");
                    thisProgram.xmlWriter.WriteString("GST");
                    thisProgram.xmlWriter.WriteEndElement();// closing Description Tag

                    thisProgram.xmlWriter.WriteEndElement();// closing Shipping Tag


                    thisProgram.xmlWriter.WriteStartElement("Comments");// starting comments Tag
                    thisProgram.xmlWriter.WriteAttributeString("xml", "lang", null, "en-AU");
                    thisProgram.xmlWriter.WriteString("Confirmation of Order # " + OrderNumber);
                    thisProgram.xmlWriter.WriteEndElement();// closing comments Tag

                    thisProgram.xmlWriter.WriteEndElement();// closing confirmation header tag

                    thisProgram.xmlWriter.WriteStartElement("OrderReference");//Start Order Reference Tag
                                                                  //  thisProgram.xmlWriter.WriteAttributeString("payloadID", "2017-05-18T12:00:00+10:00");
                    thisProgram.xmlWriter.WriteAttributeString("orderID", OrderNumber);


                    DocRefPayloadID = PayloadID;
                    thisProgram.xmlWriter.WriteStartElement("DocumentReference");//Start Document Reference Tag
                    thisProgram.xmlWriter.WriteAttributeString("payloadID", DocRefPayloadID);
                    thisProgram.xmlWriter.WriteEndElement();// Closing Document Reference tag

                    thisProgram.xmlWriter.WriteEndElement();// Closing order Reference tag

                    string selectPOCItemData = ConfigurationManager.AppSettings["selectPOCItemData"].ToString();
                    string strPOCItemData = selectPOCItemData + "'" + orderNum + "'";
                    SqlDataAdapter dtPOCItemData = new SqlDataAdapter(strPOCItemData, con);
                    DataSet dsPOCItemData = new DataSet();
                    dtPOCItemData.Fill(dsPOCItemData);
                    if (dsPOCItemData.Tables[0].Rows.Count == 0)
                        {
                        throw new Exception("NO Data in View_POC_Item_Data, Please check the data and run the exe again");
                        }

                    

                    for (int j = 0; j < dsPOCItemData.Tables[0].Rows.Count; j++)
                        {
                        string LineNumber = null;
                        string OrderQuantity = null;
                        string InvoiceQuantity = null;
                        string ORD_UnitOfMeasure = null;
                        string INV_UnitOfMeasure = null;
                        string ORD_Line_Comments = null;
                        string OrderLineType = null;
                        string reqDevDate = null;
                        string convReqDevDate = "";
                        string orderPrice = null;
                        string invoicePrice = null;
                        int priceChangeFlag = 0;


                        LineNumber = dsPOCItemData.Tables[0].Rows[j]["ord_LineNumber"].ToString();
                        OrderQuantity = dsPOCItemData.Tables[0].Rows[j]["ord_Quantity"].ToString();
                        InvoiceQuantity = dsPOCItemData.Tables[0].Rows[j]["Invoice_Quantity"].ToString();
                        ORD_UnitOfMeasure = dsPOCItemData.Tables[0].Rows[j]["ord_UnitOfMeasure"].ToString();
                        INV_UnitOfMeasure = dsPOCItemData.Tables[0].Rows[j]["inv_UnitOfMeasure"].ToString();
                        ORD_Line_Comments = dsPOCItemData.Tables[0].Rows[j]["Exception_Reason"].ToString();
                        OrderLineType = dsPOCItemData.Tables[0].Rows[j]["Item_Type"].ToString();
                        orderPrice = dsPOCItemData.Tables[0].Rows[j]["ord_UnitPrice"].ToString();
                        invoicePrice = dsPOCItemData.Tables[0].Rows[j]["inv_Item_Proce"].ToString();
                        priceChangeFlag = int.Parse(dsPOCItemData.Tables[0].Rows[j]["poc_line_Flag"].ToString());

                        reqDevDate = dsPOCItemData.Tables[0].Rows[j]["Inv_DeliveryDate"].ToString();
                        if (reqDevDate != "")
                            {
                            convReqDevDate = String.Format("{0:s}", Convert.ToDateTime(reqDevDate));
                            convReqDevDate = convReqDevDate + "+11:00";
                            }

                        thisProgram.xmlWriter.WriteStartElement("ConfirmationItem");//Start Confirmation item Tag
                        thisProgram.xmlWriter.WriteAttributeString("lineNumber", LineNumber);
                        thisProgram.xmlWriter.WriteAttributeString("quantity", OrderQuantity);


                        thisProgram.xmlWriter.WriteStartElement("UnitOfMeasure");//Start UOM  Tag
                        thisProgram.xmlWriter.WriteString(ORD_UnitOfMeasure);
                        thisProgram.xmlWriter.WriteEndElement();//Closing UOM  Tag

                        thisProgram.xmlWriter.WriteStartElement("ConfirmationStatus");//Start Confirmation Status  Tag
                        if (reqDevDate != "")
                            {
                            thisProgram.xmlWriter.WriteAttributeString("deliveryDate", convReqDevDate);
                            }
                        thisProgram.xmlWriter.WriteAttributeString("type", OrderLineType);
                        thisProgram.xmlWriter.WriteAttributeString("quantity", InvoiceQuantity);


                        thisProgram.xmlWriter.WriteStartElement("UnitOfMeasure");//Start UOM  Tag
                        thisProgram.xmlWriter.WriteString(INV_UnitOfMeasure);
                        thisProgram.xmlWriter.WriteEndElement();//Closing UOM  Tag

                        //implement condition for Unit price change 

                        
                        if (priceChangeFlag == 3 && invoicePrice != "0.00")
                            {
                            thisProgram.xmlWriter.WriteStartElement("UnitPrice"); // Start Unit Price Tag

                            thisProgram.xmlWriter.WriteStartElement("Money");//Start Money Tag
                            thisProgram.xmlWriter.WriteAttributeString("currency", "AUD");
                            thisProgram.xmlWriter.WriteString(invoicePrice);
                            thisProgram.xmlWriter.WriteEndElement();// closing Money Tag

                            //thisProgram.xmlWriter.WriteStartElement("Modifications");//Start Modifications Tag
                            //thisProgram.xmlWriter.WriteStartElement("Modification");//Start Modification Tag
                            //thisProgram.xmlWriter.WriteStartElement("OriginalPrice");//Start Original Price tag
                            //thisProgram.xmlWriter.WriteStartElement("Money");//Start Money Tag
                            //thisProgram.xmlWriter.WriteAttributeString("currency", "AUD");
                            //thisProgram.xmlWriter.WriteString(orderPrice);
                            //thisProgram.xmlWriter.WriteEndElement();// closing Money Tag
                            //thisProgram.xmlWriter.WriteEndElement();//closing original price tag
                            //thisProgram.xmlWriter.WriteEndElement();//Closing Modification Tag
                            //thisProgram.xmlWriter.WriteEndElement();//Closing Modifications Tag

                            thisProgram.xmlWriter.WriteEndElement();// closing Unit price tag
                            }

                       

                        thisProgram.xmlWriter.WriteStartElement("Comments");//Start Comments  Tag
                        thisProgram.xmlWriter.WriteString(ORD_Line_Comments);
                        thisProgram.xmlWriter.WriteEndElement();//Closing Comments  Tag

                        thisProgram.xmlWriter.WriteEndElement();//Closing Confirmation Status  Tag
                        thisProgram.xmlWriter.WriteEndElement();//Closing Confirmation item Tag

                        }

                    thisProgram.xmlWriter.WriteEndElement();// Closing ConfirmationRequest Tag
                    thisProgram.xmlWriter.WriteEndElement();//Closing Request Tag



                    thisProgram.xmlWriter.WriteEndDocument();


                    #endregion Request


                    thisProgram.xmlWriter.Flush();
                    thisProgram.xmlWriter.Close();
                    thisProgram.xmlWriter.Dispose();

                    thisProgram.PostPOC(orderNum);

                    thisProgram.MoveFiles(orderNum);

                    thisProgram.UpdatePOCFlag(orderNum);


                    }


                
             


                }
            catch(Exception ex)
                {
                ExceptionLogging.SendExcepToDB(ex, "CreateStaplesPOCConfirmation - Static Main");
                }
          

            }

        private void PostPOC(string _orderFile)
            {
            WebRequest req = null;
            WebResponse rsp = null;
            string fileName = string.Empty;
          //  string fileName = "C:\\Staples\\cXMLFiles\\New Test PO\\POC\\4501679673_POC.xml";
            fileName = POCCreationPath + _orderFile + "_POC.xml";
            string uri = ConfigurationManager.AppSettings["postURL"].ToString();
            string responseString = string.Empty;



            try
                {
                if ((!string.IsNullOrEmpty(uri)) && (!string.IsNullOrEmpty(fileName)))
                    {
                    req = WebRequest.Create(uri);
                    //req.Proxy = WebProxy.GetDefaultProxy(); // Enable if using proxy
                    req.Method = "POST";        // Post method
                    req.ContentType = "text/xml";     // content type
                    // Wrap the request stream with a text-based writer                  
                    StreamWriter writer = new StreamWriter(req.GetRequestStream());
                    // Write the XML text into the stream
                    StreamReader reader = new StreamReader(fileName);
                    string ret = reader.ReadToEnd();
                    reader.Close();
                    writer.WriteLine(ret);
                    writer.Close();
                    // Send the data to the webserver
                    rsp = req.GetResponse();
                    HttpWebResponse hwrsp = (HttpWebResponse)rsp;
                    Stream streamResponse = hwrsp.GetResponseStream();
                    StreamReader streamRead = new StreamReader(streamResponse);
                    responseString = streamRead.ReadToEnd();
                    SavePOCResponse(responseString, _orderFile);


                    rsp.Close();
                    }
                }
            catch (WebException webEx)
                {
                ExceptionLogging.SendExcepToDB(webEx, "CreateStaplesPOCConfirmation - PostPOC");
                }
            catch (Exception ex)
                {
                ExceptionLogging.SendExcepToDB(ex, "CreateStaplesPOCConfirmation - PostPOC");
                }
            finally
                {
                if (req != null) req.GetRequestStream().Close();
                if (rsp != null) rsp.GetResponseStream().Close();
                }
           
            }


        public void MoveFiles(string _orderFileName)
            {

            try
                {

                string sourcePath = string.Empty;
                string destinationPath = string.Empty;
                string orderFile = string.Empty;

                orderFile = _orderFileName + "_POC.xml";

                string sourcePathFromAppSettings = ConfigurationManager.AppSettings["sourcePathFromAppSettings"].ToString();
                sourcePath = sourcePathFromAppSettings ;

                //destinationPath = @"\\teapot\D$\ORDERS_IN\SPOTLESS\Invoices\Processed\";

                string destPathFromAppSettings = ConfigurationManager.AppSettings["destPathFromAppSettings"].ToString();
                destinationPath = destPathFromAppSettings;                

                File.Copy(Path.Combine(sourcePath, orderFile), Path.Combine(destinationPath, orderFile), true);
                sourcePath = sourcePath + "\\" + orderFile;
                File.Delete(sourcePath);
                }
            catch (Exception ex)
                {

                ExceptionLogging.SendExcepToDB(ex, "CreateStaplesPOCConfirmation - MoveFiles");
                }


            }

        private void SavePOCResponse(string _response, string _orderNumber)
            {

            try
                {
                XmlDocument pocResponseDOC = new XmlDocument();
                pocResponseDOC.LoadXml(_response);

                string timeStamp = string.Empty;
                string payloadID = string.Empty;
                int statusCode = 0;
                string statusText = string.Empty;
                string statusMessage = string.Empty;
                string createdDate = null;

                if (pocResponseDOC.SelectSingleNode("//cXML") != null)
                    {

                    timeStamp = (pocResponseDOC.SelectSingleNode("//cXML").Attributes["timestamp"].Value == null) ? null : pocResponseDOC.SelectSingleNode("//cXML").Attributes["timestamp"].Value;

                    }
                else
                    {


                    }

                if (pocResponseDOC.SelectSingleNode("//cXML") != null)
                    {

                    payloadID = (pocResponseDOC.SelectSingleNode("//cXML").Attributes["payloadID"].Value == null) ? null : pocResponseDOC.SelectSingleNode("//cXML").Attributes["payloadID"].Value;

                    }
                else
                    {


                    }

                if (pocResponseDOC.SelectSingleNode("//cXML/Response/Status") != null)
                    {


                    statusCode = int.Parse((pocResponseDOC.SelectSingleNode("//cXML/Response/Status").Attributes["code"].Value == null) ? null : pocResponseDOC.SelectSingleNode("//cXML/Response/Status").Attributes["code"].Value);

                    }
                else
                    {


                    }

                if (pocResponseDOC.SelectSingleNode("//cXML/Response/Status") != null)
                    {


                    statusText = (pocResponseDOC.SelectSingleNode("//cXML/Response/Status").Attributes["text"].Value == null) ? null : pocResponseDOC.SelectSingleNode("//cXML/Response/Status").Attributes["text"].Value;

                    }
                else
                    {


                    }

                if (pocResponseDOC.SelectSingleNode("//cXML/Response/Status") != null)
                    {


                    statusMessage = pocResponseDOC.SelectSingleNode("//cXML/Response/Status").InnerText == "" ? null : pocResponseDOC.SelectSingleNode("//cXML/Response/Status").InnerText;

                    }
                else
                    {


                    }

                createdDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

                InsertPOCResponseDetails(_orderNumber, timeStamp, payloadID, statusCode, statusText, statusMessage, createdDate);
                }
            catch (Exception ex)
                {

                ExceptionLogging.SendExcepToDB(ex, "CreateStaplesPOCConfirmation - SavePOCResponse");
                }
         
            
            }

        private void InsertPOCResponseDetails(string _orderNumber, string _timeStamp, string _payloadID, int _statusCode, string _statusText, string _statusMessage, string _CreatredDate)
            {

            try
                {
                string insertResponseMSG = ConfigurationManager.AppSettings["insertResponseMSG"].ToString();

                string query = insertResponseMSG;
                // create connection and command
                using (SqlConnection cn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(query, cn))
                    {
                    // define parameters and their values
                    cmd.Parameters.AddWithValue("@orderNumber", _orderNumber);
                    cmd.Parameters.AddWithValue("@timeStamp", _timeStamp);
                    cmd.Parameters.AddWithValue("@payloadID", _payloadID);
                    cmd.Parameters.AddWithValue("@statusCode", _statusCode);
                    cmd.Parameters.AddWithValue("@statusText", _statusText);
                    cmd.Parameters.AddWithValue("@statusMessage", _statusMessage);
                    cmd.Parameters.AddWithValue("@CreatedDate", _CreatredDate);


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

                ExceptionLogging.SendExcepToDB(ex, "CreateStaplesPOCConfirmation - InsertPOCResponseDetails");
                }

            }

        private string GeneratePaylodID(int length)
            {
            string DocumentHeaderPayloadId = null;
            Random random = new Random();
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            DocumentHeaderPayloadId = new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
            return DocumentHeaderPayloadId;
            }

        public void UpdatePOCFlag(string _orderNumber)
            {
            try
                {
                SqlConnection conn = new SqlConnection(connectionString);
                string query = ConfigurationManager.AppSettings["updatePOCFlag"].ToString();
                string updateQuery = query + "'" + _orderNumber + "'";
                SqlCommand cmd = new SqlCommand(updateQuery, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();

                }
            catch (Exception ex)
                {
                ExceptionLogging.SendExcepToDB(ex, "CreateStaplesPOC - UpdatePOCFlag");
                }



            }


        }
    }
