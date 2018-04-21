using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using OfferinoApi.Controllers;
using OfferinoApi.Models;
using OfferinoApi.Services;
using Xunit;

namespace OfferinoApiTests.UnitTests
{
    public class OfferServiceTests
    {   
        IOptions<AppSettings> _settings;
        MongoDbContext _db;
        MongoClient _client;
        IMongoDatabase _testDb;

        public OfferServiceTests(){
            SetupTestMongoDatabase();
        }
        
        [Fact]
        public async void GetAllOffers_Return_All_Offers()
        {
            var offerCollection = _testDb.GetCollection<Offer>("Offer");

            var sample = new List<Offer>{
                new Offer{
                    Owner = "TestOwner1",
                    Title = "TestTitle1"
                },
                new Offer{
                    Owner = "TestOwner2",
                    Title = "TestTitle2"
                },
                new Offer{
                    Owner = "TestOwner3",
                    Title = "TestTitle3"
                }
            };
            
            await offerCollection.InsertManyAsync(sample);

            var cache = new Mock<ICacheService>();
            cache
                .Setup(x => x.GetAsync<IEnumerable<Offer>>($"{nameof(OfferService)}-{nameof(OfferService.GetAllOffers)}"))
                .ReturnsAsync((IEnumerable<Offer>)null);

            var offerService = new OfferService(_settings, cache.Object, _db);
            var offers = await offerService.GetAllOffers();
            
            Assert.Equal(sample.Count(), offers.Count());
        }

        void SetupTestMongoDatabase(){
            var appSettings = new AppSettings();
            _settings = Options.Create(appSettings);
            _settings.Value.Mongo.ConnectionString = "mongodb://localhost:27017";
            _settings.Value.Mongo.Database = "TestDb";

            _db = new MongoDbContext(_settings);
            _client = new MongoClient();
            _testDb = _client.GetDatabase("TestDb");
            _testDb.DropCollection("Offer");
        }

        
    }
}
