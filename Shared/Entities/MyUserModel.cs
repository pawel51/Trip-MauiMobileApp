namespace Shared.Entities
{
    public sealed class MyUserModel
    {
        public MyUserModel(Guid id, string userName, string password, string email)
        {
            Id = id;
            UserName = userName;
            Password = password;
            Email = email;
        }

        public Guid Id { get; set; }
        public string UserName { get; set; } 
        public string Password { get; set; } 
        public string Email { get; set; }
    }
}
