#if FAKE_XRM_EASY_9
using System;
using System.Collections.Generic;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.FileStorage;
using FakeXrmEasy.Core.FileStorage.Db.Exceptions;

namespace FakeXrmEasy
{
    public partial class XrmFakedContext : IXrmFakedContext
    {
        /// <summary>
        /// Initializes the context with a given state of pre-existing file uploads
        /// </summary>
        /// <param name="files">The list of files used to initialised the In-Memory File Storage. InitializeMetadata and Initialize must have been called prior to calling this method.</param>
        public void InitializeFiles(IEnumerable<IFileAttachment> files)
        {
            if (!MetadataInitialized)
            {
                throw new InitializeMetadataNotCalledException();
            }
            
            if (!Initialized)
            {
                throw new InitializeNotCalledException();
            }
            
            if (FilesInitialized)
            {
                throw new AlreadyInitializedFilesException();
            }
            
            foreach (var file in files)
            {
                FileDb.AddFile(file);
            }

            FilesInitialized = true;
        }
    }
}
#endif
