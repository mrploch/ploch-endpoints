using Xunit;

// One test swaps CultureInfo.CurrentCulture to prove culture-invariant parsing.
// Culture is ambient state, so tests in this assembly run sequentially to keep that swap isolated.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
