using Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tripaui.Test.ServicesTest
{
    public abstract class FirebaseServiceTest
    {
        protected RepositoryContext ct;
        public FirebaseServiceTest()
        {
            ct = new RepositoryContext();
            Task.Run(() => ct.Authenticate("pawel.szapiel@onet.pl", "admin123")).Wait();
        }
    }
}
