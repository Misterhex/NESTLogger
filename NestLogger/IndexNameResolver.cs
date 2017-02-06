using System;

namespace NESTLogger
{
    internal class IndexNameResolver : IIndexNameResolver
    {
        private static readonly string System = ConfigurationManager.AppSettings[StringConstants.AppSettingSystem];
        private static readonly string Environment = ConfigurationManager.AppSettings[StringConstants.AppSettingEnvironment];

        public string Resolve<T>() where T : ElasticModelBase
        {
            Environment outEnv;
            if (string.IsNullOrWhiteSpace(Environment) || !Enum.TryParse(Environment, true, out outEnv))
                throw new ConfigurationErrorsException(string.Format("'{0}' is required in appsettings.", StringConstants.AppSettingEnvironment));

            if (string.IsNullOrWhiteSpace(System))
                throw new ConfigurationErrorsException(string.Format("'{0}' is required in appsettings.", StringConstants.AppSettingSystem));

            var utcNow = DateTimeOffset.UtcNow;
            var fqn = typeof(T).FullName;
            var indexName =
                string.Format("{0}-{1}-{2}-{3:yyyy.MM.dd}", Environment, System, fqn, utcNow).ToLowerInvariant();
            return indexName;
        }
    }
}