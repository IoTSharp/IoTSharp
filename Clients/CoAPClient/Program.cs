
using CoAP;
using CoAP.Util;
using System;
using System.Collections.Generic;

namespace CoAPClient
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string method = null;
            Uri uri = null;
            string payload = null;
            bool loop = false;
            bool byEvent = true;

            if (args.Length == 0)
                PrintUsage();

            Int32 index = 0;
            foreach (String arg in args)
            {
                if (arg[0] == '-')
                {
                    if (arg.Equals("-l"))
                        loop = true;
                    if (arg.Equals("-e"))
                        byEvent = true;
                    else
                        Console.WriteLine("Unknown option: " + arg);
                }
                else
                {
                    switch (index)
                    {
                        case 0:
                            method = arg.ToUpper();
                            break;

                        case 1:
                            try
                            {
                                uri = new Uri(arg);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Failed parsing URI: " + ex.Message);
                                Environment.Exit(1);
                            }
                            break;

                        case 2:
                            payload = arg;
                            break;

                        default:
                            Console.WriteLine("Unexpected argument: " + arg);
                            break;
                    }
                    index++;
                }
            }

            if (method == null || uri == null)
                PrintUsage();

            Request request = NewRequest(method);
            if (request == null)
            {
                Console.WriteLine("Unknown method: " + method);
                Environment.Exit(1);
            }

            if ("OBSERVE".Equals(method))
            {
                request.MarkObserve();
                loop = true;
            }
            else if ("DISCOVER".Equals(method) &&
                (String.IsNullOrEmpty(uri.AbsolutePath) || uri.AbsolutePath.Equals("/")))
            {
                uri = new Uri(uri, "/.well-known/core");
            }

            request.URI = uri;
            request.SetPayload(payload, MediaType.TextPlain);
            // uncomment the next line if you want to specify a draft to use
            // request.EndPoint = CoAP.Net.EndPointManager.Draft13;

            Console.WriteLine(Utils.ToString(request));

            try
            {
                if (byEvent)
                {
                    request.Respond += delegate (Object sender, ResponseEventArgs e)
                    {
                        Response response = e.Response;
                        if (response == null)
                        {
                            Console.WriteLine("Request timeout");
                        }
                        else
                        {
                            Console.WriteLine(Utils.ToString(response));
                            Console.WriteLine("Time (ms): " + response.RTT);
                        }
                        if (!loop)
                            Environment.Exit(0);
                    };
                    request.Send();
                    while (true)
                    {
                        Console.ReadKey();
                    }
                }
                else
                {
                    // uncomment the next line if you need retransmission disabled.
                    // request.AckTimeout = -1;

                    request.Send();

                    do
                    {
                        Console.WriteLine("Receiving response...");

                        Response response = null;
                        response = request.WaitForResponse();

                        if (response == null)
                        {
                            Console.WriteLine("Request timeout");
                            break;
                        }
                        else
                        {
                            Console.WriteLine(Utils.ToString(response));
                            Console.WriteLine("Time elapsed (ms): " + response.RTT);

                            if (response.ContentType == MediaType.ApplicationLinkFormat)
                            {
                                IEnumerable<WebLink> links = LinkFormat.Parse(response.PayloadString);
                                if (links == null)
                                {
                                    Console.WriteLine("Failed parsing link format");
                                    Environment.Exit(1);
                                }
                                else
                                {
                                    Console.WriteLine("Discovered resources:");
                                    foreach (var link in links)
                                    {
                                        Console.WriteLine(link);
                                    }
                                }
                            }
                        }
                    } while (loop);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed executing request: " + ex.Message);
                Console.WriteLine(ex);
                Environment.Exit(1);
            }
        }

        private static Request NewRequest(String method)
        {
            switch (method)
            {
                case "POST":
                    return Request.NewPost();

                case "PUT":
                    return Request.NewPut();

                case "DELETE":
                    return Request.NewDelete();

                case "GET":
                case "DISCOVER":
                case "OBSERVE":
                    return Request.NewGet();

                default:
                    return null;
            }
        }

        private static void PrintUsage()
        {
            Console.WriteLine("CoAP.NET  Client");
            Console.WriteLine();
            Console.WriteLine("Usage: CoAPClient [-e] [-l] method uri [payload]");
            Console.WriteLine("  method  : { GET, POST, PUT, DELETE, DISCOVER, OBSERVE }");
            Console.WriteLine("  uri     : The CoAP URI of the remote endpoint or resource.");
            Console.WriteLine("  payload : The data to send with the request.");
            Console.WriteLine("Options:");
            Console.WriteLine("  -e      : Receives responses by the Responded event.");
            Console.WriteLine("  -l      : Loops for multiple responses.");
            Console.WriteLine("            (automatic for OBSERVE and separate responses)");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  CoAPClient DISCOVER coap://localhost");
            Console.WriteLine("  CoAPClient POST coap://localhost/storage data");
            Environment.Exit(0);
        }
    }
}