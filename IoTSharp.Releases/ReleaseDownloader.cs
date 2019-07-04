using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace IoTSharp.Releases
{
    public class ReleaseDownloader
    {
        private readonly string _baseUri;

        private readonly string _accessToken;

        private readonly string _userAgent;

        private readonly string _releaseUri;

        private string _user;
        private string _repo;
        private string _token;


        public ReleaseDownloader(string _url, string accessToken)
        {
            var uri = new Uri(_url);
            _baseUri = $"{ GetBaseUri(uri)}/repos/{GetUserFromUri(uri)}/{GetRepoFromUri(uri)}";
            _accessToken = accessToken;
            _userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/77.0.3829.0 Safari/537.36 Edg/77.0.197.1"; ;
            _releaseUri = GetReleaseUri();
        }

        private static string GetUserFromUri(Uri uri)
        {
            return !uri.LocalPath.Contains("/") ? string.Empty : uri.Segments[1].TrimEnd('/');
        }

        private static string GetRepoFromUri(Uri uri)
        {
            return !uri.LocalPath.Contains("/") ? string.Empty : uri.Segments[2].TrimEnd('/');
        }

        private static string GetBaseUri(Uri uri)
        {
            return uri.Host.Equals("github.com", StringComparison.OrdinalIgnoreCase) ? "https://api.github.com" : $"{uri.Scheme}://{uri.Host}/api/v3";
        }


        public ICollection<GithubRelease> GetDataForAllReleases()
        {
            var requestingUri = GetAccessTokenUri(_releaseUri);
            return DownloadReleases(requestingUri);
        }

        public ICollection<GithubRelease> DownloadReleases(string requestingUri)
        {

            Console.WriteLine("Requesting: {0}", requestingUri);
            var request = (HttpWebRequest)WebRequest.Create(new Uri(requestingUri));
            request.UserAgent = _userAgent;

            var response = request.GetResponse();
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            // Get the stream containing content returned by the server.

            var responseFromServer = ReadResponseFromServer(response);

            var releases = JsonConvert.DeserializeObject<List<GithubRelease>>(responseFromServer);

            var parser = new LinkHeaderParser();

            var linkHeader = response.Headers["Link"];

            var nextUrl = parser.GetNextPageFromHeader(linkHeader);

            if (!string.IsNullOrEmpty(nextUrl))
            {
                releases.AddRange(DownloadReleases(nextUrl));
            }

            // Clean up the streams and the response.
            response.Close();
            return releases;
        }

        private string GetReleaseUri()
        {
            var releaseUri = $"{_baseUri}/releases";
            return releaseUri;
        }

        private static string ReadResponseFromServer(WebResponse response)
        {
            using (var dataStream = response.GetResponseStream())
            {
                // Open the stream using a StreamReader for easy access.
                using (var reader = new StreamReader(dataStream))
                {
                    // Read the content.
                    return reader.ReadToEnd();
                }
            }
        }

        private string GetAssetsUriForId(string id)
        {
            var assetUri = $"{_releaseUri}/assets/{id}";
            return assetUri;
        }

        private string GetAccessTokenUri(string uri)
        {
            return _accessToken == string.Empty ? uri : uri += $"?access_token={_accessToken}";
        }

        public bool DownloadAsset(string id, string path)
        {
            WebResponse response = GetAssetResponse(id);
            GetBinaryResponseFromResponse(path,response);
            return true;
        }
        public bool DownloadAsset(string id, out byte[] assetdata)
        {
            WebResponse response = GetAssetResponse(id);
            assetdata = GetBinaryResponseFromResponse(response);
            return true;
        }

        private WebResponse GetAssetResponse(string id)
        {
            var assetUri = GetAccessTokenUri(GetAssetsUriForId(id));

            var request = (HttpWebRequest)WebRequest.Create(new Uri(assetUri));
            request.Accept = "application/octet-stream";
            request.UserAgent = "mwhitis";

            var response = request.GetResponse();
            return response;
        }

        private static byte[] GetBinaryResponseFromResponse( WebResponse response)
        {
            byte[] result = null;
            long received = 0;
            byte[] buffer = new byte[1024];
            using (var ms = new  MemoryStream() )
            {
                using (var input = response.GetResponseStream())
                {
                    int size = input.Read(buffer, 0, buffer.Length);
                    while (size > 0)
                    {
                        ms.Write(buffer, 0, size);
                        received += size;

                        size = input.Read(buffer, 0, buffer.Length);
                    }
                }

                result = ms.ToArray();
            }
            return result;
        }
        private static void GetBinaryResponseFromResponse(string path, WebResponse response)
        {
            System.IO.File.WriteAllBytes(path, GetBinaryResponseFromResponse(response));
        }
    }
}
