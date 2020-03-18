using System.Text;
using Microsoft.Extensions.Logging;

namespace Abc.WebsocketClient.SystemNetWebSockets
{
    public class WebsocketClient : WebsocketClientBase
    {
        public WebsocketClient(string url,
            InnerClientFactory<System.Net.WebSockets.ClientWebSocket>? innerClientFactory = null,
            Encoding? encoding = null,
            ILogger? logger = null)
            : base(new WebsocketLiteClient(url, innerClientFactory, encoding, logger), logger)
        {
        }
    }
}