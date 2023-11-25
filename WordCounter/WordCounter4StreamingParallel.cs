using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WordCounter;

public class WordCounter4StreamingParallel
{
    private const string EXCLUDE_INPUT_FILE_NAME = "exclude.txt";
    private const string FILE_EXTENSION = "*.txt";
    private const string ALPHABET = "abcdefghijklmnopqrstuvwxyz";
    private const string EXCLUDE_OUTPUT_FILE_NAME = "excluded_words.txt";

    private static readonly char[] _newWordSeperators = { ' ', '\t', '\n', '\r', '.', ',', ';', '!', '?' };
    private readonly string _directoryPath;

    public WordCounter4StreamingParallel(string directoryPath) => _directoryPath = directoryPath;

    /// <summary>
    ///     Finds all files in the directory, counts the words in each file and how many times each word exists in the files.
    ///     Creates a file for each letter in the alphabet, and writes each word beginning with that letter and how many times it exists in the files.
    ///     Creates a file with all the excluded words and how many times they exist in the files.
    /// </summary>
    public void ProcessFiles()
    {
        if (!Directory.Exists(_directoryPath))
        {
            Console.WriteLine("Directory not found.");
            return;
        }

        if (!Directory.Exists(OutputDirectoryPath))
            Directory.CreateDirectory(OutputDirectoryPath);

        var fileNames = Directory.GetFiles(_directoryPath, FILE_EXTENSION);
        var filesToProcess = fileNames.Where(x => !x.Contains(EXCLUDE_INPUT_FILE_NAME)).ToList();

        var countedWords = GetWordCount(filesToProcess);
        var excludedWords = RemoveExcludedWordsFromDictionary(countedWords);
        WriteWordsToFile(countedWords);
        WriteExcludedWordsToFile(excludedWords);
    }

    /// <summary>
    ///     Counts the words in the files.
    /// </summary>
    /// <param name="fileNames">List of file names to process</param>
    /// <returns>Dictionary of words and how many times they exist in the files</returns>
    public ConcurrentDictionary<string, int> GetWordCount(List<string> fileNames)
    {
        var words = new ConcurrentDictionary<string, int>();

        Parallel.ForEach(fileNames, fileName =>
        {
            using var streamReader = new StreamReader(fileName);
            while (!streamReader.EndOfStream)
            {
                var line = streamReader.ReadLine();
                if (line is null)
                    continue;

                CountWords(line, words);
            }
        });

        return words;
    }

    /// <summary>
    ///     Counts the words in the file content.
    /// </summary>
    /// <param name="fileLine">String representing the file content</param>
    /// <param name="words">Dictionary of words and how many times they exist in the files</param>
    private static void CountWords(string fileLine, ConcurrentDictionary<string, int> words)
    {
        var fileWords = fileLine.Split(_newWordSeperators, StringSplitOptions.RemoveEmptyEntries);
        foreach (var fileWord in fileWords)
        {
            var word = fileWord.ToLower();
            words.AddOrUpdate(word, 1, (_, value) => value + 1);
        }
    }

    /// <summary>
    ///     Removes the excluded words from the words dictionary.
    /// </summary>
    /// <param name="words">Dictionary of words and how many times they exist in the files</param>
    /// <returns>Dictionary of excluded words and how many times they exist in the files</returns>
    public Dictionary<string, int> RemoveExcludedWordsFromDictionary(ConcurrentDictionary<string, int> words)
    {
        var excludedWords = new Dictionary<string, int>();
        var inputFilePath = Path.Combine(_directoryPath, EXCLUDE_INPUT_FILE_NAME);

        if (!File.Exists(inputFilePath))
            return excludedWords;

        using var streamReader = new StreamReader(inputFilePath);
        while (!streamReader.EndOfStream)
        {
            var line = streamReader.ReadLine();
            if (line is null) 
                continue;
                
            var word = line.Trim();
            if (!words.TryGetValue(word, out var value)) 
                continue;
            
            excludedWords.Add(word, value);
            words.TryRemove(word, out _);
        }

        return excludedWords;
    }

    /// <summary>
    ///     Counts the words in the files. Creates a file for each letter in the alphabet, and writes each word and how many times it exists in the files.
    /// </summary>
    /// <param name="words"></param>
    public void WriteWordsToFile(ConcurrentDictionary<string, int> words)
    {
        foreach (var letter in ALPHABET)
        {
            var stringLetter = letter.ToString();
            var filePath = Path.Combine(OutputDirectoryPath, $"FILE_{stringLetter.ToUpper()}.txt");

            using var streamWriter = new StreamWriter(filePath);
            foreach (var word in words.Where(x => x.Key.StartsWith(stringLetter)))
            {
                streamWriter.WriteLine($"{word.Key} {word.Value}");
            }
        }
    }

    /// <summary>
    ///     Writes the excluded words to a file.
    /// </summary>
    /// <param name="words">Dictionary of excluded words and how many times they exist in the files</param>
    public void WriteExcludedWordsToFile(Dictionary<string, int> words)
    {
        if (words.Count == 0)
        {
            File.Create(Path.Combine(OutputDirectoryPath, EXCLUDE_OUTPUT_FILE_NAME)).Dispose();
            return;
        }

        var filePath = Path.Combine(OutputDirectoryPath, EXCLUDE_OUTPUT_FILE_NAME);

        using var streamWriter = new StreamWriter(filePath);
        foreach (var word in words)
        {
            streamWriter.WriteLine($"{word.Key} {word.Value}");
        }
    }

    /// <summary>
    ///     Gets the output directory path.
    /// </summary>
    private string OutputDirectoryPath => Path.Combine(_directoryPath, "Output");
}