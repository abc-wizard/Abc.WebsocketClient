using System.Text;
using Microsoft.Extensions.Logging;
using WebSocket4Net;

namespace Abc.WebsocketClient.WebSocket4Net
{
    public class WebsocketClient : WebsocketClientBase
    {
        public WebsocketClient(string url,
            InnerClientFactory<WebSocket>? innerClientFactory = null,
            Encoding? encoding = null,
            ILogger? logger = null)
            : base(new WebsocketLiteClient(url, innerClientFactory, encoding, logger), logger)
        {
        }
    }
}