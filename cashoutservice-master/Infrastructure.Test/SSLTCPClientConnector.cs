using Infrastructure.Libraries.HTTP;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Test {
    [TestFixture]
    public class SSLTCPClientConnector {

        [TestCase]
        public async Task KeyEx() {
            string ip = "arca-pos.qa.arca-payments.network";
            int port = 11000;
            TCPConnector c = new TCPConnector(ip, port);
            //var resp = await c.sendRequest(" 080022380000008000009A0000082910293610293610293608292033ZV15");
            byte[] messageBytes = Encoding.UTF8.GetBytes("080022380000008000009A0000082910293610293610293608292033ZV15");
            await c.PostMessageWithSSLAsync(messageBytes);
            Assert.AreEqual("", "");
        }

        [TestCase]
        public async Task TestClient() {

            Assert.AreEqual("", "");
        }
    }
}
