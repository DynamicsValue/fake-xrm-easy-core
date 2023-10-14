using System;
using System.Reflection;

namespace FakeXrmEasy.Core.Exceptions
{
    public class FindReflectedTypeException : Exception
    {
        public FindReflectedTypeException(string message) : base(message)
        {
            
        }
        
        public static FindReflectedTypeException New(ReflectionTypeLoadException reflectionTypeLoadException)
        {
            // now look at ex.LoaderExceptions - this is an Exception[], so:
            var s = "";
            foreach (var innerException in reflectionTypeLoadException.LoaderExceptions)
            {
                // write details of "inner", in particular inner.Message
                s += innerException.Message + " ";
            }

            return new FindReflectedTypeException("XrmFakedContext.FindReflectedType: " + s);
        }
    }
}