using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Shared {
    public class NIBSSStandardResponseCode {
        public string code { get; private set; }
        public string message { get; private set; }
        private Dictionary<string, string> responses;
        public NIBSSStandardResponseCode(string code) {
            this.code = code;
            this.message = "UNKNOWN ERROR";
            populateResponse();
            if (string.IsNullOrEmpty(code))
                return;
            if (responses == null)
                return;
            string messageObj;
            this.responses.TryGetValue(code, out messageObj);
            this.message = messageObj ?? "UNKNOWN ERROR";
        }

        private void populateResponse() {
            responses = new Dictionary<string, string>();
            this.responses.Add("00", "Approved or completed successfully");
            this.responses.Add("01", "Refer to card issuer");
            this.responses.Add("02", "Refer to card issuer, special condition");
            this.responses.Add("03", "Invalid merchant");
            this.responses.Add("04", "Pick-up card");
            this.responses.Add("05", "Do not honor");
            this.responses.Add("06", "Error");
            this.responses.Add("07", "Pick-up card, special condition");
            this.responses.Add("08", "Honor with identification");
            this.responses.Add("09", "Request in progress");
            this.responses.Add("10", "Approved, partial");
            this.responses.Add("11", "Approved, VIP");
            this.responses.Add("12", "Invalid transaction");
            this.responses.Add("13", "Invalid amount");
            this.responses.Add("14", "Invalid card number");
            this.responses.Add("15", "No such issuer");
            this.responses.Add("16", "Approved, update track 3");
            this.responses.Add("17", "Customer cancellation");
            this.responses.Add("18", "Customer dispute");
            this.responses.Add("19", "Re-enter transaction");
            this.responses.Add("20", "Invalid response");
            this.responses.Add("21", "No action taken");
            this.responses.Add("22", "Suspected malfunction");
            this.responses.Add("23", "Unacceptable transaction fee");
            this.responses.Add("24", "File update not supported");
            this.responses.Add("25", "Unable to locate record");
            this.responses.Add("26", "Duplicate record");
            this.responses.Add("27", "File update edit error");
            this.responses.Add("28", "File update file locked");
            this.responses.Add("29", "File update failed");
            this.responses.Add("30", "Format error");
            this.responses.Add("31", "Bank not supported");
            this.responses.Add("32", "Completed partially");
            this.responses.Add("33", "Expired card, pick-up");
            this.responses.Add("34", "Suspected fraud, pick-up");
            this.responses.Add("35", "Contact acquirer, pick-up");
            this.responses.Add("36", "Restricted card, pick-up");
            this.responses.Add("37", "Call acquirer security, pick-up");
            this.responses.Add("38", "PIN tries exceeded, pick-up");
            this.responses.Add("39", "No credit account");
            this.responses.Add("40", "Function not supported");
            this.responses.Add("41", "Lost card");
            this.responses.Add("42", "No universal account");
            this.responses.Add("43", "Stolen card");
            this.responses.Add("44", "No investment account");
            this.responses.Add("51", "Not sufficient funds");
            this.responses.Add("52", "No check account");
            this.responses.Add("53", "No savings account");
            this.responses.Add("54", "Expired card");
            this.responses.Add("55", "Incorrect PIN");
            this.responses.Add("56", "No card record");
            this.responses.Add("57", "Transaction not permitted to cardholder");
            this.responses.Add("58", "Transaction not permitted on terminal");
            this.responses.Add("59", "Suspected fraud");
            this.responses.Add("60", "Contact acquirer");
            this.responses.Add("61", "Exceeds withdrawal limit");
            this.responses.Add("62", "Restricted card");
            this.responses.Add("63", "Security violation");
            this.responses.Add("64", "Original amount incorrect");
            this.responses.Add("65", "Exceeds withdrawal frequency");
            this.responses.Add("66", "Call acquirer security");
            this.responses.Add("67", "Hard capture");
            this.responses.Add("68", "Response received too late");
            this.responses.Add("75", "PIN tries exceeded");
            this.responses.Add("77", "Intervene, bank approval required");
            this.responses.Add("78", "Intervene, bank approval required for partial amount");
            this.responses.Add("90", "Cut-off in progress");
            this.responses.Add("91", "Issuer or switch inoperative");
            this.responses.Add("92", "Routing error");
            this.responses.Add("93", "Violation of law");
            this.responses.Add("94", "Duplicate transaction");
            this.responses.Add("95", "Reconcile error");
            this.responses.Add("96", "System malfunction");
            this.responses.Add("98", "Exceeds cash limit");
        }

        public static NIBSSStandardResponseCode findResponse(string code) {
            return new NIBSSStandardResponseCode(code);
        }
    }
}
