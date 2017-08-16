using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System.Windows.Media;

namespace bmakeLanguageService
{
    /// <summary>
    /// Classification type definition export for BmakeClassifier
    /// </summary>
    internal static class BmakeClassifierClassificationDefinition
    {
        // This disables "The field is never used" compiler's warning. Justification: the field is used by MEF.
#pragma warning disable 169

        public static class bmakeClassificationTypes
        {
            public const string Variable = "Variable";
            public const string At = "At";

            [Export, Name(Variable)]
            public static ClassificationTypeDefinition bmakeClassificationVariable { get; set; }

            [Export, Name(At)]
            public static ClassificationTypeDefinition bmakeClassificationAt { get; set; }
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = bmakeClassificationTypes.Variable)]
        [Name(bmakeClassificationTypes.Variable)]
        [Order(After = Priority.Default)]
        [UserVisible(true)]
        internal sealed class bmakeVariableFormatDefinition : ClassificationFormatDefinition
        {
            public bmakeVariableFormatDefinition()
            {
                ForegroundBrush = Brushes.SlateGray;
                DisplayName = "Identifier";
                IsBold = true;
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = bmakeClassificationTypes.At)]
        [Name(bmakeClassificationTypes.At)]
        [Order(After = Priority.Default)]
        [UserVisible(true)]
        internal sealed class bmakeAtFormatDefinition : ClassificationFormatDefinition
        {
            public bmakeAtFormatDefinition()
            {
                DisplayName = "Quiet";
            }
        }

#pragma warning restore 169
    }
}
