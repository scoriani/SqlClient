using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Data.SqlClient.Reliability
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class SqlRetryPolicyProvider
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reliabilityMethod"></param>
        /// <returns></returns>
        public static SqlRetryPolicyProvider GetProvider(SqlRetryLogicMethod reliabilityMethod)
        {
            return SqlRetryPolicyProviderManager.Instance.GetProvider(reliabilityMethod);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reliabilityMethod"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static bool SetProvider(SqlRetryLogicMethod reliabilityMethod, SqlRetryPolicyProvider provider)
        {
            return SqlRetryPolicyProviderManager.Instance.SetProvider(provider, reliabilityMethod);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reliabilityMethod"></param>
        public virtual void BeforeLoad(SqlRetryLogicMethod reliabilityMethod) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reliabilityMethod"></param>
        public virtual void BeforeUnload(SqlRetryLogicMethod reliabilityMethod) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reliabilityMethod"></param>
        /// <returns></returns>
        public abstract bool IsSupported(SqlRetryLogicMethod reliabilityMethod);
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public abstract SqlRetryPolicy GetRetryPolicy(SqlRetryPolicyParameters parameters);

    }
}
