using System;
using System.Reflection;

namespace ArgsToClass.Exceptions
{
    public class ArgsAnalysisException : Exception
    {
        public ArgsAnalysisException(PropertyInfo analysisErrorPropertyInfo, string message, Exception innerException) : base(message, innerException)
        {
            AnalysisErrorPropertyInfo = analysisErrorPropertyInfo;
        }
        public ArgsAnalysisException(PropertyInfo analysisErrorPropertyInfo, string message) : base(message)
        {
            AnalysisErrorPropertyInfo = analysisErrorPropertyInfo;
        }
        public ArgsAnalysisException(PropertyInfo analysisErrorPropertyInfo)
        {
            AnalysisErrorPropertyInfo = analysisErrorPropertyInfo;
        }

        public PropertyInfo AnalysisErrorPropertyInfo { get; }
    }
}