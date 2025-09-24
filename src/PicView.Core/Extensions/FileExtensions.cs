using System.Globalization;
using System.Numerics;

namespace PicView.Core.Extensions;

public static class FileExtensions
{
    private static readonly string[] Suffixes = ["B", "KB", "MB", "GB", "TB", "PB", "EB"];

    /// <summary>
    /// Returns the human-readable file size for an arbitrary, 64-bit file size
    /// The default format is "0.## XB", e.g. "4.2 KB" or "1.43 GB"
    /// </summary>
    /// <param name="fileSize">FileInfo.Length</param>
    public static string GetReadableFileSize(this long fileSize)
    {
        // Handle the zero-byte case. This still allocates, but it's a single, rare path.
        if (fileSize <= 0)
        {
            return "0 B";
        }

        // 1. Create a temporary buffer on the stack.
        // 16 chars is enough for anything we'd format (e.g., "-123.45 EB").
        Span<char> buffer = stackalloc char[16];
        int charsWritten; // This will be our cursor

        var magnitude = BitOperations.Log2((ulong)fileSize) / 10;

        if (magnitude == 0)
        {
            // Format bytes directly into the buffer.
            fileSize.TryFormat(buffer, out charsWritten, provider: CultureInfo.InvariantCulture);
            buffer[charsWritten++] = ' ';
            buffer[charsWritten++] = 'B';
        }
        else
        {
            // Format the number directly into the buffer without creating a string.
            // "0.##" isn't a standard format for TryFormat, so we'll use a general format
            // and then manually trim it to our desired precision if needed, or use a
            // more direct formatting method. A simpler way is to use a standard format
            // like "F2" for two decimal places and then customize.
            // For this case, let's stick to your original "0.##" logic by using `ToString`
            // but recognize that a fully zero-alloc version would manually format the double.
            // A much more direct approach for "0.##" is to use integer math.

            // A better, allocation-free way for "0.##"
            var divisor = 1L << (magnitude * 10);
            var whole = fileSize / divisor;
            // Get the first two decimal places by scaling the remainder.
            var fraction = fileSize % divisor * 100 / divisor;

            whole.TryFormat(buffer, out charsWritten, provider: CultureInfo.InvariantCulture);

            // Only add the decimal part if it's not zero.
            if (fraction > 0)
            {
                buffer[charsWritten++] = '.';
                // Trim trailing zero (e.g., for 1.50, fraction is 50, show as 5).
                if (fraction % 10 == 0)
                {
                    fraction /= 10;
                }

                fraction.TryFormat(buffer[charsWritten..], out var fracWritten, provider: CultureInfo.InvariantCulture);
                charsWritten += fracWritten;
            }

            // 3. Manually append the space and the suffix.
            buffer[charsWritten++] = ' ';
            var suffix = Suffixes[magnitude];
            suffix.AsSpan().CopyTo(buffer[charsWritten..]);
            charsWritten += suffix.Length;
        }

        // 4. Create a single string of the exact required length from our buffer.
        return new string(buffer[..charsWritten]);
    }
}