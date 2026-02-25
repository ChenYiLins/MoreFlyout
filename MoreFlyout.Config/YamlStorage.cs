using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MoreFlyout.Config
{
    public class YamlStorage<T>(string filePath)
        where T : new()
    {
        private readonly ISerializer _serializer = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
        private readonly IDeserializer _deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).IgnoreUnmatchedProperties().Build();

        public T Load()
        {
            if (!File.Exists(filePath))
            {
                var newData = new T();
                Save(newData);
                return newData;
            }

            var yaml = File.ReadAllText(filePath);
            return _deserializer.Deserialize<T>(yaml);
        }

        public void Save(T data)
        {
            var yaml = _serializer.Serialize(data);
            File.WriteAllText(filePath, yaml);
        }
    }
}
