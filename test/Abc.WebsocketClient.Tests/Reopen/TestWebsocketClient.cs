using Microsoft.Extensions.Logging;

namespace Abc.WebsocketClient.Reopen
{
    public class TestWebsocketClient : WebsocketClientBase
    {
        public TestWebsocketClient(IWebsocketLiteClient websocketLiteClient, ILogger logger) : base(websocketLiteClient,
            logger)
        {
        }
    }
}