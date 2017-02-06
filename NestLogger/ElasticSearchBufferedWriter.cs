using System;
using System.Collections.Generic;

namespace NESTLogger
{
    internal class ElasticSearchBufferedWriter<T> : ObserverBase<IList<T>> where T : ElasticModelBase
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly TraceSource Tracer = new TraceSource(StringConstants.TraceSource);

        private readonly Func<ElasticClient> _elasticClientFactory;
        private readonly IIndexNameResolver _indexNameResolver;
        private readonly ITypeNameResolver _typeNameResolver;
        private readonly Action _throwOnMissingConfiguration;

        public ElasticSearchBufferedWriter(Func<ElasticClient> elasticClientFactory,
        IIndexNameResolver indexNameResolver, ITypeNameResolver typeNameResolver)
            : this(elasticClientFactory, indexNameResolver, typeNameResolver, ConfigChecker.EnsureRequiredConfiguration)
        {

        }

        public ElasticSearchBufferedWriter(Func<ElasticClient> elasticClientFactory, IIndexNameResolver indexNameResolver, ITypeNameResolver typeNameResolver, Action throwOnMissingConfiguration)
        {
            if (elasticClientFactory == null) throw new ArgumentNullException("elasticClientFactory");
            if (indexNameResolver == null) throw new ArgumentNullException("indexNameResolver");
            if (typeNameResolver == null) throw new ArgumentNullException("typeNameResolver");
            if (throwOnMissingConfiguration == null) throw new ArgumentNullException("throwOnMissingConfiguration");

            _elasticClientFactory = elasticClientFactory;
            _indexNameResolver = indexNameResolver;
            _typeNameResolver = typeNameResolver;
            _throwOnMissingConfiguration = throwOnMissingConfiguration;
        }

        protected override void OnNextCore(IList<T> values)
        {
            try
            {
                _throwOnMissingConfiguration();

                var elasticClient = _elasticClientFactory();
                Tracer.TraceEvent(TraceEventType.Verbose, 10, "received some buffered items ...");

                var indexName = _indexNameResolver.Resolve<T>();
                var typeName = _typeNameResolver.Resolve();

                Tracer.TraceEvent(TraceEventType.Verbose, 11, "{0} resolved index name and type name", indexName);
                Tracer.TraceEvent(TraceEventType.Verbose, 12, "{0} checking if index exist...", indexName);

                var indexExist = elasticClient.IndexExists(indexName);

                if (!indexExist.Exists)
                {
                    var createIndexResponse = elasticClient.CreateIndex(
                        new CreateIndexDescriptor(indexName).Mappings(ms =>
                                ms.Map<T>(m => m.AutoMap())));

                    if (!createIndexResponse.IsValid)
                    {
                        Tracer.TraceEvent(TraceEventType.Error, 400, "{0} create index failed. {1}", indexName, createIndexResponse.DebugInformation);
                        return;
                    }
                    Tracer.TraceEvent(TraceEventType.Verbose, 14, "{0} created index", indexName);
                }

                Tracer.TraceEvent(TraceEventType.Information, 15, "{0} going to bulk send...", indexName);

                var bulkResponse = elasticClient.Bulk(br => br.CreateMany(values.Where(i => i != null), (d, v) => d.Document(v).Index(indexName).Type(typeName)));
                if (!bulkResponse.IsValid)
                {
                    Tracer.TraceEvent(TraceEventType.Error, 400, "{0} bulk send failed. {1}", indexName, bulkResponse.DebugInformation);
                    return;
                }

                Tracer.TraceEvent(TraceEventType.Information, 17, "{0} bulk send success.", indexName);
            }
            catch (Exception ex)
            {
                Tracer.TraceEvent(TraceEventType.Error, 178, ex.Message);
                Tracer.TraceEvent(TraceEventType.Error, 179, ex.StackTrace);
            }

        }

        protected override void OnErrorCore(Exception error)
        {
            Tracer.TraceEvent(TraceEventType.Error, 180, error.Message);
        }

        protected override void OnCompletedCore()
        {
            Tracer.TraceEvent(TraceEventType.Error, 181, "writer should not stop.");
        }
    }
}