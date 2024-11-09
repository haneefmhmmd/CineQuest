using _301301555_301287005_Laylay_Muhammad__Lab3.Models;
using Amazon.DynamoDBv2.DataModel;
using Amazon.S3;
using Microsoft.AspNetCore.Mvc;

namespace _301301555_301287005_Laylay_Muhammad__Lab3.Controllers
{
    public class MovieController : Controller
    {
        private readonly IDynamoDBContext _dbContext;
        private readonly IAmazonS3 _s3Client;
        private readonly string S3BucketPath = "https://movies-haneef.s3.us-east-1.amazonaws.com/";

        public MovieController(IDynamoDBContext dbContext, IAmazonS3 s3Client, ILogger<HomeController> logger)
        {
            _dbContext = dbContext;
            _s3Client = s3Client;
        }

        public async Task<IActionResult> Index(string movieId)
        {
            // Fetch the movie from DynamoDB using the MovieId
            var movie = await _dbContext.LoadAsync<Movie>(movieId);
            if (movie == null)
            {
                return NotFound();
            }

            // Construct the full S3 URL for the movie file
            var movieUrl = movie.MovieHref;

            ViewBag.MovieUrl = movieUrl; // Pass the movie URL to the view
            return View(movie);
        }

        public class CommentRequest
        {
            public required string Content { get; set; }
            public required double Rating { get; set; }
            public required string MovieId { get; set; } // Add this property
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(CommentRequest comment)
        {
            // Fetch the movie from DynamoDB using the movieId
            var movie = await _dbContext.LoadAsync<Movie>(comment.MovieId);
            if (movie == null)
            {
                TempData["MovieNotFound"] = true;
                return RedirectToAction("Index", "Movie", null);
            }

            // Validate the inputs
            if (string.IsNullOrWhiteSpace(comment.Content) || comment.Rating < 1 || comment.Rating > 10)
            {
                TempData["CommentError"] = "Please enter a valid rating between 1 and 10, and ensure the comment field is not empty.";
                return RedirectToAction("Index", "Movie", new { movieId = movie.MovieId });
            }

            // Add the new comment
            var newComment = new Comment
            {
                CommentId = Guid.NewGuid().ToString(),
                UserId = HttpContext.Session.GetInt32("UserId") ?? 0,
                UserName = HttpContext.Session.GetString("FullName"),
                Content = comment.Content,
                Rating = comment.Rating,
                PostedAt = DateTime.UtcNow
            };
            movie.Comments.Add(newComment);

            // Update the average rating
            movie.Rating = Math.Round(movie.Comments.Average(c => c.Rating), 1);

            // Save the updated movie back to DynamoDB
            await _dbContext.SaveAsync(movie);

            return RedirectToAction("Index", "Movie", new { movieId = movie.MovieId });
        }


        [HttpPost]
        public async Task<IActionResult> EditComment(string movieId, string commentId, int rating, string content)
        {

            // Fetch the movie from DynamoDB using the movieId
            var movie = await _dbContext.LoadAsync<Movie>(movieId);
            if (movie == null)
            {
                TempData["MovieNotFound"] = true;
                return RedirectToAction("Index", "Movie", null);
            }

            // Find the comment to edit
            var comment = movie.Comments.FirstOrDefault(c => c.CommentId == commentId);
            if (comment == null)
            {
                TempData["CommentNotFound"] = true;
                return RedirectToAction("Index", "Movie", new { movieId });
            }

            // Update the comment's content and rating
            comment.Content = content;
            comment.Rating = rating;
            comment.PostedAt = DateTime.UtcNow;

            // Recalculate the average rating
            movie.Rating = Math.Round(movie.Comments.Average(c => c.Rating), 1);

            // Save the updated movie back to DynamoDB
            await _dbContext.SaveAsync(movie);

            return RedirectToAction("Index", "Movie", new { movieId });

        }


    }
}
