using Demo.Common;
using EFCore.Sharding;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Demo.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            string conString = "DataSource=db.db";
            services.UseEFCoreSharding(config =>
            {
                config.AddAbsDb(DatabaseType.SQLite)
                    .AddPhysicDb(ReadWriteType.Read | ReadWriteType.Write, conString)
                    .AddPhysicDbGroup()
                    .AddPhysicTable<Base_UnitTest>("Base_UnitTest_0")
                    .AddPhysicTable<Base_UnitTest>("Base_UnitTest_1")
                    .AddPhysicTable<Base_UnitTest>("Base_UnitTest_2")
                    .SetHashModShardingRule<Base_UnitTest>(nameof(Base_UnitTest.Id), 3)
                    .AddPhysicTable<Base_UnitTest_LongKey>("Base_UnitTest_LongKey_0")
                    .AddPhysicTable<Base_UnitTest_LongKey>("Base_UnitTest_LongKey_1")
                    .AddPhysicTable<Base_UnitTest_LongKey>("Base_UnitTest_LongKey_2")
                    .SetHashModShardingRule<Base_UnitTest_LongKey>(nameof(Base_UnitTest_LongKey.Id), 3);
            });
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
