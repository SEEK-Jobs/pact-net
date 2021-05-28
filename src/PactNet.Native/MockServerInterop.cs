﻿using System;
using System.Runtime.InteropServices;

namespace PactNet.Native
{
    /// <summary>
    /// Interop definitions for Rust reference implementation library
    /// </summary>
    internal static class MockServerInterop
    {
        private const string dllName = "pact_mock_server_ffi";

        /// <summary>
        /// Static constructor for <see cref="MockServerInterop"/>
        /// </summary>
        static MockServerInterop()
        {
            // TODO: make this configurable somehow, except it applies once for the entire native lifetime, so dunno
            Environment.SetEnvironmentVariable("LOG_LEVEL", "DEBUG");
            Init("LOG_LEVEL");
        }

        [DllImport(dllName, EntryPoint = "init")]
        public static extern void Init(string logEnvVar);

        [DllImport(dllName, EntryPoint = "create_mock_server")]
        public static extern int CreateMockServer(string pactStr, string addStr, bool tls);

        [DllImport(dllName, EntryPoint = "create_mock_server_for_pact")]
        public static extern int CreateMockServerForPact(PactHandle pact, string addrStr, bool tls);

        [DllImport(dllName, EntryPoint = "mock_server_matched")]
        public static extern bool MockServerMatched(int mockServerPort);

        [DllImport(dllName, EntryPoint = "mock_server_mismatches")]
        public static extern IntPtr MockServerMismatches(int mockServerPort);

        [DllImport(dllName, EntryPoint = "cleanup_mock_server")]
        public static extern bool CleanupMockServer(int mockServerPort);

        [DllImport(dllName, EntryPoint = "write_pact_file")]
        public static extern int WritePactFile(int mockServerPort, string directory, bool overwrite);

        [DllImport(dllName, EntryPoint = "new_pact")]
        public static extern PactHandle NewPact(string consumerName, string providerName);

        [DllImport(dllName, EntryPoint = "new_interaction")]
        public static extern InteractionHandle NewInteraction(PactHandle pact, string description);

        [DllImport(dllName, EntryPoint = "upon_receiving")]
        public static extern void UponReceiving(InteractionHandle interaction, string description);

        [DllImport(dllName, EntryPoint = "given")]
        public static extern void Given(InteractionHandle interaction, string description);

        [DllImport(dllName, EntryPoint = "given_with_param")]
        public static extern void GivenWithParam(InteractionHandle interaction, string description, string name, string value);

        [DllImport(dllName, EntryPoint = "with_request")]
        public static extern void WithRequest(InteractionHandle interaction, string method, string path);

        [DllImport(dllName, EntryPoint = "with_query_parameter")]
        public static extern void WithQueryParameter(InteractionHandle interaction, string name, UIntPtr index, string value);

        [DllImport(dllName, EntryPoint = "with_specification")]
        public static extern void WithSpecification(PactHandle pact, PactSpecification version);

        [DllImport(dllName, EntryPoint = "with_header")]
        public static extern void WithHeader(InteractionHandle interaction, InteractionPart part, string name, UIntPtr index, string value);

        [DllImport(dllName, EntryPoint = "response_status")]
        public static extern void ResponseStatus(InteractionHandle interaction, ushort status);

        [DllImport(dllName, EntryPoint = "with_body")]
        public static extern void WithBody(InteractionHandle interaction, InteractionPart part, string contentType, string body);

        [DllImport(dllName, EntryPoint = "free_string")]
        public static extern void FreeString(IntPtr s);
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct PactHandle
    {
        public UIntPtr Pact { get; set; }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct InteractionHandle
    {
        public UIntPtr Pact { get; set; }
        public UIntPtr Interaction { get; set; }
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
        /// A level lower than all log levels.
        Off = 0,
        /// Corresponds to the `Error` log level.
        Error = 1,
        /// Corresponds to the `Warn` log level.
        Warn = 2,
        /// Corresponds to the `Info` log level.
        Info = 3,
        /// Corresponds to the `Debug` log level.
        Debug = 4,
        /// Corresponds to the `Trace` log level.
        Trace = 5,
    }
}