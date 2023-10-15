using Crm;
using FakeXrmEasy.Query;
using Microsoft.Xrm.Sdk;
using System;
using System.Reflection;
using Xunit;

namespace FakeXrmEasy.Core.Tests.FakeContextTests.FetchXml
{
    public class TypeConversionTests : FakeXrmEasyTestsBase
    {
        [Fact]
        public void When_arithmetic_values_are_used_proxy_types_assembly_is_required()
        {
            var fetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='contact'>
                                    <attribute name='fullname' />
                                        <filter type='and'>
                                            <condition attribute='address1_longitude' operator='gt' value='1.2345' />
                                        </filter>
                                  </entity>
                            </fetch>";

            Assert.Throws<Exception>(() => fetchXml.ToQueryExpression(_context));
        }

        [Fact]
        public void Conversion_to_double_is_correct()
        {
            _context.EnableProxyTypes(Assembly.GetAssembly(typeof(Contact)));

            var fetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='contact'>
                                    <attribute name='fullname' />
                                        <filter type='and'>
                                            <condition attribute='address1_longitude' operator='le' value='1.2345' />
                                        </filter>
                                  </entity>
                            </fetch>";

            var query = fetchXml.ToQueryExpression(_context);
            Assert.IsType<double>(query.Criteria.Conditions[0].Values[0]);
            Assert.Equal(1.2345, query.Criteria.Conditions[0].Values[0]);
        }

        [Fact]
        public void Conversion_to_entityreference_is_correct()
        {
            _context.EnableProxyTypes(Assembly.GetAssembly(typeof(Contact)));

            var fetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='contact'>
                                    <attribute name='fullname' />
                                        <filter type='and'>
                                            <condition attribute='accountid' operator='eq' value='71831D66-8820-446A-BCEB-BCE14D12B216' />
                                        </filter>
                                  </entity>
                            </fetch>";

            var query = fetchXml.ToQueryExpression(_context);
            Assert.IsType<EntityReference>(query.Criteria.Conditions[0].Values[0]);
        }

        [Fact]
        public void Conversion_to_guid_is_correct()
        {
            _context.EnableProxyTypes(Assembly.GetAssembly(typeof(Contact)));

            var fetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='contact'>
                                    <attribute name='fullname' />
                                        <filter type='and'>
                                            <condition attribute='address2_addressid' operator='eq' value='71831D66-8820-446A-BCEB-BCE14D12B216' />
                                        </filter>
                                  </entity>
                            </fetch>";

            var query = fetchXml.ToQueryExpression(_context);
            Assert.IsType<Guid>(query.Criteria.Conditions[0].Values[0]);
        }

        [Fact]
        public void Conversion_to_int_is_correct()
        {
            _context.EnableProxyTypes(Assembly.GetAssembly(typeof(Contact)));

            var fetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='contact'>
                                    <attribute name='fullname' />
                                        <filter type='and'>
                                            <condition attribute='address1_utcoffset' operator='eq' value='4' />
                                        </filter>
                                  </entity>
                            </fetch>";

            var query = fetchXml.ToQueryExpression(_context);
            Assert.IsType<int>(query.Criteria.Conditions[0].Values[0]);
            Assert.Equal(4, query.Criteria.Conditions[0].Values[0]);
        }

        [Fact]
        public void Conversion_to_bool_is_correct()
        {
            _context.EnableProxyTypes(Assembly.GetAssembly(typeof(Account)));

            var fetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='account'>
                                    <attribute name='name' />
                                    <filter type='and'>
                                        <condition attribute='donotemail' operator='eq' value='0' />
                                    </filter>
                                  </entity>
                            </fetch>";

            var query = fetchXml.ToQueryExpression(_context);
            Assert.IsType<bool>(query.Criteria.Conditions[0].Values[0]);
            Assert.Equal(false, query.Criteria.Conditions[0].Values[0]);

            var fetchXml2 = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='account'>
                                    <attribute name='name' />
                                    <filter type='and'>
                                        <condition attribute='donotemail' operator='eq' value='true' />
                                    </filter>
                                  </entity>
                            </fetch>";

            var query2 = fetchXml2.ToQueryExpression(_context);
            Assert.IsType<bool>(query2.Criteria.Conditions[0].Values[0]);
            Assert.Equal(true, query2.Criteria.Conditions[0].Values[0]);
        }

        [Fact]
        public void Conversion_to_bool_throws_error_if_incorrect()
        {
            _context.EnableProxyTypes(Assembly.GetAssembly(typeof(Account)));

            var fetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='account'>
                                    <attribute name='name' />
                                    <filter type='and'>
                                        <condition attribute='donotemail' operator='eq' value='3' />
                                    </filter>
                                  </entity>
                            </fetch>";

            var exception = Assert.Throws<Exception>(() => fetchXml.ToQueryExpression(_context));
            Assert.Equal("When trying to parse value for entity account and attribute donotemail: Boolean value expected", exception.Message);

            var fetchXml2 = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='account'>
                                    <attribute name='name' />
                                    <filter type='and'>
                                        <condition attribute='donotemail' operator='eq' value='anothervalue' />
                                    </filter>
                                  </entity>
                            </fetch>";

            var exception2 = Assert.Throws<Exception>(() => fetchXml2.ToQueryExpression(_context));
            Assert.Equal("When trying to parse value for entity account and attribute donotemail: Boolean value expected", exception.Message);
        }

        [Fact]
        public void Conversion_to_string_is_correct()
        {
            _context.EnableProxyTypes(Assembly.GetAssembly(typeof(Contact)));

            var fetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='contact'>
                                    <attribute name='fullname' />
                                        <filter type='and'>
                                            <condition attribute='address1_city' operator='eq' value='123' />
                                        </filter>
                                  </entity>
                            </fetch>";

            var query = fetchXml.ToQueryExpression(_context);
            Assert.IsType<string>(query.Criteria.Conditions[0].Values[0]);
            Assert.Equal("123", query.Criteria.Conditions[0].Values[0]);
        }

        [Fact]
        public void Conversion_to_optionsetvalue_is_correct()
        {
            _context.EnableProxyTypes(Assembly.GetAssembly(typeof(Contact)));

            var fetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='contact'>
                                    <attribute name='fullname' />
                                        <filter type='and'>
                                            <condition attribute='address1_addresstypecode' operator='eq' value='2' />
                                        </filter>
                                  </entity>
                            </fetch>";

            var query = fetchXml.ToQueryExpression(_context);
            Assert.IsType<OptionSetValue>(query.Criteria.Conditions[0].Values[0]);
            Assert.Equal(2, (query.Criteria.Conditions[0].Values[0] as OptionSetValue).Value);
        }

        [Fact]
        public void Conversion_to_money_is_correct()
        {
            _context.EnableProxyTypes(Assembly.GetAssembly(typeof(Contact)));

            var fetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='contact'>
                                    <attribute name='fullname' />
                                        <filter type='and'>
                                            <condition attribute='aging30' operator='eq' value='2' />
                                        </filter>
                                  </entity>
                            </fetch>";

            var query = fetchXml.ToQueryExpression(_context);
            Assert.IsType<Money>(query.Criteria.Conditions[0].Values[0]);
            Assert.Equal(2, (query.Criteria.Conditions[0].Values[0] as Money).Value);
        }

        [Fact]
        public void Conversion_to_datetime_is_correct()
        {
            _context.EnableProxyTypes(Assembly.GetAssembly(typeof(Contact)));

            var fetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='contact'>
                                    <attribute name='fullname' />
                                        <filter type='and'>
                                            <condition attribute='anniversary' operator='on' value='2014-11-23' />
                                        </filter>
                                  </entity>
                            </fetch>";

            var query = fetchXml.ToQueryExpression(_context);
            Assert.IsType<DateTime>(query.Criteria.Conditions[0].Values[0]);

            var dtTime = query.Criteria.Conditions[0].Values[0] as DateTime?;
            Assert.Equal(2014, dtTime.Value.Year);
            Assert.Equal(11, dtTime.Value.Month);
            Assert.Equal(23, dtTime.Value.Day);
        }

        [Fact]
        public void Conversion_to_enum_is_correct()
        {
            _context.EnableProxyTypes(Assembly.GetAssembly(typeof(Contact)));

            var fetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' >
              <entity name='incident' >
                <attribute name='incidentid' />
                <attribute name='statecode' />
                <order attribute='createdon' descending='true' />
                 <filter type='and' >
                  <condition attribute='statecode' operator='neq' value='2' />
                </filter>
              </entity>
            </fetch>";

            var query = fetchXml.ToQueryExpression(_context);
            Assert.IsType<OptionSetValue>(query.Criteria.Conditions[0].Values[0]);
            Assert.Equal(2, (query.Criteria.Conditions[0].Values[0] as OptionSetValue).Value);
        }
    }
}