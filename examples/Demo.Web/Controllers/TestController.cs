using EFCore.Sharding;
using EFCore.Sharding.Tests;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Demo.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        readonly IShardingRepository _shardingRepository;
        public TestController(IShardingRepository shardingRepository)
        {
            _shardingRepository = shardingRepository;
        }

        [HttpGet]
        public async Task<string> Get()
        {
            List<Base_UnitTest> insertList = new List<Base_UnitTest>();
            for (int i = 0; i < 100; i++)
            {
                insertList.Add(new Base_UnitTest
                {
                    Id = Guid.NewGuid().ToString(),
                    Age = i,
                    CreateTime = DateTime.Now,
                    UserName = Guid.NewGuid().ToString()
                });
            }

            await _shardingRepository.InsertAsync(insertList);

            return "成功";
        }
    }
}
