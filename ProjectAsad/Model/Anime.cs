using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectAsad.Model
{
    using System.Text.Json.Serialization;

    public class Anime
    {
        public required List<AnimeTitle> Titles { get; set; }
        public required string ImageUrl { get; set; }
        public required string Status { get; set; }
        public string? Synopsis { get; set; }
        public int? Episodes { get; set; }
        public float? Score { get; set; }
        public int? Popularity { get; set; }
        public List<string> Genres { get; set; } = [];
        public List<string> Demographics { get; set; } = [];
        public List<string> Themes { get; set; } = [];
        public List<string> Producers { get; set; } = [];
        public List<string> Studios { get; set; } = [];
        public List<string> Licensors { get; set; } = [];
    }
    public class AnimeTitle
    {
        [JsonPropertyName("title")]
        public required string Title { get; set; }
        [JsonPropertyName("type")]
        public required string Type { get; set; }
    }
}
