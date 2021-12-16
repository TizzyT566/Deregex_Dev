namespace System.Text.RegularPatterns;

public struct Pattern
{
    public Func<string, int, int, Pattern, int> Logic { get; init; }
    internal string Name;

    public Pattern(string name, Func<string, int, int, Pattern, int> logic)
    {
        Name = name;
        Logic = logic;
    }

    public Pattern(string name, params Pattern[] patterns)
    {
        Name = name;
        Logic = (str, s, e, p) =>
        {
            Console.WriteLine(p.Name);
            Pattern patternChain = patterns[^1];
            foreach (Pattern pattern in patterns.Reverse().Skip(1))
            {
                if (pattern.Equals(End))
                    throw new Exception("'Pattern.End' can only be placed at end of pattern sequence.");
                Pattern crntPattern = patternChain;
                patternChain = new($"{pattern.Name} Proxy", (str, s, e, p) => pattern.Logic(str, s, e, crntPattern));
            }
            return patterns[0].Logic(str, s, e, patternChain);
        };
    }

    public readonly static Pattern None = new("None", (str, s, e, p) => s == e ? s : -1);
    public readonly static Pattern End = new("End", (str, s, e, p) => s);
    public readonly static Pattern Any = new("Any", (str, s, e, p) =>
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
    public static Pattern Text(bool ignoreCasing, string text) => new("Text", (str, s, e, p) =>
    {
        for (int j = 0; j < text.Length && s < e; s++, j++)
            if (ignoreCasing ? char.ToUpperInvariant(text[j]) == char.ToUpperInvariant(str[s]) : text[j] != str[s])
                return 0;
        return p.Logic(str, s, e, p);
    });

    public static Pattern Multi(params string[] texts) => Multi(false, texts);
    public static Pattern Multi(bool ignoreCasing, params string[] texts) => new("Multi", (str, s, e, p) =>
    {
        int idx = s, fails = 0;
        foreach (string text in texts)
            for (int j = 0; j < text.Length && idx < e; idx++, j++)
                if (ignoreCasing ? char.ToUpperInvariant(text[j]) == char.ToUpperInvariant(str[idx]) : text[j] != str[idx])
                {
                    fails++;
                    break;
                }
        if (fails == texts.Length)
            return 0;
        return p.Logic(str, idx, e, p);
    });

    public static Pattern Custom(Func<string, int, int, Pattern, int> logic) =>
        new("Custom", logic);
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
        Pattern patternChain = new($"{patterns[^1].Name} Proxy", (str, s, e, p) => patterns[^1].Logic(str, s, e, Pattern.None));
        foreach (Pattern pattern in patterns.Skip(1).Reverse().Skip(1))
        {
            if (pattern.Equals(Pattern.End))
                throw new Exception("'Pattern.End' can only be placed at end of pattern sequence.");
            Pattern crntPattern = patternChain;
            patternChain = new($"{pattern.Name} Proxy", (str, s, e, p) => pattern.Logic(@this, s, e, crntPattern));
        }
        return patterns[0].Logic(@this, range.Start.Value, length, patternChain) > 0;
    }

    public static Range RangeOf(this string @this, params Pattern[] patterns) =>
        RangeOf(@this, new Range(0, @this.Length), patterns);
    public static Range RangeOf(this string @this, int startIndex, params Pattern[] patterns) =>
        RangeOf(@this, new Range(startIndex, @this.Length), patterns);
    public static Range RangeOf(this string @this, Range range, params Pattern[] patterns)
    {
        if (patterns == null || patterns.Length == 0)
            return new(0, 0);
        int length = Math.Min(@this.Length, range.End.Value);
        if (patterns.Length == 1)
        {
            for (int s = range.Start.Value; s < length; s++)
            {
                int result = patterns[0].Logic(@this, s, length, Pattern.None);
                if (result > 0)
                    return new(s, result);
            }
            return new(0, 0);
        }
        else
        {
            Pattern patternChain = new($"{patterns[^1].Name} Proxy", (str, s, e, p) => patterns[^1].Logic(str, s, e, Pattern.End));
            foreach (Pattern pattern in patterns.Skip(1).Reverse().Skip(1))
            {
                if (pattern.Equals(Pattern.End))
                    throw new Exception("'Pattern.End' can only be placed at end of pattern sequence.");
                Pattern crntPattern = patternChain;
                patternChain = new($"{pattern.Name} Proxy", (str, s, e, p) => pattern.Logic(@this, s, e, crntPattern));
            }
            for (int s = range.Start.Value; s < length; s++)
            {
                int result = patterns[0].Logic(@this, s, length, patternChain);
                if (result > 0)
                    return new Range(s, result);
            }
            return new(0, 0);
        }
    }

    public static Range[] RangesOf(this string @this, params Pattern[] patterns) =>
        RangesOf(@this, new Range(0, @this.Length), patterns);
    public static Range[] RangesOf(this string @this, int startIndex, params Pattern[] patterns) =>
        RangesOf(@this, new Range(startIndex, @this.Length), patterns);
    public static Range[] RangesOf(this string @this, Range range, params Pattern[] patterns)
    {


        List<Range> ranges = new();
        int length = Math.Min(@this.Length, range.End.Value);
        if (patterns == null || patterns.Length == 0)
        {
            return ranges.ToArray();
        }
        else if (patterns.Length == 1)
        {
            for (int s = range.Start.Value; s < length; s++)
            {
                int result = patterns[0].Logic(@this, s, length, Pattern.None);
                if (result > 0)
                    ranges.Add(new(s, result));
            }
        }
        else
        {
            Pattern patternChain = new($"{patterns[^1].Name} Proxy", (str, s, e, p) => patterns[^1].Logic(str, s, e, Pattern.End));
            foreach (Pattern pattern in patterns.Skip(1).Reverse().Skip(1))
            {
                if (pattern.Equals(Pattern.End))
                    throw new Exception("'Pattern.End' can only be placed at end of pattern sequence.");
                Pattern crntPattern = patternChain;
                patternChain = new($"{pattern.Name} Proxy", (str, s, e, p) => pattern.Logic(@this, s, e, crntPattern));
            }
            for (int s = range.Start.Value; s < length; s++)
            {
                int result = patterns[0].Logic(@this, s, length, patternChain);
                if (result > 0)
                    ranges.Add(new(s, result));
            }
        }
        return ranges.ToArray();
    }
}