using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json;

namespace Repositories
{
    public abstract class FirebaseRepository<T>
    {
        protected IRepositoryContext _fb;
        protected string node = "";

        public FirebaseRepository(IRepositoryContext fb, string nodeName)
        {
            node = nodeName;
            _fb = fb;
        }


        public virtual async Task PostAtIdAsync(T item, string id, string id2 = "")
        {
            var itemStr = JsonConvert.SerializeObject(item);
            var query = _fb.Client
                .Child(node)
                .Child(id);
            // it save items on the same level, but post generates its own primary key
            if (id2 == "")
                await query.PostAsync(itemStr);
            else
                await query.Child(id2).PutAsync(itemStr);

        }

        public virtual async Task PostAsync(T item)
        {
            var itemStr = JsonConvert.SerializeObject(item);
            await _fb.Client
                .Child(node)
                .PostAsync(itemStr);
        }

        public virtual async Task PutAsync(T item, string key)
        {
            var itemStr = JsonConvert.SerializeObject(item);
            await _fb.Client
                .Child(node)
                .Child(key)
                .PutAsync(itemStr);
        }

        public async Task PutAsync(T item)
        {
            var itemStr = JsonConvert.SerializeObject(item);
            await _fb.Client
                .Child(node)
                .PutAsync(itemStr);
        }

        public async Task PatchAsync(T item)
        {
            var itemStr = JsonConvert.SerializeObject(item);
            await _fb.Client
                .Child(node)
                .PatchAsync(itemStr);
        }

        public async Task DeleteByIdAsync(string id)
        {
            await _fb.Client
                .Child(node)
                .Child(id)
                .DeleteAsync();
        }

        public async Task DeleteOfIdByIdAsync(string id, string id2)
        {
            await _fb.Client
                .Child(node)
                .Child(id)
                .Child(id2)
                .DeleteAsync();
        }
        public async Task DeleteAllAsync()
        {
            await _fb.Client
                .Child(node)
                .DeleteAsync();
        }

        public virtual async Task<T> GetByIdAsync(string id)
        {
            return await _fb.Client
                .Child(node)
                .Child(id)
                .OnceSingleAsync<T>();
        }

        public virtual async Task<T> GetItemOfCurrentUser()
        {
            return await _fb.Client
                .Child(node)
                .Child(_fb.AuthLink.User.LocalId)
                .OnceSingleAsync<T>();
        }

        public virtual async Task PostForCurrentUser(T item)
        {
            await _fb.Client
                .Child(node)
                .Child(_fb.AuthLink.User.LocalId)
                .PutAsync(item);
        }

        public virtual async Task<IReadOnlyCollection<FirebaseObject<T>>> GetAllAsync()
        {
            return await _fb.Client
                .Child(node)
                .OnceAsync<T>();
        }
        public virtual async Task<IReadOnlyCollection<FirebaseObject<T>>> GetAllOfTripItemsAsync(string tripId)
        {
            return await _fb.Client
                .Child(node)
                .Child(tripId)
                .OnceAsync<T>();
        }

        //public void SubscribeHandlerToObservable(Command cmd)
        //{
        //    var query = _fb.Client.Child(node);
        //    query.AsObservable<IEnumerable<T>>().Subscribe((dbEvent) =>
        //    {
        //        if (dbEvent.Object != null)
        //        {
        //            cmd.Execute(dbEvent.Object);
        //        }
        //    });
        //}
    }
}
