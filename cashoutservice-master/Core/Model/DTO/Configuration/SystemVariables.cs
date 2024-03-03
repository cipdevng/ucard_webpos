using Core.Model.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Model.DTO.Configuration {
    public class SystemVariables {
        public string version { get; set; }
        public string appName { get; set; }
        public string environmentName { get; set; }
        public string siteRoot { get; set; }
        public bool debug { get; set; }
        public string emailDomainDir { get; set; }
        public int invitationExpiry { get; set; }
        public string systemKey { get; set; }
        public DBConfig MySQL { get; set; }
        public DBConfig SQLite { get; set; }
        public DBConfig MongoDB { get; set; }
        public ElasticSearch ElasticSearch { get; set; }
        public KeySalt KeySalt { get; set; }
        public JWTSettings JWTSettings { get; set; }
        public QueueServer QueueServer { get; set; }
        public Termii Termii { get; set; }
        public EmailParam EmailParam { get; set; }
        public Kimono KimonoConfig { get; set; }
        public LuxConfig LuxConfig { get; set; }
        public AzureStorage AzureStorage { get; set; }
        public GoogleCloudStorageConfig GoogleCloudStorageConfig {
            get; set;
        }
        public UPConfig UPConfig { get; set; }
        public ArcaConfig ArcaConfig { get; set; }
        public FidesicConfiguration FidesicConfig { get; set; }
    }
    public class EmailParam {
        public string fromAddress { get; set; }
        public string fromName { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string smtpServer { get; set; }
        public int smtpPort { get; set; }
    }
    public class FLWSettings {
        public string secretKey { get; set; }
        public string publicKey { get; set; }
        public string encryptionKey { get; set; }
        public string transferURI { get; set; }
        public string getBanksURI { get; set; }
        public string currency { get; set; }
        public string webhook { get; set; }
    }
    public class Termii {
        public string root { get; set; }
        public string sendSMS { get; set; }
        public string sendOTP { get; set; }
        public string verify { get; set; }
        public string api_key { get; set; }
        public string senderID { get; set; }
    }
    public class QueueServer {
        public string mqhost { get; set; }
        public string mquser { get; set; }
        public string mqpw { get; set; }
        public Jobs Jobs { get; set; }
    }
    public class Jobs {
        public string Errlog { get; set; }
    }
    public class JWTSettings {
        public string jwtSecret { get; set; }
        public bool identityExpires { get; set; } = true;
        public int identityExpiryMins { get; set; } = 1440; //1 Day
    }
    public class DBConfig {
        public string server { get; set; }
        public string port { get; set; }
        public string database { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string protocol { get; set; } = "None";
        public string sslMode { get; set; } = "None";
        public string authMechanism { get; set; }
    }
    public class GoogleCloudStorageConfig {
        public string credentialFile { get; set; }
        public string bucketName { get; set; }
    }
    public class ElasticSearch {
        public BasicAuthentication BasicAuthentication { get; set; }
        public string[] nodes { get; set; }
        public ApiKeyAuthentication ApiKeyAuthentication { get; set; }
    }
    public class BasicAuthentication {
        public string username { get; set; }
        public string password { get; set; }
    }
    public class ApiKeyAuthentication {
        public string id { get; set; }
        public string apiKey { get; set; }
    }
    public class KeySalt {
        public string salt { get; set; }
        public int saltIndex { get; set; }
    }
    public class AzureStorage {
        public string connectionString { get; set; }
        public string accountName { get; set; }
        public string accountKey { get; set; }
        public string containerName { get; set; }
        public int SASExpiryMins { get; set; }
    }

    public class Kimono {
        public List<string> terminals { get; set; }
        public string merchantID { get; set; }
        public string IPEK { get; set; }
        public string KSN { get; set; }
        public string KSNd { get; set; }
        public string currencyCode { get; set; }
        public string language { get; set; }
        public string HQAddress { get; set; }
        public string keyLabel { get; set; }
        public string destinationAccountNumber { get; set; }
        public string extendedTransactionType { get; set; }
        public string receivingInstitutionId { get; set; }
        public string uniqueID { get; set; }
        public KimonoEndpoints Endpoints { get; set; }
    }

    public class KimonoEndpoints {
        public string auth { get; set; }
        public string purchase { get; set; }
        public string transactionStatus { get; set; }
    }

    public class LuxConfig {
        public string basicUsername { get; set; }
        public string basicPassword { get; set; }
        public string postDataCode { get; set; }
        public string institutionCode { get; set; }
        public LuxEndpoint Endpoints { get; set; }
    }
    public class LuxEndpoint {
        private string _startSession, _query, _transaction, _getBalance;
        public string startSession { 
            get {
                return $"{rootURL}{_startSession}";
            }
            set {
                _startSession = value;
            }
        }
        public string query {
            get {
                return $"{rootURL}{_query}";
            }
            set {
                _query = value;
            }
        }
        public string transaction {
            get {
                return $"{rootURL}{_transaction}";
            }
            set {
                _transaction = value;
            }
        }
        public string getBalance {
            get {
                return $"{rootURL}{_getBalance}";
            }
            set {
                _getBalance = value;
            }
        }
        public string rootURL { get; set; }
    }

    public class UPConfig {
        public string ctmk { get; set; }
        public string ip { get; set; }
        public int port { get; set; }
        public string acquiringInstCode { get; set; }
        public string forwardingInstitutionID { get; set; }
        public string posPinCaptureCode { get; set; }
        public string posCondition { get; set; }
        public string posDataCode { get; set; }
        public int readTimeout { get; set; } = 30000;
        public int writeTimeout { get; set; } = 30000;
    }

    public class ArcaConfig {
        public string ctmk { get; set; }
        public string ip { get; set; }
        public int port { get; set; }
        public string acquiringInstCode { get; set; }
        public string forwardingInstitutionID { get; set; }
        public string posPinCaptureCode { get; set; }
        public string posCondition { get; set; }
        public string posDataCode { get; set; }
        public Parameters terminalParameter { get; set; }
        public int readTimeout { get; set; } = 30000;
        public int writeTimeout { get; set; } = 30000;
        public string tmkDecryptEndpoint { get; set; }
        public string tmkDecryptionAPIKey { get; set; }
    }

    public class FidesicConfiguration {
        public string bearerToken { get; set; }
        public string clientID { get; set; }
        public string clientSecret { get; set; }
        public string posCondition { get; set; }
        public string posDataCode { get; set; }
        public string posPinCaptureCode { get; set; }
        public string acquiringInstCode { get; set; }
        public string forwardingInstitutionID { get; set; }
        public FidesicEndpoints Endpoints { get; set; }
    }

    public class Parameters {
        private int _totalAssigned = 0;
        public string CTMSDateAndTime { get; set; }
        public string channelSerial { get; set; }
        public string CAICode { get; set; }
        public string timeout { get; set; }
        public string currencyCode { get; set; }
        public string countryCode { get; set; }
        public string callHomeTime { get; set; }
        public string merchantNameAndLocation { get; set; }
        public string MerchantCategoryCode { get; set; }
        public void assign(Tags tag, string value) {
            switch (tag) {
                case Tags.CAICode:
                    this.CAICode = value;
                    break;
                case Tags.CALL_HOME_TIME:
                    this.callHomeTime = value;
                    break;
                case Tags.CHANNEL_SERIAL:
                    this.channelSerial = value;
                    break;
                case Tags.COUNTRY_CODE:
                    this.countryCode = value;
                    break;
                case Tags.CTMS_DATE_AND_TIME:
                    this.CTMSDateAndTime = value;
                    break;
                case Tags.CURRENCY_CODE:
                    this.currencyCode = value;
                    break;
                case Tags.MERCHANT_CATEGORY_CODE:
                    this.MerchantCategoryCode = value;
                    break;
                case Tags.MERCHANT_NAME_AND_LOCATION:
                    this.merchantNameAndLocation = value;
                    break;
                case Tags.TIMEOUT:
                    this.timeout = value;
                    break;
            }
        }
    }

    public class FidesicEndpoints {
        private string _root;
        private string _keyRequest;
        private string _purchase;
        private string _gettoken;
        public string keyRequest {
            get {
                return $"{_root}{_keyRequest}";
            }
            set {
                _keyRequest = value;
            }
        }
        public string purchase {
            get {
                return $"{_root}{_purchase}";
            }
            set {
                _purchase = value;
            }
        }
        public string root {
            get {
                return _root;
            }
            set {
                _root = value;
            }
        }
        public string gettoken {
            get {
                return $"{_gettoken}";
            }
            set {
                _gettoken = value;
            }
        }
    }
}
