using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Libraries {
    public class JPOSInterpreter {
        private readonly List<JPOSField> definition;
        public List<int> maps { get; private set; }
        public JPOSInterpreter(string file) {

        }

        public void unpackBinary(string data) {
            maps = new List<int>();
            var primaryMap = hextobin(data[4..20]);
            string secondaryMap = "";
            if (primaryMap.StartsWith("1")) {
                secondaryMap = hextobin(data[20..36]);
            }
            int counter = 1;
            foreach(char c in primaryMap) {
                if (c == '1')
                    maps.Add(counter);
                counter++;
            }
            foreach(char c in secondaryMap) {
                if (c == '1')
                    maps.Add(counter);
                counter++;
            }
        }

        public string hextobin(byte bite) {
            return Convert.ToString(bite, 2).PadLeft(8, '0');
        }
        public string hextobin(string hex) {
            string binarystring = string.Join(string.Empty,
              hex.Select(
                c => Convert.ToString(Convert.ToInt32(c.ToString(), 16), 2).PadLeft(4, '0')
              )
            );
            return binarystring;
        }
    }
    public class JPOSField {
        public int id { get; set; }
        public int length { get; set; }
        public string name { get; set; }
        public string @class { get; set; }
    }
    public class JPOSDefinition {
        public List<JPOSField> isoFields { get; set; }
    }
}
