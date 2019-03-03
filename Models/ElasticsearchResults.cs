using System.Collections.Generic;

namespace elasticsearch.Models
{
    public class ElasticsearchResults
    {
        public string CreateDebug { get; set; }
        public string IndexDebug { get; set; }
        public string QueryDebug { get; set; }
        public List<BoardGame> BoardGames { get; set; }
    }
}