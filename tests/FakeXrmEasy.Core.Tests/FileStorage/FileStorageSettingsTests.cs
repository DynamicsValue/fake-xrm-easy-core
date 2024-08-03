using System.Linq;
using System.Reflection;
using DataverseEntities;
using FakeXrmEasy.Core.FileStorage;
using Microsoft.Xrm.Sdk.Metadata;
using Xunit;

namespace FakeXrmEasy.Core.Tests.FileStorage
{
    public class FileStorageSettingsTests: FakeXrmEasyTestsBase
    {
        [Fact]
        public void Should_return_default_max_file_size()
        {
            var fileStorageSettings = _context.GetProperty<IFileStorageSettings>();
            Assert.NotNull(fileStorageSettings);
            Assert.Equal(FileStorageSettings.DEFAULT_MAX_FILE_SIZE_IN_KB, fileStorageSettings.MaxSizeInKB);
        }
        
        [Fact]
        public void Should_return_default_max_image_file_size()
        {
            var fileStorageSettings = _context.GetProperty<IFileStorageSettings>();
            Assert.NotNull(fileStorageSettings);
            Assert.Equal(FileStorageSettings.MAX_SUPPORTED_IMAGE_FILE_SIZE_IN_KB, fileStorageSettings.ImageMaxSizeInKB);
        }
        
        [Fact]
        public void Should_return_a_custom_max_file_size()
        {
            _context.SetProperty<IFileStorageSettings>(new FileStorageSettings()
            {
                MaxSizeInKB = 1234
            });
            
            var fileStorageSettings = _context.GetProperty<IFileStorageSettings>();
            Assert.NotNull(fileStorageSettings);
            Assert.Equal(1234, fileStorageSettings.MaxSizeInKB);
        }

        #if FAKE_XRM_EASY_9
        [Fact]
        public void Should_return_default_image_file_size_when_querying_image_attribute_metadata()
        {
            _context.EnableProxyTypes(Assembly.GetAssembly(typeof(dv_test)));
            _context.InitializeMetadata(Assembly.GetAssembly(typeof(dv_test)));

            var entityMetadata = _context.CreateMetadataQuery()
                .FirstOrDefault(em => em.LogicalName == dv_test.EntityLogicalName);
            
            Assert.NotNull(entityMetadata);

            var attributeMetadata = entityMetadata.Attributes.FirstOrDefault(am => am.LogicalName == "dv_image");
            Assert.NotNull(attributeMetadata);

            var imageAttributeMetadata = attributeMetadata as ImageAttributeMetadata;
            Assert.Equal(FileStorageSettings.MAX_SUPPORTED_IMAGE_FILE_SIZE_IN_KB, imageAttributeMetadata.MaxSizeInKB);
        }

        [Fact]
        public void Should_return_default_file_size_when_querying_file_attribute_metadata()
        {
            _context.EnableProxyTypes(Assembly.GetAssembly(typeof(dv_test)));
            _context.InitializeMetadata(Assembly.GetAssembly(typeof(dv_test)));

            var entityMetadata = _context.CreateMetadataQuery()
                .FirstOrDefault(em => em.LogicalName == dv_test.EntityLogicalName);
            
            Assert.NotNull(entityMetadata);

            var attributeMetadata = entityMetadata.Attributes.FirstOrDefault(am => am.LogicalName == "dv_file");
            Assert.NotNull(attributeMetadata);

            var fileAttributeMetadata = attributeMetadata as FileAttributeMetadata;
            Assert.Equal(FileStorageSettings.DEFAULT_MAX_FILE_SIZE_IN_KB, fileAttributeMetadata.MaxSizeInKB);
        }
        #endif
    }
}