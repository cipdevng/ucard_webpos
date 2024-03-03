using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Shared {
    public class ISO8583FieldReader {
        string _data;
        public ISO8583FieldReader(string data) {
            _data = data;
        }

        public List<string> getAvailableFields() {
            List<string> fields = new List<string>();
            string bin = Convert.ToString(Convert.ToInt64(_data.Substring(1, 16), 16), 2);
            int counter = 0;
            foreach(char c in bin) {
                counter++;
                if (c == '1')
                    continue;
                if(counter == 0) {
                    fields.Add("Secondary Bitmap");
                } else {

                }
            }
            return fields;
        }

    }
}
