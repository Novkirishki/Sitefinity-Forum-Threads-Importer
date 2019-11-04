using Microsoft.TeamFoundation.Core.WebApi.Types;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ForumThreadsImporter
{
    internal class AzureDevOpsService
    {
        private string vstsCollectionUrl;
        private string pat;
        private VssConnection connection;

        public AzureDevOpsService(string vstsCollectionUrl, string pat)
        {
            this.vstsCollectionUrl = vstsCollectionUrl;
            this.pat = pat;

            connection = new VssConnection(new Uri(vstsCollectionUrl), new VssBasicCredential(string.Empty, pat));
        }

        public WorkItem GetWorkItem(string title = "RFA", string type = Constants.PBI, string area = "sitefinity\\Arke", string iteration = "@currentIteration('[sitefinity]\\Arke <id:22bafa7e-b3fa-4e91-8f41-0702715d148a>')")
        {         
            WorkItemTrackingHttpClient witClient = this.connection.GetClient<WorkItemTrackingHttpClient>();

            var teamContext = new TeamContext(Constants.SitefinityProjectName);

            var query = "SELECT * FROM workitems " +
                           $"WHERE [Work Item Type] = '{type}' " +
                               $"AND [{Constants.Area}] = '{area}' " +
                               $"AND [{Constants.Iteration}] = {iteration} " +
                               $"AND [{Constants.Title}] CONTAINS '{title}' ";

            var wiqlQuery = new Wiql() { Query = query };

            WorkItemQueryResult queryResults = witClient.QueryByWiqlAsync(wiqlQuery, teamContext).Result;

            if (queryResults != null && queryResults.WorkItems.Count() != 0)
            {
                var id = queryResults.WorkItems.First().Id;
                return witClient.GetWorkItemAsync(id, expand: WorkItemExpand.Relations).Result;
            }

            return null;
        }

        public WorkItem CreateAndLinkToWorkItem(WorkItem itemToLinkTo, string title, string description = null, string projectName = Constants.SitefinityProjectName, string type = Constants.Task)
        {
            WorkItemTrackingHttpClient witClient = this.connection.GetClient<WorkItemTrackingHttpClient>();

            JsonPatchDocument patchDocument = new JsonPatchDocument();

            patchDocument.Add(
                new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = $"/fields/{Constants.Title}",
                    Value = title
                }
            );

            patchDocument.Add(
                new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = $"/fields/{Constants.Description}",
                    Value = description
                }
            );

            patchDocument.Add(
                new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = $"/fields/{Constants.Iteration}",
                    Value = itemToLinkTo.Fields[Constants.Iteration]
                }
            );

            patchDocument.Add(
                new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = $"/fields/{Constants.Area}",
                    Value = itemToLinkTo.Fields[Constants.Area]
                }
            );

            patchDocument.Add(
                new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/relations/-",
                    Value = new
                    {
                        rel = "System.LinkTypes.Hierarchy-Reverse",
                        itemToLinkTo.Url
                    }
                }
            );

            WorkItem result = witClient.CreateWorkItemAsync(patchDocument, projectName, type).Result;

            return result;
        }

        public IList<WorkItem> GetChildrenWorkItems(WorkItem parentItem)
        {
            WorkItemTrackingHttpClient witClient = this.connection.GetClient<WorkItemTrackingHttpClient>();

            // check whether the thread is not already imported
            List<int> list = new List<int>();

            foreach (var relation in parentItem.Relations)
            {
                //get the child links
                if (relation.Rel == "System.LinkTypes.Hierarchy-Forward")
                {
                    var lastIndex = relation.Url.LastIndexOf("/");
                    var itemId = relation.Url.Substring(lastIndex + 1);
                    list.Add(Convert.ToInt32(itemId));
                };
            }

            int[] workitemIds = list.ToArray();

            return witClient.GetWorkItemsAsync(workitemIds, expand: WorkItemExpand.Fields).Result;
        }
    }
}
