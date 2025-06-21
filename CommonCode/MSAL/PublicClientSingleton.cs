// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Identity.Client;
using System.Reflection;
using CommonCode.MSALClient;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.IO; // Required for Stream and ArgumentNullException

namespace CommonCode.MSALClient
{
    /// <summary>
    /// This is a singleton implementation to wrap the MSALClient and associated classes.
    /// It must be initialized by calling the Initialize method from the main application.
    /// </summary>
    public class PublicClientSingleton
    {
        private static PublicClientSingleton _instance;
        private static readonly object _lock = new object(); // For thread-safe initialization

        /// <summary>
        /// Gets the application configuration.
        /// </summary>
        public IConfiguration Configuration { get; private set; }

        /// <summary>
        /// Gets the instance of DownstreamApiHelper.
        /// </summary>
        public DownstreamApiHelper DownstreamApiHelper { get; private set; }

        /// <summary>
        /// Gets the instance of MSALClientHelper.
        /// </summary>
        public MSALClientHelper MSALClientHelper { get; private set; }

        /// <summary>
        /// This will determine if the Interactive Authentication should be Embedded or System view.
        /// </summary>
        public bool UseEmbedded { get; set; } = false;

        /// <summary>
        /// Private constructor to ensure singleton pattern and force initialization via Initialize method.
        /// </summary>
        /// <param name="appSettingsStream">The stream containing the application settings.</param>
        private PublicClientSingleton(Stream appSettingsStream)
        {
            if (appSettingsStream == null)
            {
                throw new ArgumentNullException(nameof(appSettingsStream), "appSettingsStream cannot be null for PublicClientSingleton initialization.");
            }

            try
            {
                this.Configuration = new ConfigurationBuilder()
                    .AddJsonStream(appSettingsStream) // appSettingsStream will be disposed by Build()
                    .Build();

                AzureAdConfig azureADConfig = this.Configuration.GetSection("AzureAd").Get<AzureAdConfig>();
                if (azureADConfig == null)
                {
                    throw new InvalidOperationException("AzureAd configuration section is missing or could not be loaded from appsettings.json. Ensure it exists in the provided stream.");
                }
                this.MSALClientHelper = new MSALClientHelper(azureADConfig);

                DownStreamApiConfig downStreamApiConfig = this.Configuration.GetSection("DownstreamApi").Get<DownStreamApiConfig>();
                if (downStreamApiConfig == null)
                {
                    throw new InvalidOperationException("DownstreamApi configuration section is missing or could not be loaded from appsettings.json. Ensure it exists in the provided stream.");
                }
                this.DownstreamApiHelper = new DownstreamApiHelper(downStreamApiConfig, this.MSALClientHelper);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during PublicClientSingleton construction: {ex.ToString()}");
                throw;
            }
        }

        /// <summary>
        /// Gets the initialized instance of the PublicClientSingleton.
        /// Throws an InvalidOperationException if Initialize has not been called.
        /// </summary>
        public static PublicClientSingleton Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new InvalidOperationException($"{nameof(PublicClientSingleton)} is not initialized. Call {nameof(Initialize)} from your application's startup code (e.g., MauiProgram.cs).");
                }
                return _instance;
            }
        }

        /// <summary>
        /// Initializes the PublicClientSingleton with the application settings stream.
        /// This method should be called once during application startup.
        /// </summary>
        /// <param name="appSettingsStream">The stream for 'appsettings.json'.</param>
        public static void Initialize(Stream appSettingsStream)
        {
            if (appSettingsStream == null)
            {
                throw new ArgumentNullException(nameof(appSettingsStream));
            }

            if (_instance == null) // Check before locking for performance
            {
                lock (_lock) // Ensure thread-safe initialization
                {
                    if (_instance == null) // Double-check after acquiring lock
                    {
                        _instance = new PublicClientSingleton(appSettingsStream);
                    }
                }
            }
        }

        /// <summary>
        /// Acquire the token silently
        /// </summary>
        /// <returns>An access token</returns>
        public async Task<string> AcquireTokenSilentAsync()
        {
            return await this.MSALClientHelper.SignInUserAndAcquireAccessToken(this.GetScopes()).ConfigureAwait(false);
        }

        /// <summary>
        /// Acquire the token silently
        /// </summary>
        /// <param name="scopes">desired scopes</param>
        /// <returns>An access token</returns>
        public async Task<string> AcquireTokenSilentAsync(string[] scopes)
        {
            return await this.MSALClientHelper.SignInUserAndAcquireAccessToken(scopes).ConfigureAwait(false);
        }

        /// <summary>
        /// Perform the interactive acquisition of the token for the given scope
        /// </summary>
        /// <param name="scopes">desired scopes</param>
        /// <returns></returns>
        internal async Task<AuthenticationResult> AcquireTokenInteractiveAsync(string[] scopes)
        {
            this.MSALClientHelper.UseEmbedded = this.UseEmbedded;
            return await this.MSALClientHelper.SignInUserInteractivelyAsync(scopes).ConfigureAwait(false);
        }

        /// <summary>
        /// It will sign out the user.
        /// </summary>
        /// <returns></returns>
        internal async Task SignOutAsync()
        {
            await this.MSALClientHelper.SignOutUserAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Gets scopes for the application
        /// </summary>
        /// <returns>An array of all scopes</returns>
        internal string[] GetScopes()
        {
            return this.DownstreamApiHelper.DownstreamApiConfig.ScopesArray;
        }
    }
}