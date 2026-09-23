using IntegrationTests.Infrastructure;
using Xunit.Sdk;
using Xunit.v3;

// One database container for the whole test run; tests share it and reset it, so they run sequentially.
[assembly: AssemblyFixture(typeof(DatabaseFixture))]
[assembly: Parallelization(Mode = ParallelMode.None)]
