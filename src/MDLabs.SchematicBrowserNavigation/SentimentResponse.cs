using Microsoft.TypeChat.Schema;
using System.Text.Json.Serialization;

public class SentimentResponse
{
    [JsonPropertyName("sentiment")]
    [JsonVocab("negative | neutral | positive")]
    public string Sentiment { get; set; }
}
