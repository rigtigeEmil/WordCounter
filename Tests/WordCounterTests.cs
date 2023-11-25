using FluentAssertions;
using WordCounter;
using Xunit;

namespace Tests;

public class WordCounterTests : IDisposable
{
    private readonly string _directory;
    public WordCounterTests()
    {
        _directory = ArrangeTestDirectories();
    }
    
    private string ArrangeTestDirectories()
    {
        var testDirectory = Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "TestDirectory"));
        Directory.CreateDirectory(testDirectory.FullName + "/Output");
        return testDirectory.FullName;
    }

    [Fact]
    public void GetWordCount_ShouldReturnCorrectWordCount()
    {
        // Arrange
        var wordCounter = new WordCounter2Streaming(_directory);
        var fileNames = new List<string> { $"{_directory}/testFile1.txt", $"{_directory}/testFile2.txt" };
        File.WriteAllText($"{_directory}/testFile1.txt", "Hello World");
        File.WriteAllText($"{_directory}/testFile2.txt", "Hello GitHub");

        // Act
        var result = wordCounter.GetWordCount(fileNames);

        // Assert
        result.Should().ContainKey("hello").WhoseValue.Should().Be(2);
        result.Should().ContainKey("world").WhoseValue.Should().Be(1);
        result.Should().ContainKey("github").WhoseValue.Should().Be(1);
    }

    [Fact]
    public void GetExcludedWordCount_ShouldReturnCorrectExcludedWordCount()
    {
        // Arrange
        var wordCounter = new WordCounter2Streaming(_directory);
        var words = new Dictionary<string, int> { { "hello", 2 }, { "world", 1 }, { "github", 1 } };
        File.WriteAllText($"{_directory}/exclude.txt", "hello");

        // Act
        var result = wordCounter.RemoveExcludedWordsFromDictionary(words);

        // Assert
        result.Should().NotContainKey("world");
        result.Should().NotContainKey("github");
        result.Should().ContainKey("hello").WhoseValue.Should().Be(2);
    }

    [Fact]
    public void ProcessFiles_ShouldCreateOutputFiles()
    {
        // Arrange
        var wordCounter = new WordCounter2Streaming(_directory);
        File.WriteAllText($"{_directory}/testFile.txt", "Hello World");
        File.WriteAllText($"{_directory}/exclude.txt", "hello");

        // Act
        wordCounter.ProcessFiles();

        // Assert
        File.Exists($"{_directory}/Output/FILE_H.txt").Should().BeTrue();
        File.Exists($"{_directory}/Output/FILE_W.txt").Should().BeTrue();
    }


    [Fact]
    public void GetWordCount_ShouldHandleEmptyDirectory()
    {
        // Arrange
        var wordCounter = new WordCounter2Streaming(_directory);

        // Act
        var result = wordCounter.GetWordCount(new List<string>());

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetWordCount_ShouldHandleFilesWithNoWords()
    {
        // Arrange
        var wordCounter = new WordCounter2Streaming(_directory);
        var fileNames = new List<string> { $"{_directory}/emptyFile.txt" };
        File.WriteAllText($"{_directory}/emptyFile.txt", "");

        // Act
        var result = wordCounter.GetWordCount(fileNames);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetWordCount_ShouldBeCaseInsensitive()
    {
        // Arrange
        var wordCounter = new WordCounter2Streaming(_directory);
        var fileNames = new List<string> { $"{_directory}/testFile.txt" };
        File.WriteAllText($"{_directory}/testFile.txt", "Hello hello HELLO");

        // Act
        var result = wordCounter.GetWordCount(fileNames);

        // Assert
        result.Should().ContainKey("hello").WhoseValue.Should().Be(3);
    }

    [Fact]
    public void GetWordCount_ShouldHandleMultipleSpacesTabsAndNewlines()
    {
        // Arrange
        var wordCounter = new WordCounter2Streaming(_directory);
        var fileNames = new List<string> { $"{_directory}/testFile.txt" };
        File.WriteAllText($"{_directory}/testFile.txt", "Hello\n\n\nWorld  \tGitHub");

        // Act
        var result = wordCounter.GetWordCount(fileNames);

        // Assert
        result.Should().ContainKey("hello").WhoseValue.Should().Be(1);
        result.Should().ContainKey("world").WhoseValue.Should().Be(1);
        result.Should().ContainKey("github").WhoseValue.Should().Be(1);
    }

    [Fact]
    public void GetExcludedWordCount_ShouldNotExcludeWordsNotInDictionary()
    {
        // Arrange
        var wordCounter = new WordCounter2Streaming(_directory);
        var words = new Dictionary<string, int> { { "hello", 2 }, { "world", 1 }, { "github", 1 } };
        File.WriteAllText($"{_directory}/exclude.txt", "stackoverflow");

        // Act
        var result = wordCounter.RemoveExcludedWordsFromDictionary(words);

        // Assert
        result.Should().BeEmpty();
    }


    [Fact]
    public void WriteWordsToFile_ShouldWriteWordsToCorrectFile()
    {
        // Arrange
        var wordCounter = new WordCounter2Streaming(_directory);
        var words = new Dictionary<string, int> { { "hello", 2 }, { "world", 1 }, { "github", 1 } };

        // Act
        wordCounter.WriteWordsToFile(words);

        // Assert
        var fileContent = File.ReadAllLines($"{_directory}/Output/FILE_H.txt");
        fileContent[0].Should().StartWith("hello");
    }

    [Fact]
    public void WriteExcludedWordsToFile_ShouldWriteExcludedWordsToFile()
    {
        // Arrange
        var wordCounter = new WordCounter2Streaming(_directory);
        var words = new Dictionary<string, int> { { "hello", 2 } };

        // Act
        wordCounter.WriteExcludedWordsToFile(words);

        // Assert
        var fileContent = File.ReadAllLines($"{_directory}/Output/excluded_words.txt");
        fileContent[0].Should().StartWith("hello");
    }

    [Fact]
    public void ProcessFiles_ShouldCreateAllOutputFilesWithNoInputFiles()
    {
        // Arrange
        var wordCounter = new WordCounter2Streaming(_directory);

        // Act
        wordCounter.ProcessFiles();

        // Assert
        Directory.GetFiles($"{_directory}/Output").Length.Should().Be(27);
    }

    [Fact]
    public void ProcessFiles_ShouldHandleFilesWithNoWords()
    {
        // Arrange
        var wordCounter = new WordCounter2Streaming(_directory);
        File.WriteAllText($"{_directory}/emptyFile.txt", "");

        // Act
        wordCounter.ProcessFiles();

        // Assert
        var fileContent = File.ReadAllText($"{_directory}/Output/FILE_E.txt");
        fileContent.Should().BeNullOrWhiteSpace();
    }

    [Fact]
    public void ProcessFiles_ShouldExcludeWordsCorrectly()
    {
        // Arrange
        var wordCounter = new WordCounter2Streaming(_directory);
        File.WriteAllText($"{_directory}/testFile.txt", "Hello hello HELLO");
        File.WriteAllText($"{_directory}/exclude.txt", "hello");

        // Act
        wordCounter.ProcessFiles();

        // Assert
        File.Exists($"{_directory}/Output/FILE_H.txt").Should().BeTrue();
        File.ReadAllText($"{_directory}/Output/FILE_H.txt").Should().BeNullOrWhiteSpace();
    }
    
    [Fact]
    public void ProcessFiles_ShouldNotCountPartialWords()
    {
        // Arrange
        var wordCounter = new WordCounter2Streaming(_directory);
        File.WriteAllText($"{_directory}/testFile.txt", "Hello hello HELLO el hel lo");

        // Act
        wordCounter.ProcessFiles();

        // Assert
        var fileH = File.ReadAllLines($"{_directory}/Output/FILE_H.txt");
        fileH.Length.Should().Be(2);
        
        var fileE = File.ReadAllLines($"{_directory}/Output/FILE_E.txt");
        fileE.Length.Should().Be(1);
        
        var fileL = File.ReadAllLines($"{_directory}/Output/FILE_L.txt");
        fileL.Length.Should().Be(1);
    }

    public void Dispose()
    {
        Directory.Delete(_directory, true);
    }
}