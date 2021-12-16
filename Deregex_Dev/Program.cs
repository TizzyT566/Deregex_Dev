using System.Text.RegularPatterns;
using static System.Text.RegularPatterns.Pattern;

string rss =
@"
<item>
    <guid>https://www.livechart.me/anime/10919#episode95403:1639533600</guid>
    <link>https://www.livechart.me/anime/10919</link>
    <title>Yao Shen Ji 5th Season #16</title>
    <pubDate>Wed, 15 Dec 2021 02:00:00 +0000</pubDate>
    <category>Anime/OVAs/Episodes</category>

    <enclosure url=""https://u.livechart.me/anime/10919/poster_image/ee105a8db5d5a572c25b69105419f302.png?style=small&amp;format=jpg"" length=""0"" type=""image/jpeg"" />
    <media:thumbnail url = ""https://u.livechart.me/anime/10919/poster_image/ee105a8db5d5a572c25b69105419f302.png?style=small&amp;format=jpg"" width=""175"" height=""250"" />
</item>
this is some random text
<item>
    <guid>https://www.livechart.me/anime/10919#episode95403:1639533600</guid>
    <link>https://www.livechart.me/anime/10919</link>
    <title>Yao Shen Ji 5th Season #15</title>
    <pubDate>Wed, 15 Dec 2021 02:00:00 +0000</pubDate>
    <category>Anime/OVAs/Episodes</category>

    <enclosure url=""https://u.livechart.me/anime/10919/poster_image/ee105a8db5d5a572c25b69105419f302.png?style=small&amp;format=jpg"" length=""0"" type=""image/jpeg"" />
    <media:thumbnail url = ""https://u.livechart.me/anime/10919/poster_image/ee105a8db5d5a572c25b69105419f302.png?style=small&amp;format=jpg"" width=""175"" height=""250"" />
</item>";

string test = "esahdasgavrgtestfgzfanimeanimeanimeerhdfzhgHelloWorld";

StringRange repeat = test.RangeOf(Text("test"), Any, Repeat("anime", 1, 2), Any, Text("HelloWorld"));
Console.WriteLine(repeat);

//StringRange multiTest = test.RangeOf(Multi("pokemon", "anime", "hello"), Any, Multi("anime", "hello", "team"));

//Console.WriteLine(multiTest.Range);
//Console.WriteLine(multiTest);

//foreach (StringRange range in rss.RangesOf(Text("<item>"), Any, Text("</item>")))
//{
//    StringRange guid = rss.RangeOf(range, Text("<guid>")).Between(Text("</guid>"));
//    Console.WriteLine($" Guid: {guid}");

//    StringRange link = rss.RangeOf(guid.End, Text("<link>")).Between(Text("</link>"));
//    Console.WriteLine($" Link: {link}");

//    StringRange title = rss.RangeOf(link.End, Text("<title>")).Between(Text("</title>"));
//    Console.WriteLine($"Title: {title}");

//    Console.WriteLine();
//}