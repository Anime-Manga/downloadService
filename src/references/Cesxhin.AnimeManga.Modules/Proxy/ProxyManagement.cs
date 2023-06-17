using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cesxhin.AnimeManga.Modules.Proxy
{
    public static class ProxyManagement
    {
        private static List<string> GetList(string name)
        {
            var env = Environment.GetEnvironmentVariable(name);
            List<string> listEnv = new();

            if (!string.IsNullOrEmpty(env))
                listEnv.AddRange(env.Split(','));

            return listEnv;
        }

        private static JObject GetBlackList()
        {
            var blackList = Environment.GetEnvironmentVariable("BLACK_LIST_PROXY");
            var jsonBlackList = new JObject();

            if (blackList != null)
                jsonBlackList = JObject.Parse(blackList);

            return jsonBlackList;
        }
        public static void InitProxy()
        {
            var enableProxy = Environment.GetEnvironmentVariable("PROXY_ENABLE") ?? "false";

            if (enableProxy == "true")
            {
                var listProxy = System.IO.File.ReadAllText("proxy.txt");

                Environment.SetEnvironmentVariable("LIST_PROXY", listProxy);
            } 
        }

        public static List<string> GetAllIP()
        {
            return GetList("LIST_PROXY");
        }

        public static string GetIp(string id)
        {
            var enableProxy = Environment.GetEnvironmentVariable("PROXY_ENABLE") ?? "false";

            if(enableProxy == "true")
            {
                var listProxy = GetList("LIST_PROXY");
                var blackProxyJson = GetBlackList();

                if (blackProxyJson.ContainsKey(id))
                {
                    var listBlackProxy = ((string)blackProxyJson[id]).Split(",").ToList();

                    foreach (var blackProxy in listBlackProxy)
                    {
                        listProxy = listProxy.Where(e => e != blackProxy).ToList();
                    }
                }

                if (listProxy.Count > 0)
                {
                    return listProxy[new Random().Next(listProxy.Count)];
                }
            }

            return null;
        }

        public static bool EnableProxy()
        {
            var enableProxy = Environment.GetEnvironmentVariable("PROXY_ENABLE") ?? "false";
            return enableProxy == "true";
        }

        public static void BlackListAdd(string ip, string id)
        {
            var jsonBlackList = GetBlackList();
            if (jsonBlackList.ContainsKey(id))
            {
                var list = ((string)jsonBlackList[id]).Split(",").ToList();
                list.Add(ip);
                jsonBlackList[id] = string.Join(",", list);
            }
            else
                jsonBlackList[id] = ip;

            Environment.SetEnvironmentVariable("BLACK_LIST_PROXY", jsonBlackList.ToString());
        }
    }
}
