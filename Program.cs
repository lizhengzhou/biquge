using HtmlAgilityPack;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace biquge
{
    class Program
    {
        static void Main(string[] args)
        {

            var ignores = new List<string> {
               "，高速全文字在线阅读！",
            "武动乾坤",
            "纯文字在线阅读本站域名手机同步阅读请访问",
            "最快更新，无弹窗阅读请。",
            "纯文字在线阅读本站域名手机同步阅读请访问"
            };

            var client = new RestClient("https://m.biquge.com");

            var bookRequest = new RestRequest("/booklist/161.html");

            var bookResponse = client.Execute(bookRequest);

            var doc = new HtmlDocument();
            doc.LoadHtml(bookResponse.Content);

            var titleNode = doc.DocumentNode.SelectSingleNode(@"/html[1]/head[1]/title");

            Console.WriteLine(titleNode.InnerText);

            var book = Regex.Split(titleNode.InnerText, "章节列表", RegexOptions.IgnoreCase)[0];

            using (Stream stream = File.Open(book + ".txt", FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
            using (TextWriter writer = new StreamWriter(stream))
            {

                var chapterNode = doc.DocumentNode.SelectSingleNode(@"//ul[@class='chapter']");

                if (chapterNode != null)
                {
                    foreach (var node in chapterNode.SelectNodes(@"//li"))
                    {

                        var chapterIndex = 6321;
                        try
                        {
                            chapterIndex = Convert.ToInt32(Regex.Match(node.InnerText, @"第\d*章").Value.TrimStart('第').TrimEnd('章'));
                        }
                        catch
                        {

                        }

                        if (chapterIndex < 6321) continue;


                        Console.Write(chapterIndex + "          ");
                        Console.WriteLine(node.InnerText);

                        var href = node.SelectSingleNode("a").Attributes["href"];

                        var request = new RestRequest(href.Value);

                        var response = client.Execute(request);

                        while (response.StatusCode != System.Net.HttpStatusCode.OK
                            || string.IsNullOrEmpty(response.Content))
                        {
                            response = client.Execute(request);
                        }

                        var chapterDoc = new HtmlDocument();
                        chapterDoc.LoadHtml(response.Content);

                        var contentNode = chapterDoc.DocumentNode.SelectSingleNode(@"//div[@id='nr1']");

                        var content = contentNode.InnerText;

                        content = Regex.Replace(content, "【.+】", "");
                        foreach (var ignore in ignores)
                        {
                            content = content.Replace(ignore, "");
                        }

                        writer.WriteLine(node.InnerText);
                        writer.WriteLine(content);

                    }

                }

            }




        }
    }
}
