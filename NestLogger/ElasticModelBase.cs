using System;

namespace NESTLogger
{
    public abstract class ElasticModelBase
    {
        public Metadata Metadata { get; private set; }

        protected ElasticModelBase()
        {
            ConfigChecker.EnsureRequiredConfiguration();

            Metadata = new Metadata()
            {
                CreatedTimestamp = DateTimeOffset.UtcNow,
                Env = ConfigurationManager.AppSettings[StringConstants.AppSettingEnvironment].ToLowerInvariant(),
                System = ConfigurationManager.AppSettings[StringConstants.AppSettingSystem].ToLowerInvariant(),
                Type = new TypeNameResolver().Resolve(),
            };
        }

    }
}