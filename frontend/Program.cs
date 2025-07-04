
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace StudentApiClient
{
    public class Result<T>
    {
        public Result(bool success, string message, T? data)
        {
            this.success = success;
            this.message = message;
            this.data = data;
        }
        public bool success { get; set; }
        public string message { get; set; }
        public T? data { get; set; }
    }

    class Program
    {

        static async Task ShowDottingAnimation(string message, CancellationToken token)
        {
            int dotCount = 0;
            while (!token.IsCancellationRequested)
            {
                Console.Write($"\r{message}{new string('.', dotCount)}   "); // Overwrite line
                dotCount = (dotCount + 1) % 4;
                await Task.Delay(500);
            }

            // Clear the line when done
            Console.Write("\r" + new string(' ', message.Length + 5) + "\r");
        }


        static readonly HttpClient httpClient = new HttpClient();
        static async Task Main(string[] args)
        {
            httpClient.BaseAddress = new Uri("http://localhost:5151/api/"); // Set this to the correct URI for your API

           
            Result<List<string>> databaseResult =  await GetAllDatabases();
            if (!databaseResult.success)
            {
                Console.WriteLine(databaseResult.message);
                return;
            } 
             
            DisplayDatabasesOnScreen(databaseResult.data);

            GenerateCodeDTO generatCodeDTO = ReadParamerte(databaseResult.data);

            Result<string> generateCodeResult = await GenerateCode(generatCodeDTO);
            if (!generateCodeResult.success)
            {
                Console.WriteLine(generateCodeResult.message);
                return;
            }
            Console.WriteLine($"The project is successfully generated wiht path : {generateCodeResult.data}");
        }

        
        static async Task<Result<List<string>>> GetAllDatabases()
        {
            try
            {
                Console.WriteLine("\n_____________________________");
                Console.WriteLine("\nFetching all databasess...\n");

                var cts = new CancellationTokenSource();
                var animationTask = ShowDottingAnimation("Loading", cts.Token);
                var response = await httpClient.GetAsync("databases/All");
                cts.Cancel();
                await animationTask;

                if (response.IsSuccessStatusCode)
                {
                    List<string> databases = await response.Content.ReadFromJsonAsync<List<string>>();
                    if (databases != null)
                    {
                        return new Result<List<string>>(true, "Data retrieved successfully!", databases);
                    }
                }
                string errorContent = await response.Content.ReadAsStringAsync();
                return new Result<List<string>>(false, errorContent,null);

            }
            catch (HttpRequestException ex)
            {
                return new Result<List<string>>(false, ex.InnerException.Message, null);
            }
        }
        static void DisplayDatabasesOnScreen(List<string> databases)
        {
            for (int i = 0; i < databases.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {databases[i]}");
            }
        }

        static GenerateCodeDTO ReadParamerte(List<string>databases)
        {
            int database;
            Console.Write("Select Database : ");
            while(!int.TryParse(Console.ReadLine(), out database) || database - 1 < 0 ||  database - 1 > databases.Count)
            {
                Console.Write("Invalid choice! select another : ");
            }
            Console.Write("Enter project name : ");
            string projectName = Console.ReadLine();
            while (projectName.Trim() == "")
            {
                Console.Write("Invalid project name!Enter another name : ");
                projectName = Console.ReadLine();
            }

            return new GenerateCodeDTO(projectName, databases[database - 1]);
        }
        static async Task<Result<string>> GenerateCode(GenerateCodeDTO generateCodeDTO)
        {
            try
            {
                Console.WriteLine("\n_____________________________");
                var cts = new CancellationTokenSource();
                var animationTask = ShowDottingAnimation("Generating Code", cts.Token);
                var response = await httpClient.PostAsJsonAsync("code-write", generateCodeDTO);
                cts.Cancel();
                await animationTask;
                if (response.IsSuccessStatusCode)
                {
                    string projectPath = await response.Content.ReadAsStringAsync();
                    return new Result<string>(true,"Project generated Successfully!", projectPath);
                }

                string errorContent = await response.Content.ReadAsStringAsync();
                return new Result<string>(false, errorContent, "");
            }
            catch (Exception ex)
            {
                return new Result<string>(false, "Unexpected error occur", "");
            }
        }

    }
    public class GenerateCodeDTO
    {
        public GenerateCodeDTO(string projectName, string databaseName)
        {
            this.projectName = projectName;
            this.databaseName = databaseName;
        }

        public string projectName {  get; set; }
        public string databaseName { get; set; }

    }
}
