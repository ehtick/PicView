using System.Globalization;
using System.Numerics;
using BenchmarkDotNet.Attributes;
using PicView.Core.FileHandling;
using ZLinq;

namespace PicView.Benchmarks.StringBenchmarks;

[MemoryDiagnoser] // track allocations
public class FileSizeBenchmark
{
    private const int MaxSize = 12;

    private static readonly string[] Suffixes = ["B", "KB", "MB", "GB", "TB", "PB", "EB"];
    private List<FileInfo>? _fileInfos;

    [GlobalSetup]
    public async ValueTask Setup()
    {
        await LoadSettingsAsync();

        var picturesPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        _fileInfos = new DirectoryInfo(picturesPath)
            .DescendantsAndSelf()
            .OfType<FileInfo>()
            .Where(x => x.IsSupported())
            .Take(MaxSize * 6)
            .ToList();
    }

    [Benchmark]
    public void GetReadableFileSize()
    {
        for (var i = 0; i < MaxSize; i++)
        {
            GetReadableFileSize(_fileInfos[i].Length);
        }
    }

    public static string GetReadableFileSize(long fileSize)
    {
        double value;
        char prefix;

        const long gigabyte = 0x40000000;
        const long megabyte = 0x100000;
        const long kilobyte = 0x400;

        switch (fileSize)
        {
            // Gigabyte
            case >= gigabyte:
                prefix = 'G';
                value = (double)fileSize / gigabyte;
                break;
            // Megabyte
            case >= megabyte:
                prefix = 'M';
                value = (double)fileSize / megabyte;
                break;
            // Kilobyte
            case >= kilobyte:
                prefix = 'K';
                value = (double)fileSize / kilobyte;
                break;
            // Byte
            default:
                return fileSize.ToString("0 B", CultureInfo.CurrentCulture);
        }

        return value.ToString($"0.## {prefix}B", CultureInfo.CurrentCulture);
    }

    [Benchmark]
    public void GetReadableFileSize_BitShift()
    {
        for (var i = 0; i < MaxSize; i++)
        {
            GetReadableFileSize_BitShift(_fileInfos[i].Length);
        }
    }

    [Benchmark]
    public void GetReadableFileSize_StackAlloc()
    {
        for (var i = 0; i < MaxSize; i++)
        {
            GetReadableFileSize_StackAlloc(_fileInfos[i].Length);
        }
    }

    public static string GetReadableFileSize_BitShift(long fileSize)
    {
        // Handle the zero-byte case explicitly
        if (fileSize <= 0)
        {
            return "0 B";
        }

        // Calculate the magnitude using Log2.
        // For 1023 (0-9 bits set), Log2 is 9.  9 / 10 = 0. Suffix: B
        // For 1024 (10 bits set),  Log2 is 10. 10 / 10 = 1. Suffix: KB
        var magnitude = BitOperations.Log2((ulong)fileSize) / 10;

        // If the magnitude is 0, it's just bytes. Format without decimals.
        if (magnitude == 0)
        {
            return fileSize.ToString("0 B", CultureInfo.CurrentCulture);
        }

        // Calculate the readable value using a fast bit-shift.
        var value = (double)fileSize / (1L << (magnitude * 10));

        // Format the final string.
        return value.ToString($"0.## {Suffixes[magnitude]}", CultureInfo.CurrentCulture);
    }

    public static string GetReadableFileSize_StackAlloc(long fileSize)
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

/*

* Summary *

BenchmarkDotNet v0.15.3, Windows 10 (10.0.19045.6332/22H2/2022Update)
AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.100-rc.1.25451.107
  [Host]     : .NET 10.0.0 (10.0.0-rc.1.25451.107, 10.0.25.45207), X64 RyuJIT x86-64-v4
  DefaultJob : .NET 10.0.0 (10.0.0-rc.1.25451.107, 10.0.25.45207), X64 RyuJIT x86-64-v4


| Method                         | Mean       | Error   | StdDev  | Gen0   | Allocated |
|------------------------------- |-----------:|--------:|--------:|-------:|----------:|
| GetReadableFileSize            | 1,429.9 ns | 3.36 ns | 2.62 ns | 0.0191 |     960 B |
| GetReadableFileSize_BitShift   | 1,371.5 ns | 3.66 ns | 3.25 ns | 0.0191 |     960 B |
| GetReadableFileSize_StackAlloc |   210.1 ns | 0.51 ns | 0.43 ns | 0.0095 |     480 B |

*/