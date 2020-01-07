using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SysKit.ODG.Office365Service.GraphHttpProvider;

namespace SysKit.ODG.Office365Service.Tests.GraphHttpProvider
{
    [TestFixture]
    class GraphHttpProviderTests
    {
        [Test]
        public async Task SendAsync_SendsRequest_Succesfully()
        {
            //var graphHttpProvider = new GraphHttpProviderFactory().CreateHttpProvider(5, "test");

            //var testResponse =
            //    await graphHttpProvider.SendAsync(new HttpRequestMessage(HttpMethod.Get, "www.google.com"));

            //Assert.IsTrue(testResponse.IsSuccessStatusCode);
        }
    }
}
