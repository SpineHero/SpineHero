using System;
using System.Diagnostics;
using PostSharp.Aspects;

namespace SpineHero.Common.Logging
{
    [Serializable]
    public class LogMethodCall : OnMethodBoundaryAspect
    {
        private static ILog log = Log.GetLogger<LogMethodCall>();
        private string message;
        private string methodName;
        private string className;

        [NonSerialized]
        private Stopwatch stopwatch;

        public LogMethodCall(string message = "")
        {
            this.message = message;
        }

        public override void OnEntry(MethodExecutionArgs methodExecutionArgs)
        {
            stopwatch = new Stopwatch();
            stopwatch.Start();
            methodName = methodExecutionArgs.Method.Name.Replace("~", String.Empty);
            className = methodExecutionArgs.Method.DeclaringType?.ToString();
            log.Info($"{message} {className}.{methodName}() called with arguments {string.Join(", ", methodExecutionArgs.Arguments.ToArray())}");
        }

        public override void OnSuccess(MethodExecutionArgs args)
        {
            stopwatch.Stop();
            log.Info($"{message} {className}.{methodName}() sucessfully returns {args.ReturnValue} in {stopwatch.ElapsedMilliseconds} ms.");
        }

        public override void OnExit(MethodExecutionArgs args)
        {
            base.OnExit(args);
        }

        public override void OnException(MethodExecutionArgs args)
        {
            log.Error(args.Exception, $"{message} {className}.{methodName}() throws error {args.Exception}");
        }
    }
}