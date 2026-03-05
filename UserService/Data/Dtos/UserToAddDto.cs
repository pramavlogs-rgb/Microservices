using System.ComponentModel.DataAnnotations;

namespace UserService.Dtos
{
    public partial class UserToAddDto
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 20 characters")]
        public string FirstName {get; set;}
        
        [Required(ErrorMessage = "Last name is required")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 20 characters")]
        public string LastName {get; set;}
        
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(50, ErrorMessage = "Email cannot exceed 50 characters")]
        public string Email {get; set;}
        
        [Required(ErrorMessage = "Gender is required")]
        [StringLength(6, ErrorMessage = "Gender cannot exceed 6 characters")]
        public string Gender {get; set;}
        
        public bool Active {get; set;}

        public UserToAddDto()
        {
            if (FirstName == null)
            {
                FirstName = "";
            }
            if (LastName == null)
            {
                LastName = "";
            }
            if (Email == null)
            {
                Email = "";
            }
            if (Gender == null)
            {
                Gender = "";
            }
        }
    }
}