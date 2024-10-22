using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace LookingGlass.Toolkit {
    public class DefaultHttpSender : IHttpSender, IDisposable {
        private HttpClient client;

        public int TimeoutSeconds {
            get { return (int) client.Timeout.TotalSeconds; }
            set { client.Timeout = TimeSpan.FromSeconds(value); }
        }

        public DefaultHttpSender() {
            client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);
        }

        public void Dispose() {
            if (client != null) {
                client.Dispose();
                client = null;
            }
        }

        private HttpMethod GetInnerMethod(HttpSenderMethod method) {
            switch (method) {
                case HttpSenderMethod.Get:      return HttpMethod.Get;
                case HttpSenderMethod.Post:     return HttpMethod.Post;
                case HttpSenderMethod.Put:      return HttpMethod.Put;
                default:
                    throw new NotSupportedException("Unsupported HTTP method: " + method);
            }
        }

        public string Send(HttpSenderMethod method, string url, string content)
        {
            var request = new HttpRequestMessage(GetInnerMethod(method), url);
            if (method == HttpSenderMethod.Post || method == HttpSenderMethod.Put)
            {
                request.Content = new StringContent(content);
            }

            // Send request synchronously
            HttpResponseMessage response = client.Send(request); // Sync Send
            return response.Content.ReadAsStringAsync().Result; // Read response body synchronously
        }

        public Task<string> SendAsync(HttpSenderMethod method, string url, string content)
        {
            // Call the synchronous method
            string result = Send(method, url, content);

            // Return the result as a completed task
            return Task.FromResult(result);
        }


    }
}
