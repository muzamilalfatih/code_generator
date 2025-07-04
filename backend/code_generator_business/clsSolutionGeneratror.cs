using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace code_generator_business
{
    internal class  clsSolutionGeneratror
    {
        private static void _RunDotnetCommand(string args, string workingDir = null)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                    //WorkingDirectory = workingDir ?? Directory.GetCurrentDirectory()
                }
            };

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            Console.WriteLine(output);
            if (!string.IsNullOrEmpty(error))
                Console.WriteLine("ERROR: " + error);
        }
        private static void _GenerateNugetFile()
        {
            // Step 0: Create nuget.config in the solution folder
            var sb = new StringBuilder();
            sb.AppendLine(@"<?xml version=""1.0"" encoding=""utf-8""?>");
            sb.AppendLine(@"<configuration>");
            sb.AppendLine(@"  <packageSources>");
            sb.AppendLine(@"    <add key=""OfflineSource"" value=""C:\Users\muzam\source\repos\New folder\MyOfflineNuget"" />");
            sb.AppendLine(@"  </packageSources>");
            sb.AppendLine(@"</configuration>");
            File.WriteAllText(Path.Combine(clsUtil.projectPath, "nuget.config"), sb.ToString());
        }

        public static void GenerateSolution()
        {
            _GenerateNugetFile();
            Directory.SetCurrentDirectory(clsUtil.projectPath);

            // 1. Create the solution
            _RunDotnetCommand($"new sln -n {clsUtil.projectName}", clsUtil.projectPath);

            // 2. Create the projects
            _RunDotnetCommand($"new webapi -n {clsUtil.APIProjectName} -o {clsUtil.APIProjectName}", clsUtil.projectPath);
            _RunDotnetCommand($"new classlib -n {clsUtil.BussiessProjectName} -o {clsUtil.BussiessProjectName}", clsUtil.projectPath);
            _RunDotnetCommand($"new classlib -n {clsUtil.DataAcessProjectName} -o {clsUtil.DataAcessProjectName}", clsUtil.projectPath);
            _RunDotnetCommand($"new classlib -n {clsUtil.SharedClassessProjectName} -o {clsUtil.SharedClassessProjectName}", clsUtil.projectPath);

            Directory.CreateDirectory($"{clsUtil.APIProjectName}/Controllers");

            // 3. Add projects to solution
            _RunDotnetCommand($"sln {clsUtil.projectName}.sln add {clsUtil.APIProjectName}/{clsUtil.APIProjectName}.csproj", clsUtil.projectPath);
            _RunDotnetCommand($"sln {clsUtil.projectName}.sln add {clsUtil.BussiessProjectName}/{clsUtil.BussiessProjectName}.csproj", clsUtil.projectPath);
            _RunDotnetCommand($"sln {clsUtil.projectName}.sln add {clsUtil.DataAcessProjectName}/{clsUtil.DataAcessProjectName}.csproj", clsUtil.projectPath);
            _RunDotnetCommand($"sln {clsUtil.projectName}.sln add {clsUtil.SharedClassessProjectName}/{clsUtil.SharedClassessProjectName}.csproj", clsUtil.projectPath);

            // 4. Set project references
            _RunDotnetCommand($"add {clsUtil.APIProjectName}/{clsUtil.APIProjectName}.csproj reference {clsUtil.BussiessProjectName}/{clsUtil.BussiessProjectName}.csproj", clsUtil.projectPath);
            _RunDotnetCommand($"add {clsUtil.APIProjectName}/{clsUtil.APIProjectName}.csproj reference {clsUtil.SharedClassessProjectName}/{clsUtil.SharedClassessProjectName}.csproj", clsUtil.projectPath);
            _RunDotnetCommand($"add {clsUtil.BussiessProjectName}/{clsUtil.BussiessProjectName}.csproj reference {clsUtil.DataAcessProjectName}/{clsUtil.DataAcessProjectName}.csproj", clsUtil.projectPath);
            _RunDotnetCommand($"add {clsUtil.BussiessProjectName}/{clsUtil.BussiessProjectName}.csproj reference {clsUtil.SharedClassessProjectName}/{clsUtil.SharedClassessProjectName}.csproj", clsUtil.projectPath);
            _RunDotnetCommand($"add {clsUtil.DataAcessProjectName}/{clsUtil.DataAcessProjectName}.csproj reference {clsUtil.SharedClassessProjectName}/{clsUtil.SharedClassessProjectName}.csproj", clsUtil.projectPath);

            // 5. Add Microsoft.Data.SqlClient manually to DAL
            string dalCsprojPath = Path.Combine(clsUtil.DataAcessProjectName, $"{clsUtil.DataAcessProjectName}.csproj");
            string csprojText = File.ReadAllText(dalCsprojPath);
            string sqlClientReference = @"  <ItemGroup>
                                              <PackageReference Include=""Microsoft.Data.SqlClient"" Version=""5.1.0"" />
                                            </ItemGroup>
                                            </Project>";

            if (!csprojText.Contains("Microsoft.Data.SqlClient"))
            {
                csprojText = csprojText.Replace("</Project>", sqlClientReference);
                File.WriteAllText(dalCsprojPath, csprojText);
            }

            // 6. Install Swagger offline into API project
            string apiProjectCsprojPath = Path.Combine(clsUtil.APIProjectName, $"{clsUtil.APIProjectName}.csproj");
            _RunDotnetCommand($"add \"{apiProjectCsprojPath}\" package Swashbuckle.AspNetCore --version 6.5.0 --source \"{Path.Combine(clsUtil.projectPath, "MyOfflineNuget")}\"", clsUtil.projectPath);

            // 7. Restore the solution
            _RunDotnetCommand($"restore \"{clsUtil.projectName}.sln\"", clsUtil.projectPath);
            _AddSwaggerToProgram();
        }
        private static void _AddSwaggerToProgram()
        {
            

            string programPath = Path.Combine(clsUtil.APIProjectName, "Program.cs");
            if (!File.Exists(programPath)) return;

            string programText = File.ReadAllText(programPath);

            // Remove .NET 8 AddOpenApi / MapOpenApi
            programText = programText.Replace("builder.Services.AddOpenApi();", "");
            programText = programText.Replace("app.MapOpenApi();", "");

            // Inject Swashbuckle and MVC setup if not already present
            if (!programText.Contains("AddSwaggerGen"))
            {
                programText = programText.Replace(
                    "var builder = WebApplication.CreateBuilder(args);",
                    @"var builder = WebApplication.CreateBuilder(args);
                    builder.Services.AddControllers();
                    builder.Services.AddEndpointsApiExplorer();
                    builder.Services.AddSwaggerGen();");

                programText = programText.Replace(
                    "var app = builder.Build();",
                    @"var app = builder.Build();

                    if (app.Environment.IsDevelopment())
                    {
                        app.UseSwagger();
                        app.UseSwaggerUI();
                    }

                    app.UseHttpsRedirection();
                    app.UseAuthorization();
                    app.MapControllers();");
            }

            // Write back the modified Program.cs
            File.WriteAllText(programPath, programText);
            _LaunchChromeByDefault();
        }
        private static void _LaunchChromeByDefault()
        {
            string launchSettingsPath = Path.Combine(clsUtil.APIProjectName, "Properties/launchSettings.json");
            string launchSettingContent = @"{
  ""$schema"": ""http://json.schemastore.org/launchsettings.json"",
  ""iisSettings"": {
    ""windowsAuthentication"": false,
    ""anonymousAuthentication"": true,
    ""iisExpress"": {
      ""applicationUrl"": ""http://localhost:22090"",
      ""sslPort"": 44367
    }
  },
  ""profiles"": {
    ""http"": {
      ""commandName"": ""Project"",
      ""dotnetRunMessages"": true,
      ""launchBrowser"": true,
      ""launchUrl"": ""swagger"",
      ""applicationUrl"": ""http://localhost:5083"",
      ""environmentVariables"": {
        ""ASPNETCORE_ENVIRONMENT"": ""Development""
      }
    },
    ""https"": {
      ""commandName"": ""Project"",
      ""dotnetRunMessages"": true,
      ""launchBrowser"": true,
      ""launchUrl"": ""swagger"",
      ""applicationUrl"": ""https://localhost:7262;http://localhost:5083"",
      ""environmentVariables"": {
        ""ASPNETCORE_ENVIRONMENT"": ""Development""
      }
    },
    ""IIS Express"": {
      ""commandName"": ""IISExpress"",
      ""launchBrowser"": true,
      ""launchUrl"": ""swagger"",
      ""environmentVariables"": {
        ""ASPNETCORE_ENVIRONMENT"": ""Development""
      }
    }
  }
}
";

            File.WriteAllText(launchSettingsPath, launchSettingContent);
        }

    }
}



