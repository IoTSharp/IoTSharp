using EFCore.Sharding;

namespace IoTSharp
{
    public class ShardingSetting
    {
        public DatabaseType DatabaseType { get; set; } = DatabaseType.PostgreSql;
        public ExpandByDateMode ExpandByDateMode { get; set; } = ExpandByDateMode.PerMonth;
    }
}
