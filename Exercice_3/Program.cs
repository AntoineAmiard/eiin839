using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;

namespace BasicServerHTTPlistener
{
    class Counter
    {
        private int counter = 0;

        public int increment(int value)
        {
            counter += value;
            return counter;
        }
    }

    internal class Program
    {

        private static void Main(string[] args)
        {
            Counter counter = new Counter();

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
            Console.CancelKeyPress += delegate
            {
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
                string responseBody = "";
                int statusCode = 200;

                try
                {
                    if (request.Url.AbsolutePath == "/increment/value")
                    {
                        string step = HttpUtility.ParseQueryString(request.Url.Query).Get("step");
                        if (step == null)
                        {
                            throw new Exception("step argument must be specified");
                        }

                        int value = Int32.Parse(step);
                        int finalValue = counter.increment(value);
                        
                        responseBody = "{ data : { newValue : " + finalValue.ToString() + " } }";
                    }
                    else
                    {
                        responseBody = "Not found";
                        statusCode = 404;
                    }
                }
                catch (Exception e)
                {
                    responseBody = e.Message;
                    statusCode = 500;
                }





                //
                Console.WriteLine(documentContents);

                // Obtain a response object.
                HttpListenerResponse response = context.Response;
                response.StatusCode = statusCode;

                // Construct a response.
                Console.WriteLine(responseBody);
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseBody);
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