using System.ComponentModel.DataAnnotations;

namespace PostService.Dtos
{
    public partial class PostToEditDto
    {
        [Required(ErrorMessage = "Post ID is required")]
        public int PostId {get; set;}
        
        [Required(ErrorMessage = "User ID is required")]
        public int UserId {get; set;}
        
        [Required(ErrorMessage = "Post title is required")]
        [StringLength(255, MinimumLength = 1, ErrorMessage = "Post title must be between 1 and 255 characters")]
        public string PostTitle {get; set;}
        
        [StringLength(5000, ErrorMessage = "Post content cannot exceed 5000 characters")]
        public string PostContent {get; set;}

        public PostToEditDto()
        {
            if (PostTitle == null)
            {
                PostTitle = "";
            }
            if (PostContent == null)
            {
                PostContent = "";
            }
        }
    }
}