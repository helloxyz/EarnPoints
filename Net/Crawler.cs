using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EarnPoints
{
    public class Crawler
    {
        private string requestFilePath = "";
        public Crawler(string requestFilePath)
        {
            this.requestFilePath = requestFilePath;
        }
        public bool Execute()
        {
            bool result = false;
            if (!string.IsNullOrWhiteSpace(requestFilePath))
            {
                HttpWebRequest request = parseRequest();
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    StreamWriter sw = new StreamWriter("test.html");
                    sw.Write(reader.ReadToEnd());
                    sw.Flush();
                    sw.Dispose();
                }
            }
            return result;
        }

        private HttpWebRequest parseRequest()
        {
            Dictionary<string, string> requestInfo = loadRequestInfo();
            string url = "http://" + requestInfo["Host"] + requestInfo["Url"];
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = requestInfo["Method"];
            if (requestInfo.ContainsKey("Accept"))
                request.Accept = requestInfo["Accept"];
            if (requestInfo.ContainsKey("Content-Type"))
                request.ContentType = requestInfo["Content-Type"];
            if (requestInfo.ContainsKey("User-Agent"))
                request.UserAgent = requestInfo["User-Agent"];
            //if (requestInfo.ContainsKey("Connection"))
            //request.Connection = requestInfo["Connection"];
            if (requestInfo.ContainsKey("Host"))
                request.Host = requestInfo["Host"];
            if (requestInfo.ContainsKey("Referer"))
                request.Referer = requestInfo["Referer"];
            if (requestInfo.ContainsKey("Pragma"))
                request.Headers.Add("Pragma", requestInfo["Pragma"]);
            if (requestInfo.ContainsKey("Cookie"))
            {
                var cookies = new CookieContainer();
                foreach (var c in requestInfo["Cookie"].Split(';'))
                {
                    string[] arr = c.Split('=');
                    if (arr.Length == 2)
                    {
                        Cookie cookie = new Cookie(arr[0].Trim(), arr[1].Trim()) { Domain = request.Host };
                        cookies.Add(cookie);
                    }
                }
                request.CookieContainer = cookies;
            }
            if (requestInfo.ContainsKey("Body"))
            {
                byte[] b = System.Text.Encoding.UTF8.GetBytes(requestInfo["Body"]);
                request.ContentLength = b.Length;
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(b, 0, b.Length);
                }
            }
            return request;
        }

        private Dictionary<string, string> loadRequestInfo()
        {
            Dictionary<string, string> requestInfo = new Dictionary<string, string>();
            string line = "";
            using (StreamReader sr = new StreamReader(requestFilePath))
            {
                while ((line = sr.ReadLine()) != null)
                {
                    line = line.Trim();
                    int index = line.IndexOf(':');
                    if (index > 0)
                    {
                        string name = line.Substring(0, index);
                        string value = line.Substring(index + 1, line.Length - index - 1);
                        requestInfo[name.Trim()] = value.Trim();
                    }
                    else if (line.StartsWith("POST") || line.StartsWith("GET"))
                    {
                        string[] arr = line.Split(' ');
                        if (arr.Length > 2)
                        {
                            requestInfo["Method"] = arr[0].Trim();
                            requestInfo["Url"] = arr[1].Trim();
                        }
                    }
                }
            }
            return requestInfo;
        }
    }
}
