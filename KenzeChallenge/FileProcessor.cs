using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

namespace KenzeChallenge;

public static class FileProcessor
{
    public static IEnumerable<string> ProcessFile(string inputFile, int targetLength)
    {
        Console.WriteLine("Processing file");
        var lines = ReadFile(inputFile);
        return ProcessLines(lines, targetLength);
    }

    public static IEnumerable<string> ProcessLines(HashSet<string> lines, int targetLength)
    {
        var wordDict = SortByLength(lines);

        if (!wordDict.TryGetValue(targetLength, out _))
        {
            throw new ArgumentException("Invalid target length specified, length not found in input");
        }

        var results = FindPossibleLengthCombinations(wordDict, targetLength);
        return FindValidWordCombinations(wordDict, targetLength, results);
    }

    public static IEnumerable<string> FindValidWordCombinations(Dictionary<int, List<string>> wordDict, int targetLength, IEnumerable<List<int>> combinations)
    {
        // if you do not want to hit all cores at once, this non-parallel version works fine but is slower, especially on larger input files
        // var targetWords = new HashSet<string>(wordDict[targetLength]);
        //
        // foreach (var combination in combinations)
        // {
        //     var lengthFrequency = GetLengthFrequency(combination);
        //     var possibleCombinations = CreateStringCombinations(wordDict, targetLength, lengthFrequency, new List<string>());
        //
        //     foreach (var result in possibleCombinations)
        //     {
        //         string combinedResult = string.Join("", result);
        //         if (targetWords.Contains(combinedResult))
        //         {
        //             var output = string.Join("+", result) + "=" + combinedResult;
        //             yield return output;
        //         }
        //     }
        // }


        var targetWords = new HashSet<string>(wordDict[targetLength]);
        var output = new ConcurrentBag<string>();  // Thread-safe collection for parallel operations

        // Parallelize the validation of word combinations
        Parallel.ForEach(combinations, combination =>
        {
            var lengthFrequency = GetLengthFrequency(combination);
            var possibleCombinations = CreateStringCombinations(wordDict, targetLength, lengthFrequency, new List<string>());

            foreach (var result in possibleCombinations)
            {
                string combinedResult = string.Join("", result);
                if (targetWords.Contains(combinedResult))
                {
                    output.Add(string.Join("+", result) + "=" + combinedResult);
                }
            }
        });

        return output;
    }

    public static Dictionary<int, int> GetLengthFrequency(List<int> combination)
    {
        // Computes the frequency of each length in the combination list
        var frequency = new Dictionary<int, int>();
        foreach (var len in combination)
        {
            frequency.TryAdd(len, 0); // won't do anything if the key already exists
            frequency[len]++;
        }
        return frequency;
    }

    public static Dictionary<int, List<string>> SortByLength(HashSet<string> lines)
    {
        var wordDict = new Dictionary<int, List<string>>();

        foreach (var line in lines)
        {
            if (wordDict.ContainsKey(line.Length))
            {
                wordDict[line.Length].Add(line);
            }
            else
            {
                wordDict.Add(line.Length, new List<string> { line });
            }
        }

        return wordDict;
    }

    public static HashSet<List<int>> FindPossibleLengthCombinations(Dictionary<int, List<string>> wordDict, int targetLength)
    {
        var keys = wordDict.Keys
            .Where(k => k != targetLength)
            .ToList();

        // will return values like [1,1,2], [1,2,1] so i use a sort on each result and then to Hashset with a list comparer to only keep the unique ones ([1,1,2] in this case)
        return FindLengthCombinations(keys, targetLength, new List<int>()).ToHashSet(new ListComparer());
    }

    public static IEnumerable<List<int>> FindLengthCombinations(List<int> numbers, int target, List<int> partial)
    {
        var s = partial.Sum();

        if (s == target)
        {
            partial.Sort();
            yield return partial;
        }

        if (s > target)
            yield break;

        for (var i = 0; i < numbers.Count; i++)
        {
            List<int> partial_rec = new List<int>(partial) {numbers[i]};

            foreach (var result in FindLengthCombinations(numbers, target, partial_rec))
            {
                yield return result;
            }
        }
    }

    public static IEnumerable<List<string>> CreateStringCombinations(Dictionary<int, List<string>> wordDict, int target, Dictionary<int, int> lengthFrequency, List<string> partial)
    {
        var s = new StringBuilder();
        foreach (var x in partial) s.Append(x);

        if (s.Length == target)
        {
            yield return new List<string>(partial);
        }
        else if (s.Length > target)
        {
            yield break;
        }

        foreach (var kvp in lengthFrequency.ToList())  // Iterate over a copy of lengthFrequency
        {
            int length = kvp.Key;
            int remainingCount = kvp.Value;

            if (remainingCount > 0 && wordDict.TryGetValue(length, out var value)) // check if we still need any word of this specific length
            {
                foreach (var word in value)
                {
                    partial.Add(word);
                    lengthFrequency[length]--;  // Use one word of this length

                    foreach (var result in CreateStringCombinations(wordDict, target, lengthFrequency, partial))
                    {
                        yield return result;
                    }

                    partial.RemoveAt(partial.Count - 1);  // Backtrack
                    lengthFrequency[length]++;  // Restore the count
                }
            }
        }
    }

    public static HashSet<string> ReadFile(string inputFile)
    {
        // Read the file lines to hashset to remove duplicates
        return File.ReadAllLines(inputFile).ToHashSet();
    }
}

class ListComparer : IEqualityComparer<List<int>>
{
    public bool Equals(List<int> x, List<int> y)
    {
        if (x == null || y == null || x.Count != y.Count)
            return false;
        return x.SequenceEqual(y);
    }

    public int GetHashCode(List<int> obj)
    {
        if (obj == null)
            return 0;

        // Compute a hash code for the list.
        int hash = 17;
        foreach (var item in obj)
        {
            hash = hash * 31 + item.GetHashCode();
        }
        return hash;
    }
}