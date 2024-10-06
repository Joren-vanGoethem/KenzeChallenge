using KenzeChallenge;

public class FileProcessorTests
{
    [Fact]
    public void SortByLength_CreatesCorrectWordDict()
    {
        // Arrange
        var lines = new HashSet<string> { "a", "bb", "ccc", "d", "ee", "f" };

        // Act
        var result = FileProcessor.SortByLength(lines);

        // Assert
        Assert.Equal(3, result[1].Count); // Should have 3 one-letter words
        Assert.Equal(2, result[2].Count); // Should have 2 two-letter words
        Assert.Equal(1, result[3].Count); // Should have 1 three-letter word
    }

    [Fact]
    public void FindPossibleLengthCombinations_GeneratesCorrectCombinations()
    {
        // Arrange
        var wordDict = new Dictionary<int, List<string>>
        {
            { 1, new List<string> { "a", "b", "c" } },
            { 2, new List<string> { "de", "fg" } },
            { 3, new List<string> { "dog", "cat" } }
        };
        var targetLength = 4;

        // Act
        var combinations = FileProcessor.FindPossibleLengthCombinations(wordDict, targetLength).ToList();

        // Assert
        Assert.Equal(4, combinations.Count); // There should be three combinations
        Assert.Contains(new List<int> { 1, 1, 1, 1 }, combinations);
        Assert.Contains(new List<int> { 2, 2 }, combinations);
        Assert.Contains(new List<int> { 1, 3 }, combinations);
        Assert.Contains(new List<int> { 1, 1, 2 }, combinations);
    }

    [Fact]
    public void CreateStringCombinations_ReturnsValidCombinations()
    {
        // Arrange
        var wordDict = new Dictionary<int, List<string>>
        {
            { 1, new List<string> { "a", "b", "c" } },
            { 3, new List<string> { "dog", "cat" } }
        };
        var lengthFrequency = new Dictionary<int, int>
        {
            { 1, 3 },
            { 3, 1 }
        };
        var targetLength = 4;
        var partial = new List<string>();

        // Act
        var combinations = FileProcessor.CreateStringCombinations(wordDict, targetLength, lengthFrequency, partial).ToList();

        // Assert
        Assert.NotEmpty(combinations);
        Assert.All(combinations, result => Assert.Equal(targetLength, result.Sum(x => x.Length))); // Each result should be of target length
    }

    [Fact]
    public void NoDuplicatesInResults()
    {
        // Arrange
        var wordDict = new Dictionary<int, List<string>>
        {
            { 1, new List<string> { "a", "b", "c" } },
            { 3, new List<string> { "dog" } },
            { 6, new List<string> { "dogabc" } }
        };
        var targetLength = 6;
        var combinations = new List<List<int>> { new List<int> { 1, 1, 1, 3 } };

        // Act
        var results = FileProcessor.FindValidWordCombinations(wordDict, targetLength, combinations).ToList();

        // Assert
        Assert.Equal(1, results.Distinct().Count()); // Should be unique results
    }

    [Fact]
    public void InvalidLengthTargetThrowsException()
    {
        // Arrange
        var lines = new HashSet<string> {"a", "b", "c", "dog"};
        var targetLength = 6; // does not exist in lines above

        // Act & Assert
        Assert.Throws<ArgumentException>(() => FileProcessor.ProcessLines(lines, targetLength)); // Should be unique results
    }
}
