using Microsoft.Xrm.Sdk.Metadata;

namespace FakeXrmEasy.Core.Extensions
{
    /// <summary>
    /// Extension methods for EntityKeyMetadata
    /// </summary>
    public static class EntityKeyMetadataExtensions
    {
        /// <summary>
        /// Returns the user localized label of the key if one was set in metadata, or the list of columns that are part of the key otherwise
        /// </summary>
        /// <param name="keyMetadata"></param>
        /// <returns></returns>
        public static string GetDisplayName(this EntityKeyMetadata keyMetadata)
        {
            var label = keyMetadata.DisplayName?.UserLocalizedLabel?.Label;
            if (!string.IsNullOrWhiteSpace(label))
            {
                return label;
            }
                
            return string.Join(",", keyMetadata.KeyAttributes);
        }
    }
}