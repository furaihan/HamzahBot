using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class ClickbaitResponse2
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
    public DateTime Timestamp { get; set; }
}
