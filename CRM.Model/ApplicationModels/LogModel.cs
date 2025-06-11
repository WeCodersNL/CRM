using System.Text.Json.Serialization;

namespace CRM.Model.ApplicationModels
{
    public class LogModel
    {
        [JsonPropertyName("@t")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("@mt")]
        public string MessageTemplate { get; set; } = null!;

        [JsonPropertyName("@l")]
        public string Level { get; set; } = null!;

        public DateTime Time { get; set; }

        public string? SourceContext { get; set; }

        public string? TransportConnectionId { get; set; }

        public string? RequestId { get; set; }

        public string? RequestPath { get; set; }

        public string? ConnectionId { get; set; }
        public string? Exception { get; set; }
        public string? UserEmail { get; set; }
    }
}
