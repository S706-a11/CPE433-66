using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DNWS
{
  public class HTTPRequest
  {
    protected String _url;
    protected String _filename;
    protected static Dictionary<String, String> _propertyListDictionary = null;
    protected static Dictionary<String, String> _requestListDictionary = null;

    protected String _body;

    protected int _status;

    protected String _method;

    public String Url
    {
      get { return _url; }
    }

    public String Filename
    {
      get { return _filename; }
    }

    public String Body
    {
      get { return _body; }
    }

    public int Status
    {
      get { return _status; }
    }

    public String Method
    {
      get { return _method; }
    }
    public HTTPRequest(String request)
    {
      _propertyListDictionary = new Dictionary<String, String>();
      String[] lines = Regex.Split(request, "\\n");

      if (lines.Length == 0)
      {
        _status = 500;
        return;
      }

      String[] statusLine = Regex.Split(lines[0], "\\s");
      if (statusLine.Length != 4)
      { // too short something is wrong
        _status = 401;
        return;
      }
      if (!statusLine[0].ToLower().Equals("get"))
      {
        _method = "GET";
      }
      else if (!statusLine[0].ToLower().Equals("post"))
      {
        _method = "POST";
      }
      else
      {
        _status = 501;
        return;
      }
      _status = 200;

      _url = statusLine[1];
      String[] urls = Regex.Split(_url, "/");
      _filename = urls[urls.Length - 1];
      String[] parts = Regex.Split(_filename, "[?]");
      if (parts.Length > 1 && parts[1].Contains('&'))
      {
        //Ref: http://stackoverflow.com/a/4982122
        _requestListDictionary = parts[1].Split('&').Select(x => x.Split('=')).ToDictionary(x => x[0].ToLower(), x => x[1]);
      }
      else
      {
        _requestListDictionary = new Dictionary<String, String>();
      }

      if (lines.Length == 1) return;

      for (int i = 1; i < lines.Length; i++) // Start from line 1 (skip the status line)
      {
        if (string.IsNullOrWhiteSpace(lines[i]))
          continue; // Skip empty lines

        // Split the line into key-value pairs by the first occurrence of ": "
        int separatorIndex = lines[i].IndexOf(": ");
        if (separatorIndex > 0)
        {
          // Split into key and value
          string key = lines[i].Substring(0, separatorIndex).Trim();
          string value = lines[i].Substring(separatorIndex + 2).Trim();
          addProperty(key, value);
        }
        else if (_method == "POST") // Handle POST body if the line doesn't follow "key: value" format
        {
          string body = lines[i].Trim();
          if (!string.IsNullOrEmpty(body))
          {
            try
            {
              var bodyParams = body.Split('&')
                                   .Select(x => x.Split('='))
                                   .ToDictionary(
                                       x => x[0].ToLower().Trim(),
                                       x => x.Length > 1 ? x[1].Trim() : string.Empty // Handle missing values
                                   );
              _requestListDictionary = _requestListDictionary
                  .Concat(bodyParams)
                  .ToDictionary(x => x.Key, x => x.Value);
            }
            catch
            {
              // Handle any parsing errors gracefully
              _status = 400; // Bad Request
            }
          }
        }
      }

    }
    public String getPropertyByKey(String key)
    {
      if (_propertyListDictionary.ContainsKey(key.ToLower()))
      {
        return _propertyListDictionary[key.ToLower()];
      }
      else
      {
        return null;
      }
    }
    public String getRequestByKey(String key)
    {
      if (_requestListDictionary.ContainsKey(key.ToLower()))
      {
        return _requestListDictionary[key.ToLower()];
      }
      else
      {
        return null;
      }
    }

    public void addProperty(String key, String value)
    {
      _propertyListDictionary[key.ToLower()] = value;
    }
    public void addRequest(String key, String value)
    {
      _requestListDictionary[key.ToLower()] = value;
    }
  }
}