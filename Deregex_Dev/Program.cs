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

foreach (Range range in rss.RangesOf(Text("<item>"), Any, Multi("<guid>"), Any, Multi("</guid>"), Any, Text("</item>")))
    Console.WriteLine(rss[range]);