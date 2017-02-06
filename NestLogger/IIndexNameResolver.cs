namespace NESTLogger
{
    internal interface IIndexNameResolver
    {
        string Resolve<T>() where T : ElasticModelBase;
    }
}