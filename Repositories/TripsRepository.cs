using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json;
using Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public sealed class TripsRepository : FirebaseRepository<TripModel>
    {
        public TripsRepository(IRepositoryContext fb) : base(fb, "Trips")
        {
            

        }


        public async Task<List<string>> GetPlacesIdsFromTrip(string tripId)
        {
            var places = await _fb.Client
                .Child(node)
                .Child(_fb.AuthLink.User.LocalId)
                .Child(tripId)
                .Child("Places")
                .OnceAsListAsync<string>();
            return places.Select(e => e.Object).ToList();
        }

        public override async Task PostAsync(TripModel item)
        {
            var itemStr = JsonConvert.SerializeObject(item);
            await _fb.Client
                .Child(node)
                .Child(_fb.AuthLink.User.LocalId)
                .Child(item.Id.ToString())
                .PutAsync(itemStr);
        }
        public async Task PatchAsync(string tripId, string property, object value)
        {
            var itemStr = JsonConvert.SerializeObject(value);
            await _fb.Client
                .Child(node)
                .Child(_fb.AuthLink.User.LocalId)
                .Child(tripId)
                .Child(property)
                .PutAsync(itemStr);
        }

        public async Task DeleteAsync(string tripId)
        {
            await _fb.Client
                .Child(node)
                .Child(_fb.AuthLink.User.LocalId)
                .Child(tripId)
                .DeleteAsync();
        }

        public override async Task<IReadOnlyCollection<FirebaseObject<TripModel>>> GetAllAsync()
        {
            return await _fb.Client
                .Child(node)
                .Child(_fb.AuthLink.User.LocalId)
                .OnceAsync<TripModel>();
        }

        public async Task AddPlacesToTrip(string tripId, List<string> ids)
        {
            await _fb.Client
                .Child(node)
                .Child(_fb.AuthLink.User.LocalId)
                .Child(tripId)
                .Child("Places")
                .PutAsync(ids);
        }

        public override async Task<TripModel> GetByIdAsync(string tripId)
        {
            return await _fb.Client
                .Child(node)
                .Child(_fb.AuthLink.User.LocalId)
                .Child(tripId)
                .OnceSingleAsync<TripModel>();
        }

    }
}
