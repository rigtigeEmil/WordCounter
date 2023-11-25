using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WordCounter;

public class WordCounter3Parallel
{
    private const string EXCLUDE_FILE_NAME = "exclude.txt";
    private const string FILE_EXTENSION = "*.txt";
    private const string ALPHABET = "abcdefghijklmnopqrstuvwxyz";
    private const string EXCLUDED_WORDS_FILE_NAME = "excluded_words.txt";

    private static readonly char[] _newWordSeperators = { ' ', '\t', '\n', '\r', '.', ',', ';', '!', '?' };
    private readonly string _directoryPath;
    public WordCounter3Parallel(string directoryPath) => _directoryPath = directoryPath;

    public void ProcessFiles()
    {
        if (!Directory.Exists(_directoryPath))
        {
            Console.WriteLine("Directory not found.");
            return;
        }

        var fileNames = Directory.GetFiles(_directoryPath, FILE_EXTENSION);
        var filesToProcess = fileNames.Where(x => !x.Contains(EXCLUDE_FILE_NAME)).ToList();

        var countedWords = GetWordCount(filesToProcess);
        var excludedWords = GetExcludedWordCount(countedWords);
        WriteWordsToFile(countedWords);
        WriteExcludedWordsToFile(excludedWords);
    }

    public ConcurrentDictionary<string, int> GetWordCount(List<string> fileNames)
    {
        var words = new ConcurrentDictionary<string, int>();

        Parallel.ForEach(fileNames, fileName =>
        {
            var fileContent = File.ReadAllText(fileName);
            var fileWords = fileContent.Split(_newWordSeperators, StringSplitOptions.RemoveEmptyEntries);

            foreach (var fileWord in fileWords)
            {
                var word = fileWord.ToLower();
                words.AddOrUpdate(word, 1, (_, value) => Interlocked.Increment(ref value));
            }
        });

        return words;
    }

    public ConcurrentDictionary<string, int> GetExcludedWordCount(ConcurrentDictionary<string, int> words)
    {
        var fileContents = File.ReadAllText(Path.Combine(_directoryPath, EXCLUDE_FILE_NAME));
        var wordsToExclude = fileContents.Split(_newWordSeperators, StringSplitOptions.RemoveEmptyEntries);

        var excludedWords = new ConcurrentDictionary<string, int>();
        foreach (var word in wordsToExclude)
        {
            if (!words.TryRemove(word, out var value)) continue;
            excludedWords.AddOrUpdate(word, value, (_, oldValue) => oldValue + value);
        }

        return excludedWords;
    }

    /// <summary>
    ///     Counts the words in the files. Creates a file for each letter in the alphabet, and writes each word and how many times it exists in the files.
    /// </summary>
    /// <param name="words"></param>
    private void WriteWordsToFile(ConcurrentDictionary<string, int> words)
    {
        foreach (var letter in ALPHABET)
        {
            var stringLetter = letter.ToString();
            var sb = new StringBuilder();
            foreach (var word in words.Where(x => x.Key.StartsWith(stringLetter)))
            {
                sb.AppendLine($"{word.Key} {word.Value}");
            }

            var filePath = Path.Combine(OutputDirectoryPath, $"FILE_{stringLetter.ToUpper()}.txt");
            var content = sb.ToString().Split(Environment.NewLine);
            File.WriteAllLines(filePath, content);
        }
    }

    private void WriteExcludedWordsToFile(ConcurrentDictionary<string, int> words)
    {
        var sb = new StringBuilder();
        foreach (var word in words)
        {
            sb.AppendLine($"{word.Key} {word.Value}");
        }

        var filePath = Path.Combine(OutputDirectoryPath, EXCLUDED_WORDS_FILE_NAME);
        var content = sb.ToString().Split(Environment.NewLine);
        File.WriteAllLines(filePath, content);
    }

    private string OutputDirectoryPath => Path.Combine(_directoryPath, "Output");
}