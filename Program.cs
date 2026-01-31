using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;

class Program
{
    static async System.Threading.Tasks.Task Main()
    {
        // 1. Загружаем IP адреса из файла
        var ips = File.ReadAllLines("ips.txt")
                      .Where(ip => !string.IsNullOrWhiteSpace(ip))
                      .ToList();

        var ipDataList = new List<IpData>();

        foreach (var ip in ips)
        {
            var data = await GetIpData(ip);
            if (data != null)
                ipDataList.Add(data);
        }

        // 3. Группируем по странам и считаем кол-во
        var countriesStats = ipDataList
            .GroupBy(x => x.Country)
            .Select(g => new
            {
                Country = g.Key,
                Count = g.Count(),
                Cities = g.Select(x => x.City).Distinct().ToList()
            })
            .ToList();

        // Вывод статистики по странам
        Console.WriteLine("IP addresses by country:");
        foreach (var country in countriesStats)
        {
            Console.WriteLine($"{country.Country} - {country.Count}");
        }

        // 4. Страна с максимальным количеством IP
        var topCountry = countriesStats
            .OrderByDescending(x => x.Count)
            .First();

        Console.WriteLine($"\nCities in country {topCountry.Country}:");
        foreach (var city in topCountry.Cities)
        {
            Console.WriteLine(city);
        }
    }

    // 2. Запрос к ipinfo.io
    static async System.Threading.Tasks.Task<IpData> GetIpData(string ip)
    {
        using var client = new HttpClient();

        try
        {
            var response = await client.GetStringAsync($"https://ipinfo.io/{ip}/json");
            dynamic json = JsonConvert.DeserializeObject(response);

            // Если API вернул ошибку
            if (json.error != null)
                return null;

            return new IpData
            {
                Ip = ip,
                City = json.city,
                Country = json.country
            };
        }
        catch
        {
            return null;
        }
    }
}
