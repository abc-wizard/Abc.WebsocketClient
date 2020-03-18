namespace Abc.WebsocketClient
{
    public delegate T InnerClientFactory<out T>(string url);
}