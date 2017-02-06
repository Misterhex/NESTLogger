namespace NESTLogger
{
    internal class MockElasticLogger<T> : IElasticLogger<T> where T : ElasticModelBase
    {
        public void Log(params T[] values)
        {
        }
    }
}