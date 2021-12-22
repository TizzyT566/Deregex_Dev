using System.Text;
using System.Text.RegularPatterns;
using static System.Text.RegularPatterns.Pattern;

using HttpClient client = new();

string source = "http://www.mangahere.cc";
string rss = await client.GetStringAsync("http://www.mangahere.cc/directory/2.htm?az");

int startIndex = rss.IndexOf("manga-list-1-list line");

StringBuilder sb = new();

foreach (StringRange range in rss.RangesOf(startIndex, Text("<li>"), Any, Text("</li>")))
{
    StringRange link = rss.RangeOf(range, Text("<a href=\"")).Between(Text("\""));
    StringRange title = rss.RangeOf(link.End, Text("title=\"")).Between(Text("\""));
    StringRange image = rss.RangeOf(title.End, Text("<img class=\"manga-list-1-cover\" src=\"")).Between(Text("\""));
    StringRange score = rss.RangeOf(new Range(title.End, range.End), Text("<span class=\"item-score\">")).Between(Text("</span>"));

    sb.AppendLine(title);
    sb.AppendLine(score);
    sb.AppendLine(source + link);
    sb.AppendLine(image);

    //Console.WriteLine(score);
    sb.AppendLine();
}

File.WriteAllText(@"C:\Users\tizzy\Desktop\managa.txt", sb.ToString());