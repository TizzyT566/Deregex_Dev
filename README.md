# Deregex_Dev

A small alternative to regex.

Aimed at being a little more friendly for those of us who cant deal with regex syntax.

## Features

1. The StringRange type which is a combination of string and Range as a means to pass around ranges while still keeping a reference of the string its associated with.

   It is implicitly converted to type string and to type Range.

2. Has the following built in patterns:
   - None: A pattern with no definition.
   - End: Specifies the end of pattern sequence.
   - Any: A pattern where the run is anything until the next pattern matches.
   - Except: A pattern where the run is not any of the provided parameters.
   - Text: A pattern where the run has to be any of the provided parameters.
   - Repeat: A pattern where the run repeats a specified at-least and/or at-most times.
   - Custom: A user-defined pattern.

3. Has the following methods:
   - Match: Determines if a string matches a specified pattern.
   - RangeOf: Returns a StringRange of where a specified pattern matches.
   - RangesOf: Returns an array of StringRange where ever a pattern matches.
   - Between: Returns a StringRange where the contents are between a StringRange and a pattern.

## Usage

String one or more Patterns together in the methods provided to get a StringRange for that pattern.

### Example RSS parsing

```csharp
using System.Text.RegularPatterns;
using static System.Text.RegularPatterns.Pattern;

string rss = @$"<?xml version=""1.0"" encoding=""UTF-8"" ?>
<rss version=""2.0"">

<channel>
  <title>W3Schools Home Page</title>
  <link>https://www.w3schools.com</link>
  <description>Free web building tutorials</description>
  <item>
    <title>RSS Tutorial</title>
    <link>https://www.w3schools.com/xml/xml_rss.asp</link>
    <description>New RSS tutorial on W3Schools</description>
  </item>
  <item>
    <title>XML Tutorial</title>
    <link>https://www.w3schools.com/xml</link>
    <description>New XML tutorial on W3Schools</description>
  </item>
</channel>

</rss>";


foreach(StringRange item in rss.RangesOf(Text("<item>"), Any, Text("</item>")))
{
    StringRange title = item.RangeOf(Text("<title>")).Between(Text("</title>"));
    StringRange link = item.RangeOf(title, Text("<link>")).Between(Text("</link>"));
    StringRange description = item.RangeOf(link, Text("<description>")).Between(Text("</description>"));
    Console.WriteLine(title);
    Console.WriteLine(link);
    Console.WriteLine(description);
    Console.WriteLine();
}
```