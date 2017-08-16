using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace bmakeLanguageService
{
    public class bmakeContentTypeDefinition
    {
        public const string bmakeContentType = "Make";

        [Export(typeof(ContentTypeDefinition))]
        [Name(bmakeContentType)]
        [BaseDefinition("plaintext")]
        public ContentTypeDefinition BbmakeContentType { get; set; }

        [Export(typeof(FileExtensionToContentTypeDefinition))]
        [ContentType(bmakeContentType)]
        [FileExtension(".mke")]
        public FileExtensionToContentTypeDefinition mkeFileExtension { get; set; }

        [Export(typeof(FileExtensionToContentTypeDefinition))]
        [ContentType(bmakeContentType)]
        [FileExtension(".mki")]
        public FileExtensionToContentTypeDefinition mkiFileExtension { get; set; }
    }
    /// <summary>
    /// Classifier provider. It adds the classifier to the set of classifiers.
    /// </summary>
    [Export(typeof(IClassifierProvider))]
    [ContentType(bmakeContentTypeDefinition.bmakeContentType)]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal class BmakeClassifierProvider : IClassifierProvider
    {
        // Disable "Field is never assigned to..." compiler's warning. Justification: the field is assigned by MEF.
#pragma warning disable 649

        /// <summary>
        /// Classification registry to be used for getting a reference
        /// to the custom classification type later.
        /// </summary>
        [Import]
        private IClassificationTypeRegistryService classificationRegistry { get; set; }

#pragma warning restore 649

        #region IClassifierProvider

        /// <summary>
        /// Gets a classifier for the given text buffer.
        /// </summary>
        /// <param name="buffer">The <see cref="ITextBuffer"/> to classify.</param>
        /// <returns>A classifier for the text buffer, or null if the provider cannot do so in its current state.</returns>
        public IClassifier GetClassifier(ITextBuffer buffer)
        {
            return buffer.Properties.GetOrCreateSingletonProperty<BmakeClassifier>(creator: () => new BmakeClassifier(this.classificationRegistry));
        }

        #endregion
    }
}
