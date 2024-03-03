using Core.Model.DTO.Configuration;
using Core.Model.DTO.Request;
using Core.Shared;
using Infrastructure.Libraries.Banks;
using Infrastructure.Libraries.HTTP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Test {
    public class ArcaTest {
        SystemVariables _sysvar;
        ArcaImpl _arc;

        [SetUp]
        public void Setup() {
            _sysvar = new SystemVariables {
                ArcaConfig = new ArcaConfig {
                    ctmk = "",
                    ip = "arca-pos.qa.arca-payments.network",
                    port = 11000,
                    readTimeout = 12000,
                    writeTimeout = 3000,
                    acquiringInstCode = "627629",
                    forwardingInstitutionID = "",
                    posCondition = "00",
                    posDataCode = "510101511344101",
                    posPinCaptureCode = "06",
                    tmkDecryptEndpoint = "https://api.qa.arca-payments.network/tms/security/keys/export",
                    tmkDecryptionAPIKey = "2os5C0TTSz4D9GgvqRmlEG3eVnVaOEKI",
                    terminalParameter = new Parameters {
                        CAICode = "2044RI000005312",
                        callHomeTime = "",
                        channelSerial = "",
                        countryCode = "566",
                        CTMSDateAndTime = "",
                        currencyCode = "566",
                        MerchantCategoryCode = "6012",
                        merchantNameAndLocation = "TEST MERCHANT                         NG",
                        timeout = ""
                    }
                }
            };
            HttpWebRequestClient client = new HttpWebRequestClient();
            _arc = new ArcaImpl(_sysvar, client, null);
        }

        [Test]
        public async Task KeyEx() {
            var t = await _arc.getKey("20390022");
            Assert.AreEqual("", "");
        }

        [Test]
        public async Task termParamTest() {
            var t = await _arc.parameterDownload2("20390022");
            Assert.AreEqual("", "");
        }

        [Test]
        public async Task createPayment() {
            EMVStandardPayload payload = new EMVStandardPayload {
                CardData = new CardData {
                    expiryMonth = "03",
                    expiryYear = "26",
                    pan = "5399834464354042",
                    pinBlock = "04049ECBB9BCABFB",
                    track2 = "5399834464354042D26032210015027219"
                },
                terminalID = "20390022",
                accountType = Core.Model.Enums.AccountType.DEFAULT,
                rrn = Cryptography.CharGenerator.genID(12, Cryptography.CharGenerator.characterSet.NUMERIC),
                cardSequenceNumber = "001",
                stan = "",
                amount = 1000,
                iccData = "820239009F3602028B9F2701809F34034203009F10120110A000002A0000000000000000000000FF9F3303E0F8C88407A00000000410109F3501229F37040BBFCAC59F03060000000000009F02060000000010005F3401019F1A0205665F2A0205669C01009F2608E36873BAADDDD5B89A032308249F410400000043950504802488009F1E081122334455667788"
            };
            var t = await _arc.createPayment(payload);
            Assert.AreEqual("", "");
        }

        [Test]
        public async Task createPaymentOffline() {
            EMVStandardPayload payload = new EMVStandardPayload {
                CardData = new CardData {
                    expiryMonth = "06",
                    expiryYear = "26",
                    pan = "5399237027080908",
                    pinBlock = "",
                    track2 = "5399237027080908D2606221017207924"
                },
                terminalID = "20390022",
                accountType = Core.Model.Enums.AccountType.DEFAULT,
                rrn = Cryptography.CharGenerator.genID(12, Cryptography.CharGenerator.characterSet.NUMERIC),
                cardSequenceNumber = "001",
                stan = "",
                amount = 1000,
                iccData = "9F2608072010FC78D25FBF9F2701809F10120110A50003020000000000000000000000FF9F3704E2D3D7F19F3602009D950524000080009A032308319C01009F0206000000000100820239009F1A0205669F34034103029F3303E0F9C89F3501228F01068407A00000000410109F090200029F4104000000009B02E80050104465626974204D6173746572636172649F080200029F03060000000000005F3401015F2A020566"
            };
            var t = await _arc.createPayment(payload);
            Assert.AreEqual("", "");
        }
    }
}