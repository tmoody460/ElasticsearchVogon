using System;
using System.Collections.Generic;
using System.Linq;
using Nest;

public class DemoOneService
{
    private ElasticClient _elasticClient;

    public DemoOneService()
    {
        var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
                        .DisableDirectStreaming();
        _elasticClient = new ElasticClient(settings);
    }

    public bool IsElasticsearchRunning()
    {
        var status = _elasticClient.Ping();
        return status.IsValid;
    }

    public string CreateIndex()
    {
        _elasticClient.DeleteIndex("boardgames");

        var result = _elasticClient.CreateIndex("boardgames", c => c
            .Settings(s => s)
            .Mappings(m => m
                .Map<BoardGame>(mm => mm
                    .Properties(p => p
                        .Text(t => t
                            .Name(n => n.Title)
                        )
                    )
                )
            )
        );

        return result.DebugInformation;
    }

    public string IndexData()
    {
        var boardGames = new BoardGame[]
        {
            new BoardGame() { Title = "Settlers of Catan"},
            new BoardGame() { Title = "Risk"},
            new BoardGame() { Title = "Pandemic"},
            new BoardGame() { Title = "Star Realms"},
            new BoardGame() { Title = "Hanabi"},
            new BoardGame() { Title = "Forbidden Island"},
            new BoardGame() { Title = "Ca$h 'n Guns" },
        };

        var result = _elasticClient.IndexMany(boardGames, "boardgames");
        return result.DebugInformation;
    }

    public Tuple<string, List<BoardGame>> QueryData()
    {
        _elasticClient.Flush("boardgames");

        var searchResponse = _elasticClient.Search<BoardGame>(s => s
            .Index("boardgames")
            .Query(q => q
                .MatchAll()
            )
        );
        return new Tuple<string, List<BoardGame>>(
            searchResponse.DebugInformation,
            searchResponse.Documents.ToList()
        );
    }
}