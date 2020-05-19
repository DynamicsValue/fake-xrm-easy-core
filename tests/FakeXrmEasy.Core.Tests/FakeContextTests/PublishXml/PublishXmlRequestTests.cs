using Microsoft.Crm.Sdk.Messages;
using System;
using Xunit;

namespace FakeXrmEasy.Tests.FakeContextTests.PublishXml
{
    public class PublishXmlRequestTests: FakeXrmEasyTests
    {
        [Fact]
        public void When_calling_publish_xml_exception_is_raised_if_parameter_xml_is_blank()
        {
            var req = new PublishXmlRequest()
            {
                ParameterXml = ""
            };

            Assert.Throws<Exception>(() => _service.Execute(req));
        }

        [Fact]
        public void When_calling_publish_xml_no_exception_is_raised()
        {
            var req = new PublishXmlRequest()
            {
                ParameterXml = "<somexml></somexml>"
            };

            var ex = Record.Exception(() => _service.Execute(req));
            Assert.Null(ex);
        }
    }
}