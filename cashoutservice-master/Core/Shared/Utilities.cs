using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

namespace Core.Shared {
    public class Utilities {
        public static bool isAlphaNumeric(string str) {
            if (string.IsNullOrEmpty(str))
                return false;
            Regex r = new Regex("^(?=.+\\d)(?=.+[a-zA-Z]).*$");
            return r.IsMatch(str);
        }

        public static bool isValidUsername(string str) {
            if (string.IsNullOrEmpty(str))
                return false;
            Regex r = new Regex("^[a-zA-Z0-9]*$");
            return r.IsMatch(str);
        }

        public static bool isNumeric(string str) {
            if (string.IsNullOrEmpty(str))
                return false;
            Regex r = new Regex("^[0-9]*$");
            return r.IsMatch(str);
        }
        public static bool isMD5(string str) {
            if (string.IsNullOrEmpty(str))
                return false;
            Regex r = new Regex("^[a-f0-9]{32}$");
            return r.IsMatch(str);
        }
        public static bool isHexStr(string str) {
            if (string.IsNullOrEmpty(str))
                return false;
            Regex r = new Regex("^[a-fA-F0-9]+$");
            return r.IsMatch(str);
        }
        public static bool passwordCase(string str) {
            if (string.IsNullOrEmpty(str))
                return false;
            Regex r = new Regex("^(?=.*\\d)(?=.*[A-Z])(?=.*[_.@$!#%^&()_+=]).*$");
            return r.IsMatch(str);
        }

        public static bool isPhone(string phone) {
            if (string.IsNullOrEmpty(phone))
                return false;
            Regex r = new Regex(@"^[07981]{3}[0-9]{8}|234[7981]{2}[0-9]{8}$");
            return r.IsMatch(phone);
        }

        public static bool isEmail(string email) {
            if (string.IsNullOrEmpty(email))
                return false;
            Regex r = new Regex(@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$");
            return r.IsMatch(email);
        }

        public static long getTimeStamp(
                        int year, int month, int day,
                        int hour, int minute, int second, int milliseconds) {
            DateTime value = new DateTime(year, month, day);
            var date = new DateTime(1970, 1, 1, 0, 0, 0, value.Kind);
            var unixTimestamp = System.Convert.ToInt64((value - date).TotalSeconds);
            return unixTimestamp;
        }
        public static long getTimeStamp(DateTime value) {
            var date = new DateTime(1970, 1, 1, 0, 0, 0, value.Kind);
            var unixTimestamp = Convert.ToInt64((value - date).TotalSeconds);
            return unixTimestamp;
        }
        public static (DateTime modernDate, long unixTimestamp, long unixTimestampMS) getTodayDate() {
            long totalSeconds = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            long totalmSecs = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(totalSeconds).ToLocalTime();
            return (modernDate: dtDateTime, unixTimestamp: totalSeconds, unixTimestampMS: totalmSecs);
        }
        public static DateTime unixTimeStampToDateTime(double unixTimeStamp, bool dateOnly = false) {
            try {
                System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
                if (!dateOnly)
                    return dtDateTime;
                return dtDateTime.Date;
            } catch (Exception) {
                return new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            }
        }

        public static JObject asJObject(object obj) {
            JObject json = JObject.Parse(JsonConvert.SerializeObject(obj));
            return json;
        }
        public static string objectToString(object obj) {
            try {
                JObject json = JObject.FromObject(obj);
                return json.ToString();
            } catch { }
            return null;
        }
        public static T toObject<T>(string obj) {
            JObject json = JObject.Parse(obj);
            return json.ToObject<T>();
        }
        public static string findString(JObject json, string needle) {
            try {
                return json[needle].ToString();
            } catch {
                return null;
            }
        }
        public static JArray findArray(JObject json, string needle) {
            try {
                return JArray.Parse(json[needle].ToString());
            } catch {
                return null;
            }
        }
        public static JObject findObj(JObject json, string needle) {
            try {
                return JObject.Parse(json[needle].ToString());
            } catch {
                return null;
            }
        }
        public static double? findNumber(JObject json, string needle) {
            try {
                return double.Parse(json[needle].ToString());
            } catch {
                return null;
            }
        }

        public static string validBase64(string raw) {
            int pads = raw.Length % 4;
            if (pads > 0) {
                raw += new string('=', 4 - pads);
            }
            return raw;
        }
        public static string base64Encode(string plainText) {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }
        public static string base64Decode(string base64EncodedData) {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }
        public static bool noNullValue(object obj) {
            foreach (PropertyInfo prop in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
                var data = prop.GetValue(obj);
                if (data == null)
                    return false;
            }
            return true;
        }

        public static bool noNullValue(object obj, List<string> properties) {
            try {
                foreach (string propertyName in properties) {
                    var data = obj.GetType().GetProperty(propertyName).GetValue(obj, null);
                    if (data == null)
                        return false;
                }
                return true;
            } catch {
                return false;
            }
        }

        public static long getFileSize(IFormFile file) {
            return file.Length;
        }

        public static string getFileFormat(IFormFile file) {
            var extension = Path.GetExtension(file.FileName);
            return extension;
        }
        public static byte[] stringToByteArray(string hex) {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
        public static (string day, string time, string transmDate) getTransactionDate() {
            string m = DateTime.Now.Month.ToString().PadLeft(2, '0');
            string d = DateTime.Now.Day.ToString().PadLeft(2, '0');
            int hour = DateTime.Now.Hour > 12 ? DateTime.Now.Hour - 12 : DateTime.Now.Hour < 1 ? 12 : DateTime.Now.Hour;
            string h = hour.ToString().PadLeft(2, '0');
            string mm = DateTime.Now.Minute.ToString().PadLeft(2, '0');
            string s = DateTime.Now.Second.ToString().PadLeft(2, '0');
            string day = m + d;
            string time = h + mm + s;
            string transmDate = day + time;
            return (day, time, transmDate);
        }
        public static string reconstructICC(string icc) {
            HashSet<string> tags = new HashSet<string> {
                "9F26", "9F27", "9F10", "9F37", "9F36", "95", "9A", "9C", "9F02", "5F2A", "82", "9F1A", "9F34", "9F33", "9F35", "84", "5F34", "9F1E", "9F41", "9F03",
            };
            StringBuilder strgs = new StringBuilder();
            ICCDataDecoder icd = new ICCDataDecoder(icc);
            foreach (string tag in tags) {
                string tagVal = icd.getCode(tag);
                if (tagVal == null)
                    continue;
                string len = (tagVal.Length / 2).ToString("X");
                tagVal = tag + len.PadLeft(2, '0') + tagVal;
                strgs.Append(tagVal);
            }
            return $"{strgs.ToString()}";
        }

        public class XMLSerializer<T> where T : class {
            public static string serialize(T obj, bool stripTag = true) {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.OmitXmlDeclaration = stripTag;
                XmlSerializer xsSubmit = new XmlSerializer(typeof(T));
                using (var sww = new StringWriter()) {
                    using (XmlWriter writer = XmlTextWriter.Create(sww, settings)) {
                        xsSubmit.Serialize(writer, obj);
                        return sww.ToString();
                    }
                }
            }
            public static T deserialize(string xml) {
                Type typeoft = typeof(T);
                XmlSerializer serializer;
                serializer = new XmlSerializer(typeoft);
                StringReader rdr = new StringReader(xml);
                var data = (T)serializer.Deserialize(rdr);
                return data;
            }
        }
    }
}
