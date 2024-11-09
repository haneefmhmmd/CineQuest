using _301301555_301287005_Laylay_Muhammad__Lab3.Models;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Mvc;
using _301301555_301287005_Laylay_Muhammad__Lab3.Controllers;
using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Microsoft.IdentityModel.Tokens;

public class ManageMoviesController : Controller
{
    private readonly IDynamoDBContext _dbContext;
    private readonly IAmazonS3 _s3Client;

    private readonly string S3BucketPath = "https://movies-haneef.s3.us-east-1.amazonaws.com/";


    public ManageMoviesController(IDynamoDBContext dbContext, IAmazonS3 s3Client)
    {
        _dbContext = dbContext;
        _s3Client = s3Client;
    }

    // GET: Movies
    public async Task<IActionResult> Index()
    {
        // Retrieve the UserId from the session
        var userId = HttpContext.Session.GetInt32("UserId");

        // Retrieve the list of movies from DynamoDB filtered by UploaderId
        var scanConditions = new List<ScanCondition>
    {
        new ScanCondition("UploaderId", Amazon.DynamoDBv2.DocumentModel.ScanOperator.Equal, userId)
    };

        var movies = await _dbContext.ScanAsync<Movie>(scanConditions).GetRemainingAsync();

        return View(movies);
    }

    // GET: Movies/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Movies/Create
    [HttpPost]
    public async Task<IActionResult> Create(CreateMovie createMovieModel)
    {

        bool isMovieFileInvalid = createMovieModel.MovieFile == null || createMovieModel.MovieFile.Length == 0;
        Console.WriteLine($"Is movie data Invalid: {isMovieFileInvalid}");

        // Check if the movie file and banner image file are uploaded
        if (isMovieFileInvalid)
        {
            ModelState.AddModelError("MovieFile", "Please upload a movie file.");
        }

        if (createMovieModel.BannerImageFile == null || createMovieModel.BannerImageFile.Length == 0)
        {
            ModelState.AddModelError("BannerImageFile", "Please upload a banner image file.");
        }

        // Check if the model state is valid
        if (!ModelState.IsValid)
        {
            return View(createMovieModel);
        }

        // Get the user id from the session
        var userId = HttpContext.Session.GetInt32("UserId");

        if(userId == null)
        {
            return RedirectToAction("Login", "Users");
        }

        // Create a new Movie entity from the CreateMovieModel
        var movie = new Movie
        {
            MovieId = Guid.NewGuid().ToString(), // Generate a unique ID
            Title = createMovieModel.Title,
            Genre = createMovieModel.Genre,
            Director = createMovieModel.Director,
            ReleaseTime = createMovieModel.ReleaseTime,
            Rating = createMovieModel.Rating,
            Comments = new List<Comment>(),
            UploaderId = (int)userId, // Set uploader ID
            MovieHref = "", // Initialize, will be set after upload
            BannerImageHref = "" // Initialize, will be set after upload
        };

        try
        {
            // Proceed to upload the movie file to S3
            var movieUploadKey = $"movies/{movie.MovieId}-{createMovieModel.MovieFile.FileName}";
            using (var stream = createMovieModel.MovieFile.OpenReadStream())
            {
                var uploadRequest = new TransferUtilityUploadRequest
                {
                    InputStream = stream,
                    Key = movieUploadKey,
                    BucketName = "movies-haneef",
                    CannedACL = S3CannedACL.PublicRead
                };

                var transferUtility = new Amazon.S3.Transfer.TransferUtility(_s3Client); // Use DI-injected _s3Client
                await transferUtility.UploadAsync(uploadRequest);
            }

            // Generate the S3 URL after successful movie file upload
            movie.MovieHref = $"{S3BucketPath}{movieUploadKey}";
        }
        catch (AmazonS3Exception ex)
        {
            Console.WriteLine($"Error uploading movie file: {ex.Message}");
            ModelState.AddModelError("MovieFile", "An error occurred while uploading the movie file. Please try again.");
            return View(createMovieModel);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
            return View(createMovieModel);
        }

        try
        {
            // Upload banner image to S3
            var bannerUploadKey = $"movies-banner/{movie.MovieId}-{createMovieModel.BannerImageFile.FileName}";
            using (var stream = createMovieModel.BannerImageFile.OpenReadStream())
            {
                var uploadRequest = new Amazon.S3.Transfer.TransferUtilityUploadRequest
                {
                    InputStream = stream,
                    Key = bannerUploadKey,
                    BucketName = "movies-haneef",
                    CannedACL = S3CannedACL.PublicRead
                };
                var transferUtility = new Amazon.S3.Transfer.TransferUtility(_s3Client); // Use DI-injected _s3Client
                await transferUtility.UploadAsync(uploadRequest);
            }

            // Generate the S3 URL after successful banner image upload
            movie.BannerImageHref = $"{S3BucketPath}{bannerUploadKey}";
        }
        catch (AmazonS3Exception ex)
        {
            Console.WriteLine($"Error uploading banner image: {ex.Message}");
            ModelState.AddModelError("BannerImageFile", "An error occurred while uploading the banner image. Please try again.");
            return View(createMovieModel);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
            return View(createMovieModel);
        }

        try
        {
            // Save movie data to DynamoDB
            await _dbContext.SaveAsync(movie);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving movie data to database: {ex.Message}");
            ModelState.AddModelError(string.Empty, "An error occurred while saving the movie data. Please try again.");
            return View(createMovieModel);
        }

        // Redirect to the index action
        return RedirectToAction(nameof(Index));
    }

    // GET: Movies/Edit/{id}
    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {

        var movie = await _dbContext.LoadAsync<Movie>(id);
        if (movie == null)
        {
            return NotFound();
        }

        // Check user permission to edit the movie
        var permissionResult = VerifyIfUserHasPermissionToMovie(movie);
        if (permissionResult != null)
        {
            return permissionResult;
        }

        ViewBag.MovieHref = movie.MovieHref;
        ViewBag.BannerImageHref = movie.BannerImageHref;

        // Map Movie data to CreateMovie model for editing
        var editMovieModel = new CreateMovie
        {
            Title = movie.Title,
            Genre = movie.Genre,
            Director = movie.Director,
            ReleaseTime = movie.ReleaseTime,
            Rating = movie.Rating,
            MovieFile = null,
            BannerImageFile = null 
        };

        return View(editMovieModel);
    }

    // POST: Movies/Edit/{id}
    [HttpPost]
    public async Task<IActionResult> Edit(string id, CreateMovie editMovieModel)
    {
        if (!ModelState.IsValid)
        {
            return View(editMovieModel);
        }

        var movie = await _dbContext.LoadAsync<Movie>(id);
        if (movie == null)
        {
            ModelState.AddModelError(string.Empty, "Movie not found.");
            return View(editMovieModel);
        }

        // Check user permission to edit the movie
        var permissionResult = VerifyIfUserHasPermissionToMovie(movie);
        if (permissionResult != null)
        {
            return permissionResult;
        }

        // Update movie details
        movie.Title = editMovieModel.Title;
        movie.Genre = editMovieModel.Genre;
        movie.Director = editMovieModel.Director;
        movie.ReleaseTime = editMovieModel.ReleaseTime;
        movie.Rating = editMovieModel.Rating;

        try
        {
            // Handle movie file upload if provided
            if (editMovieModel.MovieFile != null)
            {
                // Check if there's an existing movie file and delete it
                if (!string.IsNullOrEmpty(movie.MovieHref))
                {

                    var existingMovieKey = new Uri(movie.MovieHref).AbsolutePath.TrimStart('/'); // Get the full key path
                    await _s3Client.DeleteObjectAsync("movies-haneef", existingMovieKey);
                }

                var movieFileKey = $"movies/{movie.MovieId}-{editMovieModel.MovieFile.FileName}";
                using (var stream = editMovieModel.MovieFile.OpenReadStream())
                {
                    var uploadRequest = new TransferUtilityUploadRequest
                    {
                        InputStream = stream,
                        Key = movieFileKey,
                        BucketName = "movies-haneef",
                        CannedACL = S3CannedACL.PublicRead
                    };

                    var transferUtility = new Amazon.S3.Transfer.TransferUtility(_s3Client);
                    await transferUtility.UploadAsync(uploadRequest);
                }

                // Update MovieHref with the new file URL
                movie.MovieHref = $"{S3BucketPath}{movieFileKey}";
            }

            // Handle banner image upload if provided
            if (editMovieModel.BannerImageFile != null)
            {
                // Check if there's an existing banner image and delete it
                if (!string.IsNullOrEmpty(movie.BannerImageHref))
                {
                    var existingBannerKey = new Uri(movie.BannerImageHref).AbsolutePath.TrimStart('/'); // Get the full key path
                    await _s3Client.DeleteObjectAsync("movies-haneef", existingBannerKey);
                }

                var bannerImageKey = $"movies-banner/{movie.MovieId}-{editMovieModel.BannerImageFile.FileName}";
                using (var stream = editMovieModel.BannerImageFile.OpenReadStream())
                {
                    var uploadRequest = new TransferUtilityUploadRequest
                    {
                        InputStream = stream,
                        Key = bannerImageKey,
                        BucketName = "movies-haneef",
                        CannedACL = S3CannedACL.PublicRead
                    };

                    var transferUtility = new Amazon.S3.Transfer.TransferUtility(_s3Client);
                    await transferUtility.UploadAsync(uploadRequest);
                }

                // Update BannerImageHref with the new file URL
                movie.BannerImageHref = $"{S3BucketPath}{bannerImageKey}";
            }

            // Save updated movie data
            await _dbContext.SaveAsync(movie);
        }
        catch (AmazonS3Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"S3 Upload Error: {ex.Message}");
            return View(editMovieModel);
        }
        catch (AmazonDynamoDBException ex)
        {
            ModelState.AddModelError(string.Empty, $"Database Error: {ex.Message}");
            return View(editMovieModel);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"An unexpected error occurred: {ex.Message}");
            return View(editMovieModel);
        }

        return RedirectToAction(nameof(Index));
    }

    // DELETE: Movies/Delete/{id}
    [HttpGet]
    public async Task<IActionResult> Delete(string id)
    {
        var movie = await _dbContext.LoadAsync<Movie>(id);
        if (movie == null)
        {
            return NotFound();
        }

        // Check user permission to edit the movie
        var permissionResult = VerifyIfUserHasPermissionToMovie(movie);
        if (permissionResult != null)
        {
            return permissionResult;
        }

        return View(movie);
    }
    // DELETE: Movies/Delete/{id}
    [HttpPost]
    public async Task<IActionResult> DeleteConfirm(string id)
    {
        var movie = await _dbContext.LoadAsync<Movie>(id);
        if (movie == null)
        {
            return NotFound();
        }

        // Check user permission to edit the movie
        var permissionResult = VerifyIfUserHasPermissionToMovie(movie);
        if (permissionResult != null)
        {
            return permissionResult;
        }

        try
        {
            // Delete the movie file and banner image from S3 if they exist
            if (!string.IsNullOrEmpty(movie.MovieHref))
            {
                var movieFileKey = new Uri(movie.MovieHref).AbsolutePath.TrimStart('/'); // Extract the file key from URL
                await _s3Client.DeleteObjectAsync("movies-haneef", movieFileKey);
            }

            if (!string.IsNullOrEmpty(movie.BannerImageHref))
            {
                var bannerImageKey = new Uri(movie.BannerImageHref).AbsolutePath.TrimStart('/');
                await _s3Client.DeleteObjectAsync("movies-haneef", bannerImageKey);
            }

            // Delete the movie from DynamoDB
            await _dbContext.DeleteAsync<Movie>(id);

            TempData["SuccessMessage"] = "Movie deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (AmazonS3Exception ex)
        {
            ModelState.AddModelError(string.Empty, "S3 Upload Error");
            return View("Delete", movie); ;
        }
        catch (AmazonDynamoDBException ex)
        {
            ModelState.AddModelError(string.Empty, "Database Error");
            return View("Delete", movie); ;
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, "An unexpected error occurred");
            return View("Delete", movie); ;
        }

    }


    private IActionResult VerifyIfUserHasPermissionToMovie(Movie movie)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        Console.WriteLine("Movie: " + (movie.UploaderId != userId) + ", " + movie.UploaderId);
        if (userId == null || movie == null || movie.UploaderId != userId)
        {
            ModelState.AddModelError(string.Empty, "You do not have permission to delete this file.");
            return RedirectToAction("Login", "Users");
        }
        return null;
    }



}