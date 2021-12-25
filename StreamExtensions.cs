using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CsharpExtensions
{
    public static class StreamExtension
    {
        /// <summary>
        /// Saves a stream to a local path.
        /// </summary>
        /// <param name="stream">The stream to save.</param>
        /// <param name="filePath">The path to write into. If a file already exists there, it will be overwritten.</param>
        public static void Save(this Stream stream, string filePath, bool seekToStart = false)
        {
            if (seekToStart)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }
            // Define buffer and buffer size
            int bufferSize = 1024;
            byte[] buffer = new byte[bufferSize];
            int bytesRead = 0;

            // Read from response and write to file
            FileStream fileStream = File.Create(filePath);
            while ((bytesRead = stream.Read(buffer, 0, bufferSize)) != 0)
            {
                fileStream.Write(buffer, 0, bytesRead);
            }

            fileStream.Close();
        }

        public static string GetBase64SData(this MemoryStream? stream)
        {
            if (stream == null) return string.Empty;
            stream.Seek(0, SeekOrigin.Begin);
            return Convert.ToBase64String(stream.ToArray());
        }

        public static void DisposeNullable(this MemoryStream? stream)
        {
            if (stream == null) return;
            stream.Dispose();
        }

        public static async Task<MemoryStream?> CopyToMemoryStream(this Stream? stream)
        {
            if (stream == null) { return null; }
            var memStream = new MemoryStream();
            await stream.CopyToAsync(memStream);
            memStream.Seek(0, SeekOrigin.Begin);
            return memStream;
        }
        public static async Task<string?> GetAllTextAsync(this Stream? stream)
        {
            if (stream == null) { return null; }
            stream.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(stream);
            var text = await reader.ReadToEndAsync();
            return text;
        }

        public static async IAsyncEnumerable<string> StreamFileLineByLine(this StreamReader? stream)
        {
            if (stream == null) { yield break; }
            string? readText;
            while ((readText = await stream.ReadLineAsync()) != null)
            {
                yield return readText;
            }
        }
        public static async IAsyncEnumerable<byte[]> StreamFileLineByLine2(this StreamReader? stream)
        {
            if (stream == null) { yield break; }
            var bytes = new List<byte>();
            int currentByte;
            while ((currentByte = stream.Read()) != -1)
            {
                if (currentByte == '\n' || currentByte == '\r')
                {
                    if (bytes.Count > 0) { yield return bytes.ToArray(); }
                    bytes.Clear();
                    continue;
                }
                bytes.Add((byte)currentByte);
            }
            if (bytes.Count > 0) { yield return bytes.ToArray(); }
            await Task.CompletedTask;
        }
        public static IAsyncEnumerable<string> StreamFileLineByLineUsingRegex(this StreamReader? stream) => StreamFileByRegex(stream, new Regex(@"[\n\r]+"));
        public static async IAsyncEnumerable<string> StreamFileByRegex(this StreamReader? stream, Regex regex, int bufferSize = 4096)
        {
            if (stream == null) { yield break; }
            var sb = new StringBuilder();
            var buffer = new char[bufferSize];
            int currentPos;
            while (!stream.EndOfStream)
            {
                currentPos = stream.Read(buffer, 0, bufferSize);
                sb.Append(buffer, 0, currentPos);
                var lines = regex.Split(sb.ToString());
                for (int i = 0; i < lines.Length - 2; i++)
                {
                    yield return lines[i];
                }
                sb = new StringBuilder(lines[lines.Length - 1]);
            }
            if (sb.Length > 0)
            {
                yield return sb.ToString();
            }
            await Task.CompletedTask;
        }
        public static async Task<Encoding?> GetEncodingByBom(this Stream stream)
        {
            using var reader = new StreamReader(stream);
            var bom = new char[4];
            await reader.ReadBlockAsync(bom, 0, 4);
            Encoding? encodingFromBom = null;
            // Analyze the BOM
            if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) { encodingFromBom = Encoding.UTF7; }
            else if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) { encodingFromBom = Encoding.UTF8; }
            else if (bom[0] == 0xff && bom[1] == 0xfe && bom[2] == 0 && bom[3] == 0) { encodingFromBom = Encoding.UTF32; } //UTF-32LE 
            else if (bom[0] == 0xff && bom[1] == 0xfe) { encodingFromBom = Encoding.Unicode; } //UTF-16LE 
            else if (bom[0] == 0xfe && bom[1] == 0xff) { encodingFromBom = Encoding.BigEndianUnicode; } //UTF-16BE 
            else if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) { encodingFromBom = new UTF32Encoding(true, true); } //UTF-32BE 
            if (encodingFromBom != null)
            {
                encodingFromBom.DecoderFallback = DecoderFallback.ExceptionFallback;
                encodingFromBom.EncoderFallback = EncoderFallback.ExceptionFallback;
            }
            return encodingFromBom;
        }
    }
}
