using System;
using System.Reflection;
using DataverseEntities;
using FakeXrmEasy.Core.Exceptions;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Exceptions
{
    public class FindReflectedTypeExceptionTests
    {
        private readonly Type _type1;
        private readonly Exception _exception;
        private readonly ReflectionTypeLoadException _reflectionException;
        
        public FindReflectedTypeExceptionTests()
        {
            _type1 = typeof(Account);
            _exception = new Exception();
            _reflectionException = new ReflectionTypeLoadException(new Type[] {_type1}, new Exception[] {_exception});
        }
        
        [Fact]
        public void Should_create_find_reflected_type_exception()
        {
            var ex = FindReflectedTypeException.New(_reflectionException);
            Assert.NotNull(ex);
        }
    }
}