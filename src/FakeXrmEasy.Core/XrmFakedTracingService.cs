using System;
using System.Text;
using FakeXrmEasy.Abstractions;

namespace FakeXrmEasy
{
    /// <summary>
    /// A fake tracing service that stores all traces In-Memory and can then dump the entire trace log
    /// </summary>
    public class XrmFakedTracingService : IXrmFakedTracingService
    {
        /// <summary>
        /// 
        /// </summary>
        protected StringBuilder _trace { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public XrmFakedTracingService()
        {
            _trace = new StringBuilder();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void Trace(string format, params object[] args)
        {
            if (args.Length == 0)
            {
                Trace("{0}", format);
            }
            else
            { 
                Console.WriteLine(format, args);

                _trace.AppendLine(string.Format(format, args));
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string DumpTrace()
        {
            return _trace.ToString();
        }
    }
}