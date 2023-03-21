using Firebase.Database.Query;
using Newtonsoft.Json;
using Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Joins;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public sealed class ReviewsRepository : FirebaseRepository<ReviewModel>
    {
        public ReviewsRepository(IRepositoryContext fb) : base(fb, "Reviews") { }
    }
}
