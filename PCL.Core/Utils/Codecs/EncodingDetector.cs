using System;
using System.IO;
using System.Text;

namespace PCL.Core.Utils.Codecs;

public static class EncodingDetector
{
    static EncodingDetector()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    /// <summary>
    /// 检测流中的文本编码方式（支持 Seek 的流）
    /// </summary>
    /// <param name="stream">输入流，必须支持 Seek</param>
    /// <param name="readFromBegin">是否将流重置到起始点</param>
    /// <returns>检测到的编码，未识别时返回 UTF-8 或系统默认</returns>
    public static Encoding DetectEncoding(Stream stream, bool readFromBegin = false)
    {
        if (!stream.CanRead)
            throw new ArgumentException("流必须支持读操作");
        if (!stream.CanSeek)
            throw new ArgumentException("流必须支持 Seek 操作");

        var originalPosition = stream.Position;
        var detectionPosition = readFromBegin ? 0 : originalPosition;
        if (readFromBegin) stream.Seek(detectionPosition, SeekOrigin.Begin);

        try
        {
            return _DetectByBom(stream, detectionPosition) ?? _DetectWithoutBOM(stream, detectionPosition) ?? Encoding.Default;
        }
        finally
        {
            stream.Position = originalPosition;
        }
    }

    public static Encoding DetectEncoding(byte[] bytes)
    {
        return DetectEncoding(new MemoryStream(bytes), true);
    }

    /// <summary>
    /// 根据 BOM 判断编码
    /// </summary>
    private static Encoding? _DetectByBom(Stream stream, long originalPosition)
    {
        stream.Position = originalPosition;
        var buffer = _ReadSample(stream, originalPosition, 4);
        var sampleLength = buffer.Length;

        // 对样本进行分析
        if (sampleLength >= 3 && buffer[0] == 0xef && buffer[1] == 0xbb && buffer[2] == 0xbf)
            return Encoding.UTF8; // UTF-8

        if (sampleLength >= 2)
        {
            if (buffer[0] == 0xfe && buffer[1] == 0xff)
                return Encoding.BigEndianUnicode; // UTF-16 BE
            if (buffer[0] == 0xff && buffer[1] == 0xfe)
            {
                if (sampleLength >= 4 && buffer[2] == 0x00 && buffer[3] == 0x00)
                    return Encoding.UTF32; // UTF-32 LE
                return Encoding.Unicode;   // UTF-16 LE
            }
        }

        if (sampleLength >= 4)
        {
            if (buffer[0] == 0x00 && buffer[1] == 0x00 && buffer[2] == 0xfe && buffer[3] == 0xff)
                return Encoding.GetEncoding("utf-32BE"); // UTF-32 BE
            if (buffer[0] == 0xff && buffer[1] == 0xfe && buffer[2] == 0x00 && buffer[3] == 0x00)
                return Encoding.UTF32; // UTF-32 LE
        }

        return null;
    }

    /// <summary>
    /// BOM 不存在时的备用检测策略
    /// </summary>
    private static Encoding? _DetectWithoutBOM(Stream stream, long originalPosition)
    {
        var sample = _ReadSample(stream, originalPosition, 1024);
        if (_IsValidEncoding(new UTF8Encoding(false, true), sample))
            return Encoding.UTF8;

        var gb2312 = Encoding.GetEncoding("gb2312", EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);
        return _IsValidEncoding(gb2312, sample) ? Encoding.GetEncoding("gb2312") : null;
    }

    private static byte[] _ReadSample(Stream stream, long position, int maxLength)
    {
        stream.Position = position;
        var sampleLength = (int)Math.Min(stream.Length - position, maxLength);
        var buffer = new byte[sampleLength];
        var offset = 0;
        while (offset < buffer.Length)
        {
            var read = stream.Read(buffer, offset, buffer.Length - offset);
            if (read == 0) break;
            offset += read;
        }

        return offset == buffer.Length ? buffer : buffer[..offset];
    }

    private static bool _IsValidEncoding(Encoding encoding, byte[] bytes)
    {
        try
        {
            return encoding.GetBytes(encoding.GetString(bytes)).AsSpan().SequenceEqual(bytes);
        }
        catch (DecoderFallbackException)
        {
            return false;
        }
        catch (EncoderFallbackException)
        {
            return false;
        }
    }
}