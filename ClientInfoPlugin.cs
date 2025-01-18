using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace DNWS
{
  class ClientInfoPlugin : IPlugin
  {
    protected static Dictionary<String, int> statDictionary = null;
    public ClientInfoPlugin()
    {
      if (statDictionary == null)
      {
        statDictionary = new Dictionary<String, int>();

      }
    }

    public void PreProcessing(HTTPRequest request)
    {
      if (statDictionary.ContainsKey(request.Url))
      {
        statDictionary[request.Url] = (int)statDictionary[request.Url] + 1;
      }
      else
      {
        statDictionary[request.Url] = 1;
      }
    }

    public HTTPResponse GetResponse(HTTPRequest request)
    {
      HTTPResponse response = null;
      StringBuilder sb = new StringBuilder();

      // Parse client endpoint details
      string remoteEndpoint = request.getPropertyByKey("RemoteEndPoint");
      IPEndPoint endpoint = IPEndPoint.Parse(remoteEndpoint);

      // Build HTML response
      sb.Append("<html><body><pre>");
      sb.AppendFormat("Client IP: {0}<br/>\n", endpoint.Address);
      sb.AppendFormat("Client Port: {0}<br/>\n", endpoint.Port);
      sb.AppendFormat("Browser Information: {0}<br/>\n", request.getPropertyByKey("User-Agent")?.Trim() ?? "N/A");
      sb.AppendFormat("Accept Language: {0}<br/>\n", request.getPropertyByKey("Accept-Language")?.Trim() ?? "N/A");
      sb.AppendFormat("Accept Encoding: {0}<br/>\n", request.getPropertyByKey("Accept-Encoding")?.Trim() ?? "N/A");
      sb.Append("</pre></body></html>");

      // Create and return the response
      response = new HTTPResponse(200);
      response.body = Encoding.UTF8.GetBytes(sb.ToString());
      response.type = "text/html";
      return response;
    }


    public HTTPResponse PostProcessing(HTTPResponse response)
    {
      throw new NotImplementedException();
    }
  }
}