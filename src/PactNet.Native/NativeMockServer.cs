using System;
using System.Runtime.InteropServices;

namespace PactNet.Native
{
    /// <summary>
    /// Native mock server
    /// </summary>
    internal class NativeMockServer : IMockServer
    {
        /// <summary>
        /// Static constructor for <see cref="NativeMockServer"/>
        /// </summary>
        static NativeMockServer()
        {
            // TODO: make this configurable somehow, except it applies once for the entire native lifetime, so dunno
            Interop.LogToBuffer(LevelFilter.Debug);
        }

        public int CreateMockServerForPact(PactHandle pact, string addrStr, bool tls)
        {
            int result = Interop.CreateMockServerForPact(pact, addrStr, tls);

            if (result > 0)
            {
                return result;
            }

            throw result switch
            {
                -1 => new InvalidOperationException("Invalid handle when starting mock server"),
                -3 => new InvalidOperationException("Unable to start mock server"),
                -4 => new InvalidOperationException("The pact reference library panicked"),
                -5 => new InvalidOperationException("The IPAddress is invalid"),
                -6 => new InvalidOperationException("Could not create the TLS configuration with the self-signed certificate"),
                _ => new InvalidOperationException($"Unknown mock server error: {result}")
            };
        }

        public string MockServerMismatches(int mockServerPort)
        {
            IntPtr matchesPtr = Interop.MockServerMismatches(mockServerPort);

            return matchesPtr == IntPtr.Zero
                       ? string.Empty
                       : Marshal.PtrToStringAnsi(matchesPtr);
        }

        public string MockServerLogs(int mockServerPort)
        {
            IntPtr logsPtr = Interop.MockServerLogs(mockServerPort);

            return logsPtr == IntPtr.Zero
                       ? "ERROR: Unable to retrieve mock server logs"
                       : Marshal.PtrToStringAnsi(logsPtr);
        }

        public bool CleanupMockServer(int mockServerPort)
            => Interop.CleanupMockServer(mockServerPort);

        public void WritePactFile(int mockServerPort, string directory, bool overwrite)
        {
            int result = Interop.WritePactFile(mockServerPort, directory, overwrite);

            if (result == 0)
            {
                return;
            }

            throw result switch
            {
                1 => new InvalidOperationException("The pact reference library panicked"),
                2 => new InvalidOperationException("The pact file could not be written"),
                3 => new InvalidOperationException("A mock server with the provided port was not found"),
                _ => new InvalidOperationException($"Unknown error from backend: {result}")
            };
        }

        public PactHandle NewPact(string consumerName, string providerName)
            => Interop.NewPact(consumerName, providerName);

        public InteractionHandle NewInteraction(PactHandle pact, string description)
            => Interop.NewInteraction(pact, description);

        public bool Given(InteractionHandle interaction, string description)
            => Interop.Given(interaction, description);

        public bool GivenWithParam(InteractionHandle interaction, string description, string name, string value)
            => Interop.GivenWithParam(interaction, description, name, value);

        public bool WithRequest(InteractionHandle interaction, string method, string path)
            => Interop.WithRequest(interaction, method, path);

        public bool WithQueryParameter(InteractionHandle interaction, string name, string value, uint index)
            => Interop.WithQueryParameter(interaction, name, new UIntPtr(index), value);

        public bool WithSpecification(PactHandle pact, PactSpecification version)
            => Interop.WithSpecification(pact, version);

        public bool WithRequestHeader(InteractionHandle interaction, string name, string value, uint index)
            => Interop.WithHeader(interaction, InteractionPart.Request, name, new UIntPtr(index), value);

        public bool WithResponseHeader(InteractionHandle interaction, string name, string value, uint index)
            => Interop.WithHeader(interaction, InteractionPart.Response, name, new UIntPtr(index), value);

        public bool ResponseStatus(InteractionHandle interaction, ushort status)
            => Interop.ResponseStatus(interaction, status);

        public bool WithRequestBody(InteractionHandle interaction, string contentType, string body)
            => Interop.WithBody(interaction, InteractionPart.Request, contentType, body);

        public bool WithResponseBody(InteractionHandle interaction, string contentType, string body)
            => Interop.WithBody(interaction, InteractionPart.Response, contentType, body);

        /// <summary>
        /// Interop definitions for Rust reference implementation library
        /// </summary>
        private static class Interop
        {
            private const string dllName = "pact_mock_server_ffi";

            [DllImport(dllName, EntryPoint = "log_to_buffer")]
            public static extern void LogToBuffer(LevelFilter levelFilter);

            [DllImport(dllName, EntryPoint = "create_mock_server_for_pact")]
            public static extern int CreateMockServerForPact(PactHandle pact, string addrStr, bool tls);

            [DllImport(dllName, EntryPoint = "mock_server_mismatches")]
            public static extern IntPtr MockServerMismatches(int mockServerPort);

            [DllImport(dllName, EntryPoint = "mock_server_logs")]
            public static extern IntPtr MockServerLogs(int mockServerPort);

            [DllImport(dllName, EntryPoint = "cleanup_mock_server")]
            public static extern bool CleanupMockServer(int mockServerPort);

            [DllImport(dllName, EntryPoint = "write_pact_file")]
            public static extern int WritePactFile(int mockServerPort, string directory, bool overwrite);

            [DllImport(dllName, EntryPoint = "new_pact")]
            public static extern PactHandle NewPact(string consumerName, string providerName);

            [DllImport(dllName, EntryPoint = "with_specification")]
            public static extern bool WithSpecification(PactHandle pact, PactSpecification version);

            [DllImport(dllName, EntryPoint = "new_interaction")]
            public static extern InteractionHandle NewInteraction(PactHandle pact, string description);

            [DllImport(dllName, EntryPoint = "given")]
            public static extern bool Given(InteractionHandle interaction, string description);

            [DllImport(dllName, EntryPoint = "given_with_param")]
            public static extern bool GivenWithParam(InteractionHandle interaction, string description, string name, string value);

            [DllImport(dllName, EntryPoint = "with_request")]
            public static extern bool WithRequest(InteractionHandle interaction, string method, string path);

            [DllImport(dllName, EntryPoint = "with_query_parameter")]
            public static extern bool WithQueryParameter(InteractionHandle interaction, string name, UIntPtr index, string value);

            [DllImport(dllName, EntryPoint = "with_header")]
            public static extern bool WithHeader(InteractionHandle interaction, InteractionPart part, string name, UIntPtr index, string value);

            [DllImport(dllName, EntryPoint = "response_status")]
            public static extern bool ResponseStatus(InteractionHandle interaction, ushort status);

            [DllImport(dllName, EntryPoint = "with_body")]
            public static extern bool WithBody(InteractionHandle interaction, InteractionPart part, string contentType, string body);

            [DllImport(dllName, EntryPoint = "free_string")]
            public static extern void FreeString(IntPtr s);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct PactHandle
    {
        public readonly UIntPtr Pact;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct InteractionHandle
    {
        public readonly UIntPtr Pact;
        public readonly UIntPtr Interaction;
    }

    internal enum InteractionPart
    {
        Request = 0,
        Response = 1
    }

    internal enum PactSpecification
    {
        Unknown = 0,
        V1 = 1,
        V1_1 = 2,
        V2 = 3,
        V3 = 4,
        V4 = 5
    }

    internal enum LevelFilter
    {
        Off = 0,
        Error = 1,
        Warn = 2,
        Info = 3,
        Debug = 4,
        Trace = 5,
    }
}
