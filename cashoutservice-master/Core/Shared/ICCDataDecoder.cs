using BerTlv;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Shared {
    public class ICCDataTags {
        public string tag { get; private set; }
        public ICCDataTags(string tag) {
            this.tag = tag;
        }
        public static ICCDataTags APPLICATION_INTERCHANGE_PROFILE { 
            get {
                return new ICCDataTags("82");
            } 
        }
        public static ICCDataTags ATC {
            get {
                return new ICCDataTags("9F36");
            }
        }
        public static ICCDataTags CID {
            get {
                return new ICCDataTags("9F27");
            }
        }
        public static ICCDataTags CVM_RESULT {
            get {
                return new ICCDataTags("9F34");
            }
        }
        public static ICCDataTags IAD {
            get {
                return new ICCDataTags("9F10");
            }
        }
        public static ICCDataTags TERMINAL_CAPABILITIES {
            get {
                return new ICCDataTags("9F33");
            }
        }
        public static ICCDataTags DF_NAME {
            get {
                return new ICCDataTags("84");
            }
        }
        public static ICCDataTags TERMINAL_TYPE {
            get {
                return new ICCDataTags("9F35");
            }
        }
        public static ICCDataTags UNPREDICTABLE_NUMBER {
            get {
                return new ICCDataTags("9F37");
            }
        }
        public static ICCDataTags AMOUNT {
            get {
                return new ICCDataTags("9F03");
            }
        }
        public static ICCDataTags AMOUNT_AUTHORISED {
            get {
                return new ICCDataTags("9F02");
            }
        }
        public static ICCDataTags PAN_SN {
            get {
                return new ICCDataTags("5F34");
            }
        }
        public static ICCDataTags TCC {
            get {
                return new ICCDataTags("9F1A");
            }
        }
        public static ICCDataTags T_CURRECNCY_CODE {
            get {
                return new ICCDataTags("5F2A");
            }
        }
        public static ICCDataTags TRANSACTION_TYPE {
            get {
                return new ICCDataTags("9C");
            }
        }
        public static ICCDataTags TRANSACTION_DATE {
            get {
                return new ICCDataTags("9A");
            }
        }
        public static ICCDataTags APPLICATION_CRYPTOGRAM {
            get {
                return new ICCDataTags("9F26");
            }
        }
        public static ICCDataTags TRANSACTION_SEQUENCE_COUNTER {
            get {
                return new ICCDataTags("9F41");
            }
        }
        public static ICCDataTags TERMINAL_VERIFICATION_RESULT {
            get {
                return new ICCDataTags("95");
            }
        }
        public static ICCDataTags IFD_SN {
            get {
                return new ICCDataTags("9F1E");
            }
        }
    }
    public class ICCDataDecoder {
        private string iccData { get;  }
        Dictionary<string, Tlv> tlvObj = null;
        public ICCDataDecoder(string data) {
            iccData = data;
        }

        public string? getCode(string code) {
            loadTlv();
            Tlv tlvData;
            tlvObj.TryGetValue(code, out tlvData);
            if (tlvData == default)
                return null;
            return tlvData.HexValue;
        }
        public string? getCode(ICCDataTags tag) {
            string code = tag.tag;
            loadTlv();
            Tlv? tlvData;
            tlvObj.TryGetValue(code, out tlvData);
            if (tlvData == default)
                return null;
            return tlvData.HexValue;
        }
        private void loadTlv() {
            if (tlvObj != default)
                return;
            tlvObj = new Dictionary<string, Tlv>();
            ICollection<Tlv> tlvs = Tlv.ParseTlv(this.iccData);
            foreach (Tlv tlv in tlvs) {
                tlvObj.Add(tlv.HexTag, tlv);
            }
        }
    }
}
