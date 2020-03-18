using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SuperSocket.ClientEngine;
using WebSocket4Net;
using DataReceivedEventArgs = WebSocket4Net.DataReceivedEventArgs;

namespace Abc.WebsocketClient.WebSocket4Net
{
    public class WebsocketLiteClient : WebsocketLiteClientBase
    {
        private readonly string _url;
        private readonly InnerClientFactory<WebSocket>? _innerClientFactory;
        private TaskCompletionSource<bool>? _closeTaskSrc;
        private WebSocket _innerClient;
        private TaskCompletionSource<bool>? _openTaskSrc;

        public WebsocketLiteClient(string url, InnerClientFactory<WebSocket>? innerClientFactory = null,
            Encoding? encoding = null,
            ILogger? logger = null) : base(encoding, logger)
        {
            _url = url;
            _innerClientFactory = innerClientFactory;
            _innerClient = CreateNewInnerClient(_url);
        }

        public override object InnerClient => _innerClient;
        public override bool IsOpened => _innerClient.State == WebSocketState.Open;

        protected override async Task OpenAsyncInternal(CancellationToken cancellationToken)
        {
            if (_openTaskSrc != null) throw new InvalidOperationException(); //opening

            var openTaskSrc = _openTaskSrc = new TaskCompletionSource<bool>();
            var cancelAction = new Action<object>(tcs =>
            {
                ((TaskCompletionSource<bool>) tcs).TrySetCanceled(cancellationToken);
                _openTaskSrc = null;
            });
            using var register = cancellationToken.Register(cancelAction, openTaskSrc);

            var client = _innerClient = CreateNewInnerClient(_url);
            try
            {
                await Task.Factory.StartNew(() => client.Open(), cancellationToken);
            }
            catch (Exception e)
            {
                AbortInnerClient(client);
                openTaskSrc.TrySetException(e);
                _openTaskSrc = null;
            }

            await openTaskSrc.Task.ConfigureAwait(false);
            _openTaskSrc = null;
        }

        protected override Task SendAsyncInternal(string text, CancellationToken cancellationToken)
        {
            return Task.Run(() => _innerClient.Send(text), cancellationToken);
        }

        protected override Task SendAsyncInternal(byte[] bytes, CancellationToken cancellationToken)
        {
            return Task.Run(() => _innerClient.Send(bytes, 0, bytes.Length), cancellationToken);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _innerClient.Dispose();
        }

        protected override async Task<bool> CloseAsyncInternal(CloseStatusCode closeStatusCode, string reason,
            CancellationToken cancellationToken)
        {
            var closeTaskSrc = _closeTaskSrc = new TaskCompletionSource<bool>();
            var cancelAction = new Action<object>(tcs =>
            {
                ((TaskCompletionSource<bool>) tcs).TrySetCanceled(cancellationToken);
            });
            using (cancellationToken.Register(cancelAction, closeTaskSrc))
            {
                var client = _innerClient;

                try
                {
                    await Task.Factory.StartNew(() => client.Close((int) closeStatusCode, reason),
                        cancellationToken);
                }
                catch (Exception exception)
                {
                    AbortInnerClient(client);
                    closeTaskSrc.TrySetException(exception);
                }

                await closeTaskSrc.Task.ConfigureAwait(false);
                _closeTaskSrc = null;
                return client.State == WebSocketState.Closed || client.State == WebSocketState.None;
            }
        }

        private WebSocket CreateNewInnerClient(string url)
        {
            var newClient = _innerClientFactory?.Invoke(url) ?? new WebSocket(url);
            newClient.Error += WsClient_Error;
            newClient.Opened += WsClient_Opened;
            newClient.Closed += WsClient_Closed;
            newClient.MessageReceived += WsClient_MessageReceived;
            newClient.DataReceived += WsClient_DataReceived;
            return newClient;
        }

        protected override void AbortInnerClient(object client)
        {
            if (!(client is WebSocket webSocket)) return;
            webSocket.Error -= WsClient_Error;
            webSocket.Opened -= WsClient_Opened;
            webSocket.Closed -= WsClient_Closed;
            webSocket.MessageReceived -= WsClient_MessageReceived;
            webSocket.DataReceived -= WsClient_DataReceived;
            webSocket.Dispose();
        }

        private void WsClient_Opened(object sender, EventArgs e)
        {
            _closeTaskSrc?.TrySetException(new Exception("关闭期间收到Open事件."));
            _openTaskSrc?.TrySetResult(_innerClient.State == WebSocketState.Open);
        }

        private void WsClient_Closed(object sender, EventArgs e)
        {
            Debug.Assert(_innerClient.State == WebSocketState.Closed);

            if (_closeTaskSrc == null)
                OnClosed(CloseStatusCode.Normal, "closed event pass.");

            _openTaskSrc?.TrySetException(new Exception("打开期间收到Closed事件."));
            _closeTaskSrc?.TrySetResult(true);
        }

        private void WsClient_Error(object sender, ErrorEventArgs errorEventArgs)
        {
            _openTaskSrc?.TrySetException(new Exception("打开期间收到Error事件."));
            OnError(errorEventArgs.Exception);
        }

        private void WsClient_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            OnMessage(e.Message);
        }

        private void WsClient_DataReceived(object sender, DataReceivedEventArgs e)
        {
            OnMessage(e.Data);
        }
    }
}