using Firebase.Auth;
using Firebase.Database;

namespace Repositories
{
    public sealed class RepositoryContext : IRepositoryContext
    {
        private readonly string _apiKey = "AIzaSyDJzX5xiLq-bd2zRnT4JnMzIoEEWHmur3I";
        private readonly FirebaseAuthProvider _authProvider;

        private readonly string devUrl = "https://tripau-4068e-default-rtdb.europe-west1.firebasedatabase.app/";

        public FirebaseClient Client { get; set; }
        public FirebaseAuthLink AuthLink { get; set; }

        public RepositoryContext()
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
            return new FirebaseClient(devUrl, new FirebaseOptions
            {
                AuthTokenAsyncFactory = () => GetAuthToken()
            });
        }

        public async Task<string> GetAuthToken()
        {
            if (AuthLink == null)
                return "";
            if (AuthLink.IsExpired())
            {
                AuthLink = await _authProvider.RefreshAuthAsync(AuthLink);
            }
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

    }
}
