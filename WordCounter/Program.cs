using System;

Console.WriteLine("Enter directory path. Must be the full path of the directory. Output files will be available in a folder called 'Output' in the directory.");
Console.WriteLine(@"For example: C:\Users\John\Documents\WordCounter\Files");
var directoryPath = Console.ReadLine();
if(directoryPath is null)
{
    Console.WriteLine("Directory not found.");
    return;
}

Console.WriteLine($"Processing directory '{directoryPath}'...");

var wordCounter = new WordCounter.WordCounter5StreamingParallelV2(directoryPath);
wordCounter.ProcessFiles();