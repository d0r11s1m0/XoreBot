using Newtonsoft.Json;

namespace DiscordBot.Config;

public class JsonReader
{
    private readonly string _path;

    public JsonReader(string path = "config.json")
    {
        _path = path;
    }

    public async Task<AppConfig> ReadJsonAsync()
    {
        using StreamReader sr = new StreamReader(_path);
        string json = await sr.ReadToEndAsync();
        
        var config = JsonConvert.DeserializeObject<AppConfig>(json);
        
        if (config == null)
            throw new InvalidDataException("Не удалось прочитать конфигурацию из файла");

        return config;
    }
}