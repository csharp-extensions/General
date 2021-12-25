using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Text;

namespace CSharpExtensions.OpenSource
{
    public static class GenericsExtensions
    {
        private static JsonSerializerSettings? _JsonSerializerSettings;
        public static JsonSerializerSettings JsonSerializerSettings => _JsonSerializerSettings ??= GetJsonSerializerSettings();
        public static JsonSerializerSettings GetJsonSerializerSettings(TypeNameHandling typeNameHandling = TypeNameHandling.Auto)
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = typeNameHandling,
                StringEscapeHandling = StringEscapeHandling.EscapeNonAscii,
                NullValueHandling = NullValueHandling.Ignore
            };
            settings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            return settings;
        }
        public static List<T> ItemToList<T>(this T item) => new List<T> { item };
        public static Dictionary<string, string?>? ToDictionary<T>(T obj) where T : class
        {
            if (obj == null) { return null; }
            var dict = new Dictionary<string, string?>();
            foreach (var prop in obj.GetType().PowerfulGetProperties())
            {
                var value = prop.GetValue(obj);
                dict[prop.Name] = value is string str ? str : value.ToJson();
            }
            return dict;
        }

        public static ulong ToUlong<T>(this T obj) where T : notnull => ulong.Parse(obj.ToString()!);
        public static long ToLong<T>(this T obj) where T : notnull => long.Parse(obj.ToString()!);
        public static float? ToFloat<T>(this T obj) where T : notnull => float.TryParse(obj.ToString(), out var res) ? res : null;
        public static Expression<Func<T, object?>> ToObjectExpression<T, S>(this Expression<Func<T, S>> expression)
        => Expression.Lambda<Func<T, object?>>(Expression.TypeAs(expression.Body, typeof(object)), expression.Parameters);

        public static Task WithTimeout(this Task task, int timeoutInMilliSec, string extraInfo = "") => Task.Run(async () => { await task; return true; }).WithTimeout(timeoutInMilliSec, extraInfo);
        public static async Task<T?> WithTimeout<T>(this Task<T> task, int timeoutInMilliSec, string extraInfo = "")
        {
            var timespan = TimeSpan.FromMilliseconds(timeoutInMilliSec);
            var timeoutTask = Task.Delay(timespan);
            T? value = default;
            var wrappedTask = Task.Run(async () => value = await task);
            var firstComplatedTask = await Task.WhenAny(wrappedTask, timeoutTask);
            if (firstComplatedTask != timeoutTask)
            {
                if (wrappedTask.IsCompletedSuccessfully)
                {
                    return value;
                }
                else
                {
                    throw new Exception($"{extraInfo}task not successfully status={task.Status}, ex {task.Exception?.Message}", task.Exception);
                }
            }
            throw new TimeoutException($"{extraInfo}task failed to finish before timeout: {timespan.ToHumanReadableString()}", null);
        }
        // set prop or field to object with non-public set
        public static void SetFieldOrProperty<T>(this T item, string name, object? value)
        {
            var type = item?.GetType() ?? typeof(T);
            var field = type.PowerfulGetField(name);
            if (field != null)
            {
                field.SetValue(item, value);
                return;
            }

            var prop = type.PowerfulGetProperty(name);
            if (prop != null)
            {
                prop.SetValue(item, value);
                return;
            }
            throw new Exception($"SetFieldOrProperty - property/field - {name} - not found");
        }

        public static object? GetNestedFieldOrProperty<T>(this T item, string nestedPath)
        {
            object? obj = item;
            foreach (var prop in nestedPath.Split("."))
            {
                obj = obj.GetFieldOrProperty(prop);
            }
            return obj;
        }

        public static object? GetFieldOrProperty<T>(this T item, string name)
        {
            var type = item?.GetType() ?? typeof(T);
            var field = type.PowerfulGetField(name);
            if (field != null)
            {
                return field.GetValue(item);
            }

            var prop = type.PowerfulGetProperty(name);
            if (prop != null)
            {
                return prop.GetValue(item);
            }
            throw new Exception($"GetFieldOrProperty - property/field - {name} - not found");
        }

        public static FieldInfo? PowerfulGetField(this Type type, string name)
            => SafeGetter(() => type.GetField(name, (BindingFlags)(-1)))
            ?? SafeGetter(() => type.GetField(name, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));

        public static PropertyInfo? PowerfulGetProperty(this Type type, string name)
            => SafeGetter(() => type.GetProperty(name, (BindingFlags)(-1)))
            ?? SafeGetter(() => type.GetProperty(name, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));

        public static PropertyInfo[] PowerfulGetProperties(this Type type)
            => type.GetProperties((BindingFlags)(-1))
                   .Concat(type.GetProperties(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                   .DistinctBy(x => x.Name)
                   .ToArray();

        public static FieldInfo[] PowerfulGetFields(this Type type)
            => type.GetFields((BindingFlags)(-1))
                   .Concat(type.GetFields(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                   .DistinctBy(x => x.Name)
                   .Where(x => !x.Name.Contains("k__BackingField"))
                   .ToArray();

        public static MemberInfo[] PowerfulGetFieldsAndProperties(this Type type)
            => type.PowerfulGetProperties()
                   .Cast<MemberInfo>()
                   .Concat(type.PowerfulGetFields())
                   .DistinctBy(x => x.Name)
                   .ToArray();

        public static Type? GetTargetType(this MemberInfo info)
        {
            if (info is PropertyInfo propInfo)
            {
                return propInfo.PropertyType;
            }
            else if (info is FieldInfo fieldInfo)
            {
                return fieldInfo.FieldType;
            }
            return null;
        }

        public static MemberInfo? PowerfulGetFieldOrProperty(this Type type, string name) => (MemberInfo?)type.PowerfulGetProperty(name) ?? type.PowerfulGetField(name);
        public static Type GetMemberType(this MemberInfo mi) => mi switch
        {
            PropertyInfo p => p.PropertyType,
            FieldInfo f => f.FieldType,
            _ => throw new Exception("GetMemberType implitation missing")
        };

        private static T? SafeGetter<T>(Func<T?> getter) where T : class
        {
            try
            {
                return getter();
            }
            catch { return null; }
        }

        // copy all props (if not exclude) recursive between 2 diffrent types, not case sensetive, using json convert so fix convert between
        // basic types like string to int '1' to 1
        public static void CopyTo<T, S>(this T copyFrom, S copyTo, IEnumerable<string>? excludePropsList = null, Type? targetType = null)
        {
            var type = copyFrom?.GetType() ?? typeof(T);
            targetType ??= copyTo?.GetType() ?? typeof(S);
            excludePropsList = excludePropsList != null ? excludePropsList : new List<string>();
            excludePropsList = excludePropsList.Select(x => x.ToLower()).ToList();
            var propsFrom = type.PowerfulGetProperties().Where(x => !excludePropsList.Contains(x.Name.ToLower())).ToList();
            var propsTo = targetType.PowerfulGetProperties().Where(x => !excludePropsList.Contains(x.Name.ToLower())).ToList();
            var commonProps = propsTo.Select(x => x.Name.ToLower()).Intersect(propsFrom.Select(x => x.Name.ToLower()));
            foreach (var commonProp in commonProps)
            {
                try
                {
                    var propTo = propsTo.First(x => x.Name.ToLower() == commonProp);
                    var propFrom = propsFrom.First(x => x.Name.ToLower() == commonProp);
                    var newValue = propFrom.GetValue(copyFrom);
                    var oldValue = propTo.GetValue(copyTo);

                    if (oldValue.ToJson() != newValue.ToJson())
                    {
                        try
                        {
                            propTo.SetValue(copyTo, JsonConvert.DeserializeObject(JsonConvert.SerializeObject(newValue, JsonSerializerSettings), propTo.PropertyType, JsonSerializerSettings), null);
                        }
                        catch (Exception)
                        {
                            var json = JsonConvert.SerializeObject(newValue);
                            propTo.SetValue(copyTo, JsonConvert.DeserializeObject(json, propTo.PropertyType), null);
                            throw;
                        }
                    }
                }
                catch { }
            }

            var fieldsFrom = type.PowerfulGetFields().Where(x => !excludePropsList.Contains(x.Name.ToLower())).ToList();
            var fieldsTo = targetType.PowerfulGetFields().Where(x => !excludePropsList.Contains(x.Name.ToLower())).ToList();
            var commonFields = fieldsTo.Select(x => x.Name.ToLower()).Intersect(fieldsFrom.Select(x => x.Name.ToLower()));
            foreach (var commonField in commonFields)
            {
                try
                {
                    var fieldTo = fieldsTo.First(x => x.Name.ToLower() == commonField);
                    var fieldFrom = fieldsFrom.First(x => x.Name.ToLower() == commonField);
                    var newValue = fieldFrom.GetValue(copyFrom);
                    var oldValue = fieldTo.GetValue(copyTo);

                    if (oldValue.ToJson() != newValue.ToJson())
                    {
                        try
                        {
                            fieldTo.SetValue(copyTo, JsonConvert.DeserializeObject(JsonConvert.SerializeObject(newValue, JsonSerializerSettings), fieldTo.FieldType, JsonSerializerSettings));
                        }
                        catch (Exception)
                        {
                            var json = JsonConvert.SerializeObject(newValue);
                            fieldTo.SetValue(copyTo, JsonConvert.DeserializeObject(json, fieldTo.FieldType));
                        }
                    }
                }
                catch { }
            }
        }

        // compare all props (if not exclude) recursive between 2 diffrent types, not case sensitive, using json convert so fix convert between
        // basic types like string to int '1' to 1
        public static bool Compare<T, S>(this T item1, S item2, IEnumerable<string>? excludePropsList = null)
        {
            var type1 = item1?.GetType() ?? typeof(T);
            var type2 = item2?.GetType() ?? typeof(S);
            excludePropsList = excludePropsList != null ? excludePropsList : new List<string>();
            excludePropsList = excludePropsList.Select(x => x.ToLower()).ToList();
            var propsItem1 = type1.PowerfulGetProperties().Where(x => !excludePropsList.Contains(x.Name.ToLower())).ToList();
            var propsItem2 = type2.PowerfulGetProperties().Where(x => !excludePropsList.Contains(x.Name.ToLower())).ToList();
            if (propsItem1.Count != propsItem2.Count)
            {
                return false;
            }
            var commonProps = propsItem1.Select(x => x.Name.ToLower()).Intersect(propsItem2.Select(x => x.Name.ToLower())).ToList();
            if (commonProps.Count != propsItem1.Count)
            {
                return false;
            }

            foreach (var commonProp in commonProps)
            {
                var propItem1 = propsItem1.First(x => x.Name.ToLower() == commonProp);
                var val1 = propItem1.GetValue(item1);

                var propItem2 = propsItem2.First(x => x.Name.ToLower() == commonProp);
                var val2 = propItem2.GetValue(item2);

                if (val1.ToJson() != val2.ToJson())
                {
                    return false;
                }
            }
            return true;
        }

        // cast between 2 classes, using CopyTo method, ignore case sensitive
        public static S? JsonCast<T, S>(this T copyFrom, IEnumerable<string>? excludePropsList = null) => Cast<T, S>(copyFrom, excludePropsList);

        public static S? Cast<T, S>(this T copyFrom, IEnumerable<string>? excludePropsList = null)
        {
            excludePropsList = excludePropsList != null ? excludePropsList : new List<string>();
            var newInstance = ((copyFrom?.GetType() ?? typeof(T)).IsCollection() ? "[]" : "{}").FromJson<S>();
            copyFrom.CopyTo(newInstance, excludePropsList);
            return newInstance;
        }
        public static object? Cast<T>(this T copyFrom, Type targetType, IEnumerable<string>? excludePropsList = null)
        {
            excludePropsList = excludePropsList != null ? excludePropsList : new List<string>();
            var newInstance = ((copyFrom?.GetType() ?? typeof(T)).IsCollection() ? "[]" : "{}").FromJson(targetType);
            copyFrom.CopyTo(newInstance, excludePropsList, targetType);
            return newInstance;
        }

        private static bool IsCollection(this Type type)
        {
            return type.GetInterface(nameof(IDictionary)) == null && ((type.GetInterface(nameof(ICollection)) != null) || (type.GetInterface(nameof(IEnumerable)) != null));
        }

        // using json lib to create copy of class
        public static T? Clone<T>(this T source) where T : class => source.Cast<T, T>();

        // extra name when other libs like mongo block our extension
        public static string? ToJsonExt<T>(this T source, bool format = false) => ToJson(source, format);

        // using json lib to create json from class
        public static string? ToJson<T>(this T source, bool format = false)
        {
            if (source == null)
            {
                return null;
            }
            return JsonConvert.SerializeObject(source, !format ? Formatting.None : Formatting.Indented, JsonSerializerSettings);
        }

        // DeserializeObject object from json string
        public static T? FromJson<T>(this string json)
        {
            if (json.IsEmpty())
            {
                return default;
            }
            return JsonConvert.DeserializeObject<T>(json, JsonSerializerSettings);
        }

        // DeserializeObject object from json string
        public static object? FromJson(this string json, Type targetType)
        {
            if (json.IsEmpty())
            {
                return default;
            }
            return JsonConvert.DeserializeObject(json, targetType, JsonSerializerSettings);
        }

        // using json lib to create json from class, and clean all default/null values from json
        public static string? ToCleanJson<T>(this T obj) where T : class
        {
            try
            {
                return obj.ToJson()?.FromJson<JObject>()?.RemoveEmptyChildren().ToJson();
            }
            catch
            {
                return obj.ToJson();
            }
        }

        // using json lib to create json from class, and clean all default/null values from json
        public static string? ToCleanJson(this string str)
        {
            try
            {
                return str.FromJson<JObject>()?.RemoveEmptyChildren().ToJson();
            }
            catch
            {
                return str;
            }
        }
    }
}
