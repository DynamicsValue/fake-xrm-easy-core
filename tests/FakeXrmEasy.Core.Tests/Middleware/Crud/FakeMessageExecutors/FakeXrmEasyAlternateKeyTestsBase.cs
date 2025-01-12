using System.Collections.Generic;
using System.Reflection;
using DataverseEntities;
using FakeXrmEasy.Extensions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;

namespace FakeXrmEasy.Core.Tests.Middleware.Crud.FakeMessageExecutors
{
    public class FakeXrmEasyAlternateKeyTestsBase: FakeXrmEasyTestsBase
    {
        public FakeXrmEasyAlternateKeyTestsBase() : base()
        {
            var assembly = Assembly.GetAssembly(typeof(dv_test));
            _context.EnableProxyTypes(assembly);
            _context.InitializeMetadata(assembly);
            
            var metadata = _context.GetEntityMetadataByName("dv_test");
            List<LocalizedLabel> otherLocalizedLabels = null;

            metadata.SetFieldValue("_keys", new EntityKeyMetadata[]
            {
                new EntityKeyMetadata()
                {
                    DisplayName = new Label(new LocalizedLabel("Code", 1033), otherLocalizedLabels),
                    KeyAttributes = new string[]{"dv_code"}
                }
            });
            _context.SetEntityMetadata(metadata);
        }
    }
}