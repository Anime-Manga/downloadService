using Cesxhin.AnimeManga.Application.Exceptions;
using Cesxhin.AnimeManga.Application.Interfaces.Repositories;
using Cesxhin.AnimeManga.Application.NlogManager;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cesxhin.AnimeManga.Persistence.Repositories
{
    public class DescriptionRepository : IDescriptionRepository
    {
        //log
        private readonly NLogConsole _logger = new(LogManager.GetCurrentClassLogger());

        //env
        readonly string _connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_MONGO");
        readonly string _nameDatabase = Environment.GetEnvironmentVariable("NAME_DATABASE_MONGO");
        readonly JObject _schema = JObject.Parse(Environment.GetEnvironmentVariable("SCHEMA"));

        private static string RemoveObjectId(string result)
        {
            var regex = new Regex(Regex.Escape("ObjectId("));
            var partOne = regex.Replace(result, "", 1);


            regex = new Regex(Regex.Escape(")"));
            return regex.Replace(partOne, "", 1);

        }

        private string GetNameTable(string nameCfg)
        {
            if (_schema.ContainsKey(nameCfg))
            {
                return _schema.GetValue(nameCfg).ToObject<JObject>().GetValue("name").ToString();
            }

            return null;
        }

        public async Task<int> DeleteNameAsync(string nameCfg, string id)
        {
            var client = new MongoClient(_connectionString);

            int rs = 0;
            try
            {
                var database = client.GetDatabase(_nameDatabase);
                var collection = database.GetCollection<BsonDocument>("description_" + GetNameTable(nameCfg));
                var deleteFilter = Builders<BsonDocument>.Filter.Eq("name_id", id);
                var result = await collection.DeleteOneAsync(deleteFilter);

                rs = (int)result.DeletedCount;
            }
            catch (Exception ex)
            {
                client.Cluster.Dispose();
                _logger.Error($"Failed DeleteNameAsync, details error: {ex.Message}");
                throw new ApiGenericException(ex.Message);
            }

            if (rs > 0)
                return rs;
            else
                throw new ApiNotFoundException("Not found DeleteNameAsync");
        }

        public Task<List<JObject>> GetMostNameByNameAsync(string nameCfg, string name)
        {
            throw new NotImplementedException();
        }

        public async Task<List<JObject>> GetNameAllAsync(string nameCfg)
        {
            var client = new MongoClient(_connectionString);

            List<BsonDocument> list;
            try
            {
                var database = client.GetDatabase(_nameDatabase);
                var collection = database.GetCollection<BsonDocument>("description_" + GetNameTable(nameCfg));
                list = collection.Find(Builders<BsonDocument>.Filter.Empty).ToList();
            }
            catch (Exception ex)
            {
                client.Cluster.Dispose();
                _logger.Error($"Failed DeleteNameAsync, details error: {ex.Message}");
                throw new ApiGenericException(ex.Message);
            }

            if (list != null && list.Any())
            {
                var listObject = new List<JObject>();

                foreach (var item in list)
                {
                    listObject.Add(JObject.Parse(RemoveObjectId(item.ToString())));
                }

                return listObject;
            }
            else
                throw new ApiNotFoundException("Not found GetNameAllAsync");
        }

        public async Task<JObject> GetNameByNameAsync(string nameCfg, string name)
        {
            var client = new MongoClient(_connectionString);

            BsonDocument result;
            try
            {
                var database = client.GetDatabase(_nameDatabase);
                var collection = database.GetCollection<BsonDocument>("description_" + GetNameTable(nameCfg));
                var findFilter = Builders<BsonDocument>.Filter.Eq("name_id", name);
                result = collection.Find(findFilter).FirstOrDefault();
            }
            catch (Exception ex)
            {
                client.Cluster.Dispose();
                _logger.Error($"Failed DeleteNameAsync, details error: {ex.Message}");
                throw new ApiGenericException(ex.Message);
            }

            if (result != null && result.Any())
                return JObject.Parse(RemoveObjectId(result.ToString()));
            else
                throw new ApiNotFoundException("Not found GetNameByNameAsync");
        }

        public async Task<JObject> InsertNameAsync(string nameCfg, JObject description)
        {
            var client = new MongoClient(_connectionString);
            try
            {
                var database = client.GetDatabase(_nameDatabase);
                var collection = database.GetCollection<BsonDocument>("description_" + GetNameTable(nameCfg));
                await collection.InsertOneAsync(BsonDocument.Parse(description.ToString()));

                return description;
            }
            catch (Exception ex)
            {
                client.Cluster.Dispose();
                _logger.Error($"Failed DeleteNameAsync, details error: {ex.Message}");
                throw new ApiGenericException(ex.Message);
            }
        }
    }
}
