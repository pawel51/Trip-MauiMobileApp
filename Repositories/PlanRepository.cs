using Firebase.Database.Query;
using Newtonsoft.Json;
using Shared.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class PlanRepository : FirebaseRepository<TripPlan>
    {
        public PlanRepository(IRepositoryContext fb) : base(fb, "Plan") { }

    }
}
