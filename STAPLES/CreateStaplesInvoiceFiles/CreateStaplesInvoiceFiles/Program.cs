using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Xml;

namespace CreateStaplesInvoiceFiles
    {
    class Program
        {

        XmlWriter xmlWriter = null;
        string connectionString = ConfigurationManager.ConnectionStrings["sqlConn"].ToString();
        string invoiceCreationPath = ConfigurationManager.AppSettings["invoiceCreationPath"].ToString();
        static void Main(string[] args)
            {

            Program thisProgram = new Program();
            int CatalogueSeqID = 0;
            string orderNum = null;

          //  thisProgram.PostInvoice("4501680748", 39);

         //   thisProgram.MoveFiles("4501680748");

          //  thisProgram.UpdateINVFlag("4501680748");

            try
                {
                string invoiceCreationPath = ConfigurationManager.AppSettings["invoiceCreationPath"].ToString();
                thisProgram.xmlWriter = null;

                SqlConnection con = new SqlConnection(thisProgram.connectionString);
                string selectOrders = ConfigurationManager.AppSettings["selectOrders"].ToString();
                string strOrders = selectOrders;
                SqlDataAdapter dtOrders = new SqlDataAdapter(strOrders, con);
                DataSet dsOrders = new DataSet();
                dtOrders.Fill(dsOrders);

                if (dsOrders.Tables[0].Rows.Count == 0)
                    {
                    throw new Exception("NO Data in View_INV_Header, Please check the data and run the exe again");
                    }
              

                for (int i = 0; i < dsOrders.Tables[0].Rows.Count; i++)
                    {

                    orderNum = dsOrders.Tables[0].Rows[i]["OrderNumber"].ToString();


                    #region Payload


                    string OrderNumber = null;
                    string PayloadID = null;
                    string invoiceHeaderPayloadID = null; // This payloadID is for the POC Document
                    string DocRefPayloadID = null; // This payloadID is for the Document reference Payload ID which should match with PO PayloadID
                    string OrderTimeStamp = null;



                    string selectHeaderData = ConfigurationManager.AppSettings["selectHeaderData"].ToString();
                    string strHeader = selectHeaderData + "'" + orderNum + "'";
                    SqlDataAdapter dtHeader = new SqlDataAdapter(strHeader, con);
                    DataSet dsHeader = new DataSet();
                    dtHeader.Fill(dsHeader);

                    

                    DateTime invoiceCreatedDate = DateTime.Now;
                    string convINVOICECreatedDate = String.Format("{0:s}", invoiceCreatedDate);
                    convINVOICECreatedDate = convINVOICECreatedDate + "+11:00";

                    OrderNumber = orderNum;
                    DocRefPayloadID = dsHeader.Tables[0].Rows[0]["PayloadID"].ToString();
                    invoiceHeaderPayloadID = thisProgram.GeneratePaylodID(32);
                    OrderTimeStamp = dsHeader.Tables[0].Rows[0]["NoticeDate"].ToString();
                    CatalogueSeqID = Convert.ToInt32(dsHeader.Tables[0].Rows[0]["Catalogue_SeqID"]);


                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Indent = true;
                    settings.OmitXmlDeclaration = false;



                    thisProgram.xmlWriter = XmlWriter.Create(invoiceCreationPath + OrderNumber + "_INV.xml", settings);

                    thisProgram.xmlWriter.WriteStartDocument();
                    thisProgram.xmlWriter.WriteDocType("cXML", null, "http://xml.cXML.org/schemas/cXML/1.2.035/InvoiceDetail.dtd", null);
                  //  thisProgram.xmlWriter.WriteDocType("cXML", null,  "http://xml.cxml.org/schemas/cXML/1.2.026/InvoiceDetail.dtd", null);
                    thisProgram.xmlWriter.WriteStartElement("cXML");

                    thisProgram.xmlWriter.WriteAttributeString("payloadID", invoiceHeaderPayloadID);
                    thisProgram.xmlWriter.WriteAttributeString("timestamp", convINVOICECreatedDate);
                    thisProgram.xmlWriter.WriteAttributeString("xml", "lang", null, "en-US");

                    #endregion Payload


                    //string xmllang = "xml:lang";,
                    //thisProgram.xmlWriter.WriteAttributeString("xml" + ":" + "lang", "en-US");

                    #region Header Tags

              

                    string FromNetworkID = null;
                    string ToNetworkID = null;
                    string SenderNetworkID = null;
                    string SenderSharedSecret = null;
                    string UserAgent = null;
                    string ConfirmID = null;
                    string InvoiceOrigin = null;
                    string Operation = null;
                    string Purpose = null;
                    string IsTaxInLine = null;

                   

                    OrderNumber = dsHeader.Tables[0].Rows[0]["OrderNumber"].ToString();
                    FromNetworkID = dsHeader.Tables[0].Rows[0]["From_NetworkID"].ToString();
                    ToNetworkID = dsHeader.Tables[0].Rows[0]["To_NetworkID"].ToString();
                    SenderNetworkID = dsHeader.Tables[0].Rows[0]["Sender_NetworkID"].ToString();
                    SenderSharedSecret = dsHeader.Tables[0].Rows[0]["Sender_SharedSecret"].ToString();
                    UserAgent = dsHeader.Tables[0].Rows[0]["Sender_UserAgent"].ToString();
                    ConfirmID = dsHeader.Tables[0].Rows[0]["ConfirmID"].ToString();
                    InvoiceOrigin = dsHeader.Tables[0].Rows[0]["InvoiceOrigin"].ToString();
                    Operation = dsHeader.Tables[0].Rows[0]["Operation"].ToString();
                    Purpose = dsHeader.Tables[0].Rows[0]["Purpose"].ToString();
                    IsTaxInLine = dsHeader.Tables[0].Rows[0]["IsTaxInLine"].ToString();

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
                    thisProgram.xmlWriter.WriteString(SenderNetworkID);
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


                    #endregion

                    

                    DateTime timeStampDate = DateTime.Now;
                    string convTimeStampDate = String.Format("{0:s}", timeStampDate);
                    convTimeStampDate = convTimeStampDate + "+11:00";


                    string selectInvoicePartnerData = ConfigurationManager.AppSettings["selectInvoicePartnerData"].ToString();
                    string strInvoicePartnerData = selectInvoicePartnerData + "'" + orderNum + "'";
                    SqlDataAdapter dtInvoicePartnerData = new SqlDataAdapter(strInvoicePartnerData, con);
                    DataSet dsInvoicePartnerData = new DataSet();
                    dtInvoicePartnerData.Fill(dsInvoicePartnerData);

                    if (dsInvoicePartnerData.Tables[0].Rows.Count == 0)
                        {
                        throw new Exception("NO Data in View_INV_Partner, Please check the data and run the exe again");
                        }


                    string RemitToName = null;
                    string RemitToPostalAddressName = null;
                    string RemitToStreet = null;
                    string RemitToCity = null;
                    string RemitToState = null;
                    string RemitToPostalCode = null;
                    string RemitToCountryCode = null;   

                    RemitToName = dsInvoicePartnerData.Tables[0].Rows[0]["RemitTo_Name"].ToString();
                    RemitToPostalAddressName = dsInvoicePartnerData.Tables[0].Rows[0]["RemitTo_PostalAddressName"].ToString();
                    RemitToStreet = dsInvoicePartnerData.Tables[0].Rows[0]["RemitTo_Street"].ToString();
                    RemitToCity = dsInvoicePartnerData.Tables[0].Rows[0]["RemitTo_City"].ToString();
                    RemitToState = dsInvoicePartnerData.Tables[0].Rows[0]["RemitTo_State"].ToString();
                    RemitToPostalCode = dsInvoicePartnerData.Tables[0].Rows[0]["RemitTo_PostalCode"].ToString();
                    RemitToCountryCode = dsInvoicePartnerData.Tables[0].Rows[0]["RemitTo_Country"].ToString();                   


                    string BillToName = null;
                    string BillToPostalAddressName = null;
                    string BillToStreet = null;
                    string BillToCity = null;
                    string BillToState = null;
                    string BillToPostalCode = null;
                    string BillToCountryCode = null;
                    string BillToPhoneCountryCode = null;
                    string BillToPhoneAreaOrCityCode = null;
                    string BillToPhoneNumber = null;
                    string BillToFaxCountryCode = null;
                    string BillToFaxAreaOrCityCode = null;
                    string BillToFaxNumber = null;
                    

                    BillToName = dsInvoicePartnerData.Tables[0].Rows[0]["billTo_Name"].ToString();
                    BillToPostalAddressName = dsInvoicePartnerData.Tables[0].Rows[0]["billTo_PostalAddressName"].ToString();
                    BillToStreet = dsInvoicePartnerData.Tables[0].Rows[0]["billTo_Street"].ToString();
                    BillToCity = dsInvoicePartnerData.Tables[0].Rows[0]["billTo_City"].ToString();
                    BillToState = dsInvoicePartnerData.Tables[0].Rows[0]["billTo_State"].ToString();
                    BillToPostalCode = dsInvoicePartnerData.Tables[0].Rows[0]["billTo_PostalCode"].ToString();
                    BillToCountryCode = dsInvoicePartnerData.Tables[0].Rows[0]["billTo_Country"].ToString();
                    BillToPhoneCountryCode = dsInvoicePartnerData.Tables[0].Rows[0]["billTo_PhoneCountryCode"].ToString();
                    BillToPhoneAreaOrCityCode = dsInvoicePartnerData.Tables[0].Rows[0]["billTo_PhoneAreaorCityCode"].ToString();
                    BillToPhoneNumber = dsInvoicePartnerData.Tables[0].Rows[0]["billTo_PhoneNumber"].ToString();
                    BillToFaxCountryCode = dsInvoicePartnerData.Tables[0].Rows[0]["billTo_FaxCountryCode"].ToString();
                    BillToFaxAreaOrCityCode = dsInvoicePartnerData.Tables[0].Rows[0]["billTo_FaxAreaorCityCode"].ToString();
                    BillToFaxNumber = dsInvoicePartnerData.Tables[0].Rows[0]["billTo_FaxNumber"].ToString();

                    string FromName = null;                    
                    string FromStreet = null;
                    string FromCity = null;
                    string FromState = null;
                    string FromPostalCode = null;
                    string FromCountryCode = null;
                    string FromCountryName = null;

                    FromName = dsInvoicePartnerData.Tables[0].Rows[0]["from_Name"].ToString();
                    FromStreet = dsInvoicePartnerData.Tables[0].Rows[0]["from_Street"].ToString();
                    FromCity = dsInvoicePartnerData.Tables[0].Rows[0]["from_City"].ToString();
                    FromState = dsInvoicePartnerData.Tables[0].Rows[0]["from_State"].ToString();
                    FromPostalCode = dsInvoicePartnerData.Tables[0].Rows[0]["from_PostalCode"].ToString();
                    FromCountryCode = dsInvoicePartnerData.Tables[0].Rows[0]["from_Country"].ToString();
                    FromCountryName = dsInvoicePartnerData.Tables[0].Rows[0]["from_CountryName"].ToString();

                    string BillFromName = null;                    
                    string BillFromStreet = null;
                    string BillFromCity = null;
                    string BillFromState = null;
                    string BillFromPostalCode = null;
                    string BillFromCountryCode = null;
                    string BillFromCountryName = null;

                    BillFromName = dsInvoicePartnerData.Tables[0].Rows[0]["billFrom_Name"].ToString();
                    BillFromStreet = dsInvoicePartnerData.Tables[0].Rows[0]["billFrom_Street"].ToString();
                    BillFromCity = dsInvoicePartnerData.Tables[0].Rows[0]["billFrom_City"].ToString();
                    BillFromState = dsInvoicePartnerData.Tables[0].Rows[0]["billFrom_State"].ToString();
                    BillFromPostalCode = dsInvoicePartnerData.Tables[0].Rows[0]["billFrom_PostalCode"].ToString();
                    BillFromCountryCode = dsInvoicePartnerData.Tables[0].Rows[0]["billFrom_Country"].ToString();
                    BillFromCountryName = dsInvoicePartnerData.Tables[0].Rows[0]["billFrom_CountryName"].ToString();


                    string SoldToName = null;
                    string SoldToStreet = null;
                    string SoldToCity = null;
                    string SoldToState = null;
                    string SoldToPostalCode = null;
                    string SoldToCountryCode = null;
                    string SoldToCountryName = null;

                    SoldToName = dsInvoicePartnerData.Tables[0].Rows[0]["soldTo_Name"].ToString();
                    SoldToStreet = dsInvoicePartnerData.Tables[0].Rows[0]["soldTo_Street"].ToString();
                    SoldToCity = dsInvoicePartnerData.Tables[0].Rows[0]["soldTo_City"].ToString();
                    SoldToState = dsInvoicePartnerData.Tables[0].Rows[0]["soldTo_State"].ToString();
                    SoldToPostalCode = dsInvoicePartnerData.Tables[0].Rows[0]["soldTo_PostalCode"].ToString();
                    SoldToCountryCode = dsInvoicePartnerData.Tables[0].Rows[0]["soldTo_Country"].ToString();
                    SoldToCountryName = dsInvoicePartnerData.Tables[0].Rows[0]["soldTo_CountryName"].ToString();

                    string ShipFromName = null;
                    string ShipFromStreet = null;
                    string ShipFromCity = null;
                    string ShipFromState = null;
                    string ShipFromPostalCode = null;
                    string ShipFromCountryCode = null;
                    string ShipFromCountryName = null;

                    ShipFromName = dsInvoicePartnerData.Tables[0].Rows[0]["shipFrom_Name"].ToString();
                    ShipFromStreet = dsInvoicePartnerData.Tables[0].Rows[0]["shipFrom_Street"].ToString();
                    ShipFromCity = dsInvoicePartnerData.Tables[0].Rows[0]["shipFrom_City"].ToString();
                    ShipFromState = dsInvoicePartnerData.Tables[0].Rows[0]["shipFrom_State"].ToString();
                    ShipFromPostalCode = dsInvoicePartnerData.Tables[0].Rows[0]["shipFrom_PostalCode"].ToString();
                    ShipFromCountryCode = dsInvoicePartnerData.Tables[0].Rows[0]["shipFrom_CountryCode"].ToString();
                    ShipFromCountryName = dsInvoicePartnerData.Tables[0].Rows[0]["shipFrom_CountryName"].ToString();


                    string ShipToAddressID = null;
                    string ShipToName = null;
                    string ShipToStreet = null;
                    string ShipToCity = null;
                    string ShipToState = null;
                    string ShipToPostalCode = null;
                    string ShipToCountryCode = null;
                    string ShipToCountryName = null;
                    string ShipToEmailName = null;
                    string ShipToEmail = null;
                    string ShipToPhoneCountryCode = null;
                    string ShipToPhoneAreaOrCityCode = null;
                    string ShipToPhoneNumber = null;


                    ShipToAddressID = dsInvoicePartnerData.Tables[0].Rows[0]["shipTo_addressID"].ToString();
                    ShipToName = dsInvoicePartnerData.Tables[0].Rows[0]["shipTo_Name"].ToString();
                    ShipToStreet = dsInvoicePartnerData.Tables[0].Rows[0]["shipTo_Street"].ToString();
                    ShipToCity = dsInvoicePartnerData.Tables[0].Rows[0]["shipTo_City"].ToString();
                    ShipToState = dsInvoicePartnerData.Tables[0].Rows[0]["shipTo_State"].ToString();
                    ShipToPostalCode = dsInvoicePartnerData.Tables[0].Rows[0]["shipTo_PostalCode"].ToString();
                    ShipToCountryCode = dsInvoicePartnerData.Tables[0].Rows[0]["shipTo_Country"].ToString();
                    ShipToCountryName = dsInvoicePartnerData.Tables[0].Rows[0]["shipTo_CountryName"].ToString();
                    ShipToEmailName = dsInvoicePartnerData.Tables[0].Rows[0]["shipTo_EmailName"].ToString();
                    ShipToEmail = dsInvoicePartnerData.Tables[0].Rows[0]["shipTo_Email"].ToString();
                    ShipToPhoneCountryCode = dsInvoicePartnerData.Tables[0].Rows[0]["shipTo_PhoneCountryCode"].ToString();
                    ShipToPhoneAreaOrCityCode = dsInvoicePartnerData.Tables[0].Rows[0]["shipTo_PhoneAreaorCityCode"].ToString();
                    ShipToPhoneNumber = dsInvoicePartnerData.Tables[0].Rows[0]["shipTo_PhoneNumber"].ToString();


                    string PayInNumberOfDays = null;
                    string PayInDiscountPercent = null;
                    string ExtrinsicTaxName = null;
                    string ExtrinsicTaxValue = null;
                    string ExtrinsicInvoiceSourceName = null;
                    string ExtrinsicInvoiceSourceValue = null;
                    string ExtrinsicInvoiceSubName = null;
                    string ExtrinsicInvoiceSubValue = null;

                    PayInNumberOfDays = dsInvoicePartnerData.Tables[0].Rows[0]["PayIn_NoOfDays"].ToString();
                    PayInDiscountPercent = dsInvoicePartnerData.Tables[0].Rows[0]["PayIn_DiscountPercent"].ToString();
                    ExtrinsicTaxName = dsInvoicePartnerData.Tables[0].Rows[0]["Ext_TaxName"].ToString();
                    ExtrinsicTaxValue = dsInvoicePartnerData.Tables[0].Rows[0]["Ext_TaxValue"].ToString();
                    ExtrinsicInvoiceSourceName = dsInvoicePartnerData.Tables[0].Rows[0]["Ext_InvSrcName"].ToString();
                    ExtrinsicInvoiceSourceValue = dsInvoicePartnerData.Tables[0].Rows[0]["Ext_InvSrcValue"].ToString();
                    ExtrinsicInvoiceSubName = dsInvoicePartnerData.Tables[0].Rows[0]["Ext_InvSubName"].ToString();
                    ExtrinsicInvoiceSubValue = dsInvoicePartnerData.Tables[0].Rows[0]["Ext_InvSubValue"].ToString();

                    thisProgram.xmlWriter.WriteStartElement("Request");// starting Request Tag 
                    thisProgram.xmlWriter.WriteAttributeString("deploymentMode", "production");


                    #region Invoice Detail Request Tag

                    thisProgram.xmlWriter.WriteStartElement("InvoiceDetailRequest");//Starting InvoiceDetailRequest tag

                    #region Invoice Detail Request Header

                 

                    thisProgram.xmlWriter.WriteStartElement("InvoiceDetailRequestHeader"); //Starting InvoiceDetailRequestHeader tag

                    thisProgram.xmlWriter.WriteAttributeString("invoiceDate", convINVOICECreatedDate);
                    thisProgram.xmlWriter.WriteAttributeString("invoiceID", ConfirmID);
                    thisProgram.xmlWriter.WriteAttributeString("invoiceOrigin", InvoiceOrigin);
                    thisProgram.xmlWriter.WriteAttributeString("operation", Operation);
                    thisProgram.xmlWriter.WriteAttributeString("purpose", Purpose);



                    thisProgram.xmlWriter.WriteStartElement("InvoiceDetailHeaderIndicator"); //Starting InvoiceDetailHeaderIndicator tag
                    thisProgram.xmlWriter.WriteEndElement();// Closing InvoiceDetailHeaderIndicator Tag

                    thisProgram.xmlWriter.WriteStartElement("InvoiceDetailLineIndicator"); //Starting InvoiceDetailLineIndicator tag

                    thisProgram.xmlWriter.WriteAttributeString("isTaxInLine", "yes");

                    thisProgram.xmlWriter.WriteEndElement();// Closing InvoiceDetailLineIndicator Tag

                    #region Invoice Partner remit To


                    thisProgram.xmlWriter.WriteStartElement("InvoicePartner"); //Starting InvoicePartner bill to tag

                    #region Contact Remit To

                    thisProgram.xmlWriter.WriteStartElement("Contact"); //Starting Contact tag
                    thisProgram.xmlWriter.WriteAttributeString("role", "remitTo");

                    #region Name Tag

                    thisProgram.xmlWriter.WriteStartElement("Name"); //Starting Name tag
                    thisProgram.xmlWriter.WriteAttributeString("xml", "lang", null, "en-US");
                    thisProgram.xmlWriter.WriteString(RemitToName);
                    thisProgram.xmlWriter.WriteEndElement();// Closing Name Tag

                    #endregion

                    #region Postal Address

                    thisProgram.xmlWriter.WriteStartElement("PostalAddress"); //Starting PostalAddress tag
                    thisProgram.xmlWriter.WriteAttributeString("name", RemitToPostalAddressName);

                    thisProgram.xmlWriter.WriteStartElement("Street"); //Starting Street tag
                    thisProgram.xmlWriter.WriteString(RemitToStreet);
                    thisProgram.xmlWriter.WriteEndElement();// Closing Street Tag

                    thisProgram.xmlWriter.WriteStartElement("City"); //Starting City tag
                    thisProgram.xmlWriter.WriteString(RemitToCity);
                    thisProgram.xmlWriter.WriteEndElement();// Closing City Tag

                    thisProgram.xmlWriter.WriteStartElement("PostalCode"); //Starting PostalCode tag
                    thisProgram.xmlWriter.WriteString(RemitToPostalCode);
                    thisProgram.xmlWriter.WriteEndElement();// Closing PostalCode Tag

                    thisProgram.xmlWriter.WriteStartElement("Country"); //Starting Country tag
                    thisProgram.xmlWriter.WriteAttributeString("isoCountryCode", RemitToCountryCode);
                    thisProgram.xmlWriter.WriteEndElement();// Closing Country Tag

                    thisProgram.xmlWriter.WriteEndElement();// Closing PostalAddress Tag

                    #endregion



                    thisProgram.xmlWriter.WriteEndElement();// Closing Contact Tag

                    #endregion


                    thisProgram.xmlWriter.WriteEndElement();// Closing InvoicePartner Tag

                    #endregion

                    #region Invoice Partner Bill To


                    thisProgram.xmlWriter.WriteStartElement("InvoicePartner"); //Starting InvoicePartner bill to tag

                    #region Contact Bill To

                    thisProgram.xmlWriter.WriteStartElement("Contact"); //Starting Contact tag
                    thisProgram.xmlWriter.WriteAttributeString("role", "billTo");

                    #region Name Tag

                    thisProgram.xmlWriter.WriteStartElement("Name"); //Starting Name tag
                    thisProgram.xmlWriter.WriteAttributeString("xml", "lang", null, "en-US");
                    thisProgram.xmlWriter.WriteString(BillToName);
                    thisProgram.xmlWriter.WriteEndElement();// Closing Name Tag

                    #endregion

                    #region Postal Address

                    thisProgram.xmlWriter.WriteStartElement("PostalAddress"); //Starting PostalAddress tag
                    thisProgram.xmlWriter.WriteAttributeString("name", BillToPostalAddressName);

                    thisProgram.xmlWriter.WriteStartElement("Street"); //Starting Street tag
                    thisProgram.xmlWriter.WriteString(BillToStreet);
                    thisProgram.xmlWriter.WriteEndElement();// Closing Street Tag

                    thisProgram.xmlWriter.WriteStartElement("City"); //Starting City tag
                    thisProgram.xmlWriter.WriteString(BillToCity);
                    thisProgram.xmlWriter.WriteEndElement();// Closing City Tag

                    thisProgram.xmlWriter.WriteStartElement("State"); //Starting State tag
                    thisProgram.xmlWriter.WriteString(BillToState);
                    thisProgram.xmlWriter.WriteEndElement();// Closing State Tag

                    thisProgram.xmlWriter.WriteStartElement("PostalCode"); //Starting PostalCode tag
                    thisProgram.xmlWriter.WriteString(BillToPostalCode);
                    thisProgram.xmlWriter.WriteEndElement();// Closing PostalCode Tag

                    thisProgram.xmlWriter.WriteStartElement("Country"); //Starting Country tag
                    thisProgram.xmlWriter.WriteAttributeString("isoCountryCode", BillToCountryCode);
                    thisProgram.xmlWriter.WriteEndElement();// Closing Country Tag

                    thisProgram.xmlWriter.WriteEndElement();// Closing PostalAddress Tag

                    #endregion

                    #region Phone 

                    thisProgram.xmlWriter.WriteStartElement("Phone"); //Starting Phone tag

                    thisProgram.xmlWriter.WriteStartElement("TelephoneNumber"); //Starting TelephoneNumber tag

                    thisProgram.xmlWriter.WriteStartElement("CountryCode"); //Starting CountryCode tag
                    thisProgram.xmlWriter.WriteAttributeString("isoCountryCode", BillToCountryCode);
                    thisProgram.xmlWriter.WriteString(BillToPhoneCountryCode);
                    thisProgram.xmlWriter.WriteEndElement();// Closing CountryCode Tag

                    thisProgram.xmlWriter.WriteStartElement("AreaOrCityCode"); //Starting AreaOrCityCode tag
                    thisProgram.xmlWriter.WriteString(BillToPhoneAreaOrCityCode);
                    thisProgram.xmlWriter.WriteEndElement();// Closing AreaOrCityCode Tag

                    thisProgram.xmlWriter.WriteStartElement("Number"); //Starting Number tag
                    thisProgram.xmlWriter.WriteString(BillToPhoneNumber);
                    thisProgram.xmlWriter.WriteEndElement();// Closing Number Tag


                    thisProgram.xmlWriter.WriteEndElement();// Closing TelephoneNumber Tag


                    thisProgram.xmlWriter.WriteEndElement();// Closing Phone Tag

                    #endregion

                    #region Fax 

                    thisProgram.xmlWriter.WriteStartElement("Fax"); //Starting Fax tag

                    thisProgram.xmlWriter.WriteStartElement("TelephoneNumber"); //Starting TelephoneNumber tag

                    thisProgram.xmlWriter.WriteStartElement("CountryCode"); //Starting CountryCode tag
                    thisProgram.xmlWriter.WriteAttributeString("isoCountryCode", BillToCountryCode);
                    thisProgram.xmlWriter.WriteString(BillToFaxCountryCode);
                    thisProgram.xmlWriter.WriteEndElement();// Closing CountryCode Tag

                    thisProgram.xmlWriter.WriteStartElement("AreaOrCityCode"); //Starting AreaOrCityCode tag
                    thisProgram.xmlWriter.WriteString(BillToFaxAreaOrCityCode);
                    thisProgram.xmlWriter.WriteEndElement();// Closing AreaOrCityCode Tag

                    thisProgram.xmlWriter.WriteStartElement("Number"); //Starting Number tag
                    thisProgram.xmlWriter.WriteString(BillToFaxNumber);
                    thisProgram.xmlWriter.WriteEndElement();// Closing Number Tag


                    thisProgram.xmlWriter.WriteEndElement();// Closing TelephoneNumber Tag


                    thisProgram.xmlWriter.WriteEndElement();// Closing Fax Tag

                    #endregion



                    thisProgram.xmlWriter.WriteEndElement();// Closing Contact Tag

                    #endregion


                    thisProgram.xmlWriter.WriteEndElement();// Closing InvoicePartner Tag

                    #endregion


                    #region Invoice Partner From


                    thisProgram.xmlWriter.WriteStartElement("InvoicePartner"); //Starting InvoicePartner from tag

                    #region Contact From

                    thisProgram.xmlWriter.WriteStartElement("Contact"); //Starting Contact tag
                    thisProgram.xmlWriter.WriteAttributeString("role", "from");

                    #region Name Tag

                    thisProgram.xmlWriter.WriteStartElement("Name"); //Starting Name tag
                    thisProgram.xmlWriter.WriteAttributeString("xml", "lang", null, "en-US");
                    thisProgram.xmlWriter.WriteString(FromName);
                    thisProgram.xmlWriter.WriteEndElement();// Closing Name Tag

                    #endregion

                    #region Postal Address

                    thisProgram.xmlWriter.WriteStartElement("PostalAddress"); //Starting PostalAddress tag

                    thisProgram.xmlWriter.WriteStartElement("Street"); //Starting Street tag
                    thisProgram.xmlWriter.WriteString(FromStreet);
                    thisProgram.xmlWriter.WriteEndElement();// Closing Street Tag

                    thisProgram.xmlWriter.WriteStartElement("City"); //Starting City tag
                    thisProgram.xmlWriter.WriteString(FromCity);
                    thisProgram.xmlWriter.WriteEndElement();// Closing City Tag

                    thisProgram.xmlWriter.WriteStartElement("PostalCode"); //Starting PostalCode tag
                    thisProgram.xmlWriter.WriteString(FromPostalCode);
                    thisProgram.xmlWriter.WriteEndElement();// Closing PostalCode Tag

                    thisProgram.xmlWriter.WriteStartElement("Country"); //Starting Country tag
                    thisProgram.xmlWriter.WriteAttributeString("isoCountryCode", FromCountryCode);
                    thisProgram.xmlWriter.WriteString(FromCountryName);
                    thisProgram.xmlWriter.WriteEndElement();// Closing Country Tag

                    thisProgram.xmlWriter.WriteEndElement();// Closing PostalAddress Tag

                    #endregion


                    thisProgram.xmlWriter.WriteEndElement();// Closing Contact Tag

                    #endregion


                    thisProgram.xmlWriter.WriteEndElement();// Closing InvoicePartner Tag

                    #endregion


                    #region Invoice Partner Bill From


                    thisProgram.xmlWriter.WriteStartElement("InvoicePartner"); //Starting InvoicePartner Bill from tag

                    #region Contact From

                    thisProgram.xmlWriter.WriteStartElement("Contact"); //Starting Contact tag
                    thisProgram.xmlWriter.WriteAttributeString("role", "billFrom");

                    #region Name Tag

                    thisProgram.xmlWriter.WriteStartElement("Name"); //Starting Name tag
                    thisProgram.xmlWriter.WriteAttributeString("xml", "lang", null, "en-US");
                    thisProgram.xmlWriter.WriteString(BillFromName);
                    thisProgram.xmlWriter.WriteEndElement();// Closing Name Tag

                    #endregion

                    #region Postal Address

                    thisProgram.xmlWriter.WriteStartElement("PostalAddress"); //Starting PostalAddress tag

                    thisProgram.xmlWriter.WriteStartElement("Street"); //Starting Street tag
                    thisProgram.xmlWriter.WriteString(BillFromStreet);
                    thisProgram.xmlWriter.WriteEndElement();// Closing Street Tag

                    thisProgram.xmlWriter.WriteStartElement("City"); //Starting City tag
                    thisProgram.xmlWriter.WriteString(BillFromCity);
                    thisProgram.xmlWriter.WriteEndElement();// Closing City Tag

                    thisProgram.xmlWriter.WriteStartElement("PostalCode"); //Starting PostalCode tag
                    thisProgram.xmlWriter.WriteString(BillFromPostalCode);
                    thisProgram.xmlWriter.WriteEndElement();// Closing PostalCode Tag

                    thisProgram.xmlWriter.WriteStartElement("Country"); //Starting Country tag
                    thisProgram.xmlWriter.WriteAttributeString("isoCountryCode", BillFromCountryCode);
                    thisProgram.xmlWriter.WriteString(BillFromCountryName);
                    thisProgram.xmlWriter.WriteEndElement();// Closing Country Tag

                    thisProgram.xmlWriter.WriteEndElement();// Closing PostalAddress Tag

                    #endregion


                    thisProgram.xmlWriter.WriteEndElement();// Closing Contact Tag

                    #endregion


                    thisProgram.xmlWriter.WriteEndElement();// Closing InvoicePartner Tag

                    #endregion

                    #region Invoice Partner Sold To


                    thisProgram.xmlWriter.WriteStartElement("InvoicePartner"); //Starting InvoicePartner Sold To tag

                    #region Contact From

                    thisProgram.xmlWriter.WriteStartElement("Contact"); //Starting Contact tag
                    thisProgram.xmlWriter.WriteAttributeString("role", "soldTo");

                    #region Name Tag

                    thisProgram.xmlWriter.WriteStartElement("Name"); //Starting Name tag
                    thisProgram.xmlWriter.WriteAttributeString("xml", "lang", null, "en-US");
                    thisProgram.xmlWriter.WriteString(SoldToName);
                    thisProgram.xmlWriter.WriteEndElement();// Closing Name Tag

                    #endregion

                    #region Postal Address

                    thisProgram.xmlWriter.WriteStartElement("PostalAddress"); //Starting PostalAddress tag

                    thisProgram.xmlWriter.WriteStartElement("Street"); //Starting Street tag
                    thisProgram.xmlWriter.WriteString(SoldToStreet);
                    thisProgram.xmlWriter.WriteEndElement();// Closing Street Tag

                    thisProgram.xmlWriter.WriteStartElement("City"); //Starting City tag
                    thisProgram.xmlWriter.WriteString(SoldToCity);
                    thisProgram.xmlWriter.WriteEndElement();// Closing City Tag

                    thisProgram.xmlWriter.WriteStartElement("PostalCode"); //Starting PostalCode tag
                    thisProgram.xmlWriter.WriteString(SoldToPostalCode);
                    thisProgram.xmlWriter.WriteEndElement();// Closing PostalCode Tag

                    thisProgram.xmlWriter.WriteStartElement("Country"); //Starting Country tag
                    thisProgram.xmlWriter.WriteAttributeString("isoCountryCode", SoldToCountryCode);
                    thisProgram.xmlWriter.WriteString(SoldToCountryName);
                    thisProgram.xmlWriter.WriteEndElement();// Closing Country Tag

                    thisProgram.xmlWriter.WriteEndElement();// Closing PostalAddress Tag

                    #endregion


                    thisProgram.xmlWriter.WriteEndElement();// Closing Contact Tag

                    #endregion


                    thisProgram.xmlWriter.WriteEndElement();// Closing InvoicePartner Tag

                    #endregion

                    #region  Invoice Detail Shipping

                    thisProgram.xmlWriter.WriteStartElement("InvoiceDetailShipping"); //Starting InvoiceDetailShipping Sold To tag

                    #region Contact  Ship From

                    thisProgram.xmlWriter.WriteStartElement("Contact"); //Starting Contact tag
                    thisProgram.xmlWriter.WriteAttributeString("role", "shipFrom");

                    #region Name Tag

                    thisProgram.xmlWriter.WriteStartElement("Name"); //Starting Name tag
                    thisProgram.xmlWriter.WriteAttributeString("xml", "lang", null, "en-US");
                    thisProgram.xmlWriter.WriteString(ShipFromName);
                    thisProgram.xmlWriter.WriteEndElement();// Closing Name Tag

                    #endregion Name Tag

                    #region Postal Address

                    thisProgram.xmlWriter.WriteStartElement("PostalAddress"); //Starting PostalAddress tag

                    thisProgram.xmlWriter.WriteStartElement("Street"); //Starting Street tag
                    thisProgram.xmlWriter.WriteString(ShipFromStreet);
                    thisProgram.xmlWriter.WriteEndElement();// Closing Street Tag

                    thisProgram.xmlWriter.WriteStartElement("City"); //Starting City tag
                    thisProgram.xmlWriter.WriteString(ShipFromCity);
                    thisProgram.xmlWriter.WriteEndElement();// Closing City Tag

                    thisProgram.xmlWriter.WriteStartElement("PostalCode"); //Starting PostalCode tag
                    thisProgram.xmlWriter.WriteString(ShipFromPostalCode);
                    thisProgram.xmlWriter.WriteEndElement();// Closing PostalCode Tag

                    thisProgram.xmlWriter.WriteStartElement("Country"); //Starting Country tag
                    thisProgram.xmlWriter.WriteAttributeString("isoCountryCode", ShipFromCountryCode);
                    thisProgram.xmlWriter.WriteString(ShipFromCountryName);
                    thisProgram.xmlWriter.WriteEndElement();// Closing Country Tag

                    thisProgram.xmlWriter.WriteEndElement();// Closing PostalAddress Tag

                    #endregion Postal Address


                                thisProgram.xmlWriter.WriteEndElement();// Closing Contact Tag

                    #endregion Contact  Ship From

                    #region Contact one time address ship to

                    thisProgram.xmlWriter.WriteStartElement("Contact"); //Starting Contact tag

                    thisProgram.xmlWriter.WriteAttributeString("addressID", "OneTimeAddress");
                    thisProgram.xmlWriter.WriteAttributeString("role", "shipTo");

                    #region Name Tag

                    thisProgram.xmlWriter.WriteStartElement("Name"); //Starting Name tag
                    thisProgram.xmlWriter.WriteAttributeString("xml", "lang", null, "en-US");
                    thisProgram.xmlWriter.WriteString(ShipToName);
                    thisProgram.xmlWriter.WriteEndElement();// Closing Name Tag

                    #endregion Name Tag

                    #region Postal Address



                    thisProgram.xmlWriter.WriteStartElement("PostalAddress"); //Starting PostalAddress tag

                    thisProgram.xmlWriter.WriteStartElement("Street"); //Starting Street tag
                    thisProgram.xmlWriter.WriteString(ShipToStreet);
                    thisProgram.xmlWriter.WriteEndElement();// Closing Street Tag

                    thisProgram.xmlWriter.WriteStartElement("City"); //Starting City tag
                    thisProgram.xmlWriter.WriteString(ShipToCity);
                    thisProgram.xmlWriter.WriteEndElement();// Closing City Tag

                    thisProgram.xmlWriter.WriteStartElement("State"); //Starting State tag
                    thisProgram.xmlWriter.WriteString(ShipToState);
                    thisProgram.xmlWriter.WriteEndElement();// Closing State Tag

                    thisProgram.xmlWriter.WriteStartElement("PostalCode"); //Starting PostalCode tag
                    thisProgram.xmlWriter.WriteString(ShipToPostalCode);
                    thisProgram.xmlWriter.WriteEndElement();// Closing PostalCode Tag                    

                    thisProgram.xmlWriter.WriteStartElement("Country"); //Starting Country tag
                    thisProgram.xmlWriter.WriteAttributeString("isoCountryCode", ShipToCountryCode);
                    thisProgram.xmlWriter.WriteEndElement();// Closing Country Tag



                    thisProgram.xmlWriter.WriteEndElement();// Closing PostalAddress Tag



                    #endregion Postal Address

                    #region Email


                    thisProgram.xmlWriter.WriteStartElement("Email"); //Starting Email tag

                    thisProgram.xmlWriter.WriteAttributeString("name", ShipToEmailName);
                    thisProgram.xmlWriter.WriteAttributeString("preferredLang", "en");
                    thisProgram.xmlWriter.WriteString(ShipToEmail);

                    thisProgram.xmlWriter.WriteEndElement();// Closing Email Tag

                    #endregion Email

                    #region Phone 

                    thisProgram.xmlWriter.WriteStartElement("Phone"); //Starting Phone tag

                    thisProgram.xmlWriter.WriteStartElement("TelephoneNumber"); //Starting TelephoneNumber tag

                    thisProgram.xmlWriter.WriteStartElement("CountryCode"); //Starting CountryCode tag
                    thisProgram.xmlWriter.WriteAttributeString("isoCountryCode", ShipToCountryCode);
                    thisProgram.xmlWriter.WriteString(ShipToPhoneCountryCode);
                    thisProgram.xmlWriter.WriteEndElement();// Closing CountryCode Tag

                    thisProgram.xmlWriter.WriteStartElement("AreaOrCityCode"); //Starting AreaOrCityCode tag
                    thisProgram.xmlWriter.WriteString(ShipToPhoneAreaOrCityCode);
                    thisProgram.xmlWriter.WriteEndElement();// Closing AreaOrCityCode Tag

                    thisProgram.xmlWriter.WriteStartElement("Number"); //Starting Number tag
                    thisProgram.xmlWriter.WriteString(ShipToPhoneNumber);
                    thisProgram.xmlWriter.WriteEndElement();// Closing Number Tag


                    thisProgram.xmlWriter.WriteEndElement();// Closing TelephoneNumber Tag


                    thisProgram.xmlWriter.WriteEndElement();// Closing Phone Tag

                    #endregion Phone


                    thisProgram.xmlWriter.WriteEndElement();// Closing Contact Tag

                    #endregion Contact one time address ship to


                    thisProgram.xmlWriter.WriteEndElement();// Closing InvoiceDetailShipping Tag

                    #endregion Invoice Detail Shipping


                    #region Payment term




                    thisProgram.xmlWriter.WriteStartElement("PaymentTerm"); //Starting Payment Term tag
                    thisProgram.xmlWriter.WriteAttributeString("payInNumberOfDays", PayInNumberOfDays);

                    thisProgram.xmlWriter.WriteStartElement("Discount"); //Starting Discount tag
                    thisProgram.xmlWriter.WriteStartElement("DiscountPercent");//Starting Discount Percent tag
                    thisProgram.xmlWriter.WriteAttributeString("percent", PayInDiscountPercent);
                    thisProgram.xmlWriter.WriteEndElement();//Closing Discount Percent Tag
                    thisProgram.xmlWriter.WriteEndElement();//Closing Discount Tag






                    thisProgram.xmlWriter.WriteEndElement();//Closing Payment Term Tag

                    #endregion Payment term


                    #region Extrinsic

                    thisProgram.xmlWriter.WriteStartElement("Extrinsic"); //Starting Extrinsic 1 tag
                    thisProgram.xmlWriter.WriteAttributeString("name", ExtrinsicTaxName);
                    thisProgram.xmlWriter.WriteString(ExtrinsicTaxValue);
                    thisProgram.xmlWriter.WriteEndElement();//Closing Extrinsic 1 Tag

                    thisProgram.xmlWriter.WriteStartElement("Extrinsic"); //Starting Extrinsic 2 tag
                    thisProgram.xmlWriter.WriteAttributeString("name", ExtrinsicInvoiceSourceName);
                    thisProgram.xmlWriter.WriteString(ExtrinsicInvoiceSourceValue);
                    thisProgram.xmlWriter.WriteEndElement();//Closing Extrinsic 2 Tag

                    thisProgram.xmlWriter.WriteStartElement("Extrinsic"); //Starting Extrinsic 3 tag
                    thisProgram.xmlWriter.WriteAttributeString("name", ExtrinsicInvoiceSubName);
                    thisProgram.xmlWriter.WriteString(ExtrinsicInvoiceSubValue);
                    thisProgram.xmlWriter.WriteEndElement();//Closing Extrinsic 3 Tag


                    #endregion Extrinsic


                    thisProgram.xmlWriter.WriteEndElement();// Closing InvoiceDetailRequestHeader Tag

                    #endregion Invoice Detail Request Header

                    #region Invoice  Detail Order

                    thisProgram.xmlWriter.WriteStartElement("InvoiceDetailOrder"); //Starting InvoiceDetailOrder tag

                    #region  Invoice Detail Order Info Tag

                    thisProgram.xmlWriter.WriteStartElement("InvoiceDetailOrderInfo"); //Starting InvoiceDetailOrderInfo tag          

                    thisProgram.xmlWriter.WriteStartElement("OrderReference"); //Starting OrderReference tag
                    thisProgram.xmlWriter.WriteAttributeString("orderID", OrderNumber);

                    thisProgram.xmlWriter.WriteStartElement("DocumentReference"); //Starting DocumentReference tag
                    thisProgram.xmlWriter.WriteAttributeString("payloadID", DocRefPayloadID);
                    thisProgram.xmlWriter.WriteEndElement();//Closing DocumentReference Tag

                    thisProgram.xmlWriter.WriteEndElement();//Closing OrderReference Tag



                    thisProgram.xmlWriter.WriteEndElement();//Closing InvoiceDetailOrderInfo Tag

                    #endregion  Invoice Detail Order Info Tag

                    #region InvoiceDetailItem


                    string DetailItem_quantity = null;
                    string DetailItem_InvoiceLineNumber = null;
                    string DetailItem_UnitOfMeasure = null;
                    string DetailItem_Currency = null;
                    string DetailItem_UnitPrice = null;
                    string DetailItem_PriceBasisQuantity = null;
                    string DetailItem_conversionFactor = null;
                    string DetailItem_LineNumber = null;
                    string DetailItem_supplierPartID = null;
                    string DetailItem_buyerPartID = null;
                    string DetailItem_Description = null;
                    string DetailItem_ManufacturerPartID = null;
                    string DetailItem_ManufacturerName = null;
                    string DetailItem_EANID = null;
                    string NetAmount = null;
                    string TaxAmount = null;
                    string GrossAmount = null;

                    string selectInvoiceItemData = ConfigurationManager.AppSettings["selectInvoiceItemData"].ToString();
                    string strInvoiceItemData = selectInvoiceItemData + "'" + orderNum + "'";
                    SqlDataAdapter dtInvoiceItemData = new SqlDataAdapter(strInvoiceItemData, con);
                    DataSet dsInvoiceItemData = new DataSet();
                    dtInvoiceItemData.Fill(dsInvoiceItemData);
                    if (dsInvoiceItemData.Tables[0].Rows.Count == 0)
                        {
                        throw new Exception("NO Data in View_INV_LineItem, Please check the data and run the exe again");
                        }








                    for (int j = 0; j < dsInvoiceItemData.Tables[0].Rows.Count; j++)
                        {


                        DetailItem_quantity = dsInvoiceItemData.Tables[0].Rows[j]["DetailItem_quantity"].ToString();
                        DetailItem_InvoiceLineNumber = dsInvoiceItemData.Tables[0].Rows[j]["DetailItem_InvoiceLineNumber"].ToString();
                        DetailItem_UnitOfMeasure = dsInvoiceItemData.Tables[0].Rows[j]["DetailItem_UnitOfMeasure"].ToString();
                        DetailItem_Currency = dsInvoiceItemData.Tables[0].Rows[j]["DetailItem_Currency"].ToString();
                        DetailItem_UnitPrice = dsInvoiceItemData.Tables[0].Rows[j]["DetailItem_UnitPrice"].ToString();
                        DetailItem_PriceBasisQuantity = dsInvoiceItemData.Tables[0].Rows[j]["DetailItem_PriceBasisQuantity"].ToString();
                        DetailItem_conversionFactor = dsInvoiceItemData.Tables[0].Rows[j]["DetailItem_conversionFactor"].ToString();
                        DetailItem_Description = dsInvoiceItemData.Tables[0].Rows[j]["DetailItem_Description"].ToString();
                        DetailItem_LineNumber = dsInvoiceItemData.Tables[0].Rows[j]["DetailItem_LineNumber"].ToString();
                        DetailItem_ManufacturerPartID = dsInvoiceItemData.Tables[0].Rows[j]["DetailItem_ManufacturerPartID"].ToString();
                        DetailItem_ManufacturerName = dsInvoiceItemData.Tables[0].Rows[j]["DetailItem_ManufacturerName"].ToString();
                        DetailItem_supplierPartID = dsInvoiceItemData.Tables[0].Rows[j]["DetailItem_supplierPartID"].ToString();
                        DetailItem_buyerPartID = dsInvoiceItemData.Tables[0].Rows[j]["DetailItem_buyerPartID"].ToString();
                        DetailItem_EANID = dsInvoiceItemData.Tables[0].Rows[j]["DetailItem_EANID"].ToString();
                        NetAmount = dsInvoiceItemData.Tables[0].Rows[j]["NetAmount"].ToString();
                        TaxAmount = dsInvoiceItemData.Tables[0].Rows[j]["TaxAmount"].ToString();
                        GrossAmount = dsInvoiceItemData.Tables[0].Rows[j]["GrossAmount"].ToString();

                        thisProgram.xmlWriter.WriteStartElement("InvoiceDetailItem"); //Starting InvoiceDetailItem tag
                        thisProgram.xmlWriter.WriteAttributeString("invoiceLineNumber", DetailItem_InvoiceLineNumber);
                        thisProgram.xmlWriter.WriteAttributeString("quantity", DetailItem_quantity);

                        thisProgram.xmlWriter.WriteStartElement("UnitOfMeasure"); //Starting UnitOfMeasure tag
                        thisProgram.xmlWriter.WriteString(DetailItem_UnitOfMeasure);
                        thisProgram.xmlWriter.WriteEndElement();//Closing UnitOfMeasure Tag

                        thisProgram.xmlWriter.WriteStartElement("UnitPrice"); //Starting UnitPrice tag
                        thisProgram.xmlWriter.WriteStartElement("Money"); //Starting Money tag
                        thisProgram.xmlWriter.WriteAttributeString("currency", DetailItem_Currency);
                        thisProgram.xmlWriter.WriteString(DetailItem_UnitPrice);
                        thisProgram.xmlWriter.WriteEndElement();//Closing Money Tag
                        thisProgram.xmlWriter.WriteEndElement();//Closing UnitPrice Tag

                        thisProgram.xmlWriter.WriteStartElement("PriceBasisQuantity"); //Starting PriceBasisQuantity tag
                        thisProgram.xmlWriter.WriteAttributeString("conversionFactor", DetailItem_conversionFactor);
                        thisProgram.xmlWriter.WriteAttributeString("quantity", DetailItem_PriceBasisQuantity);
                        thisProgram.xmlWriter.WriteStartElement("UnitOfMeasure"); //Starting UnitOfMeasure tag
                        thisProgram.xmlWriter.WriteString(DetailItem_UnitOfMeasure);
                        thisProgram.xmlWriter.WriteEndElement();//Closing UnitOfMeasure Tag
                        thisProgram.xmlWriter.WriteStartElement("Description"); //Starting Description tag
                        thisProgram.xmlWriter.WriteAttributeString("xml", "lang", null, "en-AU");                        
                        thisProgram.xmlWriter.WriteEndElement();//Closing Description Tag
                        thisProgram.xmlWriter.WriteEndElement();//Closing PriceBasisQuantity Tag

                        thisProgram.xmlWriter.WriteStartElement("InvoiceDetailItemReference"); //Starting InvoiceDetailItemReference tag
                        thisProgram.xmlWriter.WriteAttributeString("lineNumber", DetailItem_LineNumber);

                        thisProgram.xmlWriter.WriteStartElement("ItemID"); //Starting ItemID tag

                        thisProgram.xmlWriter.WriteStartElement("SupplierPartID"); //Starting SupplierPartID tag
                        thisProgram.xmlWriter.WriteString(DetailItem_supplierPartID);
                        thisProgram.xmlWriter.WriteEndElement();//Closing SupplierPartID Tag

                        thisProgram.xmlWriter.WriteStartElement("BuyerPartID"); //Starting BuyerPartID tag
                        thisProgram.xmlWriter.WriteString(DetailItem_buyerPartID);
                        thisProgram.xmlWriter.WriteEndElement();//Closing BuyerPartID Tag

                        thisProgram.xmlWriter.WriteEndElement();//Closing ItemID Tag


                        thisProgram.xmlWriter.WriteStartElement("Description"); //Starting Description tag
                        thisProgram.xmlWriter.WriteAttributeString("xml", "lang", null, "en");
                        thisProgram.xmlWriter.WriteString(DetailItem_Description);
                        thisProgram.xmlWriter.WriteEndElement();//Closing Description Tag

                        thisProgram.xmlWriter.WriteStartElement("ManufacturerPartID"); //Starting ManufacturerPartID tag
                        thisProgram.xmlWriter.WriteString(DetailItem_ManufacturerPartID);
                        thisProgram.xmlWriter.WriteEndElement();//Closing ManufacturerPartID Tag

                        thisProgram.xmlWriter.WriteStartElement("ManufacturerName"); //Starting ManufacturerName tag
                        thisProgram.xmlWriter.WriteAttributeString("xml", "lang", null, "en");
                        thisProgram.xmlWriter.WriteString(DetailItem_ManufacturerName);
                        thisProgram.xmlWriter.WriteEndElement();//Closing ManufacturerName Tag

                        thisProgram.xmlWriter.WriteStartElement("InvoiceDetailItemReferenceIndustry"); //Starting InvoiceDetailItemReferenceIndustry tag
                        thisProgram.xmlWriter.WriteStartElement("InvoiceDetailItemReferenceRetail"); //Starting InvoiceDetailItemReferenceRetail tag
                        thisProgram.xmlWriter.WriteStartElement("EANID"); //Starting EANID tag
                        thisProgram.xmlWriter.WriteString(DetailItem_EANID);
                        thisProgram.xmlWriter.WriteEndElement();//Closing EANID Tag
                        thisProgram.xmlWriter.WriteEndElement();//Closing InvoiceDetailItemReferenceRetail Tag
                        thisProgram.xmlWriter.WriteEndElement();//Closing InvoiceDetailItemReferenceIndustry Tag

                        //thisProgram.xmlWriter.WriteStartElement("SerialNumber"); //Starting SerialNumber tag
                        //thisProgram.xmlWriter.WriteString("2278536SERIAL1");
                        //thisProgram.xmlWriter.WriteEndElement();//Closing SerialNumber Tag

                        //thisProgram.xmlWriter.WriteStartElement("SerialNumber"); //Starting SerialNumber tag
                        //thisProgram.xmlWriter.WriteString("2278536SERIAL2");
                        //thisProgram.xmlWriter.WriteEndElement();//Closing SerialNumber Tag

                        //thisProgram.xmlWriter.WriteStartElement("SerialNumber"); //Starting SerialNumber tag
                        //thisProgram.xmlWriter.WriteString("2278536SERIAL3");
                        //thisProgram.xmlWriter.WriteEndElement();//Closing SerialNumber Tag

                        //thisProgram.xmlWriter.WriteStartElement("SerialNumber"); //Starting SerialNumber tag
                        //thisProgram.xmlWriter.WriteString("2278536SERIAL4");
                        //thisProgram.xmlWriter.WriteEndElement();//Closing SerialNumber Tag

                        //thisProgram.xmlWriter.WriteStartElement("SerialNumber"); //Starting SerialNumber tag
                        //thisProgram.xmlWriter.WriteString("2278536SERIAL5");
                        //thisProgram.xmlWriter.WriteEndElement();//Closing SerialNumber Tag

                        thisProgram.xmlWriter.WriteEndElement();//Closing InvoiceDetailItemReference Tag


                       
                        thisProgram.xmlWriter.WriteStartElement("SubtotalAmount"); //Starting SubtotalAmount tag
                        thisProgram.xmlWriter.WriteStartElement("Money"); //Starting Money tag
                        thisProgram.xmlWriter.WriteAttributeString("currency", "AUD");
                        thisProgram.xmlWriter.WriteString(NetAmount.ToString());
                        thisProgram.xmlWriter.WriteEndElement();//Closing Money Tag
                        thisProgram.xmlWriter.WriteEndElement();//Closing SubtotalAmount Tag

                     

                        thisProgram.xmlWriter.WriteStartElement("Tax"); //Starting Tax tag

                        thisProgram.xmlWriter.WriteStartElement("Money"); //Starting Money tag
                        thisProgram.xmlWriter.WriteAttributeString("currency", "AUD");
                        thisProgram.xmlWriter.WriteString(TaxAmount.ToString());
                        thisProgram.xmlWriter.WriteEndElement();//Closing Money Tag

                        thisProgram.xmlWriter.WriteStartElement("Description"); //Starting Description tag
                        thisProgram.xmlWriter.WriteAttributeString("xml", "lang", null, "en-AU");
                        thisProgram.xmlWriter.WriteEndElement();//Closing Description Tag

                        thisProgram.xmlWriter.WriteStartElement("TaxDetail"); //Starting TaxDetail tag
                        thisProgram.xmlWriter.WriteAttributeString("category", "gst");
                        thisProgram.xmlWriter.WriteAttributeString("percentageRate", "10.00");

                        thisProgram.xmlWriter.WriteStartElement("TaxableAmount"); //Starting TaxableAmount tag
                        thisProgram.xmlWriter.WriteStartElement("Money"); //Starting Money tag
                        thisProgram.xmlWriter.WriteAttributeString("currency", "AUD");
                        thisProgram.xmlWriter.WriteString(NetAmount.ToString());
                        thisProgram.xmlWriter.WriteEndElement();//Closing Money Tag
                        thisProgram.xmlWriter.WriteEndElement();//Closing TaxableAmount Tag

                        thisProgram.xmlWriter.WriteStartElement("TaxAmount"); //Starting TaxAmount tag
                        thisProgram.xmlWriter.WriteStartElement("Money"); //Starting Money tag
                        thisProgram.xmlWriter.WriteAttributeString("currency", "AUD");
                        thisProgram.xmlWriter.WriteString(TaxAmount.ToString());
                        thisProgram.xmlWriter.WriteEndElement();//Closing Money Tag
                        thisProgram.xmlWriter.WriteEndElement();//Closing TaxAmount Tag                        

                        thisProgram.xmlWriter.WriteEndElement();//Closing TaxDetail Tag

                        thisProgram.xmlWriter.WriteEndElement();//Closing Tax Tag

                   

                        thisProgram.xmlWriter.WriteStartElement("GrossAmount"); //Starting GrossAmount tag
                        thisProgram.xmlWriter.WriteStartElement("Money"); //Starting Money tag
                        thisProgram.xmlWriter.WriteAttributeString("currency", "AUD");
                        thisProgram.xmlWriter.WriteString(GrossAmount.ToString());
                        thisProgram.xmlWriter.WriteEndElement();//Closing Money Tag
                        thisProgram.xmlWriter.WriteEndElement();//Closing GrossAmount Tag

                        //thisProgram.xmlWriter.WriteStartElement("InvoiceItemModifications"); //Starting InvoiceItemModifications tag
                        //thisProgram.xmlWriter.WriteStartElement("Modification"); //Starting Modification tag

                        //thisProgram.xmlWriter.WriteStartElement("AdditionalCost"); //Starting AdditionalCost tag
                        //thisProgram.xmlWriter.WriteStartElement("Money"); //Starting Money tag
                        //thisProgram.xmlWriter.WriteAttributeString("currency", "AUD");
                        //thisProgram.xmlWriter.WriteString("10");
                        //thisProgram.xmlWriter.WriteEndElement();//Closing Money Tag
                        //thisProgram.xmlWriter.WriteEndElement();//Closing AdditionalCost Tag

                        thisProgram.xmlWriter.WriteStartElement("NetAmount"); //Starting GrossAmount tag
                        thisProgram.xmlWriter.WriteStartElement("Money"); //Starting Money tag
                        thisProgram.xmlWriter.WriteAttributeString("currency", "AUD");
                        thisProgram.xmlWriter.WriteString(NetAmount.ToString());
                        thisProgram.xmlWriter.WriteEndElement();//Closing Money Tag
                        thisProgram.xmlWriter.WriteEndElement();//Closing GrossAmount Tag



                        thisProgram.xmlWriter.WriteEndElement();//Closing InvoiceDetailItem Tag


                        }

                    

                    #endregion InvoiceDetailItem

                    thisProgram.xmlWriter.WriteEndElement();//Closing InvoiceDetailOrder Tag

                    #endregion Invoice Detail Order

                    string Total_Inv_NetAmt = null;
                    string Total_Inv_TaxAmount = null;
                    string Total_Inv_GrossAmount = null;
                    string Total_Inv_TaxPercentage = null;
                    string Total_Inv_TaxCategory = null;


                    string selectInvoiceSummaryData = ConfigurationManager.AppSettings["selectInvoiceSummaryData"].ToString();
                    string strInvoiceSummaryData = selectInvoiceSummaryData + "'" + orderNum + "'";
                    SqlDataAdapter dtInvoiceSummaryData = new SqlDataAdapter(strInvoiceSummaryData, con);
                    DataSet dsInvoiceSummaryData = new DataSet();
                    dtInvoiceSummaryData.Fill(dsInvoiceSummaryData);
                    if (dsInvoiceSummaryData.Tables[0].Rows.Count == 0)
                        {
                        throw new Exception("NO Data in View_INV_Summary, Please check the data and run the exe again");
                        }

                    Total_Inv_NetAmt = dsInvoiceSummaryData.Tables[0].Rows[0]["Total_Inv_NetAmt"].ToString();
                    Total_Inv_TaxAmount = dsInvoiceSummaryData.Tables[0].Rows[0]["Total_Inv_TaxAmount"].ToString();
                    Total_Inv_GrossAmount = dsInvoiceSummaryData.Tables[0].Rows[0]["Total_Inv_GrossAmount"].ToString();
                    Total_Inv_TaxPercentage = dsInvoiceSummaryData.Tables[0].Rows[0]["Total_Inv_TaxPercentage"].ToString();
                    Total_Inv_TaxCategory = dsInvoiceSummaryData.Tables[0].Rows[0]["Total_Inv_TaxCategory"].ToString();

                    #region InvoiceDetailSummary

                    thisProgram.xmlWriter.WriteStartElement("InvoiceDetailSummary"); //Starting InvoiceDetailSummary tag

                    thisProgram.xmlWriter.WriteStartElement("SubtotalAmount"); //Starting SubTotalAmount tag
                    thisProgram.xmlWriter.WriteStartElement("Money"); //Starting Money tag
                    thisProgram.xmlWriter.WriteAttributeString("currency", "AUD");
                    thisProgram.xmlWriter.WriteString(Total_Inv_NetAmt);
                    thisProgram.xmlWriter.WriteEndElement();//Closing Money Tag
                    thisProgram.xmlWriter.WriteEndElement();//Closing SubTotalAmount Tag

                    thisProgram.xmlWriter.WriteStartElement("Tax"); //Starting Tax tag

                    thisProgram.xmlWriter.WriteStartElement("Money"); //Starting Money tag
                    thisProgram.xmlWriter.WriteAttributeString("currency", "AUD");
                    thisProgram.xmlWriter.WriteString(Total_Inv_TaxAmount);
                    thisProgram.xmlWriter.WriteEndElement();//Closing Money Tag

                    thisProgram.xmlWriter.WriteStartElement("Description"); //Starting Description tag
                    thisProgram.xmlWriter.WriteAttributeString("xml", "lang", null, "en-AU");
                    thisProgram.xmlWriter.WriteEndElement();//Closing Description Tag

                    thisProgram.xmlWriter.WriteStartElement("TaxDetail"); //Starting TaxDetail tag
                    thisProgram.xmlWriter.WriteAttributeString("percentageRate", Total_Inv_TaxPercentage);
                    thisProgram.xmlWriter.WriteAttributeString("category", Total_Inv_TaxCategory);

                    thisProgram.xmlWriter.WriteStartElement("TaxableAmount"); //Starting TaxableAmount tag
                    thisProgram.xmlWriter.WriteStartElement("Money"); //Starting Money tag
                    thisProgram.xmlWriter.WriteAttributeString("currency", "AUD");
                    thisProgram.xmlWriter.WriteString(Total_Inv_NetAmt);
                    thisProgram.xmlWriter.WriteEndElement();//Closing Money Tag
                    thisProgram.xmlWriter.WriteEndElement();//Closing TaxableAmount Tag

                    thisProgram.xmlWriter.WriteStartElement("TaxAmount"); //Starting TaxAmount tag
                    thisProgram.xmlWriter.WriteStartElement("Money"); //Starting Money tag
                    thisProgram.xmlWriter.WriteAttributeString("currency", "AUD");
                    thisProgram.xmlWriter.WriteString(Total_Inv_TaxAmount);
                    thisProgram.xmlWriter.WriteEndElement();//Closing Money Tag
                    thisProgram.xmlWriter.WriteEndElement();//Closing TaxAmount Tag                    

                    thisProgram.xmlWriter.WriteEndElement();//Closing TaxDetail Tag
                    thisProgram.xmlWriter.WriteEndElement();//Closing Tax Tag

                    thisProgram.xmlWriter.WriteStartElement("GrossAmount"); //Starting GrossAmount tag
                    thisProgram.xmlWriter.WriteStartElement("Money"); //Starting Money tag
                    thisProgram.xmlWriter.WriteAttributeString("currency", "AUD");
                    
                    thisProgram.xmlWriter.WriteString(Total_Inv_GrossAmount);
                    thisProgram.xmlWriter.WriteEndElement();//Closing Money Tag
                    thisProgram.xmlWriter.WriteEndElement();//Closing GrossAmount Tag


                    thisProgram.xmlWriter.WriteStartElement("NetAmount"); //Starting NetAmount tag
                    thisProgram.xmlWriter.WriteStartElement("Money"); //Starting Money tag
                    thisProgram.xmlWriter.WriteAttributeString("currency", "AUD");
                    thisProgram.xmlWriter.WriteString(Total_Inv_NetAmt);
                    thisProgram.xmlWriter.WriteEndElement();//Closing Money Tag
                    thisProgram.xmlWriter.WriteEndElement();//Closing NetAmount Tag

                    thisProgram.xmlWriter.WriteStartElement("DueAmount"); //Starting DueAmount tag
                    thisProgram.xmlWriter.WriteStartElement("Money"); //Starting Money tag
                    thisProgram.xmlWriter.WriteAttributeString("currency", "AUD");

                    thisProgram.xmlWriter.WriteString(Total_Inv_GrossAmount);
                    thisProgram.xmlWriter.WriteEndElement();//Closing Money Tag
                    thisProgram.xmlWriter.WriteEndElement();//Closing DueAmount Tag


                    //thisProgram.xmlWriter.WriteStartElement("InvoiceHeaderModifications"); //Starting InvoiceHeaderModifications tag
                    //thisProgram.xmlWriter.WriteStartElement("Modification"); //Starting Modification tag

                    //thisProgram.xmlWriter.WriteStartElement("AdditionalCost"); //Starting AdditionalCost tag
                    //thisProgram.xmlWriter.WriteStartElement("Money"); //Starting Money tag
                    //thisProgram.xmlWriter.WriteAttributeString("currency", "AUD");
                    //thisProgram.xmlWriter.WriteString("0");
                    //thisProgram.xmlWriter.WriteEndElement();//Closing Money Tag
                    //thisProgram.xmlWriter.WriteEndElement();//Closing AdditionalCost Tag

                    //thisProgram.xmlWriter.WriteStartElement("Tax"); //Starting Tax tag

                    //thisProgram.xmlWriter.WriteStartElement("Money"); //Starting Money tag
                    //thisProgram.xmlWriter.WriteAttributeString("currency", "AUD");
                    //thisProgram.xmlWriter.WriteString("0");
                    //thisProgram.xmlWriter.WriteEndElement();//Closing Money Tag

                    //thisProgram.xmlWriter.WriteStartElement("Description"); //Starting Description tag
                    //thisProgram.xmlWriter.WriteAttributeString("xml", "lang", null, "en-AU");
                    //thisProgram.xmlWriter.WriteEndElement();//Closing Description Tag

                    //thisProgram.xmlWriter.WriteStartElement("TaxDetail"); //Starting TaxDetail tag
                    //thisProgram.xmlWriter.WriteAttributeString("xml", "lang", null, "en-AU");

                    //thisProgram.xmlWriter.WriteStartElement("TaxableAmount"); //Starting TaxableAmount tag
                    //thisProgram.xmlWriter.WriteStartElement("Money"); //Starting Money tag
                    //thisProgram.xmlWriter.WriteAttributeString("currency", "AUD");
                    //thisProgram.xmlWriter.WriteString("0");
                    //thisProgram.xmlWriter.WriteEndElement();//Closing Money Tag
                    //thisProgram.xmlWriter.WriteEndElement();//Closing TaxableAmount Tag

                    //thisProgram.xmlWriter.WriteStartElement("TaxAmount"); //Starting TaxAmount tag
                    //thisProgram.xmlWriter.WriteStartElement("Money"); //Starting Money tag
                    //thisProgram.xmlWriter.WriteAttributeString("currency", "AUD");
                    //thisProgram.xmlWriter.WriteString("0");
                    //thisProgram.xmlWriter.WriteEndElement();//Closing Money Tag
                    //thisProgram.xmlWriter.WriteEndElement();//Closing TaxAmount Tag

                    //thisProgram.xmlWriter.WriteStartElement("Description"); //Starting Description tag
                    //thisProgram.xmlWriter.WriteAttributeString("xml", "lang", null, "en-AU");
                    //thisProgram.xmlWriter.WriteEndElement();//Closing Description Tag

                    //thisProgram.xmlWriter.WriteEndElement();//Closing TaxDetail Tag

                    //thisProgram.xmlWriter.WriteEndElement();//Closing Tax Tag

                    ////thisProgram.xmlWriter.WriteStartElement("ModificationDetail"); //Starting ModificationDetail tag
                    ////thisProgram.xmlWriter.WriteAttributeString("name", "Freight");
                    ////thisProgram.xmlWriter.WriteStartElement("Description"); //Starting Description tag
                    ////thisProgram.xmlWriter.WriteAttributeString("xml", "lang", null, "en-AU");
                    ////thisProgram.xmlWriter.WriteEndElement();//Closing Description Tag
                    ////thisProgram.xmlWriter.WriteEndElement();//Closing ModificationDetail Tag

                    //thisProgram.xmlWriter.WriteEndElement();//Closing Modification Tag
                    //thisProgram.xmlWriter.WriteEndElement();//Closing InvoiceHeaderModifications Tag

                    //thisProgram.xmlWriter.WriteStartElement("TotalCharges"); //Starting TotalCharges tag
                    //thisProgram.xmlWriter.WriteStartElement("Money"); //Starting Money tag
                    //thisProgram.xmlWriter.WriteAttributeString("currency", "AUD");
                    //thisProgram.xmlWriter.WriteString("0");
                    //thisProgram.xmlWriter.WriteEndElement();//Closing Money Tag
                    //thisProgram.xmlWriter.WriteEndElement();//Closing TotalCharges Tag

                    //thisProgram.xmlWriter.WriteStartElement("TotalAmountWithoutTax"); //Starting TotalAmountWithoutTax tag
                    //thisProgram.xmlWriter.WriteStartElement("Money"); //Starting Money tag
                    //thisProgram.xmlWriter.WriteAttributeString("currency", "AUD");
                    //thisProgram.xmlWriter.WriteString("0");
                    //thisProgram.xmlWriter.WriteEndElement();//Closing Money Tag
                    //thisProgram.xmlWriter.WriteEndElement();//Closing TotalAmountWithoutTax Tag

                    //thisProgram.xmlWriter.WriteStartElement("NetAmount"); //Starting NetAmount tag
                    //thisProgram.xmlWriter.WriteStartElement("Money"); //Starting Money tag
                    //thisProgram.xmlWriter.WriteAttributeString("currency", "AUD");
                    //thisProgram.xmlWriter.WriteString("0");
                    //thisProgram.xmlWriter.WriteEndElement();//Closing Money Tag
                    //thisProgram.xmlWriter.WriteEndElement();//Closing NetAmount Tag

                    //thisProgram.xmlWriter.WriteStartElement("DueAmount"); //Starting NetAmount tag
                    //thisProgram.xmlWriter.WriteStartElement("Money"); //Starting Money tag
                    //thisProgram.xmlWriter.WriteAttributeString("currency", "AUD");
                    //thisProgram.xmlWriter.WriteString("0");
                    //thisProgram.xmlWriter.WriteEndElement();//Closing Money Tag
                    //thisProgram.xmlWriter.WriteEndElement();//Closing NetAmount Tag


                    thisProgram.xmlWriter.WriteEndElement();//Closing InvoiceDetailSummary Tag

                    

                    #endregion InvoiceDetailSummary

                    thisProgram.xmlWriter.WriteEndElement();// Closing InvoiceDetailRequest Tag

                    #endregion   Invoice Detail Request Tag


                    thisProgram.xmlWriter.WriteEndElement();//Closing Request Tag



                    thisProgram.xmlWriter.WriteEndDocument();



                    thisProgram.xmlWriter.Flush();
                    thisProgram.xmlWriter.Close();
                    thisProgram.xmlWriter.Dispose();

                    thisProgram.PostInvoice(orderNum, CatalogueSeqID);

                    thisProgram.MoveFiles(orderNum);

                    thisProgram.UpdateINVFlag(orderNum);

                    }
               
                }
            catch (Exception ex)
                {
                ExceptionLogging.SendExcepToDB(ex, "CreateStaplesInvoiceFiles");
                }
            }

      


        private string GenerateUniquePayloadID()
            {
            Guid g;
            // Create and display the value of two GUIDs.
            g = Guid.NewGuid();
            return g.ToString();
            }


        private void PostInvoice(string _orderFile, int _catalogueSeqID)
            {
            WebRequest req = null;
            WebResponse rsp = null;
            // string fileName = "C:\\Staples\\cXMLFiles\\Invoice.xml";
            string fileName = invoiceCreationPath + _orderFile + "_INV.xml";
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
                    SaveINVResponse(responseString, _orderFile, _catalogueSeqID);

                    rsp.Close();
                    }
                }
            catch (WebException webEx) { }
            catch (Exception ex) { ExceptionLogging.SendExcepToDB(ex, "CreateStaplesInvoiceFiles"); }
            finally
                {
                if (req != null) req.GetRequestStream().Close();
                if (rsp != null) rsp.GetResponseStream().Close();
                }
            }


        private void SaveINVResponse(string _response, string _orderNumber, int _catalogueSeqID)
            {

            try
                {
                XmlDocument invResponseDOC = new XmlDocument();
                invResponseDOC.LoadXml(_response);

                string timeStamp = string.Empty;
                string payloadID = string.Empty;
                int statusCode = 0;
                string statusText = string.Empty;
                string statusMessage = string.Empty;
                string createdDate = null;

                if (invResponseDOC.SelectSingleNode("//cXML") != null)
                    {

                    timeStamp = (invResponseDOC.SelectSingleNode("//cXML").Attributes["timestamp"].Value == null) ? null : invResponseDOC.SelectSingleNode("//cXML").Attributes["timestamp"].Value;

                    }
                else
                    {


                    }

                if (invResponseDOC.SelectSingleNode("//cXML") != null)
                    {

                    payloadID = (invResponseDOC.SelectSingleNode("//cXML").Attributes["payloadID"].Value == null) ? null : invResponseDOC.SelectSingleNode("//cXML").Attributes["payloadID"].Value;

                    }
                else
                    {


                    }

                if (invResponseDOC.SelectSingleNode("//cXML/Response/Status") != null)
                    {


                    statusCode = int.Parse((invResponseDOC.SelectSingleNode("//cXML/Response/Status").Attributes["code"].Value == null) ? null : invResponseDOC.SelectSingleNode("//cXML/Response/Status").Attributes["code"].Value);

                    }
                else
                    {


                    }

                if (invResponseDOC.SelectSingleNode("//cXML/Response/Status") != null)
                    {


                    statusText = (invResponseDOC.SelectSingleNode("//cXML/Response/Status").Attributes["text"].Value == null) ? null : invResponseDOC.SelectSingleNode("//cXML/Response/Status").Attributes["text"].Value;

                    }
                else
                    {


                    }

                if (invResponseDOC.SelectSingleNode("//cXML/Response/Status") != null)
                    {


                    statusMessage = invResponseDOC.SelectSingleNode("//cXML/Response/Status").InnerText == "" ? null : invResponseDOC.SelectSingleNode("//cXML/Response/Status").InnerText;

                    }
                else
                    {


                    }

                createdDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

                InsertINVResponseDetails(_orderNumber, timeStamp, payloadID, statusCode, statusText, statusMessage, createdDate, _catalogueSeqID);
                }
            catch (Exception ex)
                {

                ExceptionLogging.SendExcepToDB(ex, "CreateStaplesInvoiceFiles - SaveINVResponse");
                }


            }

        private void InsertINVResponseDetails(string _orderNumber, string _timeStamp, string _payloadID, int _statusCode, string _statusText, string _statusMessage, string _CreatredDate, int _catalogueSeqID)
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

                ExceptionLogging.SendExcepToDB(ex, "CreateStaplesInvoiceFiles - InsertINVResponseDetails");
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

        public void MoveFiles(string _orderFileName)
            {

            try
                {

                string sourcePath = string.Empty;
                string destinationPath = string.Empty;
                string orderFile = string.Empty;

                orderFile = _orderFileName + "_INV.xml";

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
                ExceptionLogging.SendExcepToDB(ex, "CreateStaplesINVConfirmation - MoveFiles");
                }


            }

        public void UpdateINVFlag(string _orderNumber)
            {
            try
                {
                SqlConnection conn = new SqlConnection(connectionString);
                string query = ConfigurationManager.AppSettings["updateINVFlag"].ToString();
                string updateQuery = query + "'" + _orderNumber + "'";
                SqlCommand cmd = new SqlCommand(updateQuery, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();

                }
            catch (Exception ex)
                {
                ExceptionLogging.SendExcepToDB(ex, "CreateStaplesINVFiles - UpdateINVFlag");
                }



            }


    
        }
    }
