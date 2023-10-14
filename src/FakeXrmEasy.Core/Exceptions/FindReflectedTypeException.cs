using System;
using System.Reflection;
using System.Text;

namespace FakeXrmEasy.Core.Exceptions
{
    /// <summary>
    /// Exception raised when the generated early bound types have an unsupported format
    /// </summary>
    public class FindReflectedTypeException : Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="message"></param>
        public FindReflectedTypeException(string message) : base(message)
        {
            
        }
        
        /// <summary>
        /// Creates a FindReflectedException from a ReflectionTypeLoadException
        /// </summary>
        /// <param name="reflectionTypeLoadException"></param>
        /// <returns></returns>
        public static FindReflectedTypeException New(ReflectionTypeLoadException reflectionTypeLoadException)
        {
            // now look at ex.LoaderExceptions - this is an Exception[], so:
            var s = new StringBuilder();
            foreach (var innerException in reflectionTypeLoadException.LoaderExceptions)
            {
                // write details of "inner", in particular inner.Message
                s.AppendLine(innerException.Message);
            }

            return new FindReflectedTypeException("XrmFakedContext.FindReflectedType: " + s.ToString());
        }
    }
}