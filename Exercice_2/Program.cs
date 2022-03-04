
using System.Net;
using System.IO;
using System.Text;
using System.Web;
using System.Diagnostics;

namespace BasicServerHTTPlistener
{
    class MyMethods
    {
        public static string Exec(string value1, string value2)
        {
            System.Diagnostics.ProcessStartInfo start = new ProcessStartInfo();
            Console.Write(Directory.GetCurrentDirectory());
            start.FileName = @"../../../../builder.sh";
            start.Arguments = value1 + " " + value2;
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            //
            // Start the process.
            //
            using (Process process = Process.Start(start))
            {
                //
                // Read in all the text from the process with the StreamReader.
                //
                using (StreamReader reader = process.StandardOutput)
                {
                    string result = reader.ReadToEnd();
                    Console.WriteLine(result);
                    Console.ReadLine();
                }
            }
            return "<html><body>ERROR 500</body></html>";
        }
    }

    internal class Program
    {
        private static void Main(string[] args)
        {

            //if HttpListener is not supported by the Framework
            if (!HttpListener.IsSupported)
            {
                Console.WriteLine("A more recent Windows version is required to use the HttpListener class.");
                return;
            }


            // Create a listener.
            HttpListener listener = new HttpListener();

            // Add the prefixes.
            if (args.Length != 0)
            {
                foreach (string s in args)
                {
                    listener.Prefixes.Add(s);
                    // don't forget to authorize access to the TCP/IP addresses localhost:xxxx and localhost:yyyy 
                    // with netsh http add urlacl url=http://localhost:xxxx/ user="Tout le monde"
                    // and netsh http add urlacl url=http://localhost:yyyy/ user="Tout le monde"
                    // user="Tout le monde" is language dependent, use user=Everyone in english 

                }
            }
            else
            {
                Console.WriteLine("Syntax error: the call must contain at least one web server url as argument");
            }
            listener.Start();

            // get args 
            foreach (string s in args)
            {
                Console.WriteLine("Listening for connections on " + s);
            }

            // Trap Ctrl-C on console to exit 
            Console.CancelKeyPress += delegate {
                // call methods to close socket and exit
                listener.Stop();
                listener.Close();
                Environment.Exit(0);
            };


            while (true)
            {
                // Note: The GetContext method blocks while waiting for a request.
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;

                string documentContents;
                using (Stream receiveStream = request.InputStream)
                {
                    using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                    {
                        documentContents = readStream.ReadToEnd();
                    }
                }

                // get url 
                Console.WriteLine($"Received request for {request.Url}");
                string result = "<html><body>Not found</body></html>";
                if (request.Url.AbsolutePath == "/generate/html/exe")
                {
                    string value1 = HttpUtility.ParseQueryString(request.Url.Query).Get("param1");
                    string value2 = HttpUtility.ParseQueryString(request.Url.Query).Get("param2");
                    result = MyMethods.Exec(value1, value2);
                };


               
                Console.WriteLine(documentContents);

                // Obtain a response object.
                HttpListenerResponse response = context.Response;

                // Construct a response.
                Console.WriteLine(result);
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(result);
                // Get a response stream and write the response to it.
                response.ContentLength64 = buffer.Length;
                System.IO.Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                // You must close the output stream.
                output.Close();
            }
            // Httplistener neither stop ... But Ctrl-C do that ...
            // listener.Stop();
        }
    }
}