using System;
using System.Collections.Generic;
using System.Linq;

namespace BTCMachine
{
    internal static class DictionaryExtensions
    {
        internal static string ToQueryString(this Dictionary<string, object> source)
        {
            if (source == null)
                throw new ArgumentNullException();
            return source.Count == 0 ? string.Empty : "?" + string.Join("&", source.Select<KeyValuePair<string, object>, string>((Func<KeyValuePair<string, object>, string>)(x => x.Key + "=" + x.Value.ToString())));
        }
    }
}
