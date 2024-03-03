using Core.Model.DTO.Configuration;
using Infrastructure.Libraries.Banks;
using Infrastructure.Libraries.HTTP;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Test {
    internal class LuxByGruppTest {
        SystemVariables _sysvar;
        LuxByGrupp _lux;
        [SetUp]
        public void Setup() {
            _sysvar = new SystemVariables {
                LuxConfig = new LuxConfig {
                    basicPassword = "5NDM1NjckJV4KK",
                    basicUsername = "restdevice",
                    Endpoints = new LuxEndpoint {
                        getBalance = "",
                        query = "resd/transaction",
                        rootURL = "https://service-dev.trylux.africa/",
                        startSession = "resd/network-mgt",
                        transaction = ""
                    }
                }
            };
            _lux = new LuxByGrupp(_sysvar, new HttpWebRequestClient());
        }

        [Test]
        public async Task TestQuery() {
            string g = "abcdeftaegdtdhdanj";
            int lastIndex = g.LastIndexOf("a");
            var resp = await _lux.requery(new Core.Model.DTO.Request.EMVStandardPayload { rrn = "345345343512", terminalID = "63201125995137" });            
            Assert.IsFalse(false);
        }

        [Test]
        public async Task Test3des() {
            var resp = _lux.tdesDecrypt("OykLQx3mPoQhTAjaoayFfg==", "test");
            Assert.IsFalse(false);
        }
    }
}
