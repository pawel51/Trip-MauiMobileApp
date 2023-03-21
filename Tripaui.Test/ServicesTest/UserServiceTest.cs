using Repositories;
using Services;
using Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tripaui.Test.ServicesTest
{
    public sealed class UserServiceTest : FirebaseServiceTest
    {
        private readonly UsersService _usersService;
        public UserServiceTest() : base()
        {
            _usersService = new UsersService(new UsersRepository(ct));
        }

        [Fact]
        public async Task AddUser_ShouldAddUser ()
        {
            await _usersService.DropUsers();
            MyUserModel model 
                = new MyUserModel(Guid.NewGuid(), "pawel123", "admin123", "pawel@gmail.com");
            await _usersService.AddUser(model);
            var user = await _usersService.GetUserExtendedData();

            Assert.True(user.Id == model.Id);
        }


        [Fact]
        public void GetCurrentUser()
        {
            var user = _usersService.GetCurrentUser();
            Assert.True(user != null);
        }
    }
}
