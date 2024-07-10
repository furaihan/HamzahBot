using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ProjectAsad.Model
{
    internal class ClickbaitResponse
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }
        [JsonPropertyName("prediction")]
        public string? Prediction { get; set; }
        [JsonPropertyName("is_clickbait")]
        public bool IsClickbait { get; set; }
        [JsonPropertyName("clickbait_probability")]
        public double ClickbaitProbability { get; set; }
        [JsonPropertyName("headline")]
        public string? Headline { get; set; }
    }
}
