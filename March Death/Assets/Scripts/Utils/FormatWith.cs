using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Utils
{
    public static class StringExtension
    {
        public static string Repeat(this string str, int count)
        {
            return Enumerable.Repeat(str, count).Aggregate(
                new StringBuilder(), (sb, s) => sb.Append(s)).ToString();
        }

        public static string FormatWith(this string format, object source, string opener = @"\{\{", string closer = @"\}\}")
        {
            return FormatWith(format, null, source, opener, closer);
        }

        public static string FormatWith(this string format, IFormatProvider provider, object source, string opener, string closer)
        {
            if (format == null)
            {
                throw new ArgumentNullException("format");

            }
            Regex r = new Regex(@"(?<start>" + opener + @")+(?<property>[\w\.\[\]]+)(?<format>:[^}]+)?(?<end>" + closer + @")+",
            /*RegexOptions.Compiled | */RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

            List<object> values = new List<object>();
            string rewrittenFormat = r.Replace(format, delegate(Match m)
            {
                Group startGroup = m.Groups["start"];
                Group propertyGroup = m.Groups["property"];
                Group formatGroup = m.Groups["format"];
                Group endGroup = m.Groups["end"];

                string[] splitted = propertyGroup.Value.Split(new string[] { "." }, StringSplitOptions.None);
                object current = source;
                int i = 0;
                do
                {
                    if (current.GetType().GetField(splitted[i]) != null)
                    {
                        current = current.GetType().GetField(splitted[i]).GetValue(current);
                    }
                    else
                    {
                        current = current.GetType().GetProperty(splitted[i]).GetValue(current, null);
                    }

                    ++i;
                }
                while (i < splitted.Length);

                values.Add(current);

                return new string('{', startGroup.Captures.Count) + (values.Count - 1) + formatGroup.Value
                + new string('}', endGroup.Captures.Count);
            });

            if (values.Count == 0)
            {
                return format;
            }

            return string.Format(provider, rewrittenFormat, values.ToArray());
        }
    }
}
