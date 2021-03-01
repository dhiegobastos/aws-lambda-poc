using System.Text.Json.Serialization;

namespace Function.Read.SQS
{
    public class MessageBody
    {
        [JsonPropertyName("responsePayload")]

        public object ResponsePayload { get; set; }
    }
}
