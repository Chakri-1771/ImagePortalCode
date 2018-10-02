using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Xml;

namespace SaveStaplesXMLOrderDataToDB
    {
    class Program
        {

       
            
        string connectionString = ConfigurationManager.ConnectionStrings["sqlConn"].ToString();
        string errorRegion = string.Empty;

        static void Main(string[] args)
            {

            try
                {
                Program thisProgram = new Program();

                XmlDocument doc = new XmlDocument();

                string readFileFromPath = ConfigurationManager.AppSettings["readFileFromPath"].ToString();

                string readFileType = ConfigurationManager.AppSettings["readFileType"].ToString();

                string orderNumber = null;




                foreach (string file in Directory.EnumerateFiles(readFileFromPath, readFileType))
                    {


                    doc.Load(file);

                    thisProgram.InsertOrderCatalogueData(Path.GetFileName(file), readFileFromPath);

                  //  thisProgram.InsertOrderPayloadDetails(doc);

                    thisProgram.InsertOrderHeaderData(doc);

                    thisProgram.InsertOrderRequestData(doc);

                    thisProgram.InsertOrderRequestShipToData(doc);

                    thisProgram.InsertOrderRequestBillToData(doc);

                    thisProgram.InsertOrderRequestSupplierContactData(doc);

                    thisProgram.InsertOrderRequestPurchaserContactData(doc);

                    thisProgram.InsertOrderRequestItemData(doc);

                    thisProgram.UpdateProcessedFlag();

                    thisProgram.MoveProcessedFiles(Path.GetFileName(file));


                    }
                }
            catch (Exception ex)
                {
                ExceptionLogging.SendExcepToDB(ex, "CreateStaplesInvoiceFiles");
                }
            


             }

        public void InsertOrderCatalogueData(string _fileName, string _filePath)
            {

            try
                {
                string OrderNumber = null;
                string OrderDate = null;
                string OrderFileName = null;
                int ProcessedFlag = 0;
                int InternalFlag = 0;
                string CreatedDate;
                int AcrPush_Flag = 0;
                


                OrderNumber = _fileName.Substring(0, 10);
                OrderDate = DateTime.Now.ToString("yyyy-MM-dd");
                OrderFileName = _filePath + "\\" + _fileName;
                ProcessedFlag = 0;
                CreatedDate = DateTime.Now.ToString("yyyy-MM-dd");
                AcrPush_Flag = 0;

                InsertIntoOrderCatalogueData(OrderNumber, OrderDate, OrderFileName, ProcessedFlag,InternalFlag, CreatedDate, AcrPush_Flag);
                }
            catch (Exception ex )
                {
                ExceptionLogging.SendExcepToDB(ex, "CreateStaplesInvoiceFiles");
                }
            
            
            }


        public void InsertIntoOrderCatalogueData(string _OrderNumber, string _OrderDate, string _OrderFileName, int _ProcessedFlag, int _IntetrnalFlag, string _CreatedDate, int _AcrPush_Flag)
            {

            try
                {

                string insertIntotblOrderCatalogueData = ConfigurationManager.AppSettings["insertIntotblOrderCatalogueData"].ToString();
                string query = insertIntotblOrderCatalogueData;

                // create connection and command
                using (SqlConnection cn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(query, cn))
                    {
                    // define parameters and their values
                    
                    cmd.Parameters.AddWithValue("@OrderNumber", _OrderNumber);
                    cmd.Parameters.AddWithValue("@OrderDate", _OrderDate);
                    cmd.Parameters.AddWithValue("@OrderFileName", _OrderFileName);
                    cmd.Parameters.AddWithValue("@Processed_Flag", _ProcessedFlag);
                    cmd.Parameters.AddWithValue("@Validation_Flag", _IntetrnalFlag);
                    cmd.Parameters.AddWithValue("@CreatedDate", _CreatedDate);
                    cmd.Parameters.AddWithValue("@AcrPush_Flag", _AcrPush_Flag);


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
               
                ExceptionLogging.SendExcepToDB(ex, "CreateStaplesInvoiceFiles");
                }
            }


        public void InsertOrderPayloadDetails(XmlDocument xmlDoc)
            {
            try
                {
                int CatalogueSeqID = 0;
                string OrderNumber = null;
                string OrderDate = null;
                string OrderPayloadID = null;
                string OrderTimeStamp = null;
                string CreatedDate = null;


                XmlNode orh = xmlDoc.SelectSingleNode("//OrderRequestHeader");
                if (xmlDoc.SelectSingleNode("//OrderRequestHeader") != null)
                    {
                    OrderNumber = (orh.Attributes["orderID"].Value == null) ? null : orh.Attributes["orderID"].Value;
                    }

                XmlNode cxml = xmlDoc.SelectSingleNode("//cXML");
                if (xmlDoc.SelectSingleNode("//cXML") != null)
                    {
                    OrderTimeStamp = (cxml.Attributes["timestamp"].Value == null) ? null : cxml.Attributes["timestamp"].Value;
                    OrderPayloadID = (cxml.Attributes["payloadID"].Value == null) ? null : cxml.Attributes["payloadID"].Value;
                    }

                OrderDate = OrderTimeStamp.Substring(0, 10);
                CreatedDate = DateTime.Now.ToString("yyyy-MM-dd");
                CatalogueSeqID = getCatalogueID(OrderNumber);
                InsertIntoOrderPayloadDetails(CatalogueSeqID, OrderNumber, OrderDate, OrderTimeStamp, OrderPayloadID, CreatedDate);
                }
            catch (Exception ex)
                {
                ExceptionLogging.SendExcepToDB(ex, "CreateStaplesInvoiceFiles");
                }
            

            }

        public void InsertIntoOrderPayloadDetails(int _CatalogueSeqID, string _OrderNumber,string _OrderDate, string _OrderTimeStamp, string _OrderPayloadID, string _CreatedDate)
            {

            try
                {

                string InsertIntoOrderPayloadDetails = ConfigurationManager.AppSettings["InsertIntoOrderPayloadDetails"].ToString();
                string query = InsertIntoOrderPayloadDetails;

                // create connection and command
                using (SqlConnection cn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(query, cn))
                    {
                    // define parameters and their values
                    cmd.Parameters.AddWithValue("@Catalogue_SeqID", _CatalogueSeqID);
                    cmd.Parameters.AddWithValue("@OrderNumber", _OrderNumber);
                    cmd.Parameters.AddWithValue("@OrderDate", _OrderDate);
                    cmd.Parameters.AddWithValue("@OrderTimeStamp", _OrderTimeStamp);
                    cmd.Parameters.AddWithValue("@OrderPayloadID", _OrderPayloadID);
                    cmd.Parameters.AddWithValue("@CreatedDate", _CreatedDate);

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
              
                ExceptionLogging.SendExcepToDB(ex, "CreateStaplesInvoiceFiles");
                }
            }


        public void InsertOrderHeaderData(XmlDocument xmlDoc)
            {

            try
                {
                #region  Order Header data variables

                int CatalogueSeqID = 0;
                string OrderNumber = null;
                string PayloadID = null;
                string Version = null;
                string PayloadTimeStamp = null;
                string FromNetworkID = null;
                string FromSystemID = null;
                string ToNetworkID = null;
                string SenderAribaNetworkUserID = null;
                string SenderSharedSecret = null;
                string UserAgent = null;
                string HeaderComments = null;
                string CreatedDate = null;

                #endregion Order Header data variables

                XmlNode orh = xmlDoc.SelectSingleNode("//OrderRequestHeader");
                if (xmlDoc.SelectSingleNode("//OrderRequestHeader") != null)
                    {
                    OrderNumber = (orh.Attributes["orderID"].Value == null) ? null : orh.Attributes["orderID"].Value;
                    }

                XmlNode cxml = xmlDoc.SelectSingleNode("//cXML");
                if (xmlDoc.SelectSingleNode("//cXML") != null)
                    {
                    PayloadTimeStamp = (cxml.Attributes["timestamp"].Value == null) ? null : cxml.Attributes["timestamp"].Value;
                    PayloadID = (cxml.Attributes["payloadID"].Value == null) ? null : cxml.Attributes["payloadID"].Value;
                    Version = (cxml.Attributes["version"].Value == null) ? null : cxml.Attributes["version"].Value;
                    }

                if (xmlDoc.SelectSingleNode("//Header/From/Credential[@domain='NetworkID']/Identity") != null)
                    {
                    FromNetworkID = (xmlDoc.SelectSingleNode("//Header/From/Credential[@domain='NetworkID']/Identity").InnerText == null) ? null : xmlDoc.SelectSingleNode("//Header/From/Credential[@domain='NetworkID']/Identity").InnerText;
                    }
                else
                    {
                    LogError("//Header/From/Credential[@domain='NetworkID']/Identity Not Found");
                    }

                if (xmlDoc.SelectSingleNode("//Header/From/Credential[@domain='SystemID']/Identity") != null)
                    {
                    FromSystemID = (xmlDoc.SelectSingleNode("//Header/From/Credential[@domain='SystemID']/Identity").InnerText == null) ? null : xmlDoc.SelectSingleNode("//Header/From/Credential[@domain='SystemID']/Identity").InnerText;
                    }
                else
                    {
                    LogError("//Header/From/Credential[@domain='SystemID']/Identity not Found");
                    }

                if (xmlDoc.SelectSingleNode("//Header/To/Credential[@domain='NetworkID']/Identity") != null)
                    {
                    ToNetworkID = (xmlDoc.SelectSingleNode("//Header/To/Credential[@domain='NetworkID']/Identity").InnerText == null) ? null : xmlDoc.SelectSingleNode("//Header/To/Credential[@domain='NetworkID']/Identity").InnerText;
                    }
                else
                    {
                    LogError("//Header/To/Credential[@domain='NetworkID']/Identity not Found");
                    }

                if (xmlDoc.SelectSingleNode("//Header/Sender/Credential[@domain='AribaNetworkUserId']/Identity") != null)
                    {
                    SenderAribaNetworkUserID = (xmlDoc.SelectSingleNode("//Header/Sender/Credential[@domain='AribaNetworkUserId']/Identity").InnerText == null) ? null : xmlDoc.SelectSingleNode("//Header/Sender/Credential[@domain='AribaNetworkUserId']/Identity").InnerText;
                    }
                else
                    {
                    LogError("//Header/Sender/Credential[@domain='AribaNetworkUserID']/Identity not Found");
                    }

                if (xmlDoc.SelectSingleNode("//Header/Sender/UserAgent") != null)
                    {
                    UserAgent = (xmlDoc.SelectSingleNode("//Header/Sender/UserAgent").InnerText == null) ? null : xmlDoc.SelectSingleNode("//Header/Sender/UserAgent").InnerText;
                    }
                else
                    {
                    LogError("//Header/Sender/UserAgent not Found");
                    }

                if (xmlDoc.SelectSingleNode("//Request/OrderRequest/OrderRequestHeader/Comments") != null)
                    {
                    HeaderComments = (xmlDoc.SelectSingleNode("//Request/OrderRequest/OrderRequestHeader/Comments").InnerText == null) ? null : xmlDoc.SelectSingleNode("//Request/OrderRequest/OrderRequestHeader/Comments").InnerText;
                    }
                else
                    {
                    LogError("//Request/OrderRequest/OrderRequestHeader/Comments not Found");
                    }

                CreatedDate = DateTime.Now.ToString("yyyy-MM-dd");
                SenderSharedSecret = null;
                CatalogueSeqID = getCatalogueID(OrderNumber);
                InsertIntoOrderHeaderTable(CatalogueSeqID, OrderNumber, PayloadID, Version, PayloadTimeStamp, FromNetworkID, FromSystemID, ToNetworkID, SenderAribaNetworkUserID, SenderSharedSecret, UserAgent, HeaderComments,CreatedDate);

                }
            catch (Exception ex)
                {
                ExceptionLogging.SendExcepToDB(ex, "CreateStaplesInvoiceFiles");
                }

           

            }

        public int getCatalogueID(string _orderNumber)
            {
            int catalogueSeqID = 0;
            try
                {
                
                string sqlQuery = "select SeqID from tbl_Ord_Catalogue where orderNumber = " + "'" + _orderNumber + "' and isnull(Processed_Flag,0) = 0";
                SqlConnection sqlConn = new SqlConnection(connectionString);
                SqlCommand cmd = new SqlCommand(sqlQuery, sqlConn);
                sqlConn.Open();
                catalogueSeqID = (Int32)cmd.ExecuteScalar();
                sqlConn.Close();

                }
            catch (Exception ex)
                {
                 
                ExceptionLogging.SendExcepToDB(ex, "CreateStaplesInvoiceFiles");
                }
            return catalogueSeqID;
            }

        public void InsertIntoOrderHeaderTable(int _CatalogueSeqID, string _OrderNumber, string _PayloadID, string _Version, string _PayloadTimestamp, string _FromNetworkID, string _FromSystemID, string _ToNetworkID, string _SenderAribaNetworkUserID, string _SenderSharedSecret, string _UserAgent,string _HeaderComments,string _CreatedDate)
            {

            try
                {
                
                string insertIntotblOrderHeaderData = ConfigurationManager.AppSettings["insertIntotblOrderHeaderData"].ToString();
                string query = insertIntotblOrderHeaderData;

                // create connection and command
                using (SqlConnection cn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(query, cn))
                    {
                    // define parameters and their values
                    cmd.Parameters.AddWithValue("@Catalogue_SeqID", _CatalogueSeqID);
                    cmd.Parameters.AddWithValue("@OrderNumber", _OrderNumber);
                    cmd.Parameters.AddWithValue("@PayloadID", _PayloadID);
                    cmd.Parameters.AddWithValue("@Version", _Version);
                    cmd.Parameters.AddWithValue("@PayloadTimestamp", _PayloadTimestamp);
                    cmd.Parameters.AddWithValue("@From_NetworkID", _FromNetworkID);
                    cmd.Parameters.AddWithValue("@From_SystemID", _FromSystemID);
                    cmd.Parameters.AddWithValue("@To_NetworkID", _ToNetworkID);
                    cmd.Parameters.AddWithValue("@Sender_AribaNetworkUserID", _SenderAribaNetworkUserID);
                    cmd.Parameters.AddWithValue("@Sender_SharedSecret", _SenderSharedSecret);
                    cmd.Parameters.AddWithValue("@UserAgent", _UserAgent);
                    cmd.Parameters.AddWithValue("@Header_Comments", _HeaderComments);
                    cmd.Parameters.AddWithValue("@CreatedDate", _CreatedDate);

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
                 
                ExceptionLogging.SendExcepToDB(ex, "CreateStaplesInvoiceFiles");
                }
            }


        public void InsertOrderRequestData(XmlDocument xmlDoc)
            {

            try
                {
                #region  Order Header data variables

                int CatalogueSeqID = 0;
                string OrderNumber = null;
                string OrderDate = null;
                string OrderDateTS = null;
                string RequestedDeliveryDate = null;
                string Type = null;
                int OrderVersion = 0;
                string OrderType = null;
                string OrderTotal = null;
                int PayInNumberOfDays = 0;
                decimal DiscountPercent = 0;
                string CreatedDate = null;

                #endregion Order Header data variables
              
                XmlNode orh = xmlDoc.SelectSingleNode("//OrderRequestHeader");
                if (xmlDoc.SelectSingleNode("//OrderRequestHeader") != null)
                    {
                    OrderNumber = (orh.Attributes["orderID"].Value == null) ? null : orh.Attributes["orderID"].Value;
                    Type = (orh.Attributes["type"].Value == null) ? null : orh.Attributes["type"].Value;
                    OrderVersion = int.Parse((orh.Attributes["orderVersion"].Value == null) ? null : orh.Attributes["orderVersion"].Value);
                    OrderType = (orh.Attributes["orderType"].Value == null) ? null : orh.Attributes["orderType"].Value;
                    OrderDateTS = (orh.Attributes["orderDate"].Value == null) ? null : orh.Attributes["orderDate"].Value;
                    OrderDate = OrderDateTS.Substring(0, 10);
                    }

                if (xmlDoc.SelectSingleNode("//OrderRequestHeader/Total/Money") != null)
                    {
                    OrderTotal = (xmlDoc.SelectSingleNode("//OrderRequestHeader/Total/Money").InnerText == null) ? null : xmlDoc.SelectSingleNode("//OrderRequestHeader/Total/Money").InnerText;
                    }
                else
                    {
                    LogError("//OrderRequestHeader/Total/Money not Found");
                    }

                XmlNode orhPayment = xmlDoc.SelectSingleNode("//OrderRequestHeader/PaymentTerm");
                if (xmlDoc.SelectSingleNode("//OrderRequestHeader/PaymentTerm") != null)
                    {
                    PayInNumberOfDays = int.Parse((orhPayment.Attributes["payInNumberOfdays"].Value == null) ? null : orhPayment.Attributes["payInNumberOfdays"].Value);
                    }
                else
                    {
                    LogError("//OrderRequestHeader/PaymentTerm not Found");
                    }

                XmlNode orhDiscount = xmlDoc.SelectSingleNode("//OrderRequestHeader/PaymentTerm/Discount/DiscountPercent");
                if (xmlDoc.SelectSingleNode("//OrderRequestHeader/PaymentTerm/Discount/DiscountPercent") != null)
                    {
                    DiscountPercent = decimal.Parse((orhDiscount.Attributes["percent"].Value == null) ? null : orhDiscount.Attributes["percent"].Value);
                    }
                else
                    {
                    LogError("//OrderRequestHeader/PaymentTerm/Discount/DiscountPercent not Found");
                    }

                XmlNode odd = xmlDoc.SelectSingleNode("//ItemOut");
                if (xmlDoc.SelectSingleNode("//ItemOut") != null)
                    {
                    RequestedDeliveryDate = (odd.Attributes["requestedDeliveryDate"].Value == null) ? null : odd.Attributes["requestedDeliveryDate"].Value;
                    RequestedDeliveryDate = RequestedDeliveryDate.Substring(0, 10);
                    }
                else
                    {
                    LogError("//Header/Sender/UserAgent not Found");
                    }

                CreatedDate = DateTime.Now.ToString("yyyy-MM-dd");
                CatalogueSeqID = getCatalogueID(OrderNumber);
               InsertIntoOrderRequestTable(CatalogueSeqID, OrderNumber, OrderDate, OrderDateTS, RequestedDeliveryDate, Type, OrderVersion, OrderType, OrderTotal, PayInNumberOfDays, DiscountPercent, CreatedDate);

                }
            catch (Exception ex)
                {
                ExceptionLogging.SendExcepToDB(ex, "CreateStaplesInvoiceFiles");
                }
          

            }

        public void InsertIntoOrderRequestTable(int _CatalogueSeqID, string _OrderNumber, string _OrderDate, string _OrderDateTS, string _RequestedDeliveryDate, string _Type, int _OrderVersion,string _OrderType, string _OrderTotal, int _PayInNumberOfDays, decimal _DiscountPercent, string _CreatedDate)
            {

            try
                {

                string insertIntotblOrderHeaderData = ConfigurationManager.AppSettings["insertIntotblOrderRequestData"].ToString();
                string query = insertIntotblOrderHeaderData;

                // create connection and command
                using (SqlConnection cn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(query, cn))
                    {
                    // define parameters and their values
                    cmd.Parameters.AddWithValue("@Catalogue_SeqID", _CatalogueSeqID);
                    cmd.Parameters.AddWithValue("@OrderNumber", _OrderNumber);
                    cmd.Parameters.AddWithValue("@OrderDate", _OrderDate);
                    cmd.Parameters.AddWithValue("@OrderDateTS", _OrderDateTS);
                    cmd.Parameters.AddWithValue("@RequestedDeliveryDate", _RequestedDeliveryDate);
                    cmd.Parameters.AddWithValue("@Type", _Type);
                    cmd.Parameters.AddWithValue("@OrderVersion", _OrderVersion);
                    cmd.Parameters.AddWithValue("@OrderType", _OrderType);
                    cmd.Parameters.AddWithValue("@OrderTotal", _OrderTotal);
                    cmd.Parameters.AddWithValue("@PayInNumberOfDays", _PayInNumberOfDays);
                    cmd.Parameters.AddWithValue("@DiscountPercent", _DiscountPercent);
                    cmd.Parameters.AddWithValue("@CreatedDate", _CreatedDate);

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
                 
                ExceptionLogging.SendExcepToDB(ex, "CreateStaplesInvoiceFiles");
                }
            }



        public void InsertOrderRequestShipToData(XmlDocument xmlDoc)
            {

            try
                {
                #region  Order Header data variables

                int CatalogueSeqID = 0;
                string OrderNumber = null;
                string CountryCode = null;
                string AddressIDDomain = null;
                string AddressID = null;
                string Name = null;
                string PostalAddressName = null;
                string Street1 = null;
                string Street2 = null;
                string Street3 = null;
                string City = null;
                string State = null;
                string PostalCode = null;
                int PhoneCountryCode = 0;
                int PhoneAreaorCityCode = 0;
                string PhoneNumber = null;
                int FaxCountryCode = 0;
                int FaxAreaorCityCode = 0;
                string FaxNumber = null;
                string IdreferenceDomain = null;
                string IdReferenceIdentifier = null;
                string CreatedDate = null;

                #endregion Order Header data variables

                XmlNode orh = xmlDoc.SelectSingleNode("//OrderRequestHeader");
                if (xmlDoc.SelectSingleNode("//OrderRequestHeader") != null)
                    {
                    OrderNumber = (orh.Attributes["orderID"].Value == null) ? null : orh.Attributes["orderID"].Value;
                    }

                XmlNode countryCode = xmlDoc.SelectSingleNode("//OrderRequestHeader/ShipTo/Address");
                if (xmlDoc.SelectSingleNode("//OrderRequestHeader/ShipTo/Address") != null)
                    {
                    CountryCode = (countryCode.Attributes["isoCountryCode"].Value == null) ? null : countryCode.Attributes["isoCountryCode"].Value;
                    AddressIDDomain = (countryCode.Attributes["addressIDDomain"].Value == null) ? null : countryCode.Attributes["addressIDDomain"].Value;
                    AddressID = (countryCode.Attributes["addressID"].Value == null) ? null : countryCode.Attributes["addressID"].Value;
                    }


                if (xmlDoc.SelectSingleNode("//OrderRequestHeader/ShipTo/Address/Name") != null)
                    {
                    Name = (xmlDoc.SelectSingleNode("//OrderRequestHeader/ShipTo/Address/Name").InnerText == null) ? null : xmlDoc.SelectSingleNode("//OrderRequestHeader/ShipTo/Address/Name").InnerText;
                    }
                else
                    {
                    LogError("//OrderRequestHeader/ShipTo/Address/Name");
                    }

                XmlNode postalAddress = xmlDoc.SelectSingleNode("//OrderRequestHeader/ShipTo/Address/PostalAddress");
                if (xmlDoc.SelectSingleNode("//OrderRequestHeader/ShipTo/Address/PostalAddress") != null)
                    {
                    PostalAddressName = (postalAddress.Attributes["name"].Value == null) ? null : postalAddress.Attributes["name"].Value;
                    }

                XmlNodeList streetlist = postalAddress.SelectNodes("//OrderRequestHeader/ShipTo/Address/PostalAddress/Street");
                if (streetlist.Count == 1)
                    {

                    if (streetlist != null)
                        {
                        for (int i = 0; i < streetlist.Count; i++)
                            {
                            Street1 = streetlist[0].InnerXml;


                            }
                        }
                    }

                if (streetlist.Count == 2)
                    {

                    if (streetlist != null)
                        {
                        for (int i = 0; i < streetlist.Count; i++)
                            {
                            Street1 = streetlist[0].InnerXml;
                            Street2 = streetlist[1].InnerXml;

                            }
                        }
                    }

                if (streetlist.Count == 3)
                    {

                    if (streetlist != null)
                        {
                        for (int i = 0; i < streetlist.Count; i++)
                            {
                            Street1 = streetlist[0].InnerXml;
                            Street2 = streetlist[1].InnerXml;
                            Street3 = streetlist[2].InnerXml;

                            }
                        }
                    }

                if (xmlDoc.SelectSingleNode("//OrderRequestHeader/ShipTo/Address/PostalAddress/City") != null)
                    {
                    City = (xmlDoc.SelectSingleNode("//OrderRequestHeader/ShipTo/Address/PostalAddress/City").InnerText == null) ? null : xmlDoc.SelectSingleNode("//OrderRequestHeader/ShipTo/Address/PostalAddress/City").InnerText;
                    }
                else
                    {
                    LogError("//OrderRequestHeader/ShipTo/Address/PostalAddress/City");
                    }

                if (xmlDoc.SelectSingleNode("//OrderRequestHeader/ShipTo/Address/PostalAddress/State") != null)
                    {
                    State = (xmlDoc.SelectSingleNode("//OrderRequestHeader/ShipTo/Address/PostalAddress/State").InnerText == null) ? null : xmlDoc.SelectSingleNode("//OrderRequestHeader/ShipTo/Address/PostalAddress/State").InnerText;
                    }
                else
                    {
                    LogError("//OrderRequestHeader/ShipTo/Address/PostalAddress/State");
                    }

                if (xmlDoc.SelectSingleNode("//OrderRequestHeader/ShipTo/Address/PostalAddress/PostalCode") != null)
                    {
                    PostalCode = (xmlDoc.SelectSingleNode("//OrderRequestHeader/ShipTo/Address/PostalAddress/PostalCode").InnerText == null) ? null : xmlDoc.SelectSingleNode("//OrderRequestHeader/ShipTo/Address/PostalAddress/PostalCode").InnerText;
                    }
                else
                    {
                    LogError("//OrderRequestHeader/ShipTo/Address/PostalAddress/PostalCode");
                    }

                if (xmlDoc.SelectSingleNode("//OrderRequestHeader/ShipTo/Address/Phone/TelephoneNumber/CountryCode") != null)
                    {
                    PhoneCountryCode = int.Parse((xmlDoc.SelectSingleNode("//OrderRequestHeader/ShipTo/Address/Phone/TelephoneNumber/CountryCode").InnerText == null) ? null : xmlDoc.SelectSingleNode("//OrderRequestHeader/ShipTo/Address/Phone/TelephoneNumber/CountryCode").InnerText);
                    }
                else
                    {
                    LogError("//OrderRequestHeader/ShipTo/Address/Phone/TelephoneNumber/CountryCode");
                    }

                if (xmlDoc.SelectSingleNode("//OrderRequestHeader/ShipTo/Address/Phone/TelephoneNumber/AreaOrCityCode") != null)
                    {
                    PhoneAreaorCityCode = int.Parse((xmlDoc.SelectSingleNode("//OrderRequestHeader/ShipTo/Address/Phone/TelephoneNumber/AreaOrCityCode").InnerText == null) ? null : xmlDoc.SelectSingleNode("//OrderRequestHeader/ShipTo/Address/Phone/TelephoneNumber/AreaOrCityCode").InnerText);
                    }
                else
                    {
                    LogError("//OrderRequestHeader/ShipTo/Address/Phone/TelephoneNumber/AreaOrCityCode");
                    }

                if (xmlDoc.SelectSingleNode("//OrderRequestHeader/ShipTo/Address/Phone/TelephoneNumber/Number") != null)
                    {
                    PhoneNumber = (xmlDoc.SelectSingleNode("//OrderRequestHeader/ShipTo/Address/Phone/TelephoneNumber/Number").InnerText == null) ? null : xmlDoc.SelectSingleNode("//OrderRequestHeader/ShipTo/Address/Phone/TelephoneNumber/Number").InnerText;
                    }
                else
                    {
                    LogError("//OrderRequestHeader/ShipTo/Address/Phone/TelephoneNumber/Number");
                    }

                if (xmlDoc.SelectSingleNode("//OrderRequestHeader/ShipTo/Address/Fax/TelephoneNumber/CountryCode") != null)
                    {
                    FaxCountryCode = int.Parse((xmlDoc.SelectSingleNode("//OrderRequestHeader/ShipTo/Address/Fax/TelephoneNumber/CountryCode").InnerText == null) ? null : xmlDoc.SelectSingleNode("//OrderRequestHeader/ShipTo/Address/Fax/TelephoneNumber/CountryCode").InnerText);
                    }
                else
                    {
                    LogError("//OrderRequestHeader/ShipTo/Address/Fax/TelephoneNumber/CountryCode");
                    }

                if (xmlDoc.SelectSingleNode("//OrderRequestHeader/ShipTo/Address/Fax/TelephoneNumber/AreaOrCityCode") != null)
                    {
                    FaxAreaorCityCode = int.Parse((xmlDoc.SelectSingleNode("//OrderRequestHeader/ShipTo/Address/Fax/TelephoneNumber/AreaOrCityCode").InnerText == null) ? null : xmlDoc.SelectSingleNode("//OrderRequestHeader/ShipTo/Address/Fax/TelephoneNumber/AreaOrCityCode").InnerText);
                    }
                else
                    {
                    LogError("//OrderRequestHeader/ShipTo/Address/Fax/TelephoneNumber/AreaOrCityCode");
                    }

                if (xmlDoc.SelectSingleNode("//OrderRequestHeader/ShipTo/Address/Fax/TelephoneNumber/Number") != null)
                    {
                    FaxNumber = (xmlDoc.SelectSingleNode("//OrderRequestHeader/ShipTo/Address/Fax/TelephoneNumber/Number").InnerText == null) ? null : xmlDoc.SelectSingleNode("//OrderRequestHeader/ShipTo/Address/Fax/TelephoneNumber/Number").InnerText;
                    }
                else
                    {
                    LogError("//OrderRequestHeader/ShipTo/Address/Fax/TelephoneNumber/Number");
                    }

                XmlNode idreference = xmlDoc.SelectSingleNode("//OrderRequestHeader/ShipTo/IdReference");
                if (xmlDoc.SelectSingleNode("//OrderRequestHeader/ShipTo/IdReference") != null)
                    {
                    IdreferenceDomain = (idreference.Attributes["domain"].Value == null) ? null : idreference.Attributes["domain"].Value;
                    IdReferenceIdentifier = (idreference.Attributes["identifier"].Value == null) ? null : idreference.Attributes["identifier"].Value;
                    }



                CreatedDate = DateTime.Now.ToString("yyyy-MM-dd");
                CatalogueSeqID = getCatalogueID(OrderNumber);
                InsertIntoOrderRequestShipToTable(CatalogueSeqID, OrderNumber, CountryCode, AddressIDDomain, AddressID, Name, PostalAddressName, Street1, Street2, Street3, City, State, PostalCode, PhoneCountryCode, PhoneAreaorCityCode, PhoneNumber, FaxCountryCode, FaxAreaorCityCode, FaxNumber, IdreferenceDomain,             IdReferenceIdentifier, CreatedDate);
                }
            catch (Exception ex)
                {
                ExceptionLogging.SendExcepToDB(ex, "CreateStaplesInvoiceFiles");
                }
         


            }


        public void InsertIntoOrderRequestShipToTable(int _CatalogueSeqID,        string _OrderNumber,        string _CountryCode,            string _AddressIDDomain,  string _AddressID,                                                     string _Name,               string _PostalAddressName,  string _Street1,                string _Street2,          string _Street3,                                                       string _City,               string _State,              string _PostalCode,             int _PhoneCountryCode,          
                                                      int _PhoneAreaorCityCode,   string _PhoneNumber,        int _FaxCountryCode,            int _FaxAreaorCityCode,     
                                                      string _FaxNumber,          string _IdreferenceDomain,  string _IdReferenceIdentifier,  string _CreatedDate)
            {

            try
                {

                string insertIntotblOrderHeaderData = ConfigurationManager.AppSettings["insertIntotblOrderRequestShipToData"].ToString();
                string query = insertIntotblOrderHeaderData;

                // create connection and command
                using (SqlConnection cn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(query, cn))
                    {
                    // define parameters and their values
                    cmd.Parameters.AddWithValue("@Catalogue_SeqID", _CatalogueSeqID);
                    cmd.Parameters.AddWithValue("@OrderNumber", _OrderNumber);
                    cmd.Parameters.AddWithValue("@CountryCode", _CountryCode);
                    cmd.Parameters.AddWithValue("@AddressIDDomain", _AddressIDDomain);
                    cmd.Parameters.AddWithValue("@AddressID", _AddressID);
                    cmd.Parameters.AddWithValue("@Name", _Name);
                    cmd.Parameters.AddWithValue("@PostalAddressName", _PostalAddressName);
                    cmd.Parameters.AddWithValue("@Street1", _Street1);
                    cmd.Parameters.AddWithValue("@Street2", _Street2);
                    cmd.Parameters.AddWithValue("@Street3", _Street3);
                    cmd.Parameters.AddWithValue("@City", _City);
                    cmd.Parameters.AddWithValue("@State", _State);
                    cmd.Parameters.AddWithValue("@PostalCode", _PostalCode);
                    cmd.Parameters.AddWithValue("@PhoneCountryCode", _PhoneCountryCode);
                    cmd.Parameters.AddWithValue("@PhoneAreaorCityCode", _PhoneAreaorCityCode);
                    cmd.Parameters.AddWithValue("@PhoneNumber", _PhoneNumber);
                    cmd.Parameters.AddWithValue("@FaxCountryCode", _FaxCountryCode);
                    cmd.Parameters.AddWithValue("@FaxAreaorCityCode", _FaxAreaorCityCode);
                    cmd.Parameters.AddWithValue("@FaxNumber", _FaxNumber);
                    cmd.Parameters.AddWithValue("@IdreferenceDomain", _IdreferenceDomain);
                    cmd.Parameters.AddWithValue("@IdReferenceIdentifier", _IdReferenceIdentifier);
                    cmd.Parameters.AddWithValue("@CreatedDate", _CreatedDate);

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
                 
                ExceptionLogging.SendExcepToDB(ex, "CreateStaplesInvoiceFiles");
                }
            }



        public void InsertOrderRequestBillToData(XmlDocument xmlDoc)
            {
            try
                {
                #region  Order Header data variables

                int CatalogueSeqID = 0;
                string OrderNumber = null;
                string CountryCode = null;
                string Name = null;
                string PostalAddressName = null;
                string Street1 = null;
                string Street2 = null;
                string Street3 = null;
                string City = null;
                string State = null;
                string PostalCode = null;
                int PhoneCountryCode = 0;
                int PhoneAreaorCityCode = 0;
                string PhoneNumber = null;
                int FaxCountryCode = 0;
                int FaxAreaorCityCode = 0;
                string FaxNumber = null;
                string IdreferenceDomain = null;
                string IdReferenceIdentifier = null;
                string CreatedDate = null;

                #endregion Order Header data variables

                XmlNode orh = xmlDoc.SelectSingleNode("//OrderRequestHeader");
                if (xmlDoc.SelectSingleNode("//OrderRequestHeader") != null)
                    {
                    OrderNumber = (orh.Attributes["orderID"].Value == null) ? null : orh.Attributes["orderID"].Value;
                    }

                XmlNode countryCode = xmlDoc.SelectSingleNode("//OrderRequestHeader/BillTo/Address");
                if (xmlDoc.SelectSingleNode("//OrderRequestHeader/BillTo/Address") != null)
                    {
                    CountryCode = (countryCode.Attributes["isoCountryCode"].Value == null) ? null : countryCode.Attributes["isoCountryCode"].Value;
                    }


                if (xmlDoc.SelectSingleNode("//OrderRequestHeader/BillTo/Address/Name") != null)
                    {
                    Name = (xmlDoc.SelectSingleNode("//OrderRequestHeader/BillTo/Address/Name").InnerText == null) ? null : xmlDoc.SelectSingleNode("//OrderRequestHeader/BillTo/Address/Name").InnerText;
                    }
                else
                    {
                    LogError("//OrderRequestHeader/BillTo/Address/Name");
                    }

                XmlNode postalAddress = xmlDoc.SelectSingleNode("//OrderRequestHeader/BillTo/Address/PostalAddress");
                if (xmlDoc.SelectSingleNode("//OrderRequestHeader/BillTo/Address/PostalAddress") != null)
                    {
                    PostalAddressName = (postalAddress.Attributes["name"].Value == null) ? null : postalAddress.Attributes["name"].Value;
                    }

                XmlNodeList streetlist = postalAddress.SelectNodes("//OrderRequestHeader/BillTo/Address/PostalAddress/Street");
                if (streetlist.Count == 1)
                    {

                    if (streetlist != null)
                        {
                        for (int i = 0; i < streetlist.Count; i++)
                            {
                            Street1 = streetlist[0].InnerXml;


                            }
                        }
                    }

                if (streetlist.Count == 2)
                    {

                    if (streetlist != null)
                        {
                        for (int i = 0; i < streetlist.Count; i++)
                            {
                            Street1 = streetlist[0].InnerXml;
                            Street2 = streetlist[1].InnerXml;

                            }
                        }
                    }

                if (streetlist.Count == 3)
                    {

                    if (streetlist != null)
                        {
                        for (int i = 0; i < streetlist.Count; i++)
                            {
                            Street1 = streetlist[0].InnerXml;
                            Street2 = streetlist[1].InnerXml;
                            Street3 = streetlist[2].InnerXml;

                            }
                        }
                    }

                if (xmlDoc.SelectSingleNode("//OrderRequestHeader/BillTo/Address/PostalAddress/City") != null)
                    {
                    City = (xmlDoc.SelectSingleNode("//OrderRequestHeader/BillTo/Address/PostalAddress/City").InnerText == null) ? null : xmlDoc.SelectSingleNode("//OrderRequestHeader/BillTo/Address/PostalAddress/City").InnerText;
                    }
                else
                    {
                    LogError("//OrderRequestHeader/BillTo/Address/PostalAddress/City");
                    }

                if (xmlDoc.SelectSingleNode("//OrderRequestHeader/BillTo/Address/PostalAddress/State") != null)
                    {
                    State = (xmlDoc.SelectSingleNode("//OrderRequestHeader/BillTo/Address/PostalAddress/State").InnerText == null) ? null : xmlDoc.SelectSingleNode("//OrderRequestHeader/BillTo/Address/PostalAddress/State").InnerText;
                    }
                else
                    {
                    LogError("//OrderRequestHeader/BillTo/Address/PostalAddress/State");
                    }

                if (xmlDoc.SelectSingleNode("//OrderRequestHeader/BillTo/Address/PostalAddress/PostalCode") != null)
                    {
                    PostalCode = (xmlDoc.SelectSingleNode("//OrderRequestHeader/BillTo/Address/PostalAddress/PostalCode").InnerText == null) ? null : xmlDoc.SelectSingleNode("//OrderRequestHeader/BillTo/Address/PostalAddress/PostalCode").InnerText;
                    }
                else
                    {
                    LogError("//OrderRequestHeader/BillTo/Address/PostalAddress/PostalCode");
                    }

                if (xmlDoc.SelectSingleNode("//OrderRequestHeader/BillTo/Address/Phone/TelephoneNumber/CountryCode") != null)
                    {
                    PhoneCountryCode = int.Parse((xmlDoc.SelectSingleNode("//OrderRequestHeader/BillTo/Address/Phone/TelephoneNumber/CountryCode").InnerText == null) ? null : xmlDoc.SelectSingleNode("//OrderRequestHeader/BillTo/Address/Phone/TelephoneNumber/CountryCode").InnerText);
                    }
                else
                    {
                    LogError("//OrderRequestHeader/BillTo/Address/Phone/TelephoneNumber/CountryCode");
                    }

                if (xmlDoc.SelectSingleNode("//OrderRequestHeader/BillTo/Address/Phone/TelephoneNumber/AreaOrCityCode") != null)
                    {
                    PhoneAreaorCityCode = int.Parse((xmlDoc.SelectSingleNode("//OrderRequestHeader/BillTo/Address/Phone/TelephoneNumber/AreaOrCityCode").InnerText == null) ? null : xmlDoc.SelectSingleNode("//OrderRequestHeader/BillTo/Address/Phone/TelephoneNumber/AreaOrCityCode").InnerText);
                    }
                else
                    {
                    LogError("//OrderRequestHeader/BillTo/Address/Phone/TelephoneNumber/AreaOrCityCode");
                    }

                if (xmlDoc.SelectSingleNode("//OrderRequestHeader/BillTo/Address/Phone/TelephoneNumber/Number") != null)
                    {
                    PhoneNumber = (xmlDoc.SelectSingleNode("//OrderRequestHeader/BillTo/Address/Phone/TelephoneNumber/Number").InnerText == null) ? null : xmlDoc.SelectSingleNode("//OrderRequestHeader/BillTo/Address/Phone/TelephoneNumber/Number").InnerText;
                    }
                else
                    {
                    LogError("//OrderRequestHeader/BillTo/Address/Phone/TelephoneNumber/Number");
                    }

                if (xmlDoc.SelectSingleNode("//OrderRequestHeader/BillTo/Address/Fax/TelephoneNumber/CountryCode") != null)
                    {
                    FaxCountryCode = int.Parse((xmlDoc.SelectSingleNode("//OrderRequestHeader/BillTo/Address/Fax/TelephoneNumber/CountryCode").InnerText == null) ? null : xmlDoc.SelectSingleNode("//OrderRequestHeader/BillTo/Address/Fax/TelephoneNumber/CountryCode").InnerText);
                    }
                else
                    {
                    LogError("//OrderRequestHeader/BillTo/Address/Fax/TelephoneNumber/CountryCode");
                    }

                if (xmlDoc.SelectSingleNode("//OrderRequestHeader/BillTo/Address/Fax/TelephoneNumber/AreaOrCityCode") != null)
                    {
                    FaxAreaorCityCode = int.Parse((xmlDoc.SelectSingleNode("//OrderRequestHeader/BillTo/Address/Fax/TelephoneNumber/AreaOrCityCode").InnerText == null) ? null : xmlDoc.SelectSingleNode("//OrderRequestHeader/BillTo/Address/Fax/TelephoneNumber/AreaOrCityCode").InnerText);
                    }
                else
                    {
                    LogError("//OrderRequestHeader/BillTo/Address/Fax/TelephoneNumber/AreaOrCityCode");
                    }

                if (xmlDoc.SelectSingleNode("//OrderRequestHeader/BillTo/Address/Fax/TelephoneNumber/Number") != null)
                    {
                    FaxNumber = (xmlDoc.SelectSingleNode("//OrderRequestHeader/BillTo/Address/Fax/TelephoneNumber/Number").InnerText == null) ? null : xmlDoc.SelectSingleNode("//OrderRequestHeader/BillTo/Address/Fax/TelephoneNumber/Number").InnerText;
                    }
                else
                    {
                    LogError("//OrderRequestHeader/BillTo/Address/Fax/TelephoneNumber/Number");
                    }

                XmlNode idreference = xmlDoc.SelectSingleNode("//OrderRequestHeader/BillTo/IdReference");
                if (xmlDoc.SelectSingleNode("//OrderRequestHeader/BillTo/IdReference") != null)
                    {
                    IdreferenceDomain = (idreference.Attributes["domain"].Value == null) ? null : idreference.Attributes["domain"].Value;
                    IdReferenceIdentifier = (idreference.Attributes["identifier"].Value == null) ? null : idreference.Attributes["identifier"].Value;
                    }



                CreatedDate = DateTime.Now.ToString("yyyy-MM-dd");
                CatalogueSeqID = getCatalogueID(OrderNumber);
                InsertIntoOrderRequestBillToTable(CatalogueSeqID, OrderNumber, CountryCode, Name, PostalAddressName, Street1, Street2, Street3, City, State, PostalCode, PhoneCountryCode, PhoneAreaorCityCode, PhoneNumber, FaxCountryCode, FaxAreaorCityCode, FaxNumber, IdreferenceDomain,             IdReferenceIdentifier, CreatedDate);

                }
            catch (Exception ex)
                {
                ExceptionLogging.SendExcepToDB(ex, "CreateStaplesInvoiceFiles");
                }

         

            }


        public void InsertIntoOrderRequestBillToTable(int _CatalogueSeqID, string _OrderNumber, string _CountryCode, string _Name, string _PostalAddressName, string _Street1, string _Street2,                                                string _Street3,     string _City,        string _State,       string _PostalCode,      int _PhoneCountryCode,
                                                      int _PhoneAreaorCityCode, string _PhoneNumber, int _FaxCountryCode, int _FaxAreaorCityCode,
                                                      string _FaxNumber, string _IdreferenceDomain, string _IdReferenceIdentifier, string _CreatedDate)
            {

            try
                {

                string insertIntotblOrderRequestBillToData = ConfigurationManager.AppSettings["insertIntotblOrderRequestBillToData"].ToString();
                string query = insertIntotblOrderRequestBillToData;

                // create connection and command
                using (SqlConnection cn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(query, cn))
                    {
                    // define parameters and their values
                    cmd.Parameters.AddWithValue("@Catalogue_SeqID", _CatalogueSeqID);
                    cmd.Parameters.AddWithValue("@OrderNumber", _OrderNumber);
                    cmd.Parameters.AddWithValue("@CountryCode", _CountryCode);
                    cmd.Parameters.AddWithValue("@Name", _Name);
                    cmd.Parameters.AddWithValue("@PostalAddressName", _PostalAddressName);
                    cmd.Parameters.AddWithValue("@Street1", _Street1);
                    cmd.Parameters.AddWithValue("@Street2", _Street2);
                    cmd.Parameters.AddWithValue("@Street3", _Street3);
                    cmd.Parameters.AddWithValue("@City", _City);
                    cmd.Parameters.AddWithValue("@State", _State);
                    cmd.Parameters.AddWithValue("@PostalCode", _PostalCode);
                    cmd.Parameters.AddWithValue("@PhoneCountryCode", _PhoneCountryCode);
                    cmd.Parameters.AddWithValue("@PhoneAreaorCityCode", _PhoneAreaorCityCode);
                    cmd.Parameters.AddWithValue("@PhoneNumber", _PhoneNumber);
                    cmd.Parameters.AddWithValue("@FaxCountryCode", _FaxCountryCode);
                    cmd.Parameters.AddWithValue("@FaxAreaorCityCode", _FaxAreaorCityCode);
                    cmd.Parameters.AddWithValue("@FaxNumber", _FaxNumber);
                    cmd.Parameters.AddWithValue("@IdreferenceDomain", _IdreferenceDomain);
                    cmd.Parameters.AddWithValue("@IdReferenceIdentifier", _IdReferenceIdentifier);
                    cmd.Parameters.AddWithValue("@CreatedDate", _CreatedDate);

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
                 
                ExceptionLogging.SendExcepToDB(ex, "CreateStaplesInvoiceFiles");
                }
            }


        public void InsertOrderRequestSupplierContactData(XmlDocument xmlDoc)
            {

            try
                {
                #region  Order Header data variables

                int CatalogueSeqID = 0;
                string OrderNumber = null;
                string Role = null;
                string AddressIDDomain = null;
                string AddressID = null;
                string Name = null;
                string Street = null;
                string City = null;
                string State = null;
                string PostalCode = null;
                string CountryCode = null;
                string EmailName = null;
                string Email = null;
                int PhoneCountryCode = 0;
                int PhoneAreaorCityCode = 0;
                string PhoneNumber = null;
                int FaxCountryCode = 0;
                int FaxAreaorCityCode = 0;
                string FaxNumber = null;
                string IdreferenceDomain = null;
                string ReferenceID = null;
                string CreatedDate = null;

                #endregion Order Header data variables

                XmlNode orh = xmlDoc.SelectSingleNode("//OrderRequestHeader");
                if (xmlDoc.SelectSingleNode("//OrderRequestHeader") != null)
                    {
                    OrderNumber = (orh.Attributes["orderID"].Value == null) ? null : orh.Attributes["orderID"].Value;
                    }

                XmlNode contactRoleSupp = xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact");
                Role = (contactRoleSupp.Attributes["role"].Value == null) ? null : contactRoleSupp.Attributes["role"].Value;

                if (Role == "supplierCorporate")
                    {

                    if (xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='supplierCorporate']") != null)
                        {
                        AddressIDDomain = (contactRoleSupp.Attributes["addressIDDomain"].Value == null) ? null : contactRoleSupp.Attributes["addressIDDomain"].Value;
                        AddressID = (contactRoleSupp.Attributes["addressID"].Value == null) ? null : contactRoleSupp.Attributes["addressID"].Value;
                        }

                    if (xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='supplierCorporate']/Name") != null)
                        {
                        Name = (xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='supplierCorporate']/Name").InnerText == null) ? null : xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='supplierCorporate']/Name").InnerText;
                        }
                    else
                        {
                        LogError("//OrderRequestHeader/Contact[@role='supplierCorporate']/Name");
                        }

                    if (xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='supplierCorporate']/PostalAddress/Street") != null)
                        {
                        Street = (xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='supplierCorporate']/PostalAddress/Street").InnerText == null) ? null : xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='supplierCorporate']/PostalAddress/Street").InnerText;
                        }
                    else
                        {
                        LogError("//OrderRequestHeader/Contact[@role='supplierCorporate']/PostalAddress/Street");
                        }

                    if (xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='supplierCorporate']/PostalAddress/City") != null)
                        {
                        City = (xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='supplierCorporate']/PostalAddress/City").InnerText == null) ? null : xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='supplierCorporate']/PostalAddress/City").InnerText;
                        }
                    else
                        {
                        LogError("//OrderRequestHeader/Contact[@role='supplierCorporate']/PostalAddress/City");
                        }

                    if (xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='supplierCorporate']/PostalAddress/State") != null)
                        {
                        State = (xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='supplierCorporate']/PostalAddress/State").InnerText == null) ? null : xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='supplierCorporate']/PostalAddress/State").InnerText;
                        }
                    else
                        {
                        LogError("//OrderRequestHeader/Contact[@role='supplierCorporate']/PostalAddress/State");
                        }

                    if (xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='supplierCorporate']/PostalAddress/PostalCode") != null)
                        {
                        PostalCode = (xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='supplierCorporate']/PostalAddress/PostalCode").InnerText == null) ? null : xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='supplierCorporate']/PostalAddress/PostalCode").InnerText;
                        }
                    else
                        {
                        LogError("//OrderRequestHeader/Contact[@role='supplierCorporate']/PostalAddress/PostalCode");
                        }

                    if (xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='supplierCorporate']/PostalAddress/Country[@isoCountryCode]") != null)
                        {
                        CountryCode = (xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='supplierCorporate']/PostalAddress/Country[@isoCountryCode]").Attributes["isoCountryCode"].Value == null) ? null : xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='supplierCorporate']/PostalAddress/Country[@isoCountryCode]").Attributes["isoCountryCode"].Value;
                        }
                    else
                        {
                        LogError("//OrderRequestHeader/Contact[@role='supplierCorporate']/PostalAddress/Country[@isoCountryCode]");
                        }

                    XmlNode orhEmailName = xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='supplierCorporate']/Email");
                    if (xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='supplierCorporate']/Email") != null)
                        {
                        EmailName = (orhEmailName.Attributes["name"].Value == null) ? null : orhEmailName.Attributes["name"].Value;
                        }
                    else
                        {
                        LogError("//OrderRequestHeader/Contact[@role='supplierCorporate']/Email");
                        }

                    if (xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='supplierCorporate']/Email") != null)
                        {
                        Email = (xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='supplierCorporate']/Email").InnerText == null) ? null : xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='supplierCorporate']/Email").InnerText;
                        }
                    else
                        {
                        LogError("//OrderRequestHeader/Contact[@role='supplierCorporate']/Email");
                        }

                    if (xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='supplierCorporate']/Phone/TelephoneNumber/CountryCode") != null)
                        {
                        PhoneCountryCode = int.Parse((xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='supplierCorporate']/Phone/TelephoneNumber/CountryCode").InnerText == null) ? null : xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='supplierCorporate']/Phone/TelephoneNumber/CountryCode").InnerText);
                        }
                    else
                        {
                        LogError("//OrderRequestHeader/Contact[@role='supplierCorporate']/Phone/TelephoneNumber/CountryCode");
                        }

                    if (xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='supplierCorporate']/Phone/TelephoneNumber/AreaOrCityCode") != null)
                        {
                        PhoneAreaorCityCode = int.Parse((xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='supplierCorporate']/Phone/TelephoneNumber/AreaOrCityCode").InnerText == null) ? null : xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='supplierCorporate']/Phone/TelephoneNumber/AreaOrCityCode").InnerText);
                        }
                    else
                        {
                        LogError("//OrderRequestHeader/Contact[@role='supplierCorporate']/Phone/TelephoneNumber/AreaOrCityCode");
                        }

                    if (xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='supplierCorporate']/Phone/TelephoneNumber/Number") != null)
                        {
                        PhoneNumber = (xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='supplierCorporate']/Phone/TelephoneNumber/Number").InnerText == null) ? null : xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='supplierCorporate']/Phone/TelephoneNumber/Number").InnerText;
                        }
                    else
                        {
                        LogError("//OrderRequestHeader/Contact[@role='supplierCorporate']/Phone/TelephoneNumber/Number");
                        }

                    if (xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='supplierCorporate']/Fax/TelephoneNumber/CountryCode") != null)
                        {
                        FaxCountryCode = int.Parse((xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='supplierCorporate']/Fax/TelephoneNumber/CountryCode").InnerText == null) ? null : xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='supplierCorporate']/Fax/TelephoneNumber/CountryCode").InnerText);
                        }
                    else
                        {
                        LogError("//OrderRequestHeader/Contact[@role='supplierCorporate']/Fax/TelephoneNumber/CountryCode");
                        }


                    if (xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='supplierCorporate']/Fax/TelephoneNumber/AreaOrCityCode") != null)
                        {
                        FaxAreaorCityCode = int.Parse((xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='supplierCorporate']/Fax/TelephoneNumber/AreaOrCityCode").InnerText == null) ? null : xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='supplierCorporate']/Fax/TelephoneNumber/AreaOrCityCode").InnerText);
                        }
                    else
                        {
                        LogError("//OrderRequestHeader/Contact[@role='supplierCorporate']/Fax/TelephoneNumber/AreaOrCityCode");
                        }

                    if (xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='supplierCorporate']/Fax/TelephoneNumber/Number") != null)
                        {
                        FaxNumber = (xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='supplierCorporate']/Fax/TelephoneNumber/Number").InnerText == null) ? null : xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='supplierCorporate']/Fax/TelephoneNumber/Number").InnerText;
                        }
                    else
                        {
                        LogError("//OrderRequestHeader/Contact[@role='supplierCorporate']/Fax/TelephoneNumber/Number");
                        }


                    XmlNode idreference = xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='supplierCorporate']/IdReference");
                    if (xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='supplierCorporate']/IdReference") != null)
                        {
                        IdreferenceDomain = (idreference.Attributes["domain"].Value == null) ? null : idreference.Attributes["domain"].Value;
                        ReferenceID = (idreference.Attributes["identifier"].Value == null) ? null : idreference.Attributes["identifier"].Value;
                        }
                    }

                CreatedDate = DateTime.Now.ToString("yyyy-MM-dd");
                CatalogueSeqID = getCatalogueID(OrderNumber);
                InsertIntoOrderRequestSupplierContactTable(CatalogueSeqID, OrderNumber, Role, AddressIDDomain, AddressID, Name, Street, City, State, PostalCode, CountryCode, EmailName, Email, PhoneCountryCode, PhoneAreaorCityCode, PhoneNumber, FaxCountryCode, FaxAreaorCityCode, FaxNumber, IdreferenceDomain,             ReferenceID, CreatedDate);
                }
            catch (Exception ex)
                {
                ExceptionLogging.SendExcepToDB(ex, "CreateStaplesInvoiceFiles");
                }
           


            }


        public void InsertIntoOrderRequestSupplierContactTable(int _CatalogueSeqID, string _OrderNumber,string _Role,string _AddressIDDomain,string _AddressID, string _Name, string _Street,  string _City, string _State, string _PostalCode,string _CountryCode, string _EmailName, string _Email, int _PhoneCountryCode,
                                                      int _PhoneAreaorCityCode, string _PhoneNumber, int _FaxCountryCode, int _FaxAreaorCityCode,
                                                      string _FaxNumber, string _IdreferenceDomain, string _ReferenceID, string _CreatedDate)
            {

            try
                {

                string insertIntotblOrderRequestBillToData = ConfigurationManager.AppSettings["insertIntotblOrderRequestSupplierContactData"].ToString();
                string query = insertIntotblOrderRequestBillToData;

                // create connection and command
                using (SqlConnection cn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(query, cn))
                    {
                    // define parameters and their values
                    cmd.Parameters.AddWithValue("@Catalogue_SeqID", _CatalogueSeqID);
                    cmd.Parameters.AddWithValue("@OrderNumber", _OrderNumber);
                    cmd.Parameters.AddWithValue("@Role", _Role);
                    cmd.Parameters.AddWithValue("@AddressIDDomain", _AddressIDDomain);
                    cmd.Parameters.AddWithValue("@AddressID", _AddressID);
                    cmd.Parameters.AddWithValue("@Name", _Name);
                    cmd.Parameters.AddWithValue("@Street1", _Street);
                    cmd.Parameters.AddWithValue("@City", _City);
                    cmd.Parameters.AddWithValue("@State", _State);
                    cmd.Parameters.AddWithValue("@PostalCode", _PostalCode);
                    cmd.Parameters.AddWithValue("@CountryCode", _CountryCode);
                    cmd.Parameters.AddWithValue("@EmailName", _EmailName);
                    cmd.Parameters.AddWithValue("@Email", _Email);
                    cmd.Parameters.AddWithValue("@PhoneCountryCode", _PhoneCountryCode);
                    cmd.Parameters.AddWithValue("@PhoneAreaorCityCode", _PhoneAreaorCityCode);
                    cmd.Parameters.AddWithValue("@PhoneNumber", _PhoneNumber);
                    cmd.Parameters.AddWithValue("@FaxCountryCode", _FaxCountryCode);
                    cmd.Parameters.AddWithValue("@FaxAreaorCityCode", _FaxAreaorCityCode);
                    cmd.Parameters.AddWithValue("@FaxNumber", _FaxNumber);
                    cmd.Parameters.AddWithValue("@IdreferenceDomain", _IdreferenceDomain);
                    cmd.Parameters.AddWithValue("@IdReferenceIdentifier", _ReferenceID);
                    cmd.Parameters.AddWithValue("@CreatedDate", _CreatedDate);

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
                 
                ExceptionLogging.SendExcepToDB(ex, "CreateStaplesInvoiceFiles");
                }
            }

        

        public void InsertOrderRequestPurchaserContactData(XmlDocument xmlDoc)
            {

            try
                {
                #region  Order Header data variables

                int CatalogueSeqID = 0;
                string OrderNumber = null;
                string Role = null;
                string Name = null;
                string Email = null;
                int PhoneCountryCode = 0;
                int PhoneAreaorCityCode = 0;
                string PhoneNumber = null;
                string ContactRole = null;
                string ContactRole_Name = null;
                string CreatedDate = null;

                #endregion Order Header data variables

                XmlNode orh = xmlDoc.SelectSingleNode("//OrderRequestHeader");
                if (xmlDoc.SelectSingleNode("//OrderRequestHeader") != null)
                    {
                    OrderNumber = (orh.Attributes["orderID"].Value == null) ? null : orh.Attributes["orderID"].Value;
                    }

                XmlNode contactRoleSupp = xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='purchasingAgent']");
                Role = (contactRoleSupp.Attributes["role"].Value == null) ? null : contactRoleSupp.Attributes["role"].Value;

                if (Role == "purchasingAgent")
                    {


                    if (xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='purchasingAgent']/Name") != null)
                        {
                        Name = (xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='purchasingAgent']/Name").InnerText == null) ? null : xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='purchasingAgent']/Name").InnerText;
                        }
                    else
                        {
                        LogError("//OrderRequestHeader/Contact[@role='purchasingAgent']/Name");
                        }

                    if (xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='purchasingAgent']/Email") != null)
                        {
                        Email = (xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='purchasingAgent']/Email").InnerText == null) ? null : xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='purchasingAgent']/Email").InnerText;
                        }
                    else
                        {
                        LogError("//OrderRequestHeader/Contact[@role='purchasingAgent']/Email");
                        }

                    if (xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='purchasingAgent']/Phone/TelephoneNumber/CountryCode") != null)
                        {
                        PhoneCountryCode = int.Parse((xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='purchasingAgent']/Phone/TelephoneNumber/CountryCode").InnerText == null) ? null : xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='purchasingAgent']/Phone/TelephoneNumber/CountryCode").InnerText);
                        }
                    else
                        {
                        LogError("//OrderRequestHeader/Contact[@role='purchasingAgent']/Phone/TelephoneNumber/CountryCode");
                        }

                    if (xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='purchasingAgent']/Phone/TelephoneNumber/AreaOrCityCode").InnerText != "")
                        {
                        PhoneAreaorCityCode = int.Parse((xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='purchasingAgent']/Phone/TelephoneNumber/AreaOrCityCode").InnerText == null) ? null : xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='purchasingAgent']/Phone/TelephoneNumber/AreaOrCityCode").InnerText);
                        }
                    else
                        {
                        LogError("//OrderRequestHeader/Contact[@role='purchasingAgent']/Phone/TelephoneNumber/AreaOrCityCode");
                        }

                    if (xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='purchasingAgent']/Phone/TelephoneNumber/Number") != null)
                        {
                        PhoneNumber = (xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='purchasingAgent']/Phone/TelephoneNumber/Number").InnerText == null) ? null : xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='purchasingAgent']/Phone/TelephoneNumber/Number").InnerText;
                        }
                    else
                        {
                        LogError("//OrderRequestHeader/Contact[@role='purchasingAgent']/Phone/TelephoneNumber/Number");
                        }


                    XmlNode contactRoleSales = xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='sales']");
                    if (xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='sales']") != null)
                        {
                        ContactRole = (contactRoleSupp.Attributes["role"].Value == null) ? null : contactRoleSupp.Attributes["role"].Value;
                        }
                    else
                        {
                        LogError("//OrderRequestHeader/Contact[@role='sales']");
                        }

                    if (xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='sales']/Name") != null)
                        {
                        ContactRole_Name = (xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='sales']/Name").InnerText == null) ? null : xmlDoc.SelectSingleNode("//OrderRequestHeader/Contact[@role='sales']/Name").InnerText;
                        }
                    else
                        {
                        LogError("//OrderRequestHeader/Contact[@role='sales']/Name");
                        }



                    }

                CreatedDate = DateTime.Now.ToString("yyyy-MM-dd");
                CatalogueSeqID = getCatalogueID(OrderNumber);
                InsertIntoOrderRequestPurchaserContactData(CatalogueSeqID, OrderNumber, Role, Name, Email, PhoneCountryCode, PhoneAreaorCityCode, PhoneNumber, ContactRole, ContactRole_Name, CreatedDate);

                }
            catch (Exception ex)
                {
                ExceptionLogging.SendExcepToDB(ex, "CreateStaplesInvoiceFiles");
                }
           

            }


        public void InsertIntoOrderRequestPurchaserContactData(int _CatalogueSeqID, string _OrderNumber, string _Role, string _Name, string _Email, int _PhoneCountryCode,
                                                      int _PhoneAreaorCityCode, string _PhoneNumber,  string _ContactRole, string _ContactRole_Name,  string _CreatedDate)
            {

            try
                {

                string InsertIntoOrderRequestPurchaserContactData = ConfigurationManager.AppSettings["insertIntotblOrderRequestPurchaserContactData"].ToString();
                string query = InsertIntoOrderRequestPurchaserContactData;

                // create connection and command
                using (SqlConnection cn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(query, cn))
                    {
                    // define parameters and their values
                    cmd.Parameters.AddWithValue("@Catalogue_SeqID", _CatalogueSeqID);
                    cmd.Parameters.AddWithValue("@OrderNumber", _OrderNumber);
                    cmd.Parameters.AddWithValue("@Role", _Role);                    
                    cmd.Parameters.AddWithValue("@Name", _Name);
                    cmd.Parameters.AddWithValue("@Email", _Email);
                    cmd.Parameters.AddWithValue("@PhoneCountryCode", _PhoneCountryCode);
                    cmd.Parameters.AddWithValue("@PhoneAreaorCityCode", _PhoneAreaorCityCode);
                    cmd.Parameters.AddWithValue("@PhoneNumber", _PhoneNumber);
                    cmd.Parameters.AddWithValue("@ContactRole", _ContactRole);
                    cmd.Parameters.AddWithValue("@ContactRole_Name", _ContactRole_Name);
                    cmd.Parameters.AddWithValue("@CreatedDate", _CreatedDate);

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
                 
                ExceptionLogging.SendExcepToDB(ex, "CreateStaplesInvoiceFiles");
                }
            }


        
        

             public void InsertOrderRequestItemData(XmlDocument xmlDoc)
            {

            try
                {
                int CatalogueSeqID;
                string OrderNumber;
                string RequestedDeliveryDate;
                string SupplierPartID;
                string BuyerPartID;
                int LineNumber;
                decimal Quantity;
                decimal UnitPrice;
                string ItemDescription;
                string UnitOfMeasure;
                decimal PriceBasisQuantity;
                int ConversionFactor;
                string ClassificationDomain;
                string Classification;
                string ManufacturerPartID;
                string ManufacturerName;
                string EANID;
                string ScheduleLineReqDelivDate;
                decimal ScheduleLineQuantity;
                int ScheduleLineNumber;
                string ScheduleLineUOM;
                string Comments;
                string CreatedDate;


                if (xmlDoc.SelectNodes("//Request/OrderRequest/ItemOut") != null)
                    {
                    XmlNodeList xnList = xmlDoc.SelectNodes("//Request/OrderRequest/ItemOut");

                    foreach (XmlNode xn in xnList)
                        {

                         CatalogueSeqID = 0;
                         OrderNumber = null;
                         RequestedDeliveryDate = null;
                         SupplierPartID = null;
                         BuyerPartID = null;
                         LineNumber = 0;
                         Quantity = 0;
                         UnitPrice = 0;
                         ItemDescription = null;
                         UnitOfMeasure = null;
                         PriceBasisQuantity = 0;
                         ConversionFactor = 0;
                         ClassificationDomain = null;
                         Classification = null;
                         ManufacturerPartID = null;
                         ManufacturerName = null;
                         EANID = null;
                         ScheduleLineReqDelivDate = null;
                         ScheduleLineQuantity = 0;
                         ScheduleLineNumber = 0;
                         ScheduleLineUOM = null;
                         Comments = null;
                         CreatedDate = null;


                        XmlNode orh = xmlDoc.SelectSingleNode("//OrderRequestHeader");
                        if (xmlDoc.SelectSingleNode("//OrderRequestHeader") != null)
                            {
                            OrderNumber = (orh.Attributes["orderID"].Value == null) ? null : orh.Attributes["orderID"].Value;                            
                            }

                        XmlNode orhRDD = xmlDoc.SelectSingleNode("//Request/OrderRequest/ItemOut");
                        if (xn.SelectSingleNode("//Request/OrderRequest/ItemOut") != null)
                            {
                            RequestedDeliveryDate = (orhRDD.Attributes["requestedDeliveryDate"].Value == null) ? null : orhRDD.Attributes["requestedDeliveryDate"].Value;
                            }
                        else
                            {
                            LogError("//Request/OrderRequest/ItemOut/ItemID/SupplierPartID not found");
                            }

                        if (xn.SelectSingleNode("ItemID/SupplierPartID") != null)
                            {
                            SupplierPartID = xn.SelectSingleNode("ItemID/SupplierPartID").InnerText;
                            }
                        else
                            {
                            LogError("//Request/OrderRequest/ItemOut/ItemID/SupplierPartID not found");
                            }

                        if (xn.SelectSingleNode("ItemID/BuyerPartID") != null)
                            {
                            BuyerPartID = xn.SelectSingleNode("ItemID/BuyerPartID").InnerText;
                            }
                        else
                            {
                            LogError("//Request/OrderRequest/ItemOut/ItemID/BuyerPartID not found");
                            }



                        //XmlNode ln = xmlDoc.SelectSingleNode("//Request/OrderRequest/ItemOut");
                        //if (xmlDoc.SelectSingleNode("//Request/OrderRequest/ItemOut") != null)
                        //    {
                        //    LineNumber = int.Parse((xn.Attributes["lineNumber"].Value == null) ? null : ln.Attributes["lineNumber"].Value);
                        //    }


                        if (xn != null)
                            {
                            LineNumber = int.Parse((xn.Attributes["lineNumber"].Value == null) ? null : xn.Attributes["lineNumber"].Value);
                            }




                        if (xn != null)
                            {
                            Quantity = decimal.Parse((xn.Attributes["quantity"].Value == null) ? null : xn.Attributes["quantity"].Value);
                            }

                        if (xmlDoc.SelectSingleNode("//Request/OrderRequest/ItemOut/ItemDetail/UnitPrice") != null)
                            {
                            UnitPrice = decimal.Parse(xn.SelectSingleNode("ItemDetail/UnitPrice").InnerText);
                            }
                        else
                            {
                            LogError("//Request/OrderRequest/ItemOut/ItemDetail/UnitPrice not found");
                            }

                        if (xmlDoc.SelectSingleNode("//Request/OrderRequest/ItemOut/ItemDetail/Description") != null)
                            {
                            ItemDescription = xn.SelectSingleNode("ItemDetail/Description").InnerText;
                            }
                        else
                            {
                            LogError("//Request/OrderRequest/ItemOut/ItemDetail/Description not found");
                            }

                        if (xmlDoc.SelectSingleNode("//Request/OrderRequest/ItemOut/ItemDetail/UnitOfMeasure") != null)
                            {
                            UnitOfMeasure = xn.SelectSingleNode("ItemDetail/UnitOfMeasure").InnerText;
                            }
                        else
                            {
                            LogError("//Request/OrderRequest/ItemOut/ItemDetail/UnitOfMeasure not found");
                            }

                        XmlNode pbq = xmlDoc.SelectSingleNode("//Request/OrderRequest/ItemOut/ItemDetail/PriceBasisQuantity");
                        if (xmlDoc.SelectSingleNode("//Request/OrderRequest/ItemOut/ItemDetail/PriceBasisQuantity") != null)
                            {
                            PriceBasisQuantity = decimal.Parse((pbq.Attributes["quantity"].Value == null) ? null : pbq.Attributes["quantity"].Value);
                            ConversionFactor = int.Parse((pbq.Attributes["conversionFactor"].Value == null) ? null : pbq.Attributes["conversionFactor"].Value);
                            }

                        XmlNode cd = xmlDoc.SelectSingleNode("//Request/OrderRequest/ItemOut/ItemDetail/Classification");
                        if (xmlDoc.SelectSingleNode("//Request/OrderRequest/ItemOut/ItemDetail/Classification") != null)
                            {
                            ClassificationDomain = (cd.Attributes["domain"].Value == null) ? null : cd.Attributes["domain"].Value;
                            }

                        if (xn.SelectSingleNode("ItemDetail/Classification") != null)
                            {
                            Classification = xn.SelectSingleNode("ItemDetail/Classification").InnerText;
                            }
                        else
                            {
                            LogError("//Request/OrderRequest/ItemOut/ItemDetail/Classification not found");
                            }

                        if (xn.SelectSingleNode("ItemDetail/ManufacturerPartID") != null)
                            {
                            ManufacturerPartID = xn.SelectSingleNode("ItemDetail/ManufacturerPartID").InnerText;
                            }
                        else
                            {
                            LogError("//Request/OrderRequest/ItemOut/ItemDetail/ManufacturerPartID not found");
                            }

                        if (xn.SelectSingleNode("ItemDetail/ManufacturerName") != null)
                            {
                            ManufacturerName = xn.SelectSingleNode("ItemDetail/ManufacturerName").InnerText;
                            }
                        else
                            {
                            LogError("//Request/OrderRequest/ItemOut/ItemDetail/ManufacturerName not found");
                            }

                        if (xn.SelectSingleNode("ItemDetail/ItemDetailIndustry/ItemDetailRetail/EANID") != null)
                            {
                            EANID = xn.SelectSingleNode("ItemDetail/ItemDetailIndustry/ItemDetailRetail/EANID").InnerText;
                            }
                        else
                            {
                            LogError("//Request/OrderRequest/ItemOut/ItemDetail/ItemDetailIndustry/ItemDetailRetail/EANID not found");
                            }



                        //XmlNode srd = xn.SelectSingleNode("//Request/OrderRequest/ItemOut/ScheduleLine");
                        XmlNode srd = xn.SelectSingleNode("ScheduleLine");
                        if (xn.SelectSingleNode("ScheduleLine") != null)
                            {
                            ScheduleLineReqDelivDate = (srd.Attributes["requestedDeliveryDate"].Value == null) ? null : srd.Attributes["requestedDeliveryDate"].Value;
                            ScheduleLineReqDelivDate = ScheduleLineReqDelivDate.Substring(0, 10);
                            ScheduleLineQuantity = decimal.Parse((srd.Attributes["quantity"].Value == null) ? null : srd.Attributes["quantity"].Value);
                            ScheduleLineNumber = int.Parse((srd.Attributes["lineNumber"].Value == null) ? null : srd.Attributes["lineNumber"].Value);
                            }





                        if (xn.SelectSingleNode("ScheduleLine/UnitOfMeasure") != null)
                            {
                            ScheduleLineUOM = xn.SelectSingleNode("ScheduleLine/UnitOfMeasure").InnerText;
                            }
                        else
                            {
                            LogError("ItemOut/ScheduleLine/UnitOfMeasure not found");
                            }

                        // Comments = "Testing";

                        if (xn.SelectSingleNode("Comments") != null)
                            {
                            Comments = xn.SelectSingleNode("Comments").InnerText;
                            }
                        else
                            {
                            LogError("Comments not found");
                            }


                        CreatedDate = DateTime.Now.ToString("yyyy-MM-dd");
                        CatalogueSeqID = getCatalogueID(OrderNumber);
                        InsertIntoOrderRequestItemData(CatalogueSeqID, OrderNumber, RequestedDeliveryDate, SupplierPartID, BuyerPartID, LineNumber, Quantity, UnitPrice, ItemDescription,UnitOfMeasure, PriceBasisQuantity, ConversionFactor, ClassificationDomain, Classification, ManufacturerPartID,                                                    ManufacturerName, EANID, ScheduleLineReqDelivDate, ScheduleLineQuantity, ScheduleLineNumber, ScheduleLineUOM, Comments, CreatedDate);

                        }
                    }
                }
            catch (Exception ex)
                {
                ExceptionLogging.SendExcepToDB(ex, "CreateStaplesInvoiceFiles");
                }
            

           

            


            }


        public void InsertIntoOrderRequestItemData(int _CatalogueSeqID, string _OrderNumber, string _RequestedDeliveryDate, string _SupplierPartID, string _BuyerPartID, int _LineNumber, decimal _Quantity, decimal _UnitPrice,
                                                   string _ItemDescription, string _UnitOfMeasure, decimal _PriceBasisQuantity, int _ConversionFactor, string _ClassificationDomain,
                                                   string _Classification,  string _ManufacturerPartID, string _ManufacturerName,string _EANID, string _ScheduleLineReqDelivDate,
                                                   decimal _ScheduleLineQuantity, int _ScheduleLineNumber, string _ScheduleLineUOM, string _Comments, string _CreatedDate)
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
                    cmd.Parameters.AddWithValue("@Catalogue_SeqID", _CatalogueSeqID);
                    cmd.Parameters.AddWithValue("@OrderNumber", _OrderNumber);
                    cmd.Parameters.AddWithValue("@RequestedDeliveryDate", _RequestedDeliveryDate);
                    cmd.Parameters.AddWithValue("@SupplierPartID", _SupplierPartID);
                    cmd.Parameters.AddWithValue("@BuyerPartID", _BuyerPartID);
                    cmd.Parameters.AddWithValue("@LineNumber", _LineNumber);
                    cmd.Parameters.AddWithValue("@Quantity", _Quantity);
                    cmd.Parameters.AddWithValue("@UnitPrice", _UnitPrice);
                    cmd.Parameters.AddWithValue("@ItemDescription", _ItemDescription);
                    cmd.Parameters.AddWithValue("@UnitOfMeasure", _UnitOfMeasure);
                    cmd.Parameters.AddWithValue("@PriceBasisQuantity", _PriceBasisQuantity);
                    cmd.Parameters.AddWithValue("@ConversionFactor", _ConversionFactor);
                    cmd.Parameters.AddWithValue("@ClassificationDomain", _ClassificationDomain);
                    cmd.Parameters.AddWithValue("@Classification", _Classification);
                    cmd.Parameters.AddWithValue("@ManufacturerPartID", _ManufacturerPartID);
                    cmd.Parameters.AddWithValue("@ManufacturerName", _ManufacturerName);
                    cmd.Parameters.AddWithValue("@EANID", _EANID);
                    cmd.Parameters.AddWithValue("@ScheduleLineReqDelivDate", _ScheduleLineReqDelivDate);
                    cmd.Parameters.AddWithValue("@ScheduleLineQuantity", _ScheduleLineQuantity);
                    cmd.Parameters.AddWithValue("@ScheduleLineNumber", _ScheduleLineNumber);
                    cmd.Parameters.AddWithValue("@ScheduleLineUOM", _ScheduleLineUOM);
                    cmd.Parameters.AddWithValue("@Comments", _Comments);
                    cmd.Parameters.AddWithValue("@CreatedDate", _CreatedDate);

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
                 
                ExceptionLogging.SendExcepToDB(ex, "CreateStaplesInvoiceFiles");
                }
            }

        

        public void MoveProcessedFiles(string _fileName)
            {

            try
                {

                }
            catch (Exception ex)
                {
                ExceptionLogging.SendExcepToDB(ex, "CreateStaplesInvoiceFiles");
                }
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

        
        private void LogError(string sExceptionName)
            {
            StreamWriter log;
            string errorLogPath = ConfigurationManager.AppSettings["errorLogPath"].ToString();

            //if (!File.Exists(@"\\TEAPOT\d$\ACROrderService EDI Forward to Members\Spotless_EDI\Order_Log\ErrorLog\ErrorLogfile" + "_" + DateTime.Now.ToString("ddMMYYYY") + ".txt"))
            if (!File.Exists(errorLogPath + "_" + DateTime.Now.ToString("ddMMYYYY") + ".txt"))
                {

                log = new StreamWriter(errorLogPath + "_" + DateTime.Now.ToString("ddMMYYYY") + ".txt");

                }
            else
                {

                log = File.AppendText(errorLogPath + "_" + DateTime.Now.ToString("ddMMYYYY") + ".txt");

                }

            // Write to the file:

            log.WriteLine("Data Time:" + DateTime.Now);
            log.WriteLine("Message:" + sExceptionName);

            // Close the stream:
            log.Close();


            }

        private void UpdateProcessedFlag()
            {
            try
                {

                string sqlQuery = "update tbl_Ord_Catalogue set Processed_Flag = 1 where Processed_Flag = 0 and Validation_Flag = 0 and AcrPush_Flag = 0";
                SqlConnection sqlConn = new SqlConnection(connectionString);
                SqlCommand cmd = new SqlCommand(sqlQuery, sqlConn);
                sqlConn.Open();
                cmd.ExecuteNonQuery();
                sqlConn.Close();

                }
            catch (Exception ex)
                {
                 
                ExceptionLogging.SendExcepToDB(ex, "CreateStaplesInvoiceFiles");
                }
            }



        }
    }
