using Core.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Core.Model.DTO.Response {
    public class EMVStandardResponse {

    }

    public class KimonoPurchaseResponse {
        [XmlRoot(ElementName = "hostEmvData")]
        public class HostEmvData {
            [XmlElement(ElementName = "AmountAuthorized")]
            public string AmountAuthorized { get; set; }
            [XmlElement(ElementName = "AmountOther")]
            public string AmountOther { get; set; }
            [XmlElement(ElementName = "atc")]
            public string Atc { get; set; }
            [XmlElement(ElementName = "iad")]
            public string Iad { get; set; }
            [XmlElement(ElementName = "rc")]
            public string Rc { get; set; }
        }

        [XmlRoot(ElementName = "transferResponse")]
        public class TransferResponse {
            [XmlElement(ElementName = "description")]
            public string Description { get; set; }      
            public NIBSSStandardResponseCode statusMessageObject {
                get {
                    return NIBSSStandardResponseCode.findResponse(this.Field39);
                }
            }
            public string statusCode { 
                get {
                    return Field39;
                } 
            }
            public string responseCode { get; set; }
            public string responseMessage { get; set; }
            public string tmsResponse { get; set; }
            [XmlElement(ElementName = "field39")]
            public string Field39 { get; set; }
            [XmlElement(ElementName = "authId")]
            public string AuthId { get; set; }
            [XmlElement(ElementName = "hostEmvData")]
            public HostEmvData HostEmvData { get; set; }
            [XmlElement(ElementName = "referenceNumber")]
            public string ReferenceNumber { get; set; }
            [XmlElement(ElementName = "stan")]
            public string Stan { get; set; }
            [XmlElement(ElementName = "transactionChannelName")]
            public string TransactionChannelName { get; set; }
            [XmlElement(ElementName = "wasReceive")]
            public string WasReceive { get; set; }
            [XmlElement(ElementName = "wasSend")]
            public string WasSend { get; set; }
            public string terminalID { get; set; }
            public string makedPAN { get; set; }
            public string Reference { get; set; }
            public string MerchantName { get; set; }
            public string MerchantAddress { get; set; }
            public string ProcessingCode { get; set; }
            public string merchantID { get; set; }
            public string cardExpiry { get; set; }
            public long amount { get; set; }
        }

        [XmlRoot(ElementName = "transactionRequeryResponse")]
        public class TransactionRequeryResponse {
            [XmlElement(ElementName = "description")]
            public string Description { get; set; }
            [XmlElement(ElementName = "field39")]
            public string Field39 { get; set; }
            [XmlElement(ElementName = "referenceNumber")]
            public string ReferenceNumber { get; set; }
            [XmlElement(ElementName = "stan")]
            public string Stan { get; set; }
        }
    }
}
