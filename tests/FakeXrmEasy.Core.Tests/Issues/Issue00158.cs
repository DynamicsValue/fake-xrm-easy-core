using System;
using Crm;
using FakeXrmEasy.Core.Exceptions.Query.FetchXml;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Issues
{
    public class Issue00158: FakeXrmEasyTestsBase
    {
        private readonly Entity _lateBoundContact;
        private readonly Contact _contact;
        
        public Issue00158()
        {
            _lateBoundContact = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["numberofchildren"] = 4
            };

            _contact = new Contact()
            {
                Id = Guid.NewGuid(),
                NumberOfChildren = 4
            };
        }

        [Fact]
        public void Should_crash_as_it_is_not_possible_to_guess_the_type()
        {
            var fetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='contact'>
                                    <attribute name='fullname' />
                                        <filter type='and'>
                                            <condition attribute='address1_longitude' operator='gt' value='1.2345' />
                                        </filter>
                                  </entity>
                            </fetch>";

            Assert.Throws<ArithmeticTypeConversionException>(() => _service.RetrieveMultiple(new FetchExpression(fetchXml)));
        }
    }
}