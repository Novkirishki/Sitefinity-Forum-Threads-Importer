using ForumThreadsImporter.Crawler;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ForumThreadsImporter
{
    class Program
    {
        internal const string vstsCollectionUrl = "https://prgs-sitefinity.visualstudio.com";
        internal const string pat = "";

        static void Main(string[] args)
        {
            var forumsCrawler = new ForumsCrawler(new MarkupProvider());
            var threads = forumsCrawler.GetThreads();

            // get only threads that are not answered and are new
            threads = threads.Where(x => !x.IsAnswered && x.PostsCount == 1);

            var azureDevOpsService = new AzureDevOpsService(vstsCollectionUrl, pat);

            var rfaWorkItem = azureDevOpsService.GetWorkItem("RFA");
            if (rfaWorkItem != null)
            {
                var childrenWorkItems = azureDevOpsService.GetChildrenWorkItems(rfaWorkItem);
                var childrenWorkItemsTitles = childrenWorkItems.Select(x => x.Fields[Constants.Title].ToString()).ToList();

                foreach (var thread in threads)
                {
                    var threadTitle = $"Forum: {thread.Title}";
                    var isAlreadyLogged = childrenWorkItemsTitles.Contains(threadTitle) || threadTitle == "Forum: Test thread 01";
                    if (!isAlreadyLogged)
                    {
                        azureDevOpsService.CreateAndLinkToWorkItem(rfaWorkItem, threadTitle, thread.Link);
                    }
                }
            }
        }
    }
}
