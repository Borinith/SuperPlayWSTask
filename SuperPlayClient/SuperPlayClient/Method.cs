using System.Text.Json.Serialization;

namespace SuperPlayClient
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Method
    {
        Login,
        SendGift,
        UpdateResources
    }
}