using System;
using System.Text;
using FakeXrmEasy.Abstractions;

namespace FakeXrmEasy
{
    public class XrmFakedTracingService : IXrmFakedTracingService
    {
        protected StringBuilder _trace { get; set; }

        public XrmFakedTracingService()
        {
            _trace = new StringBuilder();
        }

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

        public string DumpTrace()
        {
            return _trace.ToString();
        }
    }
}