using Xunit;

// Тесты используют глобальные env-vars (ConnectionStrings__Default),
// поэтому классы нельзя запускать параллельно.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
