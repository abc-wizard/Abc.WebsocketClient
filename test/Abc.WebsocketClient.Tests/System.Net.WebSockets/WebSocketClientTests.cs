using Xunit.Abstractions;

namespace Abc.WebsocketClient.System.Net.WebSockets
{
    public class WebSocketClientTests : WebsocketClientTestBase
    {
        private const int Port = 12001;

        public WebSocketClientTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper, Port)
        {
        }

        protected override WebsocketClientBase CreateNewClient()
        {
            return new SystemNetWebSockets.WebsocketClient(Url, logger: Logger);
        }
    }
}