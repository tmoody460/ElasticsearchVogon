using System;
using System.Collections.Generic;
using System.Linq;
using Nest;

public class DemoTwoService
{
    private ElasticClient _elasticClient;

    public DemoTwoService()
    {
        var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
                        .DisableDirectStreaming();
        _elasticClient = new ElasticClient(settings);
    }

    public string CreateIndex()
    {
        _elasticClient.DeleteIndex("boardgames");

        var result = _elasticClient.CreateIndex("boardgames", c => c
            .Settings(s => s
                .Analysis(a => a
                    .CharFilters(cf => cf
                        .Mapping("board_games", mca => mca
                            .Mappings(new[]
                            {
                                "ca$h => cash",
                                "Ca$h => cash",
                            })
                        )
                    )
                    .TokenFilters(f => f
                            .Stemmer("tracys_stemmer", ss => ss.Language("english")
                        )
                    )
                    .Analyzers(aa => aa
                        .Custom("tracys_custom_analyzer", sa => sa
                            .CharFilters("html_strip", "board_games")
                            .Tokenizer("standard")
                            .Filters("lowercase", "stop", "tracys_stemmer")
                        )
                    )
                )
            )
            .Mappings(m => m
                .Map<BoardGame>(mm => mm
                    .Properties(p => p
                        .Text(t => t
                            .Name(n => n.Title)
                            .Boost(3)
                            .Analyzer("tracys_custom_analyzer")
                            .SearchAnalyzer("tracys_custom_analyzer")
                        )
                        .Text(t => t
                            .Name(n => n.Description)
                            .Analyzer("tracys_custom_analyzer")
                            .SearchAnalyzer("tracys_custom_analyzer")
                        )
                        .Number(n => n
                            .Name(nn => nn.PlayingTime)
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
            new BoardGame() { Title = "Settlers of Catan", PlayingTime = 60, Description= "In Catan (formerly The Settlers of Catan), players try to be the dominant force on the island of Catan by building settlements, cities, and roads. On each turn dice are rolled to determine what resources the island produces. Players collect these resources (cards)—wood, grain, brick, sheep, or stone—to build up their civilizations to get to 10 victory points and win the game."},
            new BoardGame() { Title = "Risk", PlayingTime = 120, Description="Possibly the most popular, mass market war game. The goal is conquest of the world." },
            new BoardGame() { Title = "Pandemic", PlayingTime = 45, Description="In Pandemic, several virulent diseases have broken out simultaneously all over the world! The players are disease-fighting specialists whose mission is to treat disease hotspots while researching cures for each of four plagues before they get out of hand."},
            new BoardGame() { Title = "Ca$h 'n Guns", PlayingTime = 30, Description= "In an abandoned warehouse a gangster band is splitting its loot, but they can't agree on the split! It's time to let the guns talk and soon everyone is aiming at everyone. The richest surviving gangster wins the game!"},
        };

        var result = _elasticClient.IndexMany(boardGames, "boardgames");
        return result.DebugInformation;
    }

    public Tuple<string, List<BoardGame>> QueryData_SimpleMatch(string query)
    {
        _elasticClient.Flush("boardgames");

        var queryContainer = new QueryContainer();
        queryContainer &= new MatchQuery()
        {
            Field = Infer.Field<BoardGame>(b => b.Title),
            Query = query,
        };

        var searchRequest = new SearchRequest("boardgames", typeof(BoardGame))
        {
            Query = queryContainer,
        };

        var searchResponse = _elasticClient.Search<BoardGame>(searchRequest);

        return new Tuple<string, List<BoardGame>>(
            searchResponse.DebugInformation,
            searchResponse.Documents.ToList()
        );
    }

    public Tuple<string, List<BoardGame>> QueryData_MultiMatch(string query)
    {
        _elasticClient.Flush("boardgames");

        var queryContainer = new QueryContainer();
        queryContainer &= new MultiMatchQuery()
        {
            Fields = new Field[] {
                Infer.Field<BoardGame>(b => b.Title),
                Infer.Field<BoardGame>(b => b.Description)
            },
            Type = TextQueryType.MostFields,
            Query = query,
            Fuzziness = Fuzziness.Auto
        };

        var searchRequest = new SearchRequest("boardgames", typeof(BoardGame))
        {
            Query = queryContainer,
        };

        var searchResponse = _elasticClient.Search<BoardGame>(searchRequest);

        return new Tuple<string, List<BoardGame>>(
            searchResponse.DebugInformation,
            searchResponse.Documents.ToList()
        );
    }

    public Tuple<string, List<BoardGame>> QueryData_CombinedStructured(string query)
    {
        _elasticClient.Flush("boardgames");

        var queryContainer = new QueryContainer();
        queryContainer &= new MultiMatchQuery()
        {
            Fields = new Field[] {
                Infer.Field<BoardGame>(b => b.Title),
                Infer.Field<BoardGame>(b => b.Description)
            },
            Type = TextQueryType.MostFields,
            Query = query,
            Fuzziness = Fuzziness.Auto
        };
        queryContainer &= new NumericRangeQuery
        {
            Field = Infer.Field<BoardGame>(ff => ff.PlayingTime),
            LessThan = 60,
        };

        var searchRequest = new SearchRequest("boardgames", typeof(BoardGame))
        {
            Query = queryContainer,
        };

        var searchResponse = _elasticClient.Search<BoardGame>(searchRequest);

        return new Tuple<string, List<BoardGame>>(
            searchResponse.DebugInformation,
            searchResponse.Documents.ToList()
        );
    }
}