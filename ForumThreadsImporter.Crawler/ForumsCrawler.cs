using ForumThreadsImporter.Crawler.Model;
using System;
using System.Collections.Generic;

namespace ForumThreadsImporter.Crawler
{
    public class ForumsCrawler
    {
        public const string PageLinksSelector = ".sf_pagerNumeric a";
        public const string ThreadsSelector = "table.sfforumThreadsList tr";
        public const string ThreadIdSelector = "a.sfforumThreadTitle";
        public const string ThreadIdAttribute = "href";
        public const string ThreadTitleSelector = "a.sfforumThreadTitle";
        public const string ThreadTitleAttribute = "Title";
        public const string ThreadIsAnsweredSelector = "span.sfforumThreadAnswered";
        public const string ThreadPostsCountSelector = "td.sfforumThreadPostsWrp";
        public const string BaseForumsAddress = "https://architecture.sitefinity.com/forums/sitefinity-support-forum-group/sitefinity-unit-1/sitefinity-unit-1/";
        private const string CacheKey = "cachedThreads";
        private MarkupProvider markupProvider;
        private string address;

        public ForumsCrawler(MarkupProvider markupProvider)
        {
            this.markupProvider = markupProvider;
            address = BaseForumsAddress + "page/";
        }

        public IEnumerable<RemoteForumThread> GetThreads()
        {
            var document = markupProvider.GetDomDocument(this.address + "/1");
            var pagesCount = document.QuerySelectorAll(PageLinksSelector).Length;
            pagesCount = pagesCount == 0 ? 1 : pagesCount;
            var externalThreads = new List<RemoteForumThread>();
            for (int i = 1; i < pagesCount + 1; i++)
            {
                if (i > 1)
                {
                    var address = this.address + "/" + i;
                    document = markupProvider.GetDomDocument(address);
                }

                var threadSelector = ThreadsSelector;
                var markupThreads = document.QuerySelectorAll(threadSelector);
                foreach (var mt in markupThreads)
                {
                    var id = mt.QuerySelector(ThreadIdSelector)?.GetAttribute(ThreadIdAttribute);
                    if (id == null)
                    {
                        continue;
                    }

                    var title = mt.QuerySelector(ThreadTitleSelector)?.GetAttribute(ThreadTitleAttribute);
                    var isAnswered = mt.QuerySelector(ThreadIsAnsweredSelector) != null;
                    var postsCountText = mt.QuerySelector(ThreadPostsCountSelector).TextContent.Trim();
                    if (int.TryParse(postsCountText, out var postsCount))
                    {
                        externalThreads.Add(new RemoteForumThread()
                        {
                            Id = id,
                            Link = BaseForumsAddress + id.Trim(new char[] { '.', '/' }),
                            Title = title,
                            IsAnswered = isAnswered,
                            PostsCount = postsCount
                        });
                    }
                    else
                    {
                        throw new FormatException($"Invalid posts count format for post with title ${title}");
                    }
                }
            }

            return externalThreads;
        }
    }
}
