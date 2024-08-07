using System;
using System.Collections.Generic;
using System.Linq;
using DataverseEntities;
using FakeXrmEasy.Core.Exceptions.Query.FetchXml;
using FakeXrmEasy.Extensions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Xunit;
using Contact = Crm.Contact;

namespace FakeXrmEasy.Core.Tests.Issues
{
    public class Issue00158: FakeXrmEasyTestsBase
    {
        private readonly Entity _lateBoundContact;
        private readonly Contact _contact;
        private readonly EntityMetadata _contactMetadata;
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

            _contactMetadata = new EntityMetadata() { LogicalName = "contact" };
            var fullNameAttribute = new StringAttributeMetadata() { LogicalName = "fullname" };
            var longitudeAttribute = new DoubleAttributeMetadata() { LogicalName = "address1_longitude" };
            var educationCodeAttribute = new PicklistAttributeMetadata()
            {
                LogicalName = "educationcode",
                OptionSet = new OptionSetMetadata(new OptionMetadataCollection(new List<OptionMetadata>()
                {
                    new OptionMetadata(new Label("DefaultValue", 1033), 1)
                }))
            };
            var parentContactAttribute = new LookupAttributeMetadata()
                { LogicalName = "parentcontactid", Targets = new[] { "contact" } };
            
            _contactMetadata.SetAttributeCollection(new List<AttributeMetadata>()
            {
                fullNameAttribute, 
                longitudeAttribute,
                educationCodeAttribute,
                parentContactAttribute
            });
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
        
        [Fact]
        public void Should_return_contact_when_using_injected_double_metadata()
        {
            _lateBoundContact["address1_longitude"] = 1.2345;
            _context.Initialize(_lateBoundContact);
            _context.InitializeMetadata(_contactMetadata);
            
            var fetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='contact'>
                                    <attribute name='fullname' />
                                        <filter type='and'>
                                            <condition attribute='address1_longitude' operator='eq' value='1.2345' />
                                        </filter>
                                  </entity>
                            </fetch>";

            var entities = _service.RetrieveMultiple(new FetchExpression(fetchXml)).Entities;
            Assert.Single(entities);

            var contactFound = entities.FirstOrDefault();
            Assert.Equal(_lateBoundContact.Id, contactFound.Id);
        }
        
        [Fact]
        public void Should_return_contact_when_using_injected_option_set_metadata()
        {
            _lateBoundContact["educationcode"] = new OptionSetValue((int)contact_educationcode.DefaultValue);
            _context.Initialize(_lateBoundContact);
            _context.InitializeMetadata(_contactMetadata);
            
            var fetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='contact'>
                                    <attribute name='fullname' />
                                        <filter type='and'>
                                            <condition attribute='educationcode' operator='eq' value='1' />
                                        </filter>
                                  </entity>
                            </fetch>";

            var entities = _service.RetrieveMultiple(new FetchExpression(fetchXml)).Entities;
            Assert.Single(entities);

            var contactFound = entities.FirstOrDefault();
            Assert.Equal(_lateBoundContact.Id, contactFound.Id);
        }
        
        [Fact]
        public void Should_return_contact_when_using_injected_lookup_metadata()
        {
            var otherContact = new Entity("contact") { Id = Guid.NewGuid() };
            _lateBoundContact["parentcontactid"] = otherContact.ToEntityReference();
            
            _context.Initialize(_lateBoundContact);
            _context.InitializeMetadata(_contactMetadata);
            
            var fetchXml = $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='contact'>
                                    <attribute name='fullname' />
                                        <filter type='and'>
                                            <condition attribute='parentcontactid' operator='eq' value='{otherContact.Id}' />
                                        </filter>
                                  </entity>
                            </fetch>";

            var entities = _service.RetrieveMultiple(new FetchExpression(fetchXml)).Entities;
            Assert.Single(entities);

            var contactFound = entities.FirstOrDefault();
            Assert.Equal(_lateBoundContact.Id, contactFound.Id);
        }
    }
}