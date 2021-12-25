using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpExtensions.OpenSource
{
    public static class BytesExtension
    {
        private static readonly List<Encoding> Encodings = Encoding.GetEncodings()
                                                                   .Select(x => Encoding.GetEncoding(x.Name, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback))
                                                                   .ToList();
        public static string DecodeBytes(this byte[] bytes)
        {
            foreach (var encoding in Encodings)
            {
                try
                {
                    return encoding.GetString(bytes);
                }
                catch { }
            }
            throw new System.Exception("DecodeBytes - Failed To Decode Bytes");
        }

        public static byte[] SmartEncoding(this string str)
        {
            foreach (var encoding in Encodings)
            {
                try
                {
                    return encoding.GetBytes(str);
                }
                catch { }
            }
            throw new System.Exception("DecodeBytes - Failed To Decode Bytes");
        }
    }
}
