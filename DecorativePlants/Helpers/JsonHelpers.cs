namespace Newtonsoft.Json.Linq;

public static class JsonHelpers
{

    extension(JObject j)
    {

        public JObject GetProperty(string property) => j.GetProperty<JObject>(property);
        public T GetProperty<T>(string property)
        {
            var prop = j[property]
                ?? throw new Exception($"{property} not found");

            return prop.Value<T>()!;
        }

        public JObject CopyProperty(JObject target, string property)
        {
            var value = j.GetProperty(property);
            return (JObject)(target[property] = value.DeepClone());
        }

        public T? GetProperty<T>(params string[] path)
        {
            var curr = j;

            for (int i = 0; i < path.Length - 1; i++)
            {
                curr = curr.GetProperty(path[i]);
            }

            return curr.GetProperty<T>(path[^1]);
        }

        public T? FindChild<T>(string name)
        {
            Queue<JObject> queue = [];
            queue.Enqueue(j);

            while (queue.Count > 0)
            {
                var curr = queue.Dequeue();

                foreach (var prop in curr.Properties())
                {
                    if (prop.Name == name)
                    {
                        return prop.Value.Value<T>();
                    }

                    if (prop.Value.Type == JTokenType.Object)
                    {
                        queue.Enqueue(prop.Value.Value<JObject>()!);
                    }
                }
            }

            return default;
        }

    }

}
