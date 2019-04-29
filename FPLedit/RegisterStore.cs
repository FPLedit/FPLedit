using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit
{
    public class RegisterStore
    {
        private readonly Dictionary<Type, List<object>> store;

        public RegisterStore()
        {
            store = new Dictionary<Type, List<object>>();
        }

        public void Register<T>(T obj)
        {
            Type t = typeof(T);
            if (!store.ContainsKey(t))
                store.Add(typeof(T), new List<object>());
            store[t].Add(obj);
        }

        public T[] GetRegistered<T>()
        {
            store.TryGetValue(typeof(T), out List <object> res);

            return res?.Select(o => (T)o).ToArray() ?? new T[0];
        }
    }
}
