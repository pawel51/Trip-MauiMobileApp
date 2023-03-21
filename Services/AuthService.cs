using Firebase.Auth;
using Repositories;
using Shared.Entities;
using Shared.Responses;

namespace Services
{
    public sealed class AuthService
    {
        private readonly IRepositoryContext _context;
        private readonly UsersRepository _usersRepository;
        public AuthService(IRepositoryContext repositoryContext, UsersRepository usersRepository)
        {
            _context = repositoryContext;
            _usersRepository = usersRepository;
        }

        public async Task<BaseResponse> Login(string email, string password)
        {
            try
            {
                await _context.Authenticate(email, password);
                return new OkResponse();
            }
            catch (FirebaseAuthException ex)
            {
                var authError = new AuthErrorResponse() { Reason = ex.Reason.ToString() };
                if (ex.Reason == AuthErrorReason.InvalidEmailAddress)
                {
                    authError.Message = "Wrong email";
                }
                if (ex.Reason == AuthErrorReason.TooManyAttemptsTryLater)
                {
                    authError.Message = "Too many attempts";
                }
                if (ex.Reason == AuthErrorReason.WrongPassword)
                {
                    authError.Message = "Wrong password";
                }
                return authError;
            }
        }

        public async Task<BaseResponse> Register(MyUserModel user)
        {
            // try add user to authentication
            try
            {
                await _context.SetAuthLinkAndClient(user.Email, user.Password, 1);
                user.Password = "";
                await _usersRepository.PostAsync(user);
                return new OkResponse();
            }
            catch (FirebaseAuthException ex)
            {
                var authError = new AuthErrorResponse() { Reason = ex.Reason.ToString() };
                if (ex.Reason == AuthErrorReason.EmailExists)
                {
                    authError.Message = "Email already exists in database";
                }
                else
                {
                    authError.Message = "Wrong credentials";
                }
                return authError;
            }
        }

        public bool IsAuthenticated()
        {
            return _context.IsAuthenticated();
        }

        public User? GetCurrentUser()
        {
            return _context.AuthLink?.User;
        }


    }
}
