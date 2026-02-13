using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using System.Reflection;
using System.Windows.Controls;

namespace Minimal.Mvvm.Wpf.Benchmarks
{

    #region Sample views for discovery
    // Derive from WPF controls to avoid being filtered out by your ShouldSkipType logic.
    public sealed class SampleViewA : UserControl { }
    public sealed class SampleViewB : ContentControl { }
    #endregion

    #region Probing locators
    public class ProbeViewLocator : ViewLocator
    {
        public Type? Probe(string name) => base.GetViewType(name);
    }

    public sealed class NaiveViewLocator : ProbeViewLocator
    {
        // No assembly/type caches; no skip filters; full scan on each call.
        protected override IEnumerable<Assembly> GetAssemblies()
        {
            // Keep the same set as base by default
            return AppDomain.CurrentDomain.GetAssemblies();
        }

        protected override IEnumerable<Type> GetTypes()
        {
            foreach (var asm in GetAssemblies())
            {
                Type?[] types;
                try
                {
                    types = asm.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types;
                }

                foreach (var t in types)
                {
                    if (t == null || t.IsAbstract || t.IsInterface || t.IsGenericTypeDefinition)
                        continue;
                    yield return t;
                }
            }
        }
    }
    #endregion


    [MemoryDiagnoser]
    [RankColumn]
    [HideColumns(Column.StdDev, Column.Median)]
    public class ViewLocatorBench
    {
        private readonly ProbeViewLocator _cached = new();
        private readonly NaiveViewLocator _naive = new();

        private readonly string _shortNameA = typeof(SampleViewA).Name;         // e.g., "SampleViewA"
        private readonly string _fullNameA = typeof(SampleViewA).FullName!;    // e.g., "MyNamespace.SampleViewA"
        private const string MissingName = "DefinitelyMissingViewType_42C1C6";

        [GlobalSetup]
        public void Setup()
        {
            // 1) Register one type to measure "registered" path (dictionary hit)
            _cached.RegisterType(_shortNameA, typeof(SampleViewA));

            // 2) Warm caches for name/fullname hits
            _ = _cached.Probe(_shortNameA);
            _ = _cached.Probe(_fullNameA);
        }

        [IterationSetup(Target = nameof(ColdScan_Found))]
        public void PrepColdFound() => _cached.ClearCache();

        [IterationSetup(Target = nameof(ColdScan_Miss))]
        public void PrepColdMiss() => _cached.ClearCache();

        // --- Cached locator paths ---

        [Benchmark(Baseline = true, Description = "Registered hit (dictionary)")]
        public Type? Registered_Hit() => _cached.Probe(_shortNameA);

        [Benchmark(Description = "Name cache hit")]
        public Type? NameCache_Hit() => _cached.Probe(_shortNameA);

        [Benchmark(Description = "FullName cache hit")]
        public Type? FullNameCache_Hit() => _cached.Probe(_fullNameA);

        [Benchmark(Description = "Cold scan (found)")]
        public Type? ColdScan_Found() => _cached.Probe(_shortNameA);

        [Benchmark(Description = "Cold scan (miss)")]
        public Type? ColdScan_Miss() => _cached.Probe(MissingName);

        // --- Naive locator paths (no caches, no filters) ---

        [Benchmark(Description = "Naive scan (found)")]
        public Type? NaiveScan_Found() => _naive.Probe(_shortNameA);

        [Benchmark(Description = "Naive scan (miss)")]
        public Type? NaiveScan_Miss() => _naive.Probe(MissingName);
    }

}
