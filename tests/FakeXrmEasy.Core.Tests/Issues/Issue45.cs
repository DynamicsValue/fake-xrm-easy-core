using Crm;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using Xunit;

namespace FakeXrmEasy.Tests.Issues
{
    // https://github.com/DynamicsValue/fake-xrm-easy/issues/45

    public class Issue45 : FakeXrmEasyTestsBase
    {
        // setup
        public Issue45()
        {
            _context.EnableProxyTypes(typeof(Account).Assembly);

            Contact contact1 = new Contact() { Id = Guid.NewGuid(), LastName = "May" };
            Contact contact2 = new Contact() { Id = Guid.NewGuid(), LastName = "Truss" };
            Contact contact3 = new Contact() { Id = Guid.NewGuid(), LastName = "Johnson" };

            _context.Initialize(new List<Entity> { contact1, contact2 });
        }

        // This test currently fails - returns no records
        [Fact]
        public void When_An_IN_Clause_On_A_String_Field_Contains_ValueEqualsQuote_It_Should_Return_The_Right_Records()
        {
            string fetchXML = @"<fetch>
                                  <entity name='contact'>
                                    <attribute name='lastname' />     
                                     <filter>
                                      <condition attribute='lastname' operator='in' value=''>
                                        <value>Truss</value>
                                        <value>May</value>
                                      </condition>
                                    </filter>
                                  </entity>
                                </fetch>";

            EntityCollection contacts = _service.RetrieveMultiple(new FetchExpression(fetchXML));
            Assert.Equal(2, contacts.Entities.Count);
        }
        // This test currently passes
        [Fact]
        public void When_An_IN_Clause_On_A_String_Field_Doesnt_Contain_ValueEqualsQuote_It_Should_Return_The_Right_Records()
        {
            string fetchXML = @"<fetch>
                      <entity name='contact'>
                        <attribute name='lastname' />     
                         <filter>
                          <condition attribute='lastname' operator='in'>
                            <value>Truss</value>
                            <value>May</value>
                          </condition>
                        </filter>
                      </entity>
                    </fetch>";

            EntityCollection contacts = _service.RetrieveMultiple(new FetchExpression(fetchXML));

            Assert.Equal(2, contacts.Entities.Count);
        }

        // Fails -  When trying to parse value for entity contact and attribute statecode: Integer value expected
        [Fact]
        public void When_An_IN_Clause_On_An_OptionSetValue_Field_Contains_ValueEqualsQuote_It_Should_Return_The_Right_Records()
        {
            string fetchXML = @"<fetch top='2'>
                              <entity name='contact'>
                                <attribute name='lastname' />
                                <filter>
                                  <condition attribute='statecode' operator='in' value=''>
                                    <value>0</value>
                                  </condition>
                                </filter>
                              </entity>
                            </fetch>";

            EntityCollection contacts = _service.RetrieveMultiple(new FetchExpression(fetchXML));
            Assert.Equal(2, contacts.Entities.Count);
        }

        // Passes
        [Fact]
        public void When_An_IN_Clause_On_An_OptionSetValue_Field_Doesnt_Contain_ValueEqualsQuote_It_Should_Return_The_Right_Records()
        {
            string fetchXML = @"<fetch top='2'>
                              <entity name='contact'>
                                <attribute name='lastname' />
                                <filter>
                                  <condition attribute='statecode' operator='in'>
                                    <value>0</value>
                                  </condition>
                                </filter>
                              </entity>
                            </fetch>";

            EntityCollection contacts = _service.RetrieveMultiple(new FetchExpression(fetchXML));
            Console.WriteLine(contacts.Entities.Count);
            Assert.Equal(2, contacts.Entities.Count);
        }

    }
}