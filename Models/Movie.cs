using Amazon.DynamoDBv2.DataModel;
using System.ComponentModel.DataAnnotations;

namespace _301301555_301287005_Laylay_Muhammad__Lab3.Models
{
    [DynamoDBTable("Movies")]
    public class Movie
    {
        // Primary Key
        [DynamoDBHashKey]  // Partition Key
        public string MovieId { get; set; }

        // Attributes
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
        public string Title { get; set; }

        public string Genre { get; set; }


        public string Director { get; set; }

        public DateTime ReleaseTime { get; set; }
        public int UploaderId { get; set; }

        public string MovieHref { get; set; }

        public string BannerImageHref { get; set; }

        // Rating attribute for secondary index, stores the avg. rating value
        public double Rating { get; set; }

        // List of comments (map or list of maps in DynamoDB)
        public List<Comment> Comments { get; set; } = new List<Comment>();

        [DynamoDBProperty]
        public string Category { get; set; } = "Movies";

    }
    public class Comment
    {
        public string CommentId { get; set; }  // Unique ID for each comment
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Content { get; set; }
        public DateTime PostedAt { get; set; }

        public double Rating { get; set; }
    }
}
