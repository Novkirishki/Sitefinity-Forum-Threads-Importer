using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForumThreadsImporter.Crawler.Model
{
    public class RemoteForumThread
    {
        public string Id { get; set; }
        public string Link { get; set; }
        public string Title { get; internal set; }
        public bool IsAnswered { get; internal set; }
        public int PostsCount { get; internal set; }
    }
}
