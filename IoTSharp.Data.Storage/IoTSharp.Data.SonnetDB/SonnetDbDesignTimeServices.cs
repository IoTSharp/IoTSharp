using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using SonnetDB.EntityFrameworkCore.Extensions;

namespace IoTSharp.Data.SonnetDB;

public sealed class SonnetDbDesignTimeServices : IDesignTimeServices
{
    public void ConfigureDesignTimeServices(IServiceCollection services)
    {
        services.AddEntityFrameworkSonnetDB();
        new EntityFrameworkRelationalDesignServicesBuilder(services)
            .TryAddCoreServices();
    }
}
