using Cesxhin.AnimeManga.Application.Interfaces.Repositories;
using Cesxhin.AnimeManga.Application.NlogManager;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        readonly string _nameTable = "test";

        public async Task<int> DeleteNameAsync(string id)
        {
            var client = new MongoClient(_connectionString);
            try
            {
                var database = client.GetDatabase(_nameTable);
                var collection = database.GetCollection<BsonDocument>("description_" + _nameTable);
                var deleteFilter = Builders<BsonDocument>.Filter.Eq("name_id", id);
                var result = await collection.DeleteOneAsync(deleteFilter);

                return (int)result.DeletedCount;
            }
            catch(Exception e)
            {
                _logger.Error(e);
                client.Cluster.Dispose();
                return 0;
            }
        }

        public Task<List<JObject>> GetMostNameByNameAsync(string name)
        {
            throw new NotImplementedException();
        }

        public async Task<List<JObject>> GetNameAllAsync()
        {
            var client = new MongoClient(_connectionString);
            try
            {
                var database = client.GetDatabase(_nameTable);
                var collection = database.GetCollection<BsonDocument>("description_" + _nameTable);
                var list = collection.Find(new BsonDocument()).ToList();

                if (list == null)
                    return null;

                var listObject = new List<JObject>();

                foreach(var item in list)
                {
                    listObject.Add(JObject.Parse(removeObjectId(item.ToString())));
                }

                return listObject;
            }
            catch (Exception e)
            {
                _logger.Error(e);
                client.Cluster.Dispose();
                return null;
            }
        }

        public async Task<JObject> GetNameByNameAsync(string name)
        {
            var client = new MongoClient(_connectionString);
            try
            {
                var database = client.GetDatabase(_nameTable);
                var collection = database.GetCollection<BsonDocument>("description_" + _nameTable);
                var findFilter = Builders<BsonDocument>.Filter.Eq("name_id", name);
                var result = collection.Find(findFilter).FirstOrDefault();

                if (result == null)
                    return null;
                
                return JObject.Parse(removeObjectId(result.ToString()));
            }
            catch (Exception e)
            {
                _logger.Error(e);
                client.Cluster.Dispose();
                return null;
            }
        }

        public async Task<JObject> InsertNameAsync(JObject description)
        {
            var client = new MongoClient(_connectionString);
            try
            {
                var database = client.GetDatabase(_nameTable);
                var collection = database.GetCollection<BsonDocument>("description_" + _nameTable);
                await collection.InsertOneAsync(BsonDocument.Parse(description.ToString()));

                return description;
            }
            catch(Exception e)
            {
                _logger.Error(e);
                client.Cluster.Dispose();
                return null;
            }
        }

        public string removeObjectId(string result)
        {
            var regex = new Regex(Regex.Escape("ObjectId("));
            var partOne = regex.Replace(result, "", 1);


            regex = new Regex(Regex.Escape(")"));
            return regex.Replace(partOne, "", 1);

        }
    }
}
