using AngleSharp;
using AngleSharp.Dom;

namespace ForumThreadsImporter.Crawler
{
    public class MarkupProvider
    {
        public IDocument GetDomDocument(string address)
        {
            var config = Configuration.Default.WithDefaultLoader();
            var doc = BrowsingContext.New(config).OpenAsync(address);
            return doc.Result;
        }
    }
}
