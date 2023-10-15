using System;
using System.Collections.Generic;
using System.Text;

namespace FakeXrmEasy.Core.Exceptions
{
    /// <summary>
    /// Exception raised when an early-bound type exists for the same logical name
    /// or entity type code in more than one assembly and so it's impossible to decide which one should
    /// be used 
    /// </summary>
    public class MultipleEarlyBoundTypesFoundException : Exception
    {
        private readonly string _errorMessage;
        
        /// <summary>
        /// Creates a MultipleEarlyBoundTypesFoundException from a logical name
        /// </summary>
        /// <param name="logicalName">The searched logical name</param>
        /// <param name="types">The types in which the logical name found</param>
        public MultipleEarlyBoundTypesFoundException(string logicalName, IEnumerable<Type> types)
        {
            var baseMessage =
                $"The early-bound type for logical name '{logicalName}' was found in more than one assembly. When using early-bound types, a given type must be unique across all the assemblies.";
            
            var log = GenerateLog(types);

            _errorMessage = $"{baseMessage} Assemblies where the type was found={log.ToString()}";
        }
        
        /// <summary>
        /// Creates a MultipleEarlyBoundTypesFoundException from an EntityTypeCode
        /// </summary>
        /// <param name="entityTypeCode">The searched entity type code</param>
        /// <param name="types">The types in which the entity type code was found</param>
        public MultipleEarlyBoundTypesFoundException(int entityTypeCode, IEnumerable<Type> types)
        {
            var baseMessage =
                $"The early-bound type for Entity Type Code '{entityTypeCode.ToString()}' was found in more than one assembly. When using early-bound types, a given type must be unique across all the assemblies.";
            
            var log = GenerateLog(types);
            
            _errorMessage = $"{baseMessage} Assemblies where the type was found={log.ToString()}";
        }

        private string GenerateLog(IEnumerable<Type> types)
        {
            var log = new StringBuilder();
            foreach (var type in types)
            {
                log.AppendLine($"'{type.Assembly.GetName().Name}'; ");
            }

            return log.ToString();
        }
        
        /// <summary>
        /// Returns the message along with the assemblies that caused this exception
        /// </summary>
        public override string Message => _errorMessage;
    }
}