using IoTSharp.Data.Configurations;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Sharding;
using ShardingCore.Sharding.Abstractions;

namespace IoTSharp.Data.Shardings
{
    public class ShardingDbContext:AbstractShardingDbContext,IShardingTableDbContext
    {
        public ShardingDbContext(DbContextOptions<ShardingDbContext> options) : base(options)
        {
        }

        public IRouteTail RouteTail { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new TelemetryDataConfiguration());
            
        }
    }
}