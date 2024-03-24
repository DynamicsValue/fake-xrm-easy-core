

#if FAKE_XRM_EASY_9

using System.Collections.Generic;
using DataverseEntities;
using Microsoft.Xrm.Sdk;
using FakeXrmEasy.Extensions;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Extensions
{
    public class OptionSetValueCollectionExtensionsTests
    {
        [Fact]
        public void Should_convert_to_null_if_null()
        {
            Assert.Null(OptionSetValueCollectionExtensions.ConvertToHashSetOfInt(null, true));
        }

        [Fact]
        public void Should_convert_to_a_hash_of_int_from_an_option_set_value_collection()
        {
            var optionSetValueCollection = new OptionSetValueCollection()
            {
                new OptionSetValue(1), new OptionSetValue(2), new OptionSetValue(3)
            };

            var hashOfInt = optionSetValueCollection.ConvertToHashSetOfInt(isOptionSetValueCollectionAccepted: true);
            Assert.NotNull(hashOfInt);

            Assert.Contains(1, hashOfInt);
            Assert.Contains(2, hashOfInt);
            Assert.Contains(3, hashOfInt);
        }
        
        [Fact]
        public void Should_convert_to_a_hash_of_int_from_an_array_of_int()
        {
            var arrayOfInts = new[] { 1, 2, 3 };

            var hashOfInt = arrayOfInts.ConvertToHashSetOfInt(isOptionSetValueCollectionAccepted: true);
            Assert.NotNull(hashOfInt);

            Assert.Contains(1, hashOfInt);
            Assert.Contains(2, hashOfInt);
            Assert.Contains(3, hashOfInt);
        }
        
        [Fact]
        public void Should_convert_to_a_hash_of_int_from_an_array_of_strings()
        {
            var arrayOfInts = new[] { "1", "2", "3" };

            var hashOfInt = arrayOfInts.ConvertToHashSetOfInt(isOptionSetValueCollectionAccepted: true);
            Assert.NotNull(hashOfInt);

            Assert.Contains(1, hashOfInt);
            Assert.Contains(2, hashOfInt);
            Assert.Contains(3, hashOfInt);
        }
        
        [Fact]
        public void Should_convert_to_a_hash_of_int_from_an_enumerable_of_enums()
        {
            var enumerableOfEnum = new List<dv_test_dv_choice_multiple> { dv_test_dv_choice_multiple.Option1, dv_test_dv_choice_multiple.Option2 };

            var hashOfInt = enumerableOfEnum.ConvertToHashSetOfInt(isOptionSetValueCollectionAccepted: true);
            Assert.NotNull(hashOfInt);

            Assert.Contains((int) dv_test_dv_choice_multiple.Option1, hashOfInt);
            Assert.Contains((int) dv_test_dv_choice_multiple.Option2, hashOfInt);
        }
    }
}
#endif