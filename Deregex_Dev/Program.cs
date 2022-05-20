using RegularPatterns;
using static RegularPatterns.Pattern;

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