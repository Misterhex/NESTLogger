# NESTLogger

### Overview
Build some even higher abstraction on top of NEST and elasticsearch-net to automatically group indices by [Index Per Time frame](https://www.elastic.co/guide/en/elasticsearch/guide/current/time-based.html) using a common pattern that is understood by curator. Buffer the data using reactive extensions.

### Features
- buffer and bulk send to elasticsearch.
- standardization of indices name using time-based format for easy clean up.
- standardize some default json fields for searching. (e.g metadata.env, metadata.system, metadata.type)

### Installation
WIP

### Configuration 
The following appsettings are required.     
**elasticsearch:env** and **elasticsearch:system** are used to form the indices name.
```
<appSettings>
    <add key="elasticsearch:hosts" value="http://node1:9200,http://node2:9200,http://node3:9200"/>
    <add key="elasticsearch:env" value="dev"/> 
    <add key="elasticsearch:system" value="system"/>
</appSettings>
```

### Usage
```
IElasticLogger<TestLog> logger = ElasticLoggerFactory.GetLogger<TestLog>();
logger.Log(new TestLog());
```

### Strings
To analze string as full text value tag the property with the following attribute. This will prevent the text from being tokenized in elasticsearch. 
```
[String(Analyzer = "keyword")]
public string SubscriberToken { get; set; }
```

### Enums
In order to analyze enum value as string and keyword in kibana. 
Tag the enum property with the following attributes:
```
[String(Analyzer = "keyword")]
[JsonConverter(typeof(StringEnumConverter))]
public EnumType Type { get; set; }
```

### Disabling
```
<appSettings>
    <add key="elasticsearch:enabled" value="false" />
</appSettings>
```

### capture diagnostic using nlog
```
<system.diagnostics>
    <sources>
      <source name="NESTLogger" switchValue="All">
        <listeners>
          <add name="nlog"/>
        </listeners>
      </source>
    </sources>
    <sharedListeners>
      <add name="nlog" type="NLog.NLogTraceListener, NLog" />
    </sharedListeners>
</system.diagnostics>

<nlog>
    <targets>
      <target name="console" type="Console" layout="${longdate} ${windows-identity} ${message}" />
    </targets>
    <rules>
      <logger name="NESTLogger" writeTo="console" />
    </rules>
</nlog>
```
