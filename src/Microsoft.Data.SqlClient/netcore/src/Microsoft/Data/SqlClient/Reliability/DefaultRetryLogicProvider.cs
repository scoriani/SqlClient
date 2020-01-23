using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Data.SqlClient.Reliability
{
    class DefaultRetryLogicProvider : SqlRetryPolicyProvider
    {
        public override bool IsSupported(SqlRetryLogicMethod retrylogicmethod)
        {
            // Supported for Connection Open and Command Execution scenarios
            return (retrylogicmethod == SqlRetryLogicMethod.ConnectionOpen) || (retrylogicmethod == SqlRetryLogicMethod.CommandExecution);
        }

        public override SqlRetryPolicy GetRetryPolicy(SqlRetryPolicyParameters parameters)
        {
            return new SqlRetryPolicy<TransientErrorDetectionStrategy>(3);


            //    // Retry Logic
            //    switch (connString.RetryStrategy)
            //    {
            //        case "None":
            //        case "":
            //            _retryPolicy = null;
            //            break;

            //        case "FixedInterval":
            //            _retryPolicy = new SqlRetryPolicy<TransientErrorDetectionStrategy>(new FixedInterval(connString.RetryCount, TimeSpan.FromSeconds(connString.RetryInterval)));
            //            break;

            //        case "Incremental":
            //            _retryPolicy = new SqlRetryPolicy<TransientErrorDetectionStrategy>(new Incremental(connString.RetryCount, TimeSpan.FromSeconds(connString.RetryInterval), TimeSpan.FromSeconds(connString.RetryIncrement)));
            //            break;

            //        case "ExponentialBackoff":
            //            _retryPolicy = new SqlRetryPolicy<TransientErrorDetectionStrategy>(new ExponentialBackoff(connString.RetryCount, TimeSpan.FromSeconds(connString.RetryMinBackoff), TimeSpan.FromSeconds(connString.RetryMaxBackoff), TimeSpan.FromSeconds(connString.RetryDeltaBackoff)));
            //            break;
            //    }

            //    List<int> errorsToAdd = new List<int>();
            //    List<int> errorsToRemove = new List<int>();

            //    if (_retryPolicy != null)
            //    {
            //        if (connString.RetriableErrors != "")
            //        {
            //            foreach (string s in connString.RetriableErrors.Split(','))
            //            {
            //                int err;
            //                if (s.IndexOf('+') != -1)
            //                {
            //                    if (int.TryParse(s.Replace("+", ""), out err))
            //                        errorsToAdd.Add(err);
            //                }
            //                else if (s.IndexOf('-') != -1)
            //                {
            //                    if (int.TryParse(s.Replace("-", ""), out err))
            //                        errorsToRemove.Add(err);
            //                }
            //                else
            //                {
            //                    if (int.TryParse(s, out err))
            //                        errorsToAdd.Add(err);
            //                }
            //            }
            //        }
            //        foreach (int err in errorsToAdd)
            //        {
            //            _retryPolicy.ErrorDetectionStrategy.RetriableErrors.Add(err);
            //        }
            //        foreach (int err in errorsToRemove)
            //        {
            //            _retryPolicy.ErrorDetectionStrategy.RetriableErrors.Remove(err);
            //        }
            //    }

        }
    }
}
