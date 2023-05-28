using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cesxhin.AnimeManga.Application.Schema
{
    public static class SchemaControl
    {
        public static void Check()
        {
            //check valid schema
            Console.WriteLine("[STARTUP] Check schemas");
            var schemasFile = System.IO.File.ReadAllText("schemas.json");
            var schemaTemplateFile = System.IO.File.ReadAllText("schema_template.json");

            var schema = JObject.Parse(schemasFile);
            var schemaTemplate = JObject.Parse(schemaTemplateFile);

            CheckRecursive(schema, schemaTemplate, "");
        }

        private static void CheckRecursive(JObject schema, JObject schemaTemplate, string path)
        {
            foreach (var field in schemaTemplate.ToObject<JObject>())
            {
                if (field.Key == "*" && path != "")
                {
                    foreach(var fieldSchema in schema.ToObject<JObject>())
                    {
                        CheckRules(schema.GetValue(fieldSchema.Key).ToObject<JObject>(), path+"/"+fieldSchema.Key);
                    }
                }
                else
                {
                    if (field.Key != "*" && !schema.ContainsKey(field.Key))
                    {
                        throw new Exception($"Missing this field: {field.Key}");
                    }

                    if (field.Value.Type == JTokenType.Object)
                    {
                        CheckRecursive(getKeyByField(schema, field.Key), schemaTemplate.GetValue(field.Key).ToObject<JObject>(), path + "/" + field.Key);
                    }
                    else
                    {
                        //check type
                        if ((string)field.Value == "string")
                        {
                            if (schema.GetValue(field.Key).Type != JTokenType.String)
                                throw new Exception($"Field {field.Key} isn't type String");

                            //check rules
                            if (path == "/*" && field.Key == "type")
                            {
                                if (schema.Value<string>(field.Key) != "video" && schema.Value<string>(field.Key) != "book")
                                    throw new Exception($"Field Type is incorrect, please select 'video' or 'book'");
                            }
                        }
                        else if ((string)field.Value == "boolean")
                        {
                            if (schema.GetValue(field.Key).Type != JTokenType.Boolean)
                                throw new Exception($"Field {field.Key} isn't type Boolean");
                        }
                        else if ((string)field.Value == "*")
                        {
                            CheckRules(schema.Value<JObject>(field.Key), path + "/" + field.Key);
                        }
                        else
                        {
                            throw new Exception($"Uknow type, error schema template of this field {field.Key}, path: {path}");
                        }
                    }
                }
            }
        }

        private static void CheckRules(JObject schema, string path)
        {
            if (!schema.ContainsKey("type"))
                throw new Exception("Missing field type for scapring");

            if (!schema.ContainsKey("path"))
                throw new Exception("Missing field path for scapring");

            if(schema.GetValue("type").Type != JTokenType.Array)
                throw new Exception("Field type for scapring isn't array");

            if (schema.GetValue("path").Type != JTokenType.Array)
                throw new Exception("Field path for scapring isn't array");

            var arrayType = schema.GetValue("type").ToObject<IEnumerable<string>>();
            var arrayPath = schema.GetValue("path").ToObject<IEnumerable<string>>();

            /*if (arrayType.Count() != arrayPath.Count())
                throw new Exception("Lenght between type and path is different, they should be equal");*/

            foreach (var type in arrayType)
            {
                switch (type)
                {
                    case "string":
                        if (schema.ContainsKey("child_nodes"))
                        {
                            if (schema.GetValue("child_nodes").Type != JTokenType.Integer)
                                throw new Exception($"Field child_nodes should be Interger");
                        }
                        break;
                    case "image":
                        if(!schema.ContainsKey("attributes"))
                            throw new Exception($"Field attributes missing");

                        if(schema.GetValue("attributes").Type != JTokenType.String)
                            throw new Exception($"Field attributes should be String");

                        if (schema.ContainsKey("download"))
                        {
                            if (schema.GetValue("download").Type != JTokenType.Boolean)
                                throw new Exception($"Field attributes should be Boolean");
                        }

                        break;

                    case "array":
                        if (schema.ContainsKey("attributes"))
                        {
                            if (schema.GetValue("attributes").Type != JTokenType.String)
                                throw new Exception($"Field attributes should be String");
                        }
                        break;

                    case "video/m3u8/script":
                        if (!schema.ContainsKey("startSearch"))
                            throw new Exception($"Field startSearch missing");

                        if (schema.GetValue("startSearch").Type != JTokenType.String)
                            throw new Exception($"Field startSearch should be String");

                        if (!schema.ContainsKey("endSearch"))
                            throw new Exception($"Field endSearch missing");

                        if (schema.GetValue("endSearch").Type != JTokenType.String)
                            throw new Exception($"Field endSearch should be String");
                        break;
                    case "number":
                        if (schema.ContainsKey("parse"))
                        {
                            if (schema.GetValue("parse").Type != JTokenType.String)
                                throw new Exception($"Field parse should be String");

                            if (schema.Value<string>("parse") != "number" && schema.Value<string>("parse") != "float")
                                throw new Exception($"Field parse is incorrect, please select 'number' or 'float'");
                        }

                        if (schema.ContainsKey("removeWords"))
                        {
                            if (schema.GetValue("removeWords").Type != JTokenType.String)
                                throw new Exception($"Field removeWords should be String");
                        }
                        break;
                    case "book/link":
                        if (!schema.ContainsKey("attributes"))
                            throw new Exception($"Field attributes missing");

                        if (schema.GetValue("attributes").Type != JTokenType.String)
                            throw new Exception($"Field attributes should be String");

                        if (schema.ContainsKey("addUrl"))
                        {
                            if (schema.GetValue("addUrl").Type != JTokenType.String)
                                throw new Exception($"Field addUrl should be String");
                        }
                        break;
                }
            }

            //collection of video or book
            if(path == "/*/book/collection" || path == "/*/video/collection")
            {
                if (!schema.ContainsKey("thread"))
                    throw new Exception($"Field thread missing");

                if (schema.GetValue("thread").Type != JTokenType.Boolean)
                    throw new Exception($"Field thread should be Boolean");

                if (schema.ContainsKey("reverseCount"))
                {
                    if (schema.GetValue("reverseCount").Type != JTokenType.Boolean)
                        throw new Exception($"Field reverseCount should be Boolean");
                }
            }
        }

        private static JObject getKeyByField(JObject schema, string field)
        {
            if (field == "*")
            {
                var name = ((JProperty)schema.First).Name.ToString();
                return schema.GetValue(name).ToObject<JObject>();
            }
            else
                return schema.GetValue(field).ToObject<JObject>();
        }
    }
}
