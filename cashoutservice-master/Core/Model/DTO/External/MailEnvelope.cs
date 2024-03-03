using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Model.DTO.External {
    public class MailEnvelope {
        public string[] toName { get; set; }
        public string[] toAddress { get; set; }
        public string body { get; set; }
        public string subject { get; set; }
        public string[] attachment { get; set; }
        public bool bodyIsPlainText { get; set; } = false;
    }
}
