using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace CSharpExtensions.OpenSource
{
    public static class JTokenExtensions
    {

        // help func for ToCleanJson
        public static JToken RemoveEmptyChildren(this JToken token)
        {
            if (token.Type == JTokenType.Object)
            {
                JObject copy = new JObject();
                foreach (JProperty prop in token.Children<JProperty>())
                {
                    JToken child = prop.Value;
                    if (child.HasValues)
                    {
                        child = RemoveEmptyChildren(child);
                    }
                    if (!IsEmpty(child))
                    {
                        copy.Add(prop.Name, child);
                    }
                }
                return copy;
            }
            else if (token.Type == JTokenType.Array)
            {
                JArray copy = new JArray();
                foreach (JToken item in token.Children())
                {
                    JToken child = item;
                    if (child.HasValues)
                    {
                        child = RemoveEmptyChildren(child);
                    }
                    if (!IsEmpty(child))
                    {
                        copy.Add(child);
                    }
                }
                return copy;
            }
            return token;
        }

        public static JToken RemoveDuplicateKeys(this JToken token)
        {
            if (token.Type == JTokenType.Object)
            {
                JObject copy = new JObject();
                foreach (JProperty prop in token.Children<JProperty>())
                {
                    JToken child = prop.Value;
                    if (child.HasValues)
                    {
                        child = RemoveDuplicateKeys(child);
                    }
                    if (!copy.ContainsKey(prop.Name))
                    {
                        copy.Add(prop.Name, child);
                    }
                }
                return copy;
            }
            else if (token.Type == JTokenType.Array)
            {
                JArray copy = new JArray();
                foreach (JToken item in token.Children())
                {
                    JToken child = item;
                    if (child.HasValues)
                    {
                        child = RemoveDuplicateKeys(child);
                    }
                    copy.Add(child);
                }
                return copy;
            }
            return token;
        }

        public static JToken RemovePropRecursive(this JToken token, params string[] propsToRemove)
        {
            if (token.Type == JTokenType.Object)
            {
                JObject copy = new JObject();
                foreach (JProperty prop in token.Children<JProperty>())
                {
                    JToken child = prop.Value;
                    if (child.HasValues)
                    {
                        child = RemovePropRecursive(child, propsToRemove);
                    }
                    if (!propsToRemove.Contains(prop.Name))
                    {
                        copy.Add(prop.Name, child);
                    }
                }
                return copy;
            }
            else if (token.Type == JTokenType.Array)
            {
                JArray copy = new JArray();
                foreach (JToken item in token.Children())
                {
                    JToken child = item;
                    if (child.HasValues)
                    {
                        child = RemovePropRecursive(child, propsToRemove);
                    }
                    copy.Add(child);
                }
                return copy;
            }
            return token;
        }

        // help func for ToCleanJson
        public static bool IsEmpty(JToken token) => (token.Type == JTokenType.Null) || string.IsNullOrEmpty(token.ToString());

    }
}
