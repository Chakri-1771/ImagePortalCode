using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;

namespace CreateStaplesASNFiles
    {
    

    class Program
        {

        XmlWriter xmlWriter = null;
        string connectionString = ConfigurationManager.ConnectionStrings["sqlConn"].ToString();
        string ASNCreationPath = ConfigurationManager.AppSettings["ASNCreationPath"].ToString();
        static void Main(string[] args)
            {

            

            Program thisProgram = new Program();
            int CatalogueSeqID = 0;
            string orderNum = null;
      

            try
                {


                thisProgram.xmlWriter = null;
                SqlConnection con = new SqlConnection(thisProgram.connectionString);
                string selectOrders = ConfigurationManager.AppSettings["selectOrders"].ToString();
                string strOrders = selectOrders;
                SqlDataAdapter dtOrders = new SqlDataAdapter(strOrders, con);
                DataSet dsOrders = new DataSet();
                dtOrders.Fill(dsOrders);
                if (dsOrders.Tables[0].Rows.Count == 0)
                    {
                    throw new Exception("NO Data in View_ASN_Header, Please check the data and run the exe again");
                    }

                for (int i = 0; i < dsOrders.Tables[0].Rows.Count; i++)
                    {
                    orderNum = dsOrders.Tables[0].Rows[i]["OrderNumber"].ToString();

                    #region Payload


                    string OrderNumber = null;
                    string PayloadID = null;
                    string ASNPayloadID = null; // This payloadID is for the POC Document
                    string DocRefPayloadID = null; // This payloadID is for the Document reference Payload ID which should match with PO PayloadID
                    string asnTimeStamp = null;
                    


                    string selectPayloadData = ConfigurationManager.AppSettings["selectHeaderData"].ToString();
                    string strPayload = selectPayloadData  + "'" + orderNum + "'";
                    SqlDataAdapter dtPayload = new SqlDataAdapter(strPayload, con);
                    DataSet dsPayload = new DataSet();
                    dtPayload.Fill(dsPayload);
                    if (dsPayload.Tables[0].Rows.Count == 0)
                        {
                        throw new Exception("NO Data in View_ASN_Header for order #" + orderNum + ", Please check the data and run the exe again");
                        }

                    DateTime ordDate = Convert.ToDateTime(dsPayload.Tables[0].Rows[0]["noticeDate"].ToString());
                    string convOrdDate = String.Format("{0:s}", ordDate);
                    convOrdDate = convOrdDate + "+11:00";


                    DateTime asnCreatedDate = DateTime.Now; 
                    string convASNCreatedDate = String.Format("{0:s}", asnCreatedDate);
                    convASNCreatedDate = convASNCreatedDate + "+11:00";


                    OrderNumber = orderNum;
                    PayloadID = dsPayload.Tables[0].Rows[0]["PayloadID"].ToString();
                    ASNPayloadID = thisProgram.GeneratePaylodID(32);
                    asnTimeStamp = dsPayload.Tables[0].Rows[0]["noticedate"].ToString();
                    CatalogueSeqID = Convert.ToInt32(dsPayload.Tables[0].Rows[0]["Catalogue_SeqID"]);

                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Indent = true;
                    settings.OmitXmlDeclaration = false;



                    thisProgram.xmlWriter = XmlWriter.Create(thisProgram.ASNCreationPath + OrderNumber + "_ASN" + ".xml", settings);

                    thisProgram.xmlWriter.WriteStartDocument();

                    thisProgram.xmlWriter.WriteDocType("cXML", null, "http://xml.cxml.org/schemas/cXML/1.2.026/Fulfill.dtd", null);
                    thisProgram.xmlWriter.WriteStartElement("cXML");
                  
                    thisProgram.xmlWriter.WriteAttributeString("payloadID", ASNPayloadID);
                    DateTime timeStampDate = Convert.ToDateTime(dsPayload.Tables[0].Rows[0]["noticeDate"].ToString());
                    string convTimeStampDate = String.Format("{0:s}", timeStampDate);
                    thisProgram.xmlWriter.WriteAttributeString("timestamp", convTimeStampDate + "11:00");
                    // thisProgram.xmlWriter.WriteAttributeString("timestamp", convASNCreatedDate);
                    //thisProgram.xmlWriter.WriteAttributeString("version", "1.2.024");
                    thisProgram.xmlWriter.WriteAttributeString("xml", "lang", null, "en-US");

                    #endregion Payload


                    #region Header

                 
                    string FromNetworkID = null;
                    string ToNetworkID = null;
                    string SenderAribaNetworkUserID = null;
                    string SenderSharedSecret = null;
                    string UserAgent = null;

                    OrderNumber = dsPayload.Tables[0].Rows[0]["OrderNumber"].ToString();
                    FromNetworkID = dsPayload.Tables[0].Rows[0]["From_NetworkID"].ToString();
                    ToNetworkID = dsPayload.Tables[0].Rows[0]["To_NetworkID"].ToString();
                    SenderAribaNetworkUserID = dsPayload.Tables[0].Rows[0]["Sender_NetworkID"].ToString();
                    SenderSharedSecret = dsPayload.Tables[0].Rows[0]["Sender_SharedSecret"].ToString();
                    UserAgent = dsPayload.Tables[0].Rows[0]["Sender_UserAgent"].ToString();


                    thisProgram.xmlWriter.WriteStartElement("Header");//Header Tag starting


                    thisProgram.xmlWriter.WriteStartElement("From");//Starting From tag
                    thisProgram.xmlWriter.WriteStartElement("Credential");//Starting Credential tag
                    thisProgram.xmlWriter.WriteAttributeString("domain", "NetworkID");
                    thisProgram.xmlWriter.WriteStartElement("Identity");//Starting Identity tag under Credential tag
                    thisProgram.xmlWriter.WriteString(FromNetworkID);
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
                    thisProgram.xmlWriter.WriteString(SenderAribaNetworkUserID);
                    thisProgram.xmlWriter.WriteEndElement();//Closing Identity tag Under Credential tag
                    thisProgram.xmlWriter.WriteStartElement("SharedSecret");//Starting SharedSecret tag under Credential tag
                    thisProgram.xmlWriter.WriteString(SenderSharedSecret);
                    thisProgram.xmlWriter.WriteEndElement();//Closing SharedSecret tag Under Credential tag
                    thisProgram.xmlWriter.WriteEndElement();//Closing Credential tag under Sender Tag
                    thisProgram.xmlWriter.WriteStartElement("UserAgent");//Starting UserAgent tag
                    thisProgram.xmlWriter.WriteString(UserAgent);
                    thisProgram.xmlWriter.WriteEndElement();//Closing UserAgent tag under Sender Tag
                    thisProgram.xmlWriter.WriteEndElement();// Closing Sender Tag

                    thisProgram.xmlWriter.WriteEndElement();//Closing Header Tag


                    #endregion Header


                    #region Request

                    string selectShipToData = ConfigurationManager.AppSettings["selectShipToData"].ToString();
                    string strShipToData = selectShipToData + "'" + orderNum + "'";
                    SqlDataAdapter dtShipToData = new SqlDataAdapter(strShipToData, con);
                    DataSet dsShipToData = new DataSet();
                    dtShipToData.Fill(dsShipToData);
                    if (dsShipToData.Tables[0].Rows.Count == 0)
                        {
                        throw new Exception("NO Data in View_ASN_SNH_Data for order #" + orderNum + ", Please check the data and run the exe again");
                        }




                    DateTime timeStampDate1 = DateTime.Now;
                    string convTimeStampDate1 = String.Format("{0:s}", timeStampDate);
                    convTimeStampDate1 = convTimeStampDate + "+11:00";

                    DateTime reqDevDate = Convert.ToDateTime(dsShipToData.Tables[0].Rows[0]["DeliveryDate"].ToString());
                    string convReqDevDate = String.Format("{0:s}", reqDevDate);
                    convReqDevDate = convReqDevDate + "+11:00";

                    string SupplierName = null;
                    string Street = null;
                    string City = null;
                    string State = null;
                    string PostalCode = null;
                    string CountryName = "Australia";
                    string CountryCode = null;
                    string CarrierName = string.Empty;
                    

                    SupplierName = dsShipToData.Tables[0].Rows[0]["Name"].ToString();
                    Street = dsShipToData.Tables[0].Rows[0]["Street"].ToString();
                    City = dsShipToData.Tables[0].Rows[0]["City"].ToString();
                    State = dsShipToData.Tables[0].Rows[0]["State"].ToString();
                    PostalCode = dsShipToData.Tables[0].Rows[0]["PostalCode"].ToString();
                    CountryCode = dsShipToData.Tables[0].Rows[0]["CountryCode"].ToString();
                    CarrierName = dsShipToData.Tables[0].Rows[0]["CarrierName"].ToString();



                    thisProgram.xmlWriter.WriteStartElement("Request");//starting Request Tag
                    thisProgram.xmlWriter.WriteAttributeString("deploymentMode", "production");

                    thisProgram.xmlWriter.WriteStartElement("ShipNoticeRequest");//starting ShipNoticeRequest Tag

                    thisProgram.xmlWriter.WriteStartElement("ShipNoticeHeader");//starting ShipNoticeHeader Tag
                    thisProgram.xmlWriter.WriteAttributeString("shipmentID", orderNum + "_ASN");
                    thisProgram.xmlWriter.WriteAttributeString("operation", "new");
                    thisProgram.xmlWriter.WriteAttributeString("noticeDate", convTimeStampDate1);
                    thisProgram.xmlWriter.WriteAttributeString("deliveryDate", convReqDevDate);
                    thisProgram.xmlWriter.WriteAttributeString("shipmentType", "actual");
                    //thisProgram.xmlWriter.WriteAttributeString("shipmentDate", convTimeStampDate);

                    thisProgram.xmlWriter.WriteStartElement("ServiceLevel");//starting ServiceLevel Tag
                    thisProgram.xmlWriter.WriteAttributeString("xml", "lang", null, "en-AU");
                    thisProgram.xmlWriter.WriteString("Contractual");
                    thisProgram.xmlWriter.WriteEndElement();// Closing ServiceLevel Tag

                    thisProgram.xmlWriter.WriteStartElement("Contact");//starting Contact Tag
                    thisProgram.xmlWriter.WriteAttributeString("role", "shipFrom");

                    thisProgram.xmlWriter.WriteStartElement("Name");//starting Name Tag;
                    thisProgram.xmlWriter.WriteAttributeString("xml", "lang", null, "en");
                    thisProgram.xmlWriter.WriteString(SupplierName);
                    thisProgram.xmlWriter.WriteEndElement();// Closing Name Tag

                    thisProgram.xmlWriter.WriteStartElement("PostalAddress");//starting PostalAddress Tag;
                    thisProgram.xmlWriter.WriteStartElement("Street");//starting Street Tag;
                    thisProgram.xmlWriter.WriteString(Street);
                    thisProgram.xmlWriter.WriteEndElement();// Closing Street Tag
                    thisProgram.xmlWriter.WriteStartElement("City");//starting City Tag;
                    thisProgram.xmlWriter.WriteString(City);
                    thisProgram.xmlWriter.WriteEndElement();// Closing City Tag
                    thisProgram.xmlWriter.WriteStartElement("State");//starting State Tag;
                    thisProgram.xmlWriter.WriteString(State);
                    thisProgram.xmlWriter.WriteEndElement();// Closing State Tag
                    thisProgram.xmlWriter.WriteStartElement("PostalCode");//starting PostalCode Tag;
                    thisProgram.xmlWriter.WriteString(PostalCode);
                    thisProgram.xmlWriter.WriteEndElement();// Closing PostalCode Tag
                    thisProgram.xmlWriter.WriteStartElement("Country");//starting Country Tag;
                    thisProgram.xmlWriter.WriteAttributeString("isoCountryCode", CountryCode);
                    thisProgram.xmlWriter.WriteString(CountryName);
                    thisProgram.xmlWriter.WriteEndElement();// Closing Country Tag
                    thisProgram.xmlWriter.WriteEndElement();// Closing PostalAddress Tag

                    thisProgram.xmlWriter.WriteEndElement();// Closing Contact Tag

                    thisProgram.xmlWriter.WriteEndElement();// Closing ShipNoticeHeader Tag


                    thisProgram.xmlWriter.WriteStartElement("ShipControl");//starting ShipControl Tag

                    thisProgram.xmlWriter.WriteStartElement("CarrierIdentifier");//starting CarrierIdentifier Tag
                    thisProgram.xmlWriter.WriteAttributeString("domain", "companyName");
                    thisProgram.xmlWriter.WriteString(CarrierName);
                    thisProgram.xmlWriter.WriteEndElement();// Closing CarrierIdentifier Tag

                    thisProgram.xmlWriter.WriteStartElement("ShipmentIdentifier");//starting ShipmentIdentifier Tag
                    thisProgram.xmlWriter.WriteAttributeString("trackingNumberDate", convASNCreatedDate);
                    thisProgram.xmlWriter.WriteString(CarrierName);
                    thisProgram.xmlWriter.WriteEndElement();// Closing ShipmentIdentifier Tag

                    //thisProgram.xmlWriter.WriteStartElement("TransportInformation");//starting TransportInformation Tag
                    //thisProgram.xmlWriter.WriteStartElement("Route");//starting Route Tag
                    //thisProgram.xmlWriter.WriteAttributeString("method", "motor");
                    //thisProgram.xmlWriter.WriteEndElement();// Closing Route Tag

                    ////thisProgram.xmlWriter.WriteStartElement("ShippingContactNumber");//starting ShippingContactNumber Tag
                    ////thisProgram.xmlWriter.WriteString("02854264758");
                    ////thisProgram.xmlWriter.WriteEndElement();// Closing ShippingContactNumber Tag

                    //thisProgram.xmlWriter.WriteStartElement("ShippingInstructions");//starting ShippingInstructions Tag
                    //thisProgram.xmlWriter.WriteStartElement("Description");//starting Description Tag
                    //thisProgram.xmlWriter.WriteAttributeString("xml", "lang", null, "en-AU");
                    //thisProgram.xmlWriter.WriteEndElement();// Closing Description Tag
                    //thisProgram.xmlWriter.WriteEndElement();// Closing ShippingInstructions Tag

                    //thisProgram.xmlWriter.WriteEndElement();// Closing TransportInformation Tag

                    thisProgram.xmlWriter.WriteEndElement();// Closing ShipControl Tag



                    thisProgram.xmlWriter.WriteStartElement("ShipNoticePortion");//starting ShipNoticePortion Tag

                    thisProgram.xmlWriter.WriteStartElement("OrderReference");//starting OrderReference Tag
                    thisProgram.xmlWriter.WriteAttributeString("orderID", OrderNumber);
                    thisProgram.xmlWriter.WriteAttributeString("orderDate", convOrdDate);
                    DocRefPayloadID = PayloadID;
                    thisProgram.xmlWriter.WriteStartElement("DocumentReference");//starting DocumentReference Tag
                    thisProgram.xmlWriter.WriteAttributeString("payloadID", DocRefPayloadID);
                    thisProgram.xmlWriter.WriteEndElement();// Closing DocumentReference Tag
                    thisProgram.xmlWriter.WriteEndElement();// Closing OrderReference Tag

                    string selectRequestItemData = ConfigurationManager.AppSettings["selectRequestItemData"].ToString();
                    string strRequestItemData = selectRequestItemData + "'" + orderNum + "'";
                    SqlDataAdapter dtRequestItemData = new SqlDataAdapter(strRequestItemData, con);
                    DataSet dsRequestItemData = new DataSet();
                    dtRequestItemData.Fill(dsRequestItemData);
                    if (dsRequestItemData.Tables[0].Rows.Count == 0)
                        {
                        throw new Exception("NO Data in View_ASN_LineItem_Data for order #" + orderNum + ", Please check the data and run the exe again");
                        }

                    string LineNumber = null;
                    string Quantity = null;
                    string UnitOfMeasure = null;
                    int ShipNoticeLineNumber = 0;
                    string SupplierPartID = null;
                    string BuyerPartID = null;
                    string UnitPrice = null;
                    string ItemDescription = null;
                    string EANID = null;



                    for (int j = 0; j < dsRequestItemData.Tables[0].Rows.Count; j++)
                        {

                        


                        LineNumber = dsRequestItemData.Tables[0].Rows[j]["ord_LineNumber"].ToString();
                        Quantity = dsRequestItemData.Tables[0].Rows[j]["inv_Quantity"].ToString();
                        UnitOfMeasure = dsRequestItemData.Tables[0].Rows[j]["inv_UnitOfMeasure"].ToString();
                        ShipNoticeLineNumber = Convert.ToInt32(dsRequestItemData.Tables[0].Rows[j]["ShipNoticeLineNumber"]);
                        SupplierPartID = dsRequestItemData.Tables[0].Rows[j]["SupplierPartID"].ToString();
                        BuyerPartID = dsRequestItemData.Tables[0].Rows[j]["BuyerPartID"].ToString();
                        UnitPrice = dsRequestItemData.Tables[0].Rows[j]["UnitPrice"].ToString();
                        ItemDescription = dsRequestItemData.Tables[0].Rows[j]["ItemDescription"].ToString();
                        EANID = dsRequestItemData.Tables[0].Rows[j]["EANID"].ToString();

                        thisProgram.xmlWriter.WriteStartElement("ShipNoticeItem");//starting ShipNoticeItem Tag
                        thisProgram.xmlWriter.WriteAttributeString("quantity", Quantity);
                        thisProgram.xmlWriter.WriteAttributeString("lineNumber", LineNumber);
                        thisProgram.xmlWriter.WriteAttributeString("shipNoticeLineNumber", ShipNoticeLineNumber.ToString());

                        thisProgram.xmlWriter.WriteStartElement("ItemID");//starting ItemID Tag
                        thisProgram.xmlWriter.WriteStartElement("SupplierPartID");//starting SupplierPartID Tag
                        thisProgram.xmlWriter.WriteString(SupplierPartID);
                        thisProgram.xmlWriter.WriteEndElement();// Closing SupplierPartID Tag
                        thisProgram.xmlWriter.WriteStartElement("BuyerPartID");//starting BuyerPartID Tag
                        thisProgram.xmlWriter.WriteString(BuyerPartID);
                        thisProgram.xmlWriter.WriteEndElement();// Closing BuyerPartID Tag
                        thisProgram.xmlWriter.WriteEndElement();// Closing ItemID Tag

                        thisProgram.xmlWriter.WriteStartElement("ShipNoticeItemDetail");//starting ShipNoticeItemDetail Tag

                        thisProgram.xmlWriter.WriteStartElement("UnitPrice");//starting UnitPrice Tag
                        thisProgram.xmlWriter.WriteStartElement("Money");//starting Money Tag
                        thisProgram.xmlWriter.WriteAttributeString("currency", "AUD");
                        thisProgram.xmlWriter.WriteString(UnitPrice);
                        thisProgram.xmlWriter.WriteEndElement();// Closing Money Tag
                        thisProgram.xmlWriter.WriteEndElement();// Closing UnitPrice Tag

                        thisProgram.xmlWriter.WriteStartElement("Description");//starting Description Tag
                        thisProgram.xmlWriter.WriteAttributeString("xml", "lang", null, "en-AU");
                        thisProgram.xmlWriter.WriteString(ItemDescription);
                        thisProgram.xmlWriter.WriteEndElement();// Closing Description Tag

                        thisProgram.xmlWriter.WriteStartElement("UnitOfMeasure");//starting UnitOfMeasure Tag
                        thisProgram.xmlWriter.WriteString(UnitOfMeasure);
                        thisProgram.xmlWriter.WriteEndElement();// Closing UnitOfMeasure Tag

                        thisProgram.xmlWriter.WriteStartElement("ItemDetailIndustry");//starting ItemDetailIndustry Tag
                        thisProgram.xmlWriter.WriteStartElement("ItemDetailRetail");//starting ItemDetailRetail Tag
                        thisProgram.xmlWriter.WriteStartElement("EANID");//starting EANID Tag
                        thisProgram.xmlWriter.WriteString(EANID);
                        thisProgram.xmlWriter.WriteEndElement();// Closing EANID Tag
                        thisProgram.xmlWriter.WriteEndElement();// Closing ItemDetailRetail Tag
                        thisProgram.xmlWriter.WriteEndElement();// Closing ItemDetailIndustry Tag

                        thisProgram.xmlWriter.WriteEndElement();// Closing ShipNoticeItemDetail Tag

                        thisProgram.xmlWriter.WriteStartElement("UnitOfMeasure");//starting UnitOfMeasure Tag
                        thisProgram.xmlWriter.WriteString(UnitOfMeasure);
                        thisProgram.xmlWriter.WriteEndElement();// Closing UnitOfMeasure Tag

                        thisProgram.xmlWriter.WriteStartElement("SupplierBatchID");//starting SupplierBatchID Tag
                        thisProgram.xmlWriter.WriteString("BATCH");
                        thisProgram.xmlWriter.WriteEndElement();// Closing SupplierBatchID Tag

                        thisProgram.xmlWriter.WriteStartElement("ShipNoticeItemIndustry");//starting ShipNoticeItemIndustry Tag
                        thisProgram.xmlWriter.WriteStartElement("ShipNoticeItemRetail");//starting ShipNoticeItemRetail Tag

                        DateTime currDate = DateTime.Now;
                        DateTime BestDate = currDate.AddDays(7);
                        DateTime ExpiryDate = currDate.AddDays(7);
                        string bestBeforeDate = String.Format("{0:s}", BestDate);
                        bestBeforeDate = bestBeforeDate + "+11:00";
                        string expiryDate = String.Format("{0:s}", ExpiryDate);
                        expiryDate = expiryDate + "+11:00";

                        thisProgram.xmlWriter.WriteStartElement("BestBeforeDate");//starting BestBeforedate Tag
                        thisProgram.xmlWriter.WriteAttributeString("date", bestBeforeDate);
                        thisProgram.xmlWriter.WriteEndElement();// Closing BestBeforedate Tag

                        thisProgram.xmlWriter.WriteStartElement("ExpiryDate");//starting ExpiryDate Tag
                        thisProgram.xmlWriter.WriteAttributeString("date", expiryDate);
                        thisProgram.xmlWriter.WriteEndElement();// Closing ExpiryDate Tag

                        thisProgram.xmlWriter.WriteStartElement("FreeGoodsQuantity");//starting FreeGoodsQuantity Tag
                        thisProgram.xmlWriter.WriteAttributeString("quantity", "1");
                        thisProgram.xmlWriter.WriteStartElement("UnitOfMeasure");//starting UnitOfMeasure Tag
                        thisProgram.xmlWriter.WriteString(UnitOfMeasure);
                        thisProgram.xmlWriter.WriteEndElement();// Closing UnitOfMeasure Tag
                        thisProgram.xmlWriter.WriteEndElement();// Closing FreeGoodsQuantity Tag

                        thisProgram.xmlWriter.WriteEndElement();// Closing ShipNoticeItemRetail Tag
                        thisProgram.xmlWriter.WriteEndElement();// Closing ShipNoticeItemIndustry Tag


                        thisProgram.xmlWriter.WriteEndElement();// Closing ShipNoticeItem Tag
                        }

                   


                    thisProgram.xmlWriter.WriteEndElement();// Closing ShipNoticePortion Tag

                    thisProgram.xmlWriter.WriteEndElement();// Closing ShipNoticeRequest Tag

                    thisProgram.xmlWriter.WriteEndElement();// Closing Request Tag

                    #endregion Request

                    thisProgram.xmlWriter.WriteEndDocument();
                    thisProgram.xmlWriter.Flush();
                    thisProgram.xmlWriter.Close();
                    thisProgram.xmlWriter.Dispose();


                    thisProgram.PostASN(orderNum,CatalogueSeqID);

                    thisProgram.MoveFiles(orderNum);

                    thisProgram.UpdateASNFlag(orderNum);


                    }


                }
            catch (Exception ex)
                {
                ExceptionLogging.SendExcepToDB(ex, "CreateStaplesASNFiles");
                }
            
            }


        private void PostASN(string _orderFile,int _catalogueSeqID)
            {
            WebRequest req = null;
            WebResponse rsp = null;
            string fileName = string.Empty;         
            
            //string fileName = "C:\\Staples\\cXMLFiles\\ASN_4501679674.xml"; 
            fileName = ASNCreationPath + _orderFile + "_ASN.xml";
            string uri = ConfigurationManager.AppSettings["postURL"].ToString();
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
                    string responseString = streamRead.ReadToEnd();
                    SaveASNResponse(responseString, _orderFile, _catalogueSeqID);

                    rsp.Close();
                    }
                }
            catch (WebException webEx) { }
            catch (Exception ex) { ExceptionLogging.SendExcepToDB(ex, "CreateStaplesASNFiles"); }
            finally
                {
                if (req != null) req.GetRequestStream().Close();
                if (rsp != null) rsp.GetResponseStream().Close();
                }
            }


        private string GenerateUniquePayloadID()
            {
            Guid g;                      
            // Create and display the value of two GUIDs.
            g = Guid.NewGuid();              
            return g.ToString();
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

        private void SaveASNResponse(string _response, string _orderNumber,int _catalogueSeqID)
            {

            try
                {
                XmlDocument asnResponseDOC = new XmlDocument();
                asnResponseDOC.LoadXml(_response);

                string timeStamp = string.Empty;
                string payloadID = string.Empty;
                int statusCode = 0;
                string statusText = string.Empty;
                string statusMessage = string.Empty;
                string createdDate = null;

                if (asnResponseDOC.SelectSingleNode("//cXML") != null)
                    {

                    timeStamp = (asnResponseDOC.SelectSingleNode("//cXML").Attributes["timestamp"].Value == null) ? null : asnResponseDOC.SelectSingleNode("//cXML").Attributes["timestamp"].Value;

                    }
                else
                    {


                    }

                if (asnResponseDOC.SelectSingleNode("//cXML") != null)
                    {

                    payloadID = (asnResponseDOC.SelectSingleNode("//cXML").Attributes["payloadID"].Value == null) ? null : asnResponseDOC.SelectSingleNode("//cXML").Attributes["payloadID"].Value;

                    }
                else
                    {


                    }

                if (asnResponseDOC.SelectSingleNode("//cXML/Response/Status") != null)
                    {


                    statusCode = int.Parse((asnResponseDOC.SelectSingleNode("//cXML/Response/Status").Attributes["code"].Value == null) ? null : asnResponseDOC.SelectSingleNode("//cXML/Response/Status").Attributes["code"].Value);

                    }
                else
                    {


                    }

                if (asnResponseDOC.SelectSingleNode("//cXML/Response/Status") != null)
                    {


                    statusText = (asnResponseDOC.SelectSingleNode("//cXML/Response/Status").Attributes["text"].Value == null) ? null : asnResponseDOC.SelectSingleNode("//cXML/Response/Status").Attributes["text"].Value;

                    }
                else
                    {


                    }

                if (asnResponseDOC.SelectSingleNode("//cXML/Response/Status") != null)
                    {


                    statusMessage = asnResponseDOC.SelectSingleNode("//cXML/Response/Status").InnerText == "" ? null : asnResponseDOC.SelectSingleNode("//cXML/Response/Status").InnerText;

                    }
                else
                    {


                    }

                createdDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

                InsertASNResponseDetails(_orderNumber, timeStamp, payloadID, statusCode, statusText, statusMessage, createdDate, _catalogueSeqID);
                }
            catch (Exception ex)
                {

                ExceptionLogging.SendExcepToDB(ex, "CreateStaplesASNConfirmation - SaveASNResponse");
                }


            }


        private void InsertASNResponseDetails(string _orderNumber, string _timeStamp, string _payloadID, int _statusCode, string _statusText, string _statusMessage, string _CreatredDate, int _catalogueSeqID)
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
                    cmd.Parameters.AddWithValue("@catalogue_SeqID", _catalogueSeqID);


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

                ExceptionLogging.SendExcepToDB(ex, "CreateStaplesASN - InsertASNResponseDetails");
                }

            }

        public void MoveFiles(string _orderFileName)
            {

            try
                {

                string sourcePath = string.Empty;
                string destinationPath = string.Empty;
                string orderFile = string.Empty;

                orderFile = _orderFileName + "_ASN.xml";

                string sourcePathFromAppSettings = ConfigurationManager.AppSettings["sourcePathFromAppSettings"].ToString();
                sourcePath = sourcePathFromAppSettings;

                //destinationPath = @"\\teapot\D$\ORDERS_IN\SPOTLESS\Invoices\Processed\";

                string destPathFromAppSettings = ConfigurationManager.AppSettings["destPathFromAppSettings"].ToString();
                destinationPath = destPathFromAppSettings;

                File.Copy(Path.Combine(sourcePath, orderFile), Path.Combine(destinationPath, orderFile), true);
                sourcePath = sourcePath + "\\" + orderFile;
                File.Delete(sourcePath);
                }
            catch (Exception ex)
                {

                ExceptionLogging.SendExcepToDB(ex, "CreateStaplesASNConfirmation - MoveFiles");
                }


            }

        public void UpdateASNFlag(string _orderNumber)
            {
            try
                {
                SqlConnection conn = new SqlConnection(connectionString);
                string query = ConfigurationManager.AppSettings["updateASNFlag"].ToString();
                string updateQuery = query + "'" + _orderNumber + "'";
                SqlCommand cmd = new SqlCommand(updateQuery, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();

                }
            catch (Exception ex)
                {
                ExceptionLogging.SendExcepToDB(ex, "CreateStaplesASN - UpdateASNFlag");
                }



            }


        }
    }
