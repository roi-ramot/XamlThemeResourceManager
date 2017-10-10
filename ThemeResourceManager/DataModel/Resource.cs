
namespace ThemeResourceManager.DataModel
{
    public class Resource
    {
        public Resource(string value, string key, ResourceType type)
        {
            Value = value;
            Key = key;
            Type = type;
        }

        public Resource(Resource resource)
        {
            Key = resource.Key;
            Type = resource.Type;
            Value = resource.Value;
        }

        public string Key { get; set; }
        public string Value { get; set; }
        public ResourceType Type { get; set; }
    }
}
