using System;

namespace Abc.WebsocketClient.Exceptions
{
    public class WebsocketException : Exception
    {
        public WebsocketException(string? message) : base(message)
        {
        }

        public WebsocketException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected WebsocketException()
        {
        }
    }
}