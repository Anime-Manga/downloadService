using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cesxhin.AnimeManga.Application.Schema
{
    public static class SchemaControl
    {
        private static List<bool> CheckContent(JObject schema)
        {
            List<bool> checkArray = new();

            //check type
            checkArray.Add(schema.ContainsKey("type") && schema.GetValue("type").Type == JTokenType.Array);

            foreach (var singleType in schema.GetValue("type").ToObject<IEnumerable<string>>())
                checkArray.Add(checkType(singleType));

            //check child_nodes
            if (schema.ContainsKey("child_nodes"))
                checkArray.Add(schema.GetValue("child_nodes").Type == JTokenType.Integer);

            //check path
            checkArray.Add(schema.ContainsKey("path") && schema.GetValue("path").Type == JTokenType.Array);

            //check attributes
            if (schema.ContainsKey("attributes"))
                checkArray.Add(schema.GetValue("attributes").Type == JTokenType.String);

            //check download
            if (schema.ContainsKey("download"))
                checkArray.Add(schema.GetValue("download").Type == JTokenType.Boolean);

            return checkArray;
        }

        private static bool checkType(string type)
        {
            switch (type) {
                case "string":
                case "image":
                case "array":
                case "link":
                case "video/mp4":
                case "video/m3u8/script":
                case "book/link":
                case "number":
                    return true;
                default:
                    return false;
            }
        }

        public static void Check()
        {
            //check valid schema
            Console.WriteLine("[STARTUP] Check schemas");
            var schemasFile = System.IO.File.ReadAllText(".\\schemas.json");
            var schemas = JObject.Parse(schemasFile);
            var checkArray = new List<bool>();

            foreach (var schema in schemas)
            {
                var selectSchema = schemas.GetValue(schema.Key).ToObject<JObject>();

                checkArray.Add(selectSchema.ContainsKey("name") && selectSchema.GetValue("name").Type == JTokenType.String);
                checkArray.Add(selectSchema.ContainsKey("type") && selectSchema.GetValue("type").Type == JTokenType.String);
                checkArray.Add(selectSchema.ContainsKey("search") && selectSchema.GetValue("search").Type == JTokenType.Object);
                checkArray.Add(selectSchema.ContainsKey("description") && selectSchema.GetValue("description").Type == JTokenType.Object);
                checkArray.Add(selectSchema.ContainsKey(selectSchema.GetValue("type").ToString()) && selectSchema.GetValue(selectSchema.GetValue("type").ToString()).Type == JTokenType.Object);

                checkArray.Add(selectSchema.GetValue("description").ToObject<JObject>().ContainsKey("name_id"));
                checkArray.Add(selectSchema.GetValue("description").ToObject<JObject>().ContainsKey("cover"));

                foreach (var selectDescription in selectSchema.GetValue("description").ToObject<JObject>())
                {
                    var description = selectDescription.Value.ToObject<JObject>();

                    checkArray.AddRange(CheckContent(description));
                }

                var typeSchema = selectSchema.GetValue(selectSchema.GetValue("type").ToString()).ToObject<JObject>();

                //collection
                checkArray.Add(typeSchema.ContainsKey("collection") && typeSchema.GetValue("collection").Type == JTokenType.Object);
                checkArray.AddRange(CheckContent(typeSchema.GetValue("collection").ToObject<JObject>()));

                //procedure
                checkArray.Add(typeSchema.ContainsKey("procedure") && typeSchema.GetValue("procedure").Type == JTokenType.Object);
                foreach (var selectDescription in typeSchema.GetValue("procedure").ToObject<JObject>())
                {
                    var step = selectDescription.Value.ToObject<JObject>();

                    checkArray.AddRange(CheckContent(step));
                }
            }

            var result = checkArray.FindAll(value => value == false);

            if (result.Count > 0)
            {
                Console.WriteLine("[STARTUP] FATAL Wrong schemas");
                throw new Exception();
            }

            Environment.SetEnvironmentVariable("SCHEMA", schemasFile);
            Console.WriteLine("[STARTUP] Ok schemas");
        }
    }
}
