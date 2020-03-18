using Abc.WebsocketClient.SystemNetWebSockets;
using Xunit.Abstractions;

namespace Abc.WebsocketClient.System.Net.WebSockets
{
    public class WebsocketLiteClientTests : WebsocketLiteClientTestBase
    {
        private const int Port = 12002;

        public WebsocketLiteClientTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper, Port)
        {
        }

        protected override IWebsocketLiteClient CreateNewClient()
        {
            return new WebsocketLiteClient(Url, logger: Logger);
        }
    }
}