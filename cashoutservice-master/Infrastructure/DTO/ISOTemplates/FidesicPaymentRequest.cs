using Google.Protobuf.WellKnownTypes;
using MSBuild.Community.Tasks.Subversion;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.DTO.ISOTemplates {
    public class FidesicPaymentRequest {
        public string field2 { get; set; }
        public string field3 { get; set; }
        public string field4 { get; set; }
        public string field7 { get; set; }
        public string field11 { get; set; }
        public string field12 { get; set; }
        public string field13 { get; set; }
        public string field14 { get; set; }
        public string field18 { get; set; }
        public string field22 { get; set; }
        public string field23 { get; set; }
        public string field25 { get; set; }
        public string field26 { get; set; }
        public string field28 { get; set; }
        public string field35 { get; set; }
        public string field32 { get; set; }
        public string field40 { get; set; }
        public string field37 { get; set; }
        public string field41 { get; set; }
        public string field42 { get; set; }
        public string field49 { get; set; }
        public string field43 { get; set; }
        public string field52 { get; set; }
        public string field128 { get; set; }
        public string field123 { get; set; }
        public string field59 { get; set; }
        public string field55 { get; set; }
        public string sessionId { get; set; }
        public string terminalId { get; set; }
        public string terminalSerial { get; set; }
        public void add(int field, string data) {
            string propName = $"field{field}";
            var propertyInfo = this.GetType().GetProperty(propName);
            if (propertyInfo != null){
                propertyInfo.SetValue(this, data, null);
            }
        }
        public void add(string field, string data) {            
            var propertyInfo = this.GetType().GetProperty(field);
            if (propertyInfo != null) {
                propertyInfo.SetValue(this, data, null);
            }
        }
    }
}
