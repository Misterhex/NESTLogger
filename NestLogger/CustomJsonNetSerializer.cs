using System;
using System.Collections.Generic;

namespace NESTLogger
{
    internal class CustomJsonNetSerializer : JsonNetSerializer
    {
        public CustomJsonNetSerializer(IConnectionSettingsValues connectionSettingsValues) : base(connectionSettingsValues)
        { }

        protected override IList<Func<Type, JsonConverter>> ContractConverters
        {
            get
            {
                return new List<Func<Type, JsonConverter>>
                {
                    type =>
                    {
                        if (type.IsEnum ||
                            (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                             type.GetGenericArguments().First().IsEnum))
                        {
                            return new StringEnumConverter();
                        }
                        return null;
                    }
                };
            }
        }

    }
}