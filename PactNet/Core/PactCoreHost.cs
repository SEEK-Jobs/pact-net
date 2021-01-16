using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using PactNet.Models;

namespace PactNet.Core
{
    internal class PactCoreHost<T> : IPactCoreHost where T : IPactCoreHostConfig
    {
        protected readonly Process _process;
        private readonly IPactCoreHostConfig _config;

        public PactCoreHost(T config)
        {
            _config = config;

            var expectedPackage = string.Empty;

#if USE_NET4X
            var pactCoreDir = $"{Constants.BuildDirectory}{Path.DirectorySeparatorChar}"; //OS specific version will be appended
            pactCoreDir += "pact-win32";
            expectedPackage = "PactNet.Windows";
#else
            var pactCoreDir = $"{Constants.BuildDirectory}{Path.DirectorySeparatorChar}"; //OS specific version will be appended

            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                pactCoreDir += "pact-win32";
                expectedPackage = "PactNet.Windows";
            }
            else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
            {
                pactCoreDir += "pact-osx";
                expectedPackage = "PactNet.OSX";
            }
            else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux) &&
                System.Runtime.InteropServices.RuntimeInformation.OSArchitecture == System.Runtime.InteropServices.Architecture.X86)
            {
                pactCoreDir += "pact-linux-x86";
                expectedPackage = "PactNet.Linux.x86";
            }
            else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux) &&
                     System.Runtime.InteropServices.RuntimeInformation.OSArchitecture == System.Runtime.InteropServices.Architecture.X64)
            {
                pactCoreDir += "pact-linux-x86_64";
                expectedPackage = "PactNet.Linux.x64";
            }
            else
            {
                throw new PactFailureException("Sorry your current OS platform or architecture is not supported");
            }
#endif

            if (!Directory.Exists(pactCoreDir))
            {
                throw new PactFailureException($"Please install the relevant platform and architecture specific PactNet dependency from Nuget. Based on your current setup you should install '{expectedPackage}'.");

                //TODO: Fall back to using the locally installed ruby and packaged assets
            }

            var configPath = $"{pactCoreDir}{Path.DirectorySeparatorChar}config.json";
            var platformConfig = JsonConvert.DeserializeObject<PlatformCoreConfig>(File.ReadAllText(configPath));

            var startInfo = new ProcessStartInfo
            {
#if USE_NET4X
                WindowStyle = ProcessWindowStyle.Hidden,
#endif
                FileName = ReplaceConfigParams(platformConfig.FileName, pactCoreDir, _config.Script),
                Arguments = $"{ReplaceConfigParams(platformConfig.Arguments, pactCoreDir, _config.Script)} {_config.Arguments}",
                UseShellExecute = false,
                RedirectStandardInput = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            if (platformConfig.Environment != null)
            {
                foreach (var envVar in platformConfig.Environment)
                {
                    var value = ReplaceConfigParams(envVar.Value, pactCoreDir, _config.Script);
#if USE_NET4X
                    startInfo.EnvironmentVariables[envVar.Key] = value;
#else
                    startInfo.Environment[envVar.Key] = value;
#endif
                }
            }

            if (config.Environment != null)
            {
                foreach (var envVar in config.Environment)
                {
#if USE_NET4X
                    startInfo.EnvironmentVariables[envVar.Key] = envVar.Value;
#else
                    startInfo.Environment[envVar.Key] = envVar.Value;
#endif
                }
            }

            _process = new Process
            {
                StartInfo = startInfo
            };

#if USE_NET4X
            AppDomain.CurrentDomain.DomainUnload += (o, e) => Stop();
#else
            System.Runtime.Loader.AssemblyLoadContext.Default.Unloading += context => Stop();
#endif
        }

        public void Start()
        {
            _process.OutputDataReceived += WriteLineToOutput;
            _process.ErrorDataReceived += WriteLineToOutput;

            var success = _process.Start();

            if (!success)
            {
                throw new PactFailureException("Could not start the Pact Core Host");
            }

            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();

            if (_config.WaitForExit)
            {
                _process.WaitForExit();

                if (_process.ExitCode != 0)
                {
                    throw new PactFailureException("Pact verification failed. See output for details. \nIf the output is empty please provide a custom config.Outputters (IOutput) for your test framework, as we couldn't write to the console.");
                }
            }
        }

        public void Stop()
        {
            bool hasExited;

            try
            {
                hasExited = _process.HasExited;
            }
            catch (InvalidOperationException)
            {
                hasExited = true;
            }

            if (!hasExited)
            {
                try
                {
                    _process.OutputDataReceived -= WriteLineToOutput;
                    _process.ErrorDataReceived -= WriteLineToOutput;
                    _process.CancelOutputRead();
                    _process.CancelErrorRead();
                    _process.Kill();
                    _process.Dispose();
                }
                catch (Exception)
                {
                    throw new PactFailureException("Could not terminate the Pact Core Host, please manually kill the 'Ruby interpreter' process");
                }
            }
        }

        private void WriteLineToOutput(object sender, DataReceivedEventArgs eventArgs)
        {
            if (eventArgs.Data != null)
            {
                WriteToOutputters(Regex.Replace(eventArgs.Data, @"\e\[(\d+;)*(\d+)?[ABCDHJKfmsu]", ""));
            }
        }

        private void WriteToOutputters(string line)
        {
            if (_config.Outputters != null && _config.Outputters.Any())
            {
                foreach (var output in _config.Outputters)
                {
                    output.WriteLine(line);
                }
            }
        }

        private string ReplaceConfigParams(string input, string pactCoreDir, string script)
        {
            return !string.IsNullOrEmpty(input) ?
                input.Replace("{pactCoreDir}", pactCoreDir).Replace("{script}", script) :
                string.Empty;
        }
    }
}
