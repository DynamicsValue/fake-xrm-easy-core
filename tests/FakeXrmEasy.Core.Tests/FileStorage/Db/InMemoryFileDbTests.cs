using System;
using DataverseEntities;
using FakeXrmEasy.Core.FileStorage;
using FakeXrmEasy.Core.FileStorage.Db;
using Microsoft.Xrm.Sdk;
using Xunit;

namespace FakeXrmEasy.Core.Tests.FileStorage.Db
{
    public class InMemoryFileDbTests
    {
        private readonly InMemoryFileDb _fileDb;
        private readonly FileUploadProperties _fileUploadProperties;
        
        public InMemoryFileDbTests()
        {
            _fileDb = new InMemoryFileDb();
            _fileUploadProperties = new FileUploadProperties()
            {
                Target = new EntityReference(dv_test.EntityLogicalName, Guid.NewGuid()),
                FileName = "FileName.pdf",
                FileAttributeName = "dv_file"
            };
        }
        
        [Fact]
        public void Should_create_an_empty_in_memory_file_db()
        {
            Assert.Empty(_fileDb.GetAllFileUploadSessions());
        }
        
        [Fact]
        public void Should_init_and_store_file_upload_session()
        {
            var fileContinuationToken = _fileDb.InitFileUploadSession(_fileUploadProperties);

            var fileUploadSession = _fileDb.GetFileUploadSession(fileContinuationToken);
            
            Assert.NotNull(fileUploadSession);

            Assert.Equal(fileContinuationToken, fileUploadSession.FileUploadSessionId);
            Assert.Equal(_fileUploadProperties.Target.LogicalName, fileUploadSession.Properties.Target.LogicalName);
            Assert.Equal(_fileUploadProperties.Target.Id, fileUploadSession.Properties.Target.Id);
            Assert.Equal(_fileUploadProperties.FileName, fileUploadSession.Properties.FileName);
            Assert.Equal(_fileUploadProperties.FileAttributeName, fileUploadSession.Properties.FileAttributeName);
        }

        
    }
}