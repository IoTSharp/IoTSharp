using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTSharp.Releases
{
    public class GithubRelease
    {
        public string url { get; set; }
        public string html_url { get; set; }
        public string assets_url { get; set; }
        public string upload_url { get; set; }
        public string tarball_url { get; set; }
        public string zipball_url { get; set; }
        public string id { get; set; }
        public string tag_name { get; set; }
        public string name { get; set; }
        public string body { get; set; }

        public ICollection<GithubReleaseAsset> assets { get; set; }

    }

    public class GithubReleaseAsset
    {
        public string url { get; set; }
        public string browser_download_url { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string label { get; set; }
        public string state { get; set; }
        public string content_type { get; set; }
        public long size { get; set; }
        public int download_count { get; set; }
    }
}
