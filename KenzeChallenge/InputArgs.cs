using CommandLine;

namespace KenzeChallenge;

public class InputArgs
{
    [Option('i', "input", Required = true, HelpText = "Input file to be processed.")]
    public string InputFile { get; set; } = null!;

    [Option('t', "target", Required = true, HelpText = "Word length target.")]
    public int TargetLength { get; set; }
}