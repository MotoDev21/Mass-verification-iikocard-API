using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace iikoCardCheckMass
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await ReadCardAsync();
            Console.ReadKey(true);
        }

        static async Task<string> GetTokenAsync()
        {
            var options = new RestClientOptions("https://api-ru.iiko.services")
            {
                MaxTimeout = -1,
            };
            var client = new RestClient(options);
            var request = new RestRequest("/api/1/access_token", Method.Post);
            request.AddHeader("Content-Type", "application/json");
            var body = @"{ ""apiLogin"": ""a7d*****-***"" }"; //Введите ваш API логин!!!!!!!!!!!!!!!
            request.AddStringBody(body, DataFormat.Json);
            RestResponse response = await client.ExecuteAsync(request);

            JObject jsonObj = JObject.Parse(response.Content);

            // Получение значения токена
            string tokenValue = (string)jsonObj["token"];

            // Вывод значения токена
            //Console.WriteLine(tokenValue);
            return tokenValue;
            //Console.WriteLine(response.Content);

        }

        static async Task CheckCardAsync(string token, string cardNumber)
        {
            var options = new RestClientOptions("https://api-ru.iiko.services")
            {
                MaxTimeout = -1,
            };
            var client = new RestClient(options);
            var request = new RestRequest("/api/1/loyalty/iiko/customer/info", Method.Post);
            request.AddHeader("Authorization", "Bearer " + token);
            request.AddHeader("Content-Type", "application/json");
            var body = @"
            " + "\n" +
            @"{
            " + "\n" +
            @"""cardNumber"": "+ cardNumber + ", " + "\n" +
            @"""type"": ""cardNumber"",
            " + "\n" +
            @"""organizationId"": ""10591070-1a76-4dd4-a187-fa6e7e6fcba6""
            " + "\n" +
            @"}";
            request.AddStringBody(body, DataFormat.Json);
            RestResponse response = await client.ExecuteAsync(request);

            JObject jsonObj = JObject.Parse(response.Content); 
            Console.WriteLine(response.Content);

            try
            {

                // Путь к файлу для записи response.Content
                string outputPath = cardNumber+".txt";

                // Запись response.Content в файл
                using (StreamWriter sw = new StreamWriter(outputPath))
                {
                    sw.Write(response.Content);
                }

                Console.WriteLine("Response content has been written to the file: " + outputPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }

        static async Task ReadCardAsync()
        {
            try
            {
                // Путь к вашему файлу с номерами карт
                string filePath = "card.txt";

                // Чтение всех строк из файла
                string[] lines = File.ReadAllLines(filePath);

                // Массив для хранения номеров карт
                string[] cardNumbers = new string[lines.Length];

                // Копирование номеров карт из строк в массив
                for (int i = 0; i < lines.Length; i++)
                {
                    cardNumbers[i] = lines[i];
                }

                // Вывод всех номеров карт
                Console.WriteLine("Card numbers:");
                foreach (var cardNumber in cardNumbers)
                {
                    Console.WriteLine("\n\nКАРТА ДЛЯ ПРОВЕРКИ:" + cardNumber);
                    string token = await GetTokenAsync();
                    await CheckCardAsync(token, cardNumber);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }
    }
}
