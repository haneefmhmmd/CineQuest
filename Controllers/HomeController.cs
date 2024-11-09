using _301301555_301287005_Laylay_Muhammad__Lab3.Models;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.S3;
using Azure;
using Humanizer.Localisation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
using System.Globalization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace _301301555_301287005_Laylay_Muhammad__Lab3.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IDynamoDBContext _dbContext;
        private readonly IAmazonDynamoDB _dynamoDbClient;
        private readonly IAmazonS3 _s3Client;
        private readonly string S3BucketPath = "https://movies-haneef.s3.us-east-1.amazonaws.com/";


        public HomeController(IDynamoDBContext dbContext, IAmazonDynamoDB dynamoDbClient, IAmazonS3 s3Client, ILogger<HomeController> logger)
        {
            _dbContext = dbContext;
            _dynamoDbClient = dynamoDbClient;
            _s3Client = s3Client;
            _logger = logger;
        }


        public async Task<IActionResult> Index()
        {
            // Assuming you want to perform a scan to filter by genre
            var queryRequest = new QueryRequest
            {
                TableName = "Movies",
                IndexName = "RatingIndex",
                KeyConditionExpression = "Category = :category AND Rating >= :rating", 
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                        {
                            { ":category", new AttributeValue { S = "Movies" } },
                            { ":rating", new AttributeValue { N = "1" } }
                        },
                ScanIndexForward = false,
                Limit = 6
            };

            // Map response items to a list of Movie objects
            var queryResponse = await _dynamoDbClient.QueryAsync(queryRequest);

            // Map response items to a list of Movie objects
            var movies = queryResponse.Items.Select(item => new Movie
            {
                MovieId = item["MovieId"].S,
                Title = item["Title"].S,
                Rating = double.Parse(item["Rating"].N, CultureInfo.InvariantCulture),
                Genre = item["Genre"].S,
                BannerImageHref = item.ContainsKey("BannerImageHref") ? item["BannerImageHref"].S : string.Empty
            }).ToList();

            return View(movies);
        }

        [HttpGet]
        public async Task<IActionResult> SearchByRating(string rating)
        {
            // Log the input values to the console

            // Initialize a list to hold the filtered movies
            List<Movie> movies = new List<Movie>();

            try
            {
                // Query for movies by rating if a rating is provided
                if (!string.IsNullOrEmpty(rating))
                {
                    var queryRequest = new QueryRequest
                    {
                        TableName = "Movies",
                        IndexName = "RatingIndex",
                        KeyConditionExpression = "Category = :category AND Rating >= :rating", // Test with an exact match
                        ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                        {
                            { ":category", new AttributeValue { S = "Movies" } },
                            { ":rating", new AttributeValue { N = rating } }    
                        },
                        ScanIndexForward = false,

                    };
                    var queryResponse = await _dynamoDbClient.QueryAsync(queryRequest);

                    // Map response items to a list of Movie objects
                    movies = queryResponse.Items.Select(item => new Movie
                    {
                        MovieId = item["MovieId"].S,
                        Title = item["Title"].S,
                        Rating = double.Parse(item["Rating"].N, CultureInfo.InvariantCulture), // Ensure proper parsing of double
                        Genre = item["Genre"].S,
                        BannerImageHref = item.ContainsKey("BannerImageHref") ? item["BannerImageHref"].S : string.Empty
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                // Log detailed error message
                _logger.LogError($"Error querying DynamoDB: {ex.Message}");
                // You can return an error view or an empty list based on your error handling strategy
            }

            return View("Index", movies);
        }


        [HttpGet]
        public async Task<IActionResult> SearchByGenre(string genre)
        {
            // Log the input values to the console

            // Initialize a list to hold the filtered movies
            List<Movie> movies = new List<Movie>();

            try
            {
                if (!string.IsNullOrEmpty(genre))
                {
                    // Assuming you want to perform a scan to filter by genre
                    var searchResponse = await _dynamoDbClient.ScanAsync(new ScanRequest
                    {
                        TableName = "Movies",
                        FilterExpression = "Category = :category AND Genre = :genre",
                        ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                        {
                            { ":category", new AttributeValue { S = "Movies" }},
                            { ":genre", new AttributeValue { S = genre } }
                        }
                    });


                    // Map response items to a list of Movie objects
                    movies = searchResponse.Items.Select(item => new Movie
                    {
                        MovieId = item["MovieId"].S,
                        Title = item["Title"].S,
                        Rating = double.Parse(item["Rating"].N),
                        Genre = item.ContainsKey("Genre") ? item["Genre"].S : "General", // Default if not present
                        BannerImageHref = item.ContainsKey("BannerImageHref") ? item["BannerImageHref"].S : string.Empty
                    }).ToList();

                }
            }
            catch (Exception ex)
            {
                // Log detailed error message
                _logger.LogError($"Error querying DynamoDB: {ex.Message}");
                // You can return an error view or an empty list based on your error handling strategy
            }

            return View("Index", movies);
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
