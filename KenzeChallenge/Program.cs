using CommandLine;

namespace KenzeChallenge;
class Program   
{
    static void Main(string[] args)
    {
        Parser.Default.ParseArguments<InputArgs>(args)
            .WithParsed(options =>
            {
                if (!string.IsNullOrWhiteSpace(options.InputFile))
                {
                    var result = FileProcessor.ProcessFile(options.InputFile, options.TargetLength);

                    // for testing purposes to see if it all works, could be removed when logging output to a file
                    foreach (var line in result)
                    {
                        Console.WriteLine(line);
                    }
                }
                else
                {
                    Console.WriteLine($"Unable to read input file argument.");
                }
            });

    }
}