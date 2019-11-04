using ForumThreadsImporter.Crawler;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Quartz;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ForumThreadsImporter
{
    class ImportJob : IJob
    {
        internal const string vstsCollectionUrl = "https://prgs-sitefinity.visualstudio.com";
        internal const string pat = "vzbpktabxd42m7ikgv575vimfjf7rhczwcfk4s3uzagxm7hvjxzq";

        public Task Execute(IJobExecutionContext context)
        {
            var forumsCrawler = new ForumsCrawler(new MarkupProvider());
            var threads = forumsCrawler.GetThreads();

            // get only threads that are not answered and are new
            threads = threads.Where(x => !x.IsAnswered && x.PostsCount == 1);

            Console.WriteLine($"Threads count: {threads.Count()}");

            var azureDevOpsService = new AzureDevOpsService(vstsCollectionUrl, pat);

            WorkItem rfaWorkItem = null;

            try
            {
                rfaWorkItem = azureDevOpsService.GetWorkItem("RFA");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }

            if (rfaWorkItem != null)
            {
                Console.WriteLine($"Found RFA");
                var childrenWorkItems = azureDevOpsService.GetChildrenWorkItems(rfaWorkItem);
                var childrenWorkItemsTitles = childrenWorkItems.Select(x => x.Fields[Constants.Title].ToString()).ToList();

                foreach (var thread in threads)
                {
                    var threadTitle = $"Forum: {thread.Title}";
                    var isAlreadyLogged = childrenWorkItemsTitles.Contains(threadTitle) || threadTitle == "Forum: Test thread 01";
                    if (!isAlreadyLogged)
                    {
                        Console.WriteLine($"Logs thread: {threadTitle}");
                        azureDevOpsService.CreateAndLinkToWorkItem(rfaWorkItem, threadTitle, thread.Link);
                    }
                }
            }

            return Task.FromResult(0);
        }
    }
}
