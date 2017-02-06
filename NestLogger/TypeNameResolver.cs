using System;

namespace NESTLogger
{
    internal class TypeNameResolver : ITypeNameResolver
    {
        private static readonly string System = ConfigurationManager.AppSettings[StringConstants.AppSettingSystem];
        private static readonly string Environment = ConfigurationManager.AppSettings[StringConstants.AppSettingEnvironment];

        public string Resolve()
        {
            Environment outEnv;
            if (string.IsNullOrWhiteSpace(Environment) || !Enum.TryParse(Environment, true, out outEnv))
                throw new ConfigurationErrorsException(string.Format("'{0}' is required in appsettings.", StringConstants.AppSettingEnvironment));

            if (string.IsNullOrWhiteSpace(System))
                throw new ConfigurationErrorsException(string.Format("'{0}' is required in appsettings.", StringConstants.AppSettingSystem));

            return string.Format("{0}-{1}", Environment, System).ToLowerInvariant();
        }
    }
}