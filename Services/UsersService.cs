using Firebase.Auth;
using Repositories;
using Shared.Entities;
using Shared.Responses;

namespace Services
{
    public sealed class UsersService
    {
        private readonly UsersRepository _usersRepository;
        public UsersService(UsersRepository usersRepository)
        {
            _usersRepository = usersRepository;
        }

        public async Task<BaseResponse> AddUser(MyUserModel user)
        {
            user.Password = "";
            await _usersRepository.PutAsync(user, GetCurrentUser().LocalId);
            return new OkResponse();
        }

        public async Task<MyUserModel> GetUserByEmail(string email)
        {
            return await _usersRepository.GetUserByEmail(email.Replace('@', '2'));
        }

        public async Task DropUsers()
        {
            await _usersRepository.DropUsers();
        }

        public async Task<MyUserModel> GetUserExtendedData()
        {
            var currentUser = GetCurrentUser();
            return await _usersRepository.GetByIdAsync(currentUser.LocalId);
        }

        public User GetCurrentUser()
        {
            return _usersRepository.GetCurrentUser();
        }
    }
}
