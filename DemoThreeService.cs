using System;
using System.Collections.Generic;
using System.Linq;
using Nest;

public class DemoThreeService
{
    private ElasticClient _elasticClient;

    public DemoThreeService()
    {
        var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
                        .DisableDirectStreaming();
        _elasticClient = new ElasticClient(settings);
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
                            .Boost(3)
                            .Analyzer("standard")
                        )
                        .Text(t => t
                            .Name(n => n.Description)
                            .Analyzer("standard")
                        )
                        .Number(n => n
                            .Name(nn => nn.PlayingTime)
                        )
                        .Keyword(k => k
                            .Name(n => n.Category)
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
            new BoardGame() { Title = "Clank!", PlayingTime = 60, Category="Competetive", Description= "Burgle your way to adventure in the deck-building board game Clank! Sneak into an angry dragon's mountain lair to steal precious artifacts. Delve deeper to find more valuable loot. Acquire cards for your deck and watch your thievish abilities grow."},
            new BoardGame() { Title = "Risk", PlayingTime = 120, Category="Competetive", Description="Possibly the most popular, mass market war game. The goal is conquest of the world." },
            new BoardGame() { Title = "Pandemic", PlayingTime = 45, Category="Cooperative",  Description="In Pandemic, several virulent diseases have broken out simultaneously all over the world! The players are disease-fighting specialists whose mission is to treat disease hotspots while researching cures for each of four plagues before they get out of hand."},
            new BoardGame() { Title = "Forbidden Island", PlayingTime = 30, Category="Cooperative",  Description= "Dare to discover Forbidden Island! Join a team of fearless adventurers on a do-or-die mission to capture four sacred treasures from the ruins of this perilous paradise. Your team will have to work together and make some pulse-pounding maneuvers, as the island will sink beneath every step!"},
        };

        var result = _elasticClient.IndexMany(boardGames, "boardgames");
        return result.DebugInformation;
    }

    public Tuple<string, List<BoardGame>> QueryData_Buckets(string query)
    {
        _elasticClient.Flush("boardgames");

        var queryContainer = new QueryContainer();
        queryContainer &= new MatchAllQuery();

        var aggregation = new TermsAggregation("category_aggregation")
        {
            Field = Infer.Field<BoardGame>(p => p.Category),
        };

        var searchRequest = new SearchRequest("boardgames", typeof(BoardGame))
        {
            Query = queryContainer,
            Aggregations = aggregation
        };

        var searchResponse = _elasticClient.Search<BoardGame>(searchRequest);

        var categories = searchResponse.Aggregations.Terms("category_aggregation").Buckets;

        return new Tuple<string, List<BoardGame>>(
            searchResponse.DebugInformation,
            searchResponse.Documents.ToList()
        );
    }

    public Tuple<string, List<BoardGame>> QueryData_MinPlayTime(string query)
    {
        _elasticClient.Flush("boardgames");

        var queryContainer = new QueryContainer();
        queryContainer &= new MatchAllQuery();

        var aggregation = new MinAggregation("min_playing_time_aggregation",
                    Infer.Field<BoardGame>(p => p.PlayingTime));

        var searchRequest = new SearchRequest("boardgames", typeof(BoardGame))
        {
            Query = queryContainer,
            Aggregations = aggregation
        };

        var searchResponse = _elasticClient.Search<BoardGame>(searchRequest);

        var minPlayingTime = searchResponse.Aggregations.Min("min_playing_time_aggregation");

        return new Tuple<string, List<BoardGame>>(
            searchResponse.DebugInformation,
            searchResponse.Documents.ToList()
        );
    }
}