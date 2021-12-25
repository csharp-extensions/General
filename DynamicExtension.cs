using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharpExtensions.OpenSource
{
    public static class DynamicExtension
    {
        public static dynamic GetNestedObj(dynamic obj, params string[] props)
        {
            dynamic res = obj;
            foreach (var propName in props)
            {
                var item = JsonConvert.SerializeObject(res);
                res = JsonConvert.DeserializeObject<IDictionary<string, dynamic>>(item);
                res = res[propName];
            }
            return res;
        }

        public static void RemoveProp(dynamic item, string propName)
        {
            try
            {
                var dict = (IDictionary<string, dynamic>)item;
                dict.Remove(propName);
            }
            catch { }
        }

        public static List<string> GetKeys(dynamic item)
        {
            if (item == null) { return new(); }
            var dict = ((IDictionary<string, dynamic>)item);
            return dict.Keys.ToList();
        }

        public static void SetProp(dynamic item, string propName, dynamic value) => ((IDictionary<string, dynamic>)item)[propName] = value;

        public static dynamic? GetOrDefault(params Func<dynamic?>[] getValueMethods) => GetOrDefault<dynamic>(0, getValueMethods);
        public static dynamic? GetOrDefault(int startedFunc = 0, params Func<dynamic?>[] getValueMethods) => GetOrDefault<dynamic>(startedFunc, getValueMethods);
        public static T? GetOrDefault<T>(params Func<dynamic?>[] getValueMethods) => GetOrDefault<T>(0, getValueMethods);

        public static T? GetOrDefault<T>(int startedFunc = 0, params Func<dynamic?>[] getValueMethods)
        {
            var jsonSettings = GenericsExtensions.GetJsonSerializerSettings(TypeNameHandling.None);
            for (int i = startedFunc; i < getValueMethods.Length; i++)
            {
                try
                {
                    var val = getValueMethods[i]();
                    var str = JsonConvert.SerializeObject(val, jsonSettings);
                    return JsonConvert.DeserializeObject<T>(str, jsonSettings);
                }
                catch { }
            }
            return default;
        }
    }
}
