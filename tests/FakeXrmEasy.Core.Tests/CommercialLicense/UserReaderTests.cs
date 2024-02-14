using System;
using FakeXrmEasy.Core.CommercialLicense;
using Xunit;

namespace FakeXrmEasy.Core.Tests.CommercialLicense
{
    public class UserReaderTests
    {
        private readonly UserReader _userReader;

        public UserReaderTests()
        {
            _userReader = new UserReader();
        }
        
        [Fact]
        public void Should_return_current_user()
        {
            Assert.Equal(Environment.UserName, _userReader.GetCurrentUserName());
        }
    }
}