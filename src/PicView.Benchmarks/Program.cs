using BenchmarkDotNet.Running;
using PicView.Benchmarks;
using PicView.Benchmarks.ImageBenchmarks;

// BenchmarkRunner.Run<EvictingDictionaryBenchmark>();
// BenchmarkRunner.Run<ImageBenchmarks>();
BenchmarkRunner.Run<PreloadingBenchmark>();
// BenchmarkRunner.Run<TranslationBenchmarks>();