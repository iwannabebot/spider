using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Spider
{
    public static class Extensions
    {
        public static string ToJson(this object obj)
        {
            return JsonSerializer.Serialize(obj);
        }

        public static T FromJson<T>(this string obj)
        {
            return JsonSerializer.Deserialize<T>(obj);
        }
    }
}
