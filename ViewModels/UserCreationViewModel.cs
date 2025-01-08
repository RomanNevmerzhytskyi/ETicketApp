namespace ETicketApp.ViewModels
{
    public class UserCreationViewModel
    {
        public string UserName { get; set; } = string.Empty; // Initialized to avoid nullable issues
        public string Email { get; set; } = string.Empty;    // Initialized to avoid nullable issues
        public string Password { get; set; } = string.Empty; // Initialized to avoid nullable issues
    }
}
