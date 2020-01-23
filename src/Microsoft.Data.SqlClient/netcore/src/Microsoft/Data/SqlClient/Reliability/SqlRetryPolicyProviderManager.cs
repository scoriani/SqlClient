// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Microsoft.Data.SqlClient.Reliability
{
    internal class SqlRetryPolicyProviderManager
    {
        static SqlRetryPolicyProviderManager()
        {
            var defaultRetryLogicProvider = new DefaultRetryLogicProvider();
            SqlRetryPolicyProviderConfigurationSection configurationSection;
            try
            {
                configurationSection = (SqlRetryPolicyProviderConfigurationSection)ConfigurationManager.GetSection(SqlRetryPolicyProviderConfigurationSection.Name);
            }
            catch (ConfigurationErrorsException e)
            {
                throw SQL.CannotGetAuthProviderConfig(e);
            }
            Instance = new SqlRetryPolicyProviderManager(configurationSection);
            Instance.SetProvider(defaultRetryLogicProvider,SqlRetryLogicMethod.ConnectionOpen);
            Instance.SetProvider(defaultRetryLogicProvider, SqlRetryLogicMethod.CommandExecution);
        }

        public static readonly SqlRetryPolicyProviderManager Instance;
        private readonly string _typeName;
        private readonly SqlRetryLogicInitializer _initializer;
        private readonly IReadOnlyCollection<SqlRetryLogicMethod> _retryLogicWithAppSpecifiedProvider;
        private readonly ConcurrentDictionary<SqlRetryLogicMethod, SqlRetryPolicyProvider> _providers;

        public SqlRetryPolicyProviderManager(SqlRetryPolicyProviderConfigurationSection configSection)
        {
            _typeName = GetType().Name;
            _providers = new ConcurrentDictionary<SqlRetryLogicMethod, SqlRetryPolicyProvider>();

            var retryLogicWithAppSpecifiedProvider = new HashSet<SqlRetryLogicMethod>();
            _retryLogicWithAppSpecifiedProvider = retryLogicWithAppSpecifiedProvider;

            if (configSection == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(configSection.InitializerType))
            {
                try
                {
                    var initializerType = Type.GetType(configSection.InitializerType, true);
                    _initializer = (SqlRetryLogicInitializer)Activator.CreateInstance(initializerType);
                    _initializer.Initialize();
                }
                catch (Exception e)
                {
                    throw SQL.CannotCreateSqlAuthInitializer(configSection.InitializerType, e);
                }
            }

            //// add user-defined providers, if any.
            ////
            if (configSection.Providers != null && configSection.Providers.Count > 0)
            {
                foreach (ProviderSettings providerSettings in configSection.Providers)
                {
                    SqlRetryLogicMethod retrylogicmethod = RetryLogicEnumFromString(providerSettings.Name);
                    SqlRetryPolicyProvider provider;
                    try
                    {
                        var providerType = Type.GetType(providerSettings.Type, true);
                        provider = (SqlRetryPolicyProvider)Activator.CreateInstance(providerType);
                    }
                    catch (Exception e)
                    {
                        throw SQL.CannotCreateAuthProvider(retrylogicmethod.ToString(), providerSettings.Type, e);
                    }
                    if (!provider.IsSupported(retrylogicmethod))
                        throw SQL.UnsupportedAuthenticationByProvider(retrylogicmethod.ToString(), providerSettings.Type);

                    _providers[retrylogicmethod] = provider;
                    retryLogicWithAppSpecifiedProvider.Add(retrylogicmethod);
                }
            }
        }
        /// <summary>
        /// Get an authentication provider by method.
        /// </summary>
        /// <param name="reliabilityMethod">Authentication method.</param>
        /// <returns>Authentication provider or null if not found.</returns>
        public SqlRetryPolicyProvider GetProvider(SqlRetryLogicMethod reliabilityMethod)
        {
            SqlRetryPolicyProvider value;
            return _providers.TryGetValue(reliabilityMethod, out value) ? value : null;
        }

        /// <summary>
        /// Set an authentication provider by method.
        /// </summary>
        /// <param name="provider">Authentication provider.</param>
        /// <param name="sqlRetryLogicMethod">Authentication provider.</param>
        /// <returns>True if succeeded, false otherwise, e.g., the existing provider disallows overriding.</returns>
        public bool SetProvider(SqlRetryPolicyProvider provider, SqlRetryLogicMethod sqlRetryLogicMethod)
        {
            if (!provider.IsSupported(sqlRetryLogicMethod))
                throw SQL.UnsupportedAuthenticationByProvider("", provider.GetType().Name);

            _providers.AddOrUpdate(sqlRetryLogicMethod, provider, (key, oldProvider) =>
            {
                if (oldProvider != null)
                {
                    oldProvider.BeforeUnload(sqlRetryLogicMethod);
                }
                if (provider != null)
                {
                    provider.BeforeLoad(sqlRetryLogicMethod);
                }
                return provider;
            });
            return true;
        }

        private static SqlRetryLogicMethod RetryLogicEnumFromString(string retrylogic)
        {
            switch (retrylogic.ToLowerInvariant())
            {
                case "sqlconnectionretrylogic":
                    return SqlRetryLogicMethod.ConnectionOpen;
                case "sqlcommandretrylogic":
                    return SqlRetryLogicMethod.CommandExecution;
                default:
                    throw SQL.UnsupportedAuthentication(retrylogic);
            }
        }

        private static string GetProviderType(SqlRetryPolicyProvider provider)
        {
            if (provider == null)
                return "null";
            return provider.GetType().FullName;
        }
    }

    internal class SqlRetryPolicyProviderConfigurationSection : ConfigurationSection
    {
        public const string Name = "SqlRetryPolicyProviders";

        [ConfigurationProperty("providers")]
        public ProviderSettingsCollection Providers => (ProviderSettingsCollection)base["providers"];

        [ConfigurationProperty("initializerType")]
        public string InitializerType => base["initializerType"] as string;
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract class SqlRetryLogicInitializer
    {
        /// <summary>
        /// 
        /// </summary>
        public abstract void Initialize();
    }

    /// <summary>
    /// 
    /// </summary>
    public enum SqlRetryLogicMethod
    {
        /// <summary>
        /// 
        /// </summary>
        NotSpecified = 0,
        /// <summary>
        /// 
        /// </summary>
        ConnectionOpen = 1,
        /// <summary>
        /// 
        /// </summary>
        CommandExecution = 2
    }

}
