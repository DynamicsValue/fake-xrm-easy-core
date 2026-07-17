using System;
using System.Collections.Generic;
using DataverseEntities;
using FakeXrmEasy.Abstractions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

#if FAKE_XRM_EASY_9
namespace FakeXrmEasy.Core.Tests.Query.FetchXml.JoinOperatorTests
{
    public class InvalidOperatorTests : FakeXrmEasyTestsBase
    {
        private readonly Contact _contact;
        private readonly Account _contosoAccount;
        private readonly Account _contAccount;

        public InvalidOperatorTests()
        {
            _contact = new Contact()
            {
                Id = Guid.NewGuid(),
                FirstName = "Joe"
            };

            _contosoAccount = new Account()
            {
                Id = Guid.NewGuid(),
                Name = "Contoso",
                PrimaryContactId = _contact.ToEntityReference()
            };

            _contAccount = new Account()
            {
                Id = Guid.NewGuid(),
                Name = "Cont",
                PrimaryContactId = _contact.ToEntityReference()
            };
        }
        
        [Fact]
        public void Should_throw_exception_with_an_invalid_join_operator()
        {
            _context.Initialize(new List<Entity>() {_contact, _contosoAccount });
            
            var fetch = @"
                    <fetch distinct='false' useraworderby='false' no-lock='false' mapping='logical'>
                        <entity name='contact'>
                            <attribute name='firstname' />
                            <filter type='or'>
                                <link-entity name='account' to='contactid' from='primarycontactid' link-type='not-all'>
                                    <filter type='and'>
                                        <condition attribute='name' operator='eq' value='Other name' />
                                    </filter>
                                </link-entity>
                            </filter>
                        </entity>
                    </fetch>
                ";

            var ex = XAssert.ThrowsFaultCode(ErrorCodes.QueryBuilderDeserializeInvalidLinkType, () => _service.RetrieveMultiple(new FetchExpression(fetch)));
            Assert.Equal($"Invalid link-type specified, valid values are: 'natural', 'inner', 'in', 'matchfirstrowusingcrossapply','exists' and 'outer'. link-type = not-all", ex.Message);
        }
    }
}
#endif