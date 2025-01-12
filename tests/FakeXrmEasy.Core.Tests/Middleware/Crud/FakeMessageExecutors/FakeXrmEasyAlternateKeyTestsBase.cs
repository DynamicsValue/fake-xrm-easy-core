#if !FAKE_XRM_EASY && !FAKE_XRM_EASY_2013
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
            List<LocalizedLabel> otherLocalizedLabels = new List<LocalizedLabel>();

            metadata.SetFieldValue("_keys", new EntityKeyMetadata[]
            {
                #if FAKE_XRM_EASY_9
                new EntityKeyMetadata()
                {
                    DisplayName = new Label(new LocalizedLabel("Code", 1033), otherLocalizedLabels),
                    KeyAttributes = new string[]{"dv_code"}
                }
                #else
                new EntityKeyMetadata()
                {
                    DisplayName = new Label(new LocalizedLabel("Code", 1033), otherLocalizedLabels.ToArray()),
                    KeyAttributes = new string[]{"dv_code"}
                }
                #endif
            });
            _context.SetEntityMetadata(metadata);
        }
    }
}
#endif