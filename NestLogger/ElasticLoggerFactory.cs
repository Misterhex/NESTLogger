using System;
using System.Collections.Generic;

namespace NESTLogger
{
    public static class ElasticLoggerFactory
    {
        private static readonly TraceSource Tracer = new TraceSource(StringConstants.TraceSource);

        private static readonly object Gate = new object();

        private static readonly Dictionary<string, object> Map = new Dictionary<string, object>();

        private static readonly TimeSpan BufferTimeSpan = TimeSpan.FromMinutes(1);

        private const int BufferCount = 1000;

        /// <summary>
        /// IElasticLogger should be static, shared and reused.
        /// GetLogger will create an instance of IElasticLogger, or return a cached reference if exist.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IElasticLogger<T> GetLogger<T>() where T : ElasticModelBase
        {
            return GetLogger<T>(Scheduler.Default);
        }

        /// <summary>
        /// IElasticLogger should be static, shared and reused.
        /// GetLogger will create an instance of IElasticLogger, or return a cached reference if exist. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bufferScheduler">Allow a custom buffer scheduler for testing purposes. e.g. passing in an instance of TestScheduler.</param>
        /// <returns></returns>
        public static IElasticLogger<T> GetLogger<T>(IScheduler bufferScheduler) where T : ElasticModelBase
        {
            if (bufferScheduler == null) throw new ArgumentNullException("bufferScheduler");

            if (!IsLoggingEnabled())
                return new MockElasticLogger<T>();

            var fqn = typeof(T).FullName;

            Tracer.TraceEvent(TraceEventType.Verbose, 334, "GetLogger for {0} invoked.", fqn);

            // ReSharper disable once InconsistentlySynchronizedField
            if (Map.ContainsKey(fqn))
            {
                Tracer.TraceEvent(TraceEventType.Verbose, 335, "{0} found in cache, returning cached reference.", fqn);
                // ReSharper disable once InconsistentlySynchronizedField
                return Map[fqn] as IElasticLogger<T>;
            }

            lock (Gate)
            {
                if (Map.ContainsKey(fqn))
                    return Map[fqn] as IElasticLogger<T>;

                Tracer.TraceEvent(TraceEventType.Verbose, 336, "{0} not found in cache, constructing logger...", fqn);

                var elasticLogger = CreateImpl<T>(bufferScheduler);

                Map[fqn] = elasticLogger;

                return elasticLogger;
            }
        }

        private static IElasticLogger<T> CreateImpl<T>(IScheduler bufferScheduler) where T : ElasticModelBase
        {
            var subject = new Subject<T>();

            var fqn = typeof(T).FullName;

            subject.Buffer(BufferTimeSpan, BufferCount, bufferScheduler)
                .Do(_ => Tracer.TraceEvent(TraceEventType.Verbose, 111, "{0} buffering ...", fqn))
                .Where(i => i != null)
                .Where(i => i.Count > 0)
                .Do(i => Tracer.TraceEvent(TraceEventType.Verbose, 112, "{0} buffered {1} items ...", fqn, i.Count))
                .ObserveOn(TaskPoolScheduler.Default)
                .Subscribe(new ElasticSearchBufferedWriter<T>(() => SingletonElasticClient.Instance,
                    new IndexNameResolver(), new TypeNameResolver()));

            return new ElasticLogger<T>(subject);
        }

        private static bool IsLoggingEnabled()
        {
            var isEnabled = true;
            try
            {
                isEnabled = bool.Parse(ConfigurationManager.AppSettings[StringConstants.AppSettingIsLoggingEnabled]);
            }
            catch
            {
                isEnabled = true;
            }
            finally
            {
                Tracer.TraceEvent(TraceEventType.Verbose, 223,
                    string.Format("elasticsearch logging is enabled : {0}", isEnabled));
            }
            return isEnabled;
        }
    }
}
