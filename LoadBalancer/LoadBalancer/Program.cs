using System;
using System.Net;

namespace LoadBalancer
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!HttpListener.IsSupported)
            {
                Console.WriteLine("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
                return;
            }
            string[] prefixes = { "http://localhost:8000/" };
            // URI prefixes are required,
            // for example "http://localhost:8080/".
            if (prefixes == null || prefixes.Length == 0)
                throw new ArgumentException("prefixes");

            // Create a listener.
            HttpListener listener = new HttpListener();
            // Add the prefixes.
            foreach (string s in prefixes)
            {
                listener.Prefixes.Add(s);
            }
            listener.Start();
            Console.WriteLine("Listening...");
            bool Server = true;
            while (listener.IsListening)
            {
                // Note: The GetContext method blocks while waiting for a request. 
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;
                // Obtain a response object.
                HttpListenerResponse response = context.Response;
                // Construct a response.

                // Create web client simulating IE6.
                string responseString;
                using (WebClient client = new WebClient())
                {
                    client.Headers["User-Agent"] =
                    "Mozilla/4.0 (Compatible; Windows NT 5.1; MSIE 6.0) " +
                    "(compatible; MSIE 6.0; Windows NT 5.1; " +
                    ".NET CLR 1.1.4322; .NET CLR 2.0.50727)";

                    // Download data.
                    byte[] arr;
                    if (Server)
                    {
                        arr = client.DownloadData("http://localhost:8001/");
                        Server = false;
                    }
                    else
                    {
                        arr = client.DownloadData("http://localhost:8002/");
                        Server = true;
                    }

                    // Write values.
                    responseString = System.Text.Encoding.UTF8.GetString(arr);
                }

                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                // Get a response stream and write the response to it.
                response.ContentLength64 = buffer.Length;
                System.IO.Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                // You must close the output stream.
                output.Close();
                listener.GetContext();
            }
            listener.Stop();
        }
    }
}
