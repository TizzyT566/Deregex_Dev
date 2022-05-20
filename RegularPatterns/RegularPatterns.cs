using System.Linq;
using System.Collections.Generic;
using System;

namespace RegularPatterns;

/// <summary>
/// The result of a Pattern.
/// </summary>
public struct PatternState
{
    /// <summary>
    /// A PatternState which represents that the pattern sequence is aborted.
    /// </summary>
    public static readonly PatternState Abort = new(-1);
    /// <summary>
    /// A PatternState which represents that the patter sequence has backtracked.
    /// </summary>
    public static readonly PatternState Backtrack = new(0);
    /// <summary>
    /// The underlying integer value for the PatternState.
    /// </summary>
    public int Value { get; }

    internal PatternState(int value) => Value = value;

    /// <summary>
    /// If a PatternState is successful.
    /// </summary>
    /// <param name="ps">The PatternState.</param>
    public static implicit operator bool(PatternState ps) => ps.Value > 0;
    /// <summary>
    /// Converts a PatternState to its integer value.
    /// </summary>
    /// <param name="ps">The PatternState.</param>
    public static implicit operator int(PatternState ps) => ps.Value;
    /// <summary>
    /// Converts an integer to a PatternState.
    /// </summary>
    /// <param name="value">The integer value to convert.</param>
    public static implicit operator PatternState(int value) =>
        value switch
        {
            0 => Backtrack,
            < 0 => Abort,
            _ => new PatternState(value),
        };
}

/// <summary>
/// Represents a string and a range within it.
/// </summary>
public struct StringRange
{
    /// <summary>
    /// The reference to the string this is associated with.
    /// </summary>
    public string Source { get; }
    /// <summary>
    /// The range within the source this is associated with.
    /// </summary>
    public Range Range { get; }
    /// <summary>
    /// The end value of the Range property.
    /// </summary>
    public int End => Range.End.Value;
    /// <summary>
    /// Constructs a new StringRange object with a specified range.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <param name="range">The range within the source string.</param>
    public StringRange(string source, Range range)
    {
        Source = source;
        Range = range;
    }
    /// <summary>
    /// Constructs a new StringRange object with a specified start and end index.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <param name="start">The starting index.</param>
    /// <param name="end">The ending index.</param>
    public StringRange(string source, int start, int end)
    {
        Source = source;
        Range = new Range(start, end);
    }
    /// <summary>
    /// A substring bounded by the Range property.
    /// </summary>
    public override string ToString() => Source[Range];
    /// <summary>
    /// Converts a StringRange to a Range.
    /// </summary>
    /// <param name="sr">The StringRange to convert.</param>
    public static implicit operator Range(StringRange sr) => sr.Range;
    /// <summary>
    /// Converts a StringRange to a String.
    /// </summary>
    /// <param name="sr">The StringRange to convert.</param>
    public static implicit operator string(StringRange sr) => sr.ToString();
}

/// <summary>
/// Represents a constraint via a pattern.
/// </summary>
public struct Pattern
{
    /// <summary>
    /// The contraint logic used for the Pattern.
    /// </summary>
    public Func<string, int, int, Pattern, PatternState> Logic { get; }

    /// <summary>
    /// The name associated with this Pattern.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Constructs a new Pattern given logic and a specified name.
    /// </summary>
    /// <param name="logic">The function to use as the logic of the constraint.</param>
    /// <param name="name">The name of the Pattern.</param>
    public Pattern(Func<string, int, int, Pattern, PatternState> logic, string name = "")
    {
        Logic = logic;
        Name = name;
    }

    /// <summary>
    /// A pattern with no constraints.
    /// </summary>
    public readonly static Pattern None = new((str, s, e, p) => s == e ? s : PatternState.Abort, nameof(None));

    /// <summary>
    /// A pattern which represents the end of a Pattern array.
    /// </summary>
    public readonly static Pattern End = new((str, s, e, p) => s, nameof(End));

    /// <summary>
    /// A pattern which allows any condition.
    /// </summary>
    public readonly static Pattern Any = new((str, s, e, p) =>
    {
        if (p.Equals(None) || p.Equals(End))
            return str.Length;
        for (; s < e; s++)
        {
            int result = p.Logic(str, s, e, p);
            if (result > 0)
                return result;
            if (result < 0)
                return PatternState.Abort;
        }
        return PatternState.Backtrack;
    }, nameof(Any));

    /// <summary>
    /// A pattern to accepts anything but the specified arguments.
    /// </summary>
    /// <param name="texts">A list of arguments which to be excluded.</param>
    public static Pattern Except(params string[] texts) =>
        Except(false, texts);
    /// <summary>
    /// A pattern to accepts anything but the specified arguments.
    /// </summary>
    /// <param name="ignoreCasing">true to ignore case sensitivity, otherwise false.</param>
    /// <param name="texts">A list of arguments which to be excluded.</param>
    public static Pattern Except(bool ignoreCasing, params string[] texts) => new((str, s, e, p) =>
    {
        int idx = s;
        foreach (string text in texts)
        {
            idx = s;
            int j = 0;
            for (; j < text.Length && idx < e; idx++)
            {
                if (ignoreCasing ? char.ToUpperInvariant(text[j]) == char.ToUpperInvariant(str[idx]) : text[j] == str[idx])
                    j++;
                else
                    break;
            }

            if (j == text.Length)
            {
                return PatternState.Abort;
            }
        }
        return p.Logic(str, idx, e, p);
    }, nameof(Except));

    /// <summary>
    /// A pattern to accept only the specified arguments.
    /// </summary>
    /// <param name="texts">A list of arguments which to be included.</param>
    public static Pattern Text(params string[] texts) => Text(false, false, texts);
    /// <summary>
    /// A pattern to accept only the specified arguments.
    /// </summary>
    /// <param name="ignoreCasing">true to ignore case sensitivity, otherwise false.</param>
    /// <param name="texts">A list of arguments which to be included.</param>
    public static Pattern Text(bool ignoreCasing, bool acceptEnds = false, params string[] texts) => new((str, s, e, p) =>
    {
        if (acceptEnds && s == e - 1)
        {
            return e;
        }

        int idx = s;
        foreach (string text in texts)
        {
            idx = s;
            int j = 0;
            for (; j < text.Length && idx < e; idx++)
            {
                if (ignoreCasing ? char.ToUpperInvariant(text[j]) == char.ToUpperInvariant(str[idx]) : text[j] == str[idx])
                    j++;
                else
                    break;
            }



            if (j == text.Length)
            {
                int result = p.Logic(str, idx, e, p);
                if (result > 0)
                    return result;
            }
        }
        return PatternState.Backtrack;
    }, nameof(Text));

    /// <summary>
    /// A pattern that repeats.
    /// </summary>
    /// <param name="text">The repeating text.</param>
    /// <param name="atleast">The pattern should repeat at least this many times.</param>
    /// <param name="atmost">The pattern should repeat at most this many times.</param>
    public static Pattern Repeat(string text, int atleast = 0, int atmost = int.MaxValue) =>
        Repeat(false, text, atleast, atmost);
    /// <summary>
    /// A pattern that repeats.
    /// </summary>
    /// <param name="ignoreCasing">true to ignore case sensitivity, otherwise false.</param>
    /// <param name="text">The repeating text.</param>
    /// <param name="atleast">The pattern should repeat at least this many times.</param>
    public static Pattern Repeat(bool ignoreCasing, string text, int atleast = 0) =>
        Repeat(ignoreCasing, text, atleast, int.MaxValue);
    /// <summary>
    /// A pattern that repeats.
    /// </summary>
    /// <param name="ignoreCasing">true to ignore case sensitivity, otherwise false.</param>
    /// <param name="text">The repeating text.</param>
    /// <param name="atleast">The pattern should repeat at least this many times.</param>
    /// <param name="atmost">The pattern should repeat at most this many times.</param>
    public static Pattern Repeat(bool ignoreCasing, string text, int atleast, int atmost) => new((str, s, e, p) =>
    {
        int count = 0;
        for (int j = 0; s < e; s++)
        {
            if (ignoreCasing ? char.ToUpperInvariant(text[j]) != char.ToUpperInvariant(str[s]) : text[j] != str[s])
                break;
            else
                j++;
            if (j == text.Length)
            {
                j = 0;
                count++;
            }
        }
        if (count >= atleast && count <= atmost)
            return p.Logic(str, s, e, p);
        else
        {
            if (count == 0)
                return PatternState.Backtrack;
            else
                return PatternState.Abort;
        }
    }, nameof(Repeat));

    /// <summary>
    /// User defined pattern.
    /// </summary>
    /// <param name="logic">The constraint logic for the pattern.</param>
    /// <param name="customName">The name for the custom pattern.</param>
    public static Pattern Custom(Func<string, int, int, Pattern, PatternState> logic, string customName = "CustomPattern") =>
        new(logic, customName);
}

/// <summary>
/// Extensions for Patterns.
/// </summary>
public static class PatternExtensions
{
    /// <summary>
    /// Whether or not a string matches a specified pattern sequence.
    /// </summary>
    /// <param name="this">The string to apply patterns on.</param>
    /// <param name="patterns">The pattern sequence.</param>
    public static bool Match(this string @this, params Pattern[] patterns) =>
        Match(@this, new Range(0, @this.Length), patterns);
    /// <summary>
    /// Whether or not a string matches a specified pattern sequence.
    /// </summary>
    /// <param name="this">The string to apply patterns on.</param>
    /// <param name="startIndex">The index to start applying the pattern sequence at.</param>
    /// <param name="patterns">The pattern sequence.</param>
    public static bool Match(this string @this, int startIndex, params Pattern[] patterns) =>
        Match(@this, new Range(startIndex, @this.Length), patterns);
    /// <summary>
    /// Whether or not a string matches a specified pattern sequence.
    /// </summary>
    /// <param name="this">The string to apply patterns on.</param>
    /// <param name="range">The range which to apply the pattern sequence to.</param>
    /// <param name="patterns">The pattern sequence.</param>
    public static bool Match(this string @this, Range range, params Pattern[] patterns)
    {
        if (patterns == null || patterns.Length == 0)
            return false;
        int length = Math.Min(@this.Length, range.End.Value);
        if (patterns.Length == 1)
            return patterns[0].Logic(@this, range.Start.Value, length, Pattern.None) > 0;
        bool lastPatternWasAny = patterns[^1].Name == nameof(Pattern.Any);
        Pattern patternChain = new((str, s, e, p) => patterns[^1].Logic(str, s, e, Pattern.None));
        foreach (Pattern pattern in patterns.Skip(1).Reverse().Skip(1))
        {
            if (lastPatternWasAny && pattern.Name == nameof(Pattern.Except))
                throw new InvalidOperationException($"'{nameof(Pattern.Any)}' cannot come immediately after '{nameof(Pattern.Except)}'.");
            lastPatternWasAny = pattern.Name == nameof(Pattern.Any);
            if (pattern.Name == nameof(Pattern.End))
                throw new InvalidOperationException($"'{nameof(Pattern.End)}' can only be placed at end of pattern sequence.");
            Pattern crntPattern = patternChain;
            patternChain = new((str, s, e, p) => pattern.Logic(@this, s, e, crntPattern));
        }
        return patterns[0].Logic(@this, range.Start.Value, length, patternChain) > 0;
    }

    /// <summary>
    /// Gets a StringRange of first pattern sequence match.
    /// </summary>
    /// <param name="this">The StringRange to apply the pattern sequence to.</param>
    /// <param name="patterns">The pattern sequence.</param>
    /// <returns></returns>
    public static StringRange RangeOf(this StringRange @this, params Pattern[] patterns) =>
        @this.Source.RangeOf(@this.Range, patterns);
    /// <summary>
    /// Gets a StringRange of first pattern sequence match.
    /// </summary>
    /// <param name="this">The StringRange to apply the pattern sequence to.</param>
    /// <param name="continueFrom">The StringRange to continue from the end of.</param>
    /// <param name="patterns">The pattern sequence.</param>
    public static StringRange RangeOf(this StringRange @this, StringRange continueFrom, params Pattern[] patterns) =>
        @this.Source.RangeOf(new Range(continueFrom.End, @this.End), patterns);

    /// <summary>
    /// Gets a StringRange of first pattern sequence match.
    /// </summary>
    /// <param name="this">The string to apply the pattern sequence to.</param>
    /// <param name="patterns">The pattern sequence.</param>
    public static StringRange RangeOf(this string @this, params Pattern[] patterns) =>
        RangeOf(@this, new Range(0, @this.Length), patterns);
    /// <summary>
    /// Gets a StringRange of first pattern sequence match.
    /// </summary>
    /// <param name="this">The string to apply the pattern sequence to.</param>
    /// <param name="startIndex">The index to start applying the pattern sequence at.</param>
    /// <param name="patterns">The pattern sequence.</param>
    public static StringRange RangeOf(this string @this, int startIndex, params Pattern[] patterns) =>
        RangeOf(@this, new Range(startIndex, @this.Length), patterns);
    /// <summary>
    /// Gets a StringRange of first pattern sequence match.
    /// </summary>
    /// <param name="this">The string to apply the pattern sequence to.</param>
    /// <param name="range">The range which to apply the pattern sequence to.</param>
    /// <param name="patterns">The pattern sequence.</param>
    public static StringRange RangeOf(this string @this, Range range, params Pattern[] patterns)
    {
        if (patterns == null || patterns.Length == 0)
            return new(@this, 0, 0);
        int length = Math.Min(@this.Length, range.End.Value);
        if (patterns.Length == 1)
        {
            for (int s = range.Start.Value; s < length; s++)
            {
                int result = patterns[0].Logic(@this, s, length, Pattern.End);
                if (result > 0)
                    return new(@this, s, result);
            }
            return new(@this, 0, 0);
        }
        else
        {
            bool lastPatternWasAny = patterns[^1].Name == nameof(Pattern.Any);
            Pattern patternChain = new((str, s, e, p) => patterns[^1].Logic(str, s, e, Pattern.End));
            foreach (Pattern pattern in patterns.Skip(1).Reverse().Skip(1))
            {
                if (lastPatternWasAny && pattern.Name == nameof(Pattern.Except))
                    throw new InvalidOperationException($"'{nameof(Pattern.Any)}' cannot come immediately after '{nameof(Pattern.Except)}'.");
                lastPatternWasAny = pattern.Name == nameof(Pattern.Any);
                if (pattern.Name == nameof(Pattern.End))
                    throw new InvalidOperationException($"'{nameof(Pattern.End)}' can only be placed at end of pattern sequence.");
                Pattern crntPattern = patternChain;
                patternChain = new((str, s, e, p) => pattern.Logic(@this, s, e, crntPattern));
            }
            for (int s = range.Start.Value; s < length; s++)
            {
                int result = patterns[0].Logic(@this, s, length, patternChain);
                if (result > 0)
                    return new(@this, s, result);
            }
            return new(@this, 0, 0);
        }
    }

    /// <summary>
    /// Gets a StringRange array where a pattern sequence matches.
    /// </summary>
    /// <param name="this">The string to apply the pattern sequence to.</param>
    /// <param name="patterns">The pattern sequence.</param>
    public static StringRange[] RangesOf(this string @this, params Pattern[] patterns) =>
        RangesOf(@this, new Range(0, @this.Length), patterns);
    /// <summary>
    /// Gets a StringRange array where a pattern sequence matches.
    /// </summary>
    /// <param name="this">The string to apply the pattern sequence to.</param>
    /// <param name="startIndex">The index to start applying the pattern sequence at.</param>
    /// <param name="patterns">The pattern sequence.</param>
    public static StringRange[] RangesOf(this string @this, int startIndex, params Pattern[] patterns) =>
        RangesOf(@this, new Range(startIndex, @this.Length), patterns);
    /// <summary>
    /// Gets a StringRange array where a pattern sequence matches.
    /// </summary>
    /// <param name="this">The string to apply the pattern sequence to.</param>
    /// <param name="range">The range which to apply the pattern sequence to.</param>
    /// <param name="patterns">The pattern sequence.</param>
    public static StringRange[] RangesOf(this string @this, Range range, params Pattern[] patterns)
    {
        List<StringRange> ranges = new();
        int length = Math.Min(@this.Length, range.End.Value);
        if (patterns == null || patterns.Length == 0)
        {
            return ranges.ToArray();
        }
        else if (patterns.Length == 1)
        {
            for (int s = range.Start.Value; s < length; s++)
            {
                int result = patterns[0].Logic(@this, s, length, Pattern.End);
                if (result > 0)
                    ranges.Add(new(@this, s, result));
            }
        }
        else
        {
            bool lastPatternWasAny = patterns[^1].Equals(Pattern.Any);
            Pattern patternChain = new((str, s, e, p) => patterns[^1].Logic(str, s, e, Pattern.End));
            foreach (Pattern pattern in patterns.Skip(1).Reverse().Skip(1))
            {
                if (lastPatternWasAny && pattern.Name == nameof(Pattern.Except))
                    throw new InvalidOperationException($"'{nameof(Pattern.Any)}' cannot come immediately after '{nameof(Pattern.Except)}'.");
                lastPatternWasAny = pattern.Name == nameof(Pattern.Any);
                if (pattern.Name == nameof(Pattern.End))
                    throw new InvalidOperationException($"'{nameof(Pattern.End)}' can only be placed at end of pattern sequence.");
                Pattern crntPattern = patternChain;
                patternChain = new((str, s, e, p) => pattern.Logic(@this, s, e, crntPattern));
            }
            for (int s = range.Start.Value; s < length; s++)
            {
                int result = patterns[0].Logic(@this, s, length, patternChain);
                if (result > 0)
                {
                    ranges.Add(new(@this, s, result));
                    s = result;
                }
            }
        }
        return ranges.ToArray();
    }

    /// <summary>
    /// Gets the content between the end of a StringRange and another pattern sequence.
    /// </summary>
    /// <param name="this">The StringRange to apply the ending pattern sequence on.</param>
    /// <param name="Pattern">The pattern sequence.</param>
    public static StringRange Between(this StringRange @this, params Pattern[] Pattern)
    {
        StringRange strRange = RangeOf(@this.Source, @this.Range.End.Value, Pattern);
        return new(@this.Source, @this.Range.End.Value, strRange.Range.Start.Value);
    }
}