using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CsharpExtensions
{
    public static class DictionaryExtensions
    {
        public static string ToString<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        {
            return "{" + string.Join(",", dictionary.Select(kv => kv.Key + "=" + kv.Value).ToArray()) + "}";
        }

        public static TValue? GetPropertyOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey propName, TValue? defaultValue = default)
            => GetOrDefault(dictionary, propName, defaultValue);
        public static TValue? GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey propName, TValue? defaultValue = default)
            => dictionary.ContainsKey(propName) ? dictionary[propName] : defaultValue;

        public static IEnumerable<(string key, string value)> GetAllHeaders(this HttpResponseMessage res) =>
            Enumerable.Empty<(string key, string value)>()
            // Add the main Response headers as a flat list of value-tuples with potentially duplicate `name` values:
            .Concat(res.Headers.SelectMany(kvp => kvp.Value.Select(v => (name: kvp.Key, value: v))));
        // Concat with the content-specific headers as a flat list of value-tuples with potentially duplicate `name` values:
        //.Concat(res.Content.Headers.SelectMany(kvp => kvp.Value.Select(v => (name: kvp.Key, value: v))));

        public static IDictionary<T, S> Merge<T, S>(this IDictionary<T, S>? dict, IDictionary<T, S>? newDict) where T : notnull
        {
            dict ??= new Dictionary<T, S>();
            newDict ??= new Dictionary<T, S>();
            foreach (var newKey in newDict.Keys)
            {
                dict[newKey] = newDict[newKey];
            }
            return dict;
        }

        public static async Task<TValue> GetOrFill<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<Task<TValue>> fillValueFunc)
        {
            if (!dictionary.ContainsKey(key))
            {
                dictionary[key] = await fillValueFunc();
            }
            return dictionary[key];
        }
        public static TValue GetOrFill<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> fillValueFunc)
            => GetOrFill(dictionary, key, () => Task.FromResult(fillValueFunc())).Result;

        public static TValue GetOrFill<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue fillValue)
            => GetOrFill(dictionary, key, () => Task.FromResult(fillValue)).Result;

        /// <summary>
        /// Compare the values of 2 dictionaries to assert whether they are equal
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dict1"></param>
        /// <param name="dict2"></param>
        /// <returns></returns>
        public static bool AreDictionaryValuesEqual<TKey, TValue>(this IDictionary<TKey, TValue> dict1, IDictionary<TKey, TValue> dict2)
        {
            var equal = false;
            // Do the dictionaries have the same number of items?
            if (dict1.Count == dict2.Count)
            {
                equal = true;
                foreach (var pair in dict1)
                {
                    TValue value;
                    if (dict2.TryGetValue(pair.Key, out value))
                    {
                        // Are the values for each key the same?
                        if (value?.GetHashCode() != pair.Value?.GetHashCode())
                        {
                            equal = false;
                            break;
                        }
                    }
                    else
                    {
                        // Second dictionary doesn't contain a value for given key
                        equal = false;
                        break;
                    }
                }
            }
            return equal;
        }
    }
}
