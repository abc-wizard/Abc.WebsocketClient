using Microsoft.Extensions.Logging;

namespace Abc.WebsocketClient
{
    public abstract class WebsocketClient : WebsocketClientBase
    {
        protected WebsocketClient(IWebsocketLiteClient websocketLiteClient, ILogger? logger = null) : base(
            websocketLiteClient, logger)
        {
        }
    }
}