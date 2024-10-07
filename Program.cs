// See https://aka.ms/new-console-template for more information

using System.Net.Http.Json;
using System.Text.Json;

while (true)
{
    using HttpClient client = new()
    {
        BaseAddress = new Uri("http://10.56.78.4"),
        Timeout = TimeSpan.FromSeconds(1),
    };
    try
    {
        var status = await client.GetFromJsonAsync<ApRadioStatus>("status");

        if (status is not null)
        {
            Console.WriteLine(status);

            if (status.Status == "ACTIVE"
                && status.StationStatuses.TryGetValue("red1", out var Red1Status)
                && Red1Status.Ssid is not "5678")
            {
                Console.WriteLine("Needs Update");
                ApConfiguration newConfig = new()
                {
                    Channel = 45,
                    ChannelBandwidth = "40MHz",
                    RedVlans = "10_20_30",
                    BlueVlans = "40_50_60",
                    StationConfigurations = new()
                    {
                        ["red1"] = new()
                        {
                            Ssid = "5678",
                            WpaKey = "12345678",
                        },
                    },
                    SysLogIpAddress = "10.0.100.40",
                };

                var writeStatus = await client.PostAsJsonAsync("configuration", newConfig, new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                });

                Console.WriteLine(writeStatus);
            }
        }
        else
        {
            Console.WriteLine("Failure");
        }
    }
    catch (TaskCanceledException)
    {
        Console.WriteLine("Timeout. Device not found");
    }
    catch (Exception e)
    {
        Console.WriteLine($"Unhandled exception {e}");
    }
    await Task.Delay(TimeSpan.FromSeconds(1));
}

public record class StationStatus(string Ssid);

public record class ApRadioStatus(int Channel, string Status, Dictionary<string, StationStatus> StationStatuses);

public record class StationConfiguration
{
    public required string Ssid { get; init; }
    public required string WpaKey { get; init; }
}

public record class ApConfiguration
{
    public int Channel { get; init; }
    public required string ChannelBandwidth { get; init; }
    public required string RedVlans { get; init; }
    public required string BlueVlans { get; init; }
    public required Dictionary<string, StationConfiguration> StationConfigurations { get; init; }
    public required string SysLogIpAddress { get; init; }
}
