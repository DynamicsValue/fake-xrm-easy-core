using System;
using System.Collections.Generic;
using System.Linq;
using DataverseEntities;
using FakeXrmEasy.Extensions;
using Microsoft.Xrm.Sdk;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Extensions
{
    public class CloneAttributeTests
    {
        [Fact]
        public void Should_clone_activity_parties()
        {
            IEnumerable<Entity> activityParties = Enumerable.Range(1, 2).Select(index =>
            {
                var activityParty = new Entity("activityparty");
                activityParty["partyid"] = new EntityReference("contact", Guid.NewGuid());

                return activityParty;
            }).ToArray();

            var e = new Entity("email");
            e["to"] = activityParties;

            var clone = EntityExtensions.CloneAttribute(e["to"]) as IEnumerable<Entity>;

            Assert.NotNull(clone);
            Assert.Equal(2, clone.Count());

            Assert.Equal(activityParties, clone, new ActivityPartyComparer());
        }
        
#if FAKE_XRM_EASY_9
        [Fact]
        public void Should_clone_multi_option_set_values_as_an_option_set_value_collection()
        {
            var e = new Crm.Contact() { Id = Guid.NewGuid() };
            e.new_MultiSelectAttribute = new OptionSetValueCollection() { new OptionSetValue(1) , new OptionSetValue(2) };
            
            var clone = EntityExtensions.CloneAttribute(e.new_MultiSelectAttribute) as OptionSetValueCollection;
            Assert.NotNull(clone);
            Assert.True(e.new_MultiSelectAttribute != clone);
            
            Assert.Equal(1, clone[0].Value);
            Assert.Equal(2, clone[1].Value);
        }
        
        [Fact]
        public void Should_clone_multi_option_set_values_as_an_enumerable_of_enum()
        {
            var e = new dv_test() { Id = Guid.NewGuid() };
            e.dv_choice_multiple = new List<dv_test_dv_choice_multiple>() { dv_test_dv_choice_multiple.Option1 , dv_test_dv_choice_multiple.Option2 };
            
            var clone = EntityExtensions.CloneAttribute(e.dv_choice_multiple) as IEnumerable<dv_test_dv_choice_multiple>;
            Assert.NotNull(clone);
            Assert.True(e.dv_choice_multiple != clone);
            
            Assert.Contains(dv_test_dv_choice_multiple.Option1, clone);
            Assert.Contains(dv_test_dv_choice_multiple.Option2, clone);
        }
        
        [Fact]
        public void Should_clone_multi_option_set_values_as_an_array_of_enum()
        {
            var optionSetValues = new [] { dv_test_dv_choice_multiple.Option1 , dv_test_dv_choice_multiple.Option2 };
            
            var clone = EntityExtensions.CloneAttribute(optionSetValues) as dv_test_dv_choice_multiple[];
            Assert.NotNull(clone);
            Assert.True(optionSetValues != clone);
            
            Assert.Contains(dv_test_dv_choice_multiple.Option1, clone);
            Assert.Contains(dv_test_dv_choice_multiple.Option2, clone);
        }
        #endif
        
#if !FAKE_XRM_EASY
        //Entity images aren't supported in versions prior to 2013, so no need to support byte arrays as attributes

        [Fact]
        public void Should_clone_byte_array()
        {
            var random = new Random();
            byte[] image = new byte[2000];
            random.NextBytes(image);

            var e = new Entity("account");
            e["entityimage"] = image;

            var clone = EntityExtensions.CloneAttribute(e["entityimage"]) as byte[];

            Assert.NotNull(clone);
            Assert.Equal(2000, clone.Length);
            Assert.Equal(image, clone);
        }
#endif
    }
}