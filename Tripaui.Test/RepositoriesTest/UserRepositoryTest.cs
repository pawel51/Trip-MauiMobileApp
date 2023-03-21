using Repositories;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tripaui.Test.RepositoriesTest
{
    public class UserRepositoryTest
    {
        UsersRepository _usersRepository;
        public UserRepositoryTest()
        {
            TestRepositoryContext ct = new TestRepositoryContext();
            Task.Run(() => ct.Authenticate("pawel.szapiel999@gmail.com", "admin123"));
            _usersRepository = new UsersRepository(ct);
        }
    }
}
