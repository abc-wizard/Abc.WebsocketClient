using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Abc.WebsocketClient.Events;
using Abc.WebsocketClient.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Abc.WebsocketClient
{
    public abstract class WebsocketLiteClientBase : IWebsocketLiteClient
    {
        protected readonly ILogger Logger;

        private bool _closing;

        /// <summary>
        ///     1: true; 0: false
        /// </summary>
        private int _opening;


        protected WebsocketLiteClientBase(
            Encoding? encoding,
            ILogger? logger)
        {
            Logger = logger ?? NullLogger.Instance;
            Encoding = encoding ?? Encoding.UTF8;
        }

        public virtual async Task EnsureCloseAsync(CloseStatusCode? closeStatusCode = null, string? reason = null,
            CancellationToken cancellationToken = default)
        {
            if (_opening == 0 && !_closing && IsOpened)
                //未正在打开 且 未正在关闭 且已打开
                try
                {
                    _closing = true;
                    using var register = cancellationToken.Register(() => AbortInnerClient(InnerClient));
                    var closed = await CloseAsyncInternal(closeStatusCode ?? CloseStatusCode.Normal,
                        reason ?? string.Empty,
                        cancellationToken);
                    if (!closed) AbortInnerClient(InnerClient);
                }
                catch (Exception)
                {
                    AbortInnerClient(InnerClient);
                }
                finally
                {
                    _closing = false;
                }
            else
                AbortInnerClient(InnerClient);
        }

        public virtual async Task OpenAsync(CancellationToken cancellationToken = default)
        {
            //1: true; 0: false
            if (Interlocked.CompareExchange(ref _opening, 1, 0) == 1)
                throw new InvalidOperationException();

            if (IsOpened) throw new InvalidOperationException();

            try
            {
                await OpenAsyncInternal(cancellationToken);
                if (!IsOpened)
                {
                    AbortInnerClient(InnerClient);
                    throw new WebsocketException("Open failed, unknown reason.");
                }
            }
            catch (Exception e)
            {
                AbortInnerClient(InnerClient);
                throw new WebsocketException("Open failed.", e);
            }
            finally
            {
                _opening = 0;
            }
        }

        public abstract object InnerClient { get; }
        public abstract bool IsOpened { get; }
        public Encoding Encoding { get; set; }
        public event EventHandler<MessageEventArgs>? MessageReceived;
        public virtual event EventHandler<CloseEventArgs>? Closed;
        public event EventHandler<ErrorEventArgs>? Error;


        public async Task SendAsync(string text, CancellationToken cancellationToken = default)
        {
            if (!IsOpened) throw new InvalidOperationException();
            try
            {
                await SendAsyncInternal(text, cancellationToken);
            }
            catch (Exception e)
            {
                throw new WebsocketException("Send Error.", e);
            }
        }


        public async Task SendAsync(byte[] bytes, CancellationToken cancellationToken = default)
        {
            if (!IsOpened) throw new InvalidOperationException();
            try
            {
                await SendAsyncInternal(bytes, cancellationToken);
            }
            catch (Exception e)
            {
                throw new WebsocketException("Send Error.", e);
            }
        }


        protected abstract Task<bool> CloseAsyncInternal(CloseStatusCode closeStatusCode, string reason,
            CancellationToken cancellationToken);

        /// <summary>
        ///     close, remove event handler, dispose
        /// </summary>
        /// <param name="client"></param>
        protected abstract void AbortInnerClient(object client);

        protected abstract Task OpenAsyncInternal(CancellationToken cancellationToken);

        protected virtual void OnClosed(CloseStatusCode closeStatusCode, string? reason)
        {
            Closed?.Invoke(this, new CloseEventArgs(closeStatusCode, reason));
        }

        protected virtual void OnError(Exception exception)
        {
            Error?.Invoke(this, new ErrorEventArgs(exception));
        }

        protected virtual void OnMessage(byte[] bytes)
        {
            MessageReceived?.Invoke(this, new MessageEventArgs(bytes));
        }

        protected virtual void OnMessage(string text)
        {
            MessageReceived?.Invoke(this, new MessageEventArgs(text));
        }

        protected abstract Task SendAsyncInternal(string text, CancellationToken cancellationToken);
        protected abstract Task SendAsyncInternal(byte[] bytes, CancellationToken cancellationToken);

        #region dispose

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool disposing);

        #endregion
    }
}