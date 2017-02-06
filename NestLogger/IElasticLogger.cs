namespace NESTLogger
{
    public interface IElasticLogger<in T> where T : ElasticModelBase
    {
        void Log(params T[] values);
    }
}