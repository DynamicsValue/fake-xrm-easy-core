using System;
using Crm;
using Microsoft.Xrm.Sdk.Metadata;
using System.Collections.Generic;
using Xunit;

using FakeXrmEasy.Extensions;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

using System.Linq;
using FakeXrmEasy.Abstractions.Metadata;

namespace FakeXrmEasy.Tests.FakeContextTests.InsertOptionValueRequestTests
{
    public class InsertOptionValueRequestTests: FakeXrmEasyTests
    {
        [Fact]
        public void Should_also_update_entity_metadata_when_not_using_global_option_set()
        {
            var label = "dummy label";
            var attributeName = "statuscode";
            

            var entityMetadata = new EntityMetadata()
            {
                LogicalName = "contact"
            };

            StatusAttributeMetadata enumAttribute = new StatusAttributeMetadata() { LogicalName = attributeName };
            entityMetadata.SetAttributeCollection(new List<AttributeMetadata>() { enumAttribute });

            _context.InitializeMetadata(entityMetadata);

            var req = new InsertOptionValueRequest()
            {
                EntityLogicalName = Contact.EntityLogicalName,
                AttributeLogicalName = attributeName,
                Label = new Label(label, 0)
            };

            _service.Execute(req);

            //Check the optionsetmetadata was updated
            var key = $"{Contact.EntityLogicalName}#{attributeName}";
            var optionSetMetadata = _context.GetProperty<IOptionSetMetadataRepository>().GetByName(key);
            Assert.NotNull(optionSetMetadata);

            var option = optionSetMetadata.Options.FirstOrDefault();
            Assert.Equal(label, option.Label.LocalizedLabels[0].Label);

            // Get a list of Option Set values for the Status Reason fields from its metadata
            RetrieveAttributeRequest attReq = new RetrieveAttributeRequest
            {
                EntityLogicalName = "contact",
                LogicalName = "statuscode",
                RetrieveAsIfPublished = true
            };

            RetrieveAttributeResponse attResponse = (RetrieveAttributeResponse)_service.Execute(attReq);

            StatusAttributeMetadata statusAttributeMetadata = (StatusAttributeMetadata)attResponse.AttributeMetadata;
            Assert.NotNull(statusAttributeMetadata.OptionSet);
            Assert.NotNull(statusAttributeMetadata.OptionSet.Options);
            Assert.Equal(1, statusAttributeMetadata.OptionSet.Options.Count(o => o.Label.LocalizedLabels[0].Label == label));
        }

        [Fact]
        public void Should_store_user_localized_label()
        {
            var label = "fake";
            var attributeName = "statuscode";
            

            LocalizedLabel localizedLabel1 = new LocalizedLabel(label, 10);
            LocalizedLabel localizedLabel2 = new LocalizedLabel("falso", 10);
            LocalizedLabel[] localizedLabels = new LocalizedLabel[] { localizedLabel1, localizedLabel2 };

            var entityMetadata = new EntityMetadata()
            {
                LogicalName = "contact"
            };

            StatusAttributeMetadata enumAttribute = new StatusAttributeMetadata() { LogicalName = attributeName };
            entityMetadata.SetAttributeCollection(new List<AttributeMetadata>() { enumAttribute });

            _context.InitializeMetadata(entityMetadata);

            var req = new InsertOptionValueRequest()
            {
                EntityLogicalName = Contact.EntityLogicalName,
                AttributeLogicalName = attributeName,
                Label = new Label(localizedLabel1, localizedLabels)
            };

            
            _service.Execute(req);

            //Check the optionsetmetadata was updated
            var key = $"{Contact.EntityLogicalName}#{attributeName}";
            var optionSetMetadata = _context.GetProperty<IOptionSetMetadataRepository>().GetByName(key);

            Assert.NotNull(optionSetMetadata);

            var option = optionSetMetadata.Options.FirstOrDefault();
            Assert.Equal(label, option.Label.LocalizedLabels[0].Label);

            // Get a list of Option Set values for the Status Reason fields from its metadata
            RetrieveAttributeRequest attReq = new RetrieveAttributeRequest
            {
                EntityLogicalName = "contact",
                LogicalName = "statuscode",
                RetrieveAsIfPublished = true
            };

            RetrieveAttributeResponse attResponse = (RetrieveAttributeResponse)_service.Execute(attReq);

            StatusAttributeMetadata statusAttributeMetadata = (StatusAttributeMetadata)attResponse.AttributeMetadata;
            Assert.NotNull(statusAttributeMetadata.OptionSet);
            Assert.NotNull(statusAttributeMetadata.OptionSet.Options);
            Assert.Equal(1, statusAttributeMetadata.OptionSet.Options.Count(o => o.Label.LocalizedLabels[0].Label == label));

            foreach (var optionMetadata in statusAttributeMetadata.OptionSet.Options)
            {
                Console.WriteLine("Key " + optionMetadata.Value + "                    Value: " + optionMetadata.Label.UserLocalizedLabel.Label);
            }
        }
    }
}