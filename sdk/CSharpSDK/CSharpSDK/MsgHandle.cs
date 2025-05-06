using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpSDK
{
    class MsgHandle
    {
        public static JObject parseJson(string json)
        {
            JObject j = JObject.Parse(json);
            return j;
        }

        public static string toJson(JObject jsonObject)
        {
            string json = jsonObject.ToString();
            return json;
        } 
    }
}
