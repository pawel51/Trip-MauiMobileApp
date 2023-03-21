using Firebase.Auth;
using Firebase.Database;

namespace Repositories
{
    public interface IRepositoryContext
    {
        FirebaseAuthLink AuthLink { get; set; }
        FirebaseClient Client { get; set; }

        Task Authenticate(string email, string password);
        Task<string> GetAuthToken();
        bool IsAuthenticated();
        Task SetAuthLinkAndClient(string email, string password, int login);
    }
}