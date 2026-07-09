using BenchmarkDotNet.Running;
using IoTSharp.Benchmarks.Load;
using System;
using System.Linq;

if (args.Any(arg => string.Equals(arg, "--coap-load", StringComparison.OrdinalIgnoreCase)))
{
    return await CoapLoadRunner.RunAsync(args);
}

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
return 0;
