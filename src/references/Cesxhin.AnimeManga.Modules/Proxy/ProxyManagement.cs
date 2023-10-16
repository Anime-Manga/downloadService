using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cesxhin.AnimeManga.Modules.Proxy
{
    public class ProxyManagement
    {
        private string blackList = "";
        private List<string> listProxy = new();

        public void InitProxy()
        {
            var enableProxy = Environment.GetEnvironmentVariable("PROXY_ENABLE") ?? "false";

            if (enableProxy == "true")
            {
                listProxy = System.IO.File.ReadAllText("proxy.txt").Split(",").ToList();
            } 
        }

        public List<string> GetAllIP()
        {
            return listProxy;
        }

        public string GetIp()
        {
            if(EnableProxy())
            {
                var listBlackProxy = blackList.Split(",").ToList();

                foreach (var blackProxy in listBlackProxy)
                {
                    listProxy = listProxy.Where(e => e != blackProxy).ToList();
                }

                if (listProxy.Count > 0)
                {
                    return listProxy[new Random().Next(listProxy.Count)];
                }
            }

            return null;
        }

        public bool EnableProxy()
        {
            var enableProxy = Environment.GetEnvironmentVariable("PROXY_ENABLE") ?? "false";
            return enableProxy == "true";
        }

        public void BlackListAdd(string ip)
        {
            var list = blackList.Split(",").ToList();

            if(list.Find(e => e == ip) == null)
            {
                list.Add(ip);
                blackList = string.Join(",", list);
            }
        }
    }
}
