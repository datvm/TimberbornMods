namespace System.Net;

public static class HttpHelper
{
    public static readonly JsonSerializerSettings SpecSerializerSettings = new()
    {
        Converters = [
            new ComponentSpecConverter(),
            new AssetRefConverter(),
            new UnityValuesConverter(),
            new LocalizedTextConverter(),
            //new TupleConverter(),
        ],
        ContractResolver = new BlueprintContractResolver(),
    };

    extension(ComponentSpec spec)
    {
        public JObject Serialize() => JObject.Parse(JsonConvert.SerializeObject(spec, SpecSerializerSettings));
    }

    extension(HttpListenerContext context)
    {

        public void AddCorsHeaders()
        {
            var h = context.Response.Headers;
            h.Add("Access-Control-Allow-Origin", "*");
            h.Add("Access-Control-Allow-Methods", "*");
            h.Add("Access-Control-Allow-Headers", "*");
        }

        public async Task<bool> HandleAsync(Func<Task> handler)
        {
            try
            {
                await handler();
                await context.WriteText("", 204);
            }
            catch (StatusCodeException ex)
            {
                await context.WriteText(ex.Content, ex.StatusCode);
            }
            catch (Exception ex)
            {
                await context.WriteText(ex.ToString(), 500);
            }

            return true;
        }

        public async Task<bool> HandleAsync<T>(Func<Task<T>> handler)
        {
			try
			{
                var result = await handler();

                if (result is string str)
                {
                    await context.WriteText(str, 200);
                }
                else if (result is byte[] bytes)
                {
                    context.Response.StatusCode = 200;
                    await context.Write("application/octet-stream", bytes);
                }
                else
                {
                    var json = JsonConvert.SerializeObject(result, SpecSerializerSettings);
                    await context.Write("application/json", Encoding.UTF8.GetBytes(json));
                }
			}
			catch (StatusCodeException ex)
			{
                await context.WriteText(ex.Content, ex.StatusCode);
			}
            catch (Exception ex)
            {
                await context.WriteText(ex.ToString(), 500);
            }

            return true;
        }

        public async Task<string> ReadRequestBodyAsync()
        {
            using var reader = new StreamReader(context.Request.InputStream);
            return await reader.ReadToEndAsync();
        }

    }

    extension (NameValueCollection query)
    {
        public bool HasSwitch(string name) => query.Get(name) is not null;
    }

}
