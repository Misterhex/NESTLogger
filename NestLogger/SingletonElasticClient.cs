using System;

namespace NESTLogger
{
    internal static class SingletonElasticClient
    {
        private static readonly TraceSource Tracer = new TraceSource(StringConstants.TraceSource);

        private static readonly Lazy<ElasticClient> LazyDefault = new Lazy<ElasticClient>(() =>
        {
            ConfigChecker.EnsureRequiredConfiguration();

            var hosts = ConfigurationManager.AppSettings[StringConstants.AppSettingHost];
            var uris = hosts.Split(',').Select(i => new Uri(i)).ToArray();
            var nodes = uris.Select(uri => new Node(uri)).ToArray();
            var pool = new SniffingConnectionPool(nodes);
            var connectionSetting = new ConnectionSettings(pool);

            connectionSetting.OnRequestCompleted(resp =>
            {
                if (!resp.Success)
                    Tracer.TraceInformation(resp.DebugInformation);
            });

            var client = new ElasticClient(connectionSetting);
            return client;

        }, true);

        public static ElasticClient Instance
        {
            get { return LazyDefault.Value; }
        }
    }
}