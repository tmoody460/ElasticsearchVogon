using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using elasticsearch.Models;

namespace elasticsearch.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult DemoOne()
        {
            var service = new DemoOneService();
            var valid = service.IsElasticsearchRunning();
            var createDebug = service.CreateIndex();
            var indexDebug = service.IndexData();
            var queryResult = service.QueryData();
            return View("Results", new ElasticsearchResults
            {
                CreateDebug = createDebug,
                IndexDebug = indexDebug,
                QueryDebug = queryResult.Item1,
                BoardGames = queryResult.Item2
            });
        }

        public IActionResult DemoTwoSimpleMatch(string query)
        {
            var service = new DemoTwoService();
            var createDebug = service.CreateIndex();
            var indexDebug = service.IndexData();
            var queryResult = service.QueryData_SimpleMatch(query);
            return View("Results", new ElasticsearchResults
            {
                CreateDebug = createDebug,
                IndexDebug = indexDebug,
                QueryDebug = queryResult.Item1,
                BoardGames = queryResult.Item2
            });
        }

        public IActionResult DemoTwoMultiMatch(string query)
        {
            var service = new DemoTwoService();
            var createDebug = service.CreateIndex();
            var indexDebug = service.IndexData();
            var queryResult = service.QueryData_MultiMatch(query);
            return View("Results", new ElasticsearchResults
            {
                CreateDebug = createDebug,
                IndexDebug = indexDebug,
                QueryDebug = queryResult.Item1,
                BoardGames = queryResult.Item2
            });
        }

        public IActionResult DemoTwoStructured(string query)
        {
            var service = new DemoTwoService();
            var createDebug = service.CreateIndex();
            var indexDebug = service.IndexData();
            var queryResult = service.QueryData_CombinedStructured(query);
            return View("Results", new ElasticsearchResults
            {
                CreateDebug = createDebug,
                IndexDebug = indexDebug,
                QueryDebug = queryResult.Item1,
                BoardGames = queryResult.Item2
            });
        }

        public IActionResult DemoThreeBuckets(string query)
        {
            var service = new DemoThreeService();
            var createDebug = service.CreateIndex();
            var indexDebug = service.IndexData();
            var queryResult = service.QueryData_Buckets(query);
            return View("Results", new ElasticsearchResults
            {
                CreateDebug = createDebug,
                IndexDebug = indexDebug,
                QueryDebug = queryResult.Item1,
                BoardGames = queryResult.Item2
            });
        }

        public IActionResult DemoThreeMetric(string query)
        {
            var service = new DemoThreeService();
            var createDebug = service.CreateIndex();
            var indexDebug = service.IndexData();
            var queryResult = service.QueryData_MinPlayTime(query);
            return View("Results", new ElasticsearchResults
            {
                CreateDebug = createDebug,
                IndexDebug = indexDebug,
                QueryDebug = queryResult.Item1,
                BoardGames = queryResult.Item2
            });
        }

    }
}
