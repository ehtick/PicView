using BenchmarkDotNet.Running;
using PicView.Benchmarks.ImageBenchmarks;

BenchmarkRunner.Run<EvictingDictionaryBenchmark>();
BenchmarkRunner.Run<ImageBenchmarks>();