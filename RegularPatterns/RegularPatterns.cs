using System.Collections.Generic;
using System.Linq;

namespace System.Text.RegularPatterns;

public struct StringRange
{
    public readonly string Content;
    public readonly Range Range;
    public int End => Range.End.Value;
    public StringRange(string content, Range range)
    {
        Content = content;
        Range = range;
    }
    public StringRange(string content, int start, int end)
    {
        Content = content;
        Range = new Range(start, end);
    }
    public override string ToString() => Content[Range];
    public static implicit operator Range(StringRange sr) => sr.Range;
}

public struct Pattern
{
    public Func<string, int, int, Pattern, int> Logic { get; }

    public Pattern(Func<string, int, int, Pattern, int> logic) => Logic = logic;

    public Pattern(params Pattern[] patterns)
    {
        Logic = (str, s, e, p) =>
        {
            Pattern patternChain = patterns[^1];
            foreach (Pattern pattern in patterns.Reverse().Skip(1))
            {
                if (pattern.Equals(End))
                    throw new Exception("'Pattern.End' can only be placed at end of pattern sequence.");
                Pattern crntPattern = patternChain;
                patternChain = new((str, s, e, p) => pattern.Logic(str, s, e, crntPattern));
            }
            return patterns[0].Logic(str, s, e, patternChain);
        };
    }

    public readonly static Pattern None = new((str, s, e, p) => s == e ? s : -1);
    public readonly static Pattern End = new((str, s, e, p) => s);
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
                return -1;
        }
        return 0;
    });

    public static Pattern Text(string text) => Text(false, text);
    public static Pattern Text(bool ignoreCasing, string text) => new((str, s, e, p) =>
    {
        for (int j = 0; j < text.Length && s < e; s++, j++)
            if (ignoreCasing ? char.ToUpperInvariant(text[j]) == char.ToUpperInvariant(str[s]) : text[j] != str[s])
                return 0;
        return p.Logic(str, s, e, p);
    });

    public static Pattern Multi(params string[] texts) => Multi(false, texts);
    public static Pattern Multi(bool ignoreCasing, params string[] texts) => new((str, s, e, p) =>
    {
        int idx = s;
        foreach (string text in texts)
        {
            idx = s;
            int j = 0;
            for (; j < text.Length && idx < e; idx++)
                if (ignoreCasing ? char.ToUpperInvariant(text[j]) == char.ToUpperInvariant(str[idx]) : text[j] == str[idx])
                    j++;
                else
                    break;
            if (j == text.Length)
            {
                int result = p.Logic(str, idx, e, p);
                if (result > 0)
                    return result;
            }
        }
        return 0;
    });

    public static Pattern Repeat(string text, int atleast = 0, int atmost = int.MaxValue) =>
        Repeat(false, text, atleast, atmost);
    public static Pattern Repeat(bool ignoreCasing, string text, int atleast = 0) =>
        Repeat(ignoreCasing, text, atleast, int.MaxValue);
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
                return 0;
            else
                return -1;
        }
    });

    public static Pattern Custom(Func<string, int, int, Pattern, int> logic) =>
        new(logic);
}

public static class PatternExtensions
{
    public static bool Match(this string @this, params Pattern[] patterns) =>
        Match(@this, new Range(0, @this.Length), patterns);
    public static bool Match(this string @this, int startIndex, params Pattern[] patterns) =>
        Match(@this, new Range(startIndex, @this.Length), patterns);
    public static bool Match(this string @this, Range range, params Pattern[] patterns)
    {
        if (patterns == null || patterns.Length == 0)
            return false;
        int length = Math.Min(@this.Length, range.End.Value);
        if (patterns.Length == 1)
            return patterns[0].Logic(@this, range.Start.Value, length, Pattern.None) > 0;
        Pattern patternChain = new((str, s, e, p) => patterns[^1].Logic(str, s, e, Pattern.None));
        foreach (Pattern pattern in patterns.Skip(1).Reverse().Skip(1))
        {
            if (pattern.Equals(Pattern.End))
                throw new Exception("'Pattern.End' can only be placed at end of pattern sequence.");
            Pattern crntPattern = patternChain;
            patternChain = new((str, s, e, p) => pattern.Logic(@this, s, e, crntPattern));
        }
        return patterns[0].Logic(@this, range.Start.Value, length, patternChain) > 0;
    }

    public static StringRange RangeOf(this string @this, params Pattern[] patterns) =>
        RangeOf(@this, new Range(0, @this.Length), patterns);
    public static StringRange RangeOf(this string @this, int startIndex, params Pattern[] patterns) =>
        RangeOf(@this, new Range(startIndex, @this.Length), patterns);
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
            Pattern patternChain = new((str, s, e, p) => patterns[^1].Logic(str, s, e, Pattern.End));
            foreach (Pattern pattern in patterns.Skip(1).Reverse().Skip(1))
            {
                if (pattern.Equals(Pattern.End))
                    throw new Exception("'Pattern.End' can only be placed at end of pattern sequence.");
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

    public static StringRange[] RangesOf(this string @this, params Pattern[] patterns) =>
        RangesOf(@this, new Range(0, @this.Length), patterns);
    public static StringRange[] RangesOf(this string @this, int startIndex, params Pattern[] patterns) =>
        RangesOf(@this, new Range(startIndex, @this.Length), patterns);
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
            Pattern patternChain = new((str, s, e, p) => patterns[^1].Logic(str, s, e, Pattern.End));
            foreach (Pattern pattern in patterns.Skip(1).Reverse().Skip(1))
            {
                if (pattern.Equals(Pattern.End))
                    throw new Exception("'Pattern.End' can only be placed at end of pattern sequence.");
                Pattern crntPattern = patternChain;
                patternChain = new((str, s, e, p) => pattern.Logic(@this, s, e, crntPattern));
            }
            for (int s = range.Start.Value; s < length; s++)
            {
                int result = patterns[0].Logic(@this, s, length, patternChain);
                if (result > 0)
                    ranges.Add(new(@this, s, result));
            }
        }
        return ranges.ToArray();
    }

    public static StringRange Between(this StringRange @this, params Pattern[] Pattern)
    {
        StringRange strRange = RangeOf(@this.Content, @this.Range.End.Value, Pattern);
        return new(@this.Content, @this.Range.End.Value, strRange.Range.Start.Value);
    }
}