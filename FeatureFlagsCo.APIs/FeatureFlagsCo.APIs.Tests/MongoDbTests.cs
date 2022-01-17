using System.Linq;
using System.Threading.Tasks;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Services.MongoDb;
using FeatureFlags.APIs.Tests.TestBase;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Shouldly;
using Xunit;

namespace FeatureFlags.APIs.Tests
{
    public class MongoDbTests : IClassFixture<MongoClientAccessorFixture>
    {
        private readonly MongoDbPersist _mongoDb;
        
        public MongoDbTests(MongoClientAccessorFixture accessorFixture)
        {
            var accessor = accessorFixture.ClientAccessor;
            
            _mongoDb = new MongoDbPersist(accessor);
        }

        [Fact]
        public async Task Should_Get_Document()
        {
            var accounts = _mongoDb.QueryableOf<AccountV2>();

            var result = await accounts.FirstOrDefaultAsync(x => x.Id == 1);

            result.ShouldNotBeNull();
        }

        [Fact]
        public async Task Should_Create_Update_Document()
        {
            var environments = _mongoDb.CollectionOf<EnvironmentV2>();
            
            const int projectId = 1;
            var prodEnv = new EnvironmentV2(projectId, "Production", "production");
            var testEnv = new EnvironmentV2(projectId, "Test", "test");

            await environments.InsertOneAsync(prodEnv);
            await environments.InsertOneAsync(testEnv);

            const int accountId = 1;
            prodEnv.GenerateSecrets(accountId);
            testEnv.GenerateSecrets(accountId);
            
            await environments.FindOneAndReplaceAsync(env => env.Id == prodEnv.Id, prodEnv);
            await environments.FindOneAndReplaceAsync(env => env.Id == testEnv.Id, testEnv);
        }

        [Fact]
        public async Task Should_Do_Join_Query_Using_Linq()
        {
            var projects = _mongoDb.QueryableOf<ProjectV2>().Where(x => x.AccountId == 1);
            var envs = _mongoDb.QueryableOf<EnvironmentV2>();

            var query =
                from project in projects
                join env in envs on project.Id equals env.ProjectId into allEnvs
                orderby project.Id descending
                select new ProjectEnvironmentV2
                {
                    Id = project.Id,
                    Name = project.Name,
                    Environments = allEnvs
                };

            var result = await query.ToListAsync();
            result.Count.ShouldBe(2);
        }

        [Fact]
        public async Task Should_Do_Anything()
        {
            var tables = new[] {"Accounts", "AccountUsers", "Projects", "ProjectUsers", "Environments" };
            
            var counters = tables.Select(table => new CollectionIdCounter(table)).ToList();

            await _mongoDb.CollectionOf<CollectionIdCounter>().InsertManyAsync(counters);
        }
    }
}