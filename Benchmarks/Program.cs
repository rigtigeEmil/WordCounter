using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using WordCounter;

_ = BenchmarkRunner.Run<WordCounterBenchmarks>();

[MemoryDiagnoser]
public class WordCounterBenchmarks
{
    private const string DIRECTORY_PATH_SMALL_FILES = @"C:\git\DB\WordCounter\Files";
    private const string DIRECTORY_PATH_LARGE_FILES = @"C:\git\DB\WordCounter\FilesLarge";

    [Benchmark]
    public void CountWords1()
    {
        var counter = new WordCounter1(DIRECTORY_PATH_SMALL_FILES);
        counter.ProcessFiles();
    }
    
    [Benchmark]
    public void CountWords2_Streaming()
    {
        var counter = new WordCounter2Streaming(DIRECTORY_PATH_SMALL_FILES);
        counter.ProcessFiles();
    }
    
    [Benchmark]
    public void CountWords3_Parallel()
    {
        var counter = new WordCounter3Parallel(DIRECTORY_PATH_SMALL_FILES);
        counter.ProcessFiles();
    }
    
    [Benchmark]
    public void CountWords4_StreamingParallel()
    {
        var counter = new WordCounter4StreamingParallel(DIRECTORY_PATH_SMALL_FILES);
        counter.ProcessFiles();
    }
    
    [Benchmark]
    public void CountWords5_StreamingParallelV2()
    {
        var counter = new WordCounter5StreamingParallelV2(DIRECTORY_PATH_SMALL_FILES);
        counter.ProcessFiles();
    }
    
    [Benchmark]
    public void CountWords1_Large()
    {
        var counter = new WordCounter1(DIRECTORY_PATH_LARGE_FILES);
        counter.ProcessFiles();
    }
    
    [Benchmark]
    public void CountWords2_Streaming_Large()
    {
        var counter = new WordCounter2Streaming(DIRECTORY_PATH_LARGE_FILES);
        counter.ProcessFiles();
    }
    
    [Benchmark]
    public void CountWords3_Parallel_Large()
    {
        var counter = new WordCounter3Parallel(DIRECTORY_PATH_LARGE_FILES);
        counter.ProcessFiles();
    }
    
    [Benchmark]
    public void CountWords4_StreamingParallel_Large()
    {
        var counter = new WordCounter4StreamingParallel(DIRECTORY_PATH_LARGE_FILES);
        counter.ProcessFiles();
    }
    
    [Benchmark]
    public void CountWords5_StreamingParallelV2_Large()
    {
        var counter = new WordCounter5StreamingParallelV2(DIRECTORY_PATH_LARGE_FILES);
        counter.ProcessFiles();
    }
}