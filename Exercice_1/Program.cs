using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;

namespace BasicServerHTTPlistener
{
    class MyMethods
    {
        public static string Method1(string value1, string value2)
        {
            return "<html><body> Methode 1 avec " + value1 + " et " + value2 + "</body></html>";
        }

        public static string Method(string value1, string value2)
        {
            return "<html><body> Methode 2 avec" + value1 + " et " + value2 + "</body></html>";
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

                string[] path = request.Url.Segments;
                string methodToInvokde = "";
                if (path[path.Length - 2] == "mymethods/")
                {
                     methodToInvokde = path[path.Length - 1];
                }
                string value1 = HttpUtility.ParseQueryString(request.Url.Query).Get("param1");
                string value2 = HttpUtility.ParseQueryString(request.Url.Query).Get("param2");
                
                
                //
                Console.WriteLine(documentContents);

                // Obtain a response object.
                HttpListenerResponse response = context.Response;

                string htmlBody;

                Type type = typeof(MyMethods);
                MethodInfo method = type.GetMethod(methodToInvokde);

                if (method != null)
                {
                    MyMethods c = new MyMethods();
                    object[] arguments = { value1, value2 };
                    htmlBody = (string)method.Invoke(c, arguments);
                }
                else
                {
                    htmlBody = "<HTML><BODY> Hello !</BODY></HTML>";
                }

                // Construct a response.
                Console.WriteLine(htmlBody);
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(htmlBody);
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