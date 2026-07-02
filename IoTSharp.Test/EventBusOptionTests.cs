using System;
using System.Threading.Tasks;
using IoTSharp.Contracts;
using IoTSharp.EventBus;
using Xunit;

namespace IoTSharp.Test;

public class EventBusOptionTests
{
    [Fact]
    public async Task RunRules_WhenNoHandlerRegistered_CompletesWithoutThrowing()
    {
        var option = new EventBusOption();

        await option.RunRules(Guid.NewGuid(), new object(), EventType.Telemetry);
    }
}
