using Firebase.Auth;
using Firebase.Database;

namespace Repositories
{
    public sealed class TestRepositoryContext : IRepositoryContext
    {
        private readonly string _apiKey = "AIzaSyDJzX5xiLq-bd2zRnT4JnMzIoEEWHmur3I";
        private readonly FirebaseAuthProvider _authProvider;

        private readonly string testUrl = "https://tripau-4068e-default-rtdb.europe-west1.firebasedatabase.app/";

        public FirebaseClient Client { get; set; }
        public FirebaseAuthLink AuthLink { get; set; }

        public TestRepositoryContext()
        {
            _authProvider = new FirebaseAuthProvider(new FirebaseConfig(_apiKey));
            AuthLink = new FirebaseAuthLink(_authProvider, new FirebaseAuth());
            Client = GetClient();
        }

        public bool IsAuthenticated()
        {
            if (AuthLink != null && !AuthLink.IsExpired())
                return true;
            return false;
        }

        public async Task Authenticate(string email, string password)
        {
            await SetAuthLinkAndClient(email, password, 0);
        }

        private FirebaseClient GetClient()
        {
            return new FirebaseClient(testUrl, new FirebaseOptions
            {
                AuthTokenAsyncFactory = () => Task.FromResult(GetAuthToken())
            });
        }

        public string GetAuthToken()
        {
            if (AuthLink == null)
                return "";
            if (AuthLink.IsExpired())
                return AuthLink.RefreshToken;
            return AuthLink.FirebaseToken;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="login">0 - login, 1 - register</param>
        /// <returns></returns>
        public async Task SetAuthLinkAndClient(string email, string password, int login)
        {
            if (login == 0)
                AuthLink = await _authProvider.SignInWithEmailAndPasswordAsync(email, password);
            else if (login == 1)
                AuthLink = await _authProvider.CreateUserWithEmailAndPasswordAsync(email, password);
            else
                return;
            Client = GetClient();
        }

        Task<string> IRepositoryContext.GetAuthToken()
        {
            throw new NotImplementedException();
        }
    }
}
