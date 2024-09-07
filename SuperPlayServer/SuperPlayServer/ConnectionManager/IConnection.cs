using System.Net.WebSockets;
using System.Threading.Tasks;

namespace SuperPlayServer.ConnectionManager
{
    public interface IConnection
    {
        Task SendMessage(WebSocket ws, string message);
    }
}