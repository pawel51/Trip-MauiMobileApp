using Firebase.Auth;
using Firebase.Database.Query;
using Shared.Entities;

namespace Repositories
{
    public sealed class UsersRepository : FirebaseRepository<MyUserModel>
    {
        public UsersRepository(IRepositoryContext fb) : base (fb, "Users") 
        {

        }
        public async Task<bool> CheckIfUserExists(string username)
        {
            var usersList = await _fb.Client.Child("Users").OnceAsListAsync<MyUserModel>();
            foreach (var user in usersList)
            {
                if (String.Compare(user.Object.UserName, username) == 0)
                    return true;
            }
            return false;
        }

        public async Task<MyUserModel> GetUserByEmail(string email)
        {
            return await _fb.Client
                .Child("Users")
                .Child(email)
                .OnceSingleAsync<MyUserModel>();
        }

        public async Task DropUsers()
        {
            await _fb.Client.Child("Users").DeleteAsync();
        }

        public User GetCurrentUser()
        {
            return _fb.AuthLink.User;
        }
    }
}
