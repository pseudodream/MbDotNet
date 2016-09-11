﻿using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using MbDotNet.Interfaces;
using MbDotNet.Enums;
using MbDotNet.Exceptions;
using MbDotNet.Models;
using MbDotNet.Models.Imposters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;

namespace MbDotNet.Tests
{
    [TestClass]
    public class MountebankRequestProxyTests
    {
        private Mock<IHttpClientWrapper> _mockClient;
        private MountebankRequestProxy _proxy;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockClient = new Mock<IHttpClientWrapper>();
            _proxy = new MountebankRequestProxy(_mockClient.Object);
        }

        [TestMethod]
        public void DeleteAllImposters_SendsRequest()
        {
            var expectedResource = "imposters";

            var response = GetResponse(HttpStatusCode.OK);

            _mockClient.Setup(x => x.DeleteAsync(expectedResource))
                .ReturnsAsync(response);

            _proxy.DeleteAllImpostersAsync();

            _mockClient.Verify(x => x.DeleteAsync(expectedResource), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(MountebankException))]
        public async Task DeleteAllImposters_StatusCodeNotOk_ThrowsMountebankException()
        {
            var response = GetResponse(HttpStatusCode.BadRequest);

            _mockClient.Setup(x => x.DeleteAsync(It.IsAny<string>()))
                .ReturnsAsync(response);

            await _proxy.DeleteAllImpostersAsync();
        }

        [TestMethod]
        public void DeleteImposter_SendsRequest_ImpostersResourceWithPort()
        {
            const int port = 123;
            var expectedResource = string.Format("imposters/{0}", port);

            var response = GetResponse(HttpStatusCode.OK);

            _mockClient.Setup(x => x.DeleteAsync(expectedResource))
                .ReturnsAsync(response);

            _proxy.DeleteImposterAsync(port);

            _mockClient.Verify(x => x.DeleteAsync(expectedResource), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(MountebankException))]
        public async Task DeleteImposter_StatusCodeNotOk_ThrowsMountebankException()
        {
            var response = GetResponse(HttpStatusCode.BadRequest);

            _mockClient.Setup(x => x.DeleteAsync(It.IsAny<string>()))
                .ReturnsAsync(response);

            await _proxy.DeleteImposterAsync(123);
        }

        [TestMethod]
        public void CreateImposter_SendsRequest_ImpostersResource()
        {
            var expectedResource = "imposters";

            var response = GetResponse(HttpStatusCode.Created);

            _mockClient.Setup(x => x.PostAsync(expectedResource, It.IsAny<HttpContent>()))
                .ReturnsAsync(response);

            _proxy.CreateImposterAsync(new HttpImposter(123, null));

            _mockClient.Verify(x => x.PostAsync(expectedResource, It.IsAny<HttpContent>()), Times.Once);
        }

        [TestMethod]
        public void CreateImposter_SendsRequest_WithJsonBody()
        {
            var imposter = new HttpImposter(123, null);

            var response = GetResponse(HttpStatusCode.Created);

            HttpContent content = null;
            _mockClient.Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>()))
                .ReturnsAsync(response)
                .Callback<string, HttpContent>((res, cont) => content = cont);

            _proxy.CreateImposterAsync(new HttpImposter(123, null));

            var json = content.ReadAsStringAsync().Result;
            var serializedImposter = JsonConvert.DeserializeObject<HttpImposter>(json);

            Assert.AreEqual(imposter.Port, serializedImposter.Port);
        }

        [TestMethod]
        [ExpectedException(typeof(MountebankException))]
        public async Task CreateImposter_StatusCodeNotCreated_ThrowsMountebankException()
        {
            var response = GetResponse(HttpStatusCode.BadRequest);

            _mockClient.Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>()))
                .ReturnsAsync(response);

            await _proxy.CreateImposterAsync(new HttpImposter(123, null));
        }

        private HttpResponseMessage GetResponse(HttpStatusCode statusCode) 
        {
            var response = new HttpResponseMessage();
            response.StatusCode = statusCode;
            response.Content = new StringContent(string.Empty);
            return response;
        }
    }
}
