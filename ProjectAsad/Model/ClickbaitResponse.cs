using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProjectAsad.Model
{
    public class ClickbaitResponse
    {
    [JsonPropertyName("clickbait_probability")]
    public Dictionary<string, double>? ClickbaitProbability { get; set; }

    [JsonPropertyName("headline")]
    public string? Headline { get; set; }

    [JsonPropertyName("is_clickbait")]
    public int IsClickbait { get; set; }

    [JsonPropertyName("prediction")]
    public string? Prediction { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("timestamp")]
    public string? Timestamp { get; set; }
    }
}
