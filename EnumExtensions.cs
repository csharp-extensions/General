using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace CSharpExtensions.OpenSource
{
    public static class EnumExtensions
    {
        public static T Or<T>(this IEnumerable<T> enums) where T : struct, IConvertible
        {
            var intEnums = enums.Cast<int>().ToList();
            var res = intEnums.First();
            intEnums.ForEach(x => res |= x);
            return (T)Enum.ToObject(typeof(T), res);
        }
        public static string GetEnumDescription(this Enum? value)
        {
            var type = value?.GetType();
            var memInfo = type?.GetMember(value?.ToString());
            var attributes = memInfo.FirstOrDefault()?.GetCustomAttributes(typeof(DescriptionAttribute), false);
            var description = (attributes?.FirstOrDefault() as DescriptionAttribute)?.Description;

            return description ?? throw new ArgumentException("Description field was not found as part of enum value", nameof(value));
        }

        /// <summary>
        /// Convert enum entry description to a valid enum entry
        /// Throw if no valid entry was found
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="description"></param>
        /// <returns></returns>
        public static T GetEnumValueFromDescription<T>(string description) where T : struct, IConvertible
        {
            string descriptionLower = description.ToLower();
            var type = typeof(T);
            if (!type.IsEnum)
            {
                throw new ArgumentException();
            }

            FieldInfo field = type.GetFields()
                .Where(field =>
                    field.CustomAttributes.Any() &&
                    ((DescriptionAttribute)field.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault())
                    .Description.ToLower() == descriptionLower).Single();


            if (field == null)
            {
                throw new ArgumentException($"Couldn't find Description {description} in Enum {nameof(T)}", description);
            }

            return (T)field.GetValue(null);
        }

        public static IEnumerable<T> GetValues<T>() => Enum.GetValues(typeof(T)).Cast<T>();
    }
}
