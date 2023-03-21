using Firebase.Database.Query;
using Newtonsoft.Json;
using Shared.Entities;
using Shared.Recommendations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class RecoRepository : FirebaseRepository<RecoModel>
    {
        public RecoRepository(IRepositoryContext fb) : base(fb, "Recommendations") { }



    }
}
