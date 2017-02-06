using System;
using Environment = NESTLogger.Environment;

namespace NESTLogger
{
    internal static class ConfigChecker
    {
        private static readonly string System = ConfigurationManager.AppSettings[StringConstants.AppSettingSystem];
        private static readonly string Environment = ConfigurationManager.AppSettings[StringConstants.AppSettingEnvironment];
        private static readonly string Hosts = ConfigurationManager.AppSettings[StringConstants.AppSettingHost];

        public static void EnsureRequiredConfiguration()
        {
            Environment outEnv;
            if (string.IsNullOrWhiteSpace(Environment) || !Enum.TryParse(Environment, true, out outEnv))
                throw new ConfigurationErrorsException(string.Format("'{0}' is required in appsettings. or a valid value was not provided. e.g dev, qat, staging or production", StringConstants.AppSettingEnvironment));

            if (string.IsNullOrWhiteSpace(System))
                throw new ConfigurationErrorsException(string.Format("'{0}' is required in appsettings.", StringConstants.AppSettingSystem));

            var configErrorMessage = string.Format("{0} was not set or invalid values. e.g 'http://server1:9200, http://server2:9200'", StringConstants.AppSettingHost);
            if (string.IsNullOrWhiteSpace(Hosts))
                throw new ConfigurationErrorsException(configErrorMessage);

            if (Hosts.Split(',').Any(i => !Uri.IsWellFormedUriString(i, UriKind.Absolute)))
                throw new ConfigurationErrorsException(configErrorMessage);

            var uris = Hosts.Split(',').Select(i => new Uri(i)).ToArray();
            if (!uris.Any())
                throw new ConfigurationErrorsException(configErrorMessage);
        }
    }
}
