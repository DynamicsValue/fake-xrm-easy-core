#if FAKE_XRM_EASY_365 || FAKE_XRM_EASY_9
using Microsoft.Xrm.Sdk.Query;
using System;
using Xunit;
using FakeXrmEasy.Query;

namespace FakeXrmEasy.Core.Tests.Query.FetchXml.OperatorTests.HierarchyOperators
{
    /// <summary>
    /// To test all different condition operators found in FetchXml
    /// https://msdn.microsoft.com/en-us/library/gg309405.aspx

    /*
     <xs:simpleType name="operator">
    <xs:restriction base="xs:NMTOKEN">
      HIERACHY OPERATORS:
      <xs:enumeration value="under"/>
      <xs:enumeration value="eq-or-under" />
      <xs:enumeration value="not-under"/>
      <xs:enumeration value="above" />
      <xs:enumeration value="eq-or-above" />

    </xs:restriction>
  </xs:simpleType>
      
     
     */
    /// </summary>
    public class HierarchyOperatorTests : FakeXrmEasyTestsBase
    {
        private readonly Guid _guid = Guid.NewGuid();
        
        [Theory]
        [InlineData(ConditionOperator.Above, "above")]
        [InlineData(ConditionOperator.AboveOrEqual, "eq-or-above")]
        [InlineData(ConditionOperator.Under, "under")]
        [InlineData(ConditionOperator.UnderOrEqual, "eq-or-under")]
        [InlineData(ConditionOperator.NotUnder, "not-under")]
        public void Should_translate_fetch_xml_operator_to_correct_query_expression_operator(ConditionOperator expectedConditionOperator, string fetchXmlOperator)
        {
            
            var fetchXml = $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='account'>
                                    <attribute name='name' />
                                        <filter type='and'>
                                            <condition attribute='accountid' operator='{fetchXmlOperator}' value='{_guid}' />
                                        </filter>
                                  </entity>
                            </fetch>";

            var query = fetchXml.ToQueryExpression(_context);

            Assert.True(query.Criteria != null);
            Assert.Single(query.Criteria.Conditions);
            Assert.Equal("accountid", query.Criteria.Conditions[0].AttributeName);
            Assert.Equal(expectedConditionOperator, query.Criteria.Conditions[0].Operator);
            Assert.Equal(_guid.ToString(), query.Criteria.Conditions[0].Values[0].ToString());
        }
    }
}
#endif