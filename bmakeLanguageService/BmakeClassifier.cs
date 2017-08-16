using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using System.Text.RegularExpressions;

namespace bmakeLanguageService
{
    /// <summary>
    /// Classifier that classifies all text as an instance of the "BmakeClassifier" classification type.
    /// </summary>
    internal class BmakeClassifier : IClassifier
    {
        /// <summary>
        /// Classification type.
        /// </summary>
        //private IClassificationType _comment, _variable, _keyword, _reference, _referenceValue, _preProcess, _logic, _string, _digit;
        private IClassificationType _comment, _keyword, _reference, _referenceValue, _preProcess, _logic, _string, _digit;
        private static Regex _commentRegex = new Regex(@"([^\S\n]*#.+)", RegexOptions.Compiled);
        //private static Regex _variableRegex = new Regex(@"(?<name>[^\f\n\r\t\v=]+)(?<value>[^\S\n]*=[^\S\n]*[^\s=\d]+)", RegexOptions.Compiled);
        private static Regex _keywordRegex = new Regex(@"(%include\b|%undef\b|%if defined\b|%if\b|%elif\b|%else\b|%endif\b|%warn|%error|always)", RegexOptions.Compiled);
        private static Regex _referenceRegex = new Regex(@"(?<start>\$(\(|\{))(?<value>[\s\S]*?)(?<end>\)|\})", RegexOptions.Compiled);
        private static Regex _preProcessRegex = new Regex(@"@", RegexOptions.Compiled);
        private static Regex _logicRegex = new Regex(@"(\|\||&&|!=|=|\+)", RegexOptions.Compiled);
        private static Regex _stringRegex = new Regex(@"(""[^\n\r""]*"")", RegexOptions.Compiled);
        private static Regex _digitRegex = new Regex(@"(\b\d+(\.\d+)?\b)", RegexOptions.Compiled);

        /// <summary>
        /// Initializes a new instance of the <see cref="BmakeClassifier"/> class.
        /// </summary>
        /// <param name="registry">Classification registry.</param>
        internal BmakeClassifier(IClassificationTypeRegistryService registry)
        {
            this._comment = registry.GetClassificationType(PredefinedClassificationTypeNames.Comment);
            //this._variable = registry.GetClassificationType(PredefinedClassificationTypeNames.Identifier);
            this._keyword = registry.GetClassificationType(PredefinedClassificationTypeNames.Keyword);
            this._reference = registry.GetClassificationType(PredefinedClassificationTypeNames.MarkupAttribute);
            this._referenceValue = registry.GetClassificationType(PredefinedClassificationTypeNames.MarkupAttributeValue);
            this._preProcess = registry.GetClassificationType(PredefinedClassificationTypeNames.PreprocessorKeyword);
            this._logic = registry.GetClassificationType(PredefinedClassificationTypeNames.Operator);
            this._string = registry.GetClassificationType(PredefinedClassificationTypeNames.String);
            this._digit = registry.GetClassificationType(PredefinedClassificationTypeNames.Number);
        }

        #region IClassifier

#pragma warning disable 67

        /// <summary>
        /// An event that occurs when the classification of a span of text has changed.
        /// </summary>
        /// <remarks>
        /// This event gets raised if a non-text change would affect the classification in some way,
        /// for example typing /* would cause the classification to change in C# without directly
        /// affecting the span.
        /// </remarks>
        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;

#pragma warning restore 67

        /// <summary>
        /// Gets all the <see cref="ClassificationSpan"/> objects that intersect with the given range of text.
        /// </summary>
        /// <remarks>
        /// This method scans the given SnapshotSpan for potential matches for this classification.
        /// In this instance, it classifies everything and returns each span as a new ClassificationSpan.
        /// </remarks>
        /// <param name="span">The span currently being classified.</param>
        /// <returns>A list of ClassificationSpans that represent spans identified to be of this classification.</returns>
        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            string text = span.GetText();
            var list = new List<ClassificationSpan>();

            if (string.IsNullOrWhiteSpace(text))
                return list;

            // match comments
            var comment = _commentRegex.Match(text);
            if (comment.Success)
            {
                var result = new SnapshotSpan(span.Snapshot, span.Start + comment.Index, comment.Length);
                list.Add(new ClassificationSpan(result, _comment));

                // if the index is 0, the whole line is a comment, so we can return here
                if (comment.Index == 0)
                    return list;
            }

            // Removed. Left in case I decide to restore it. Don't sue me. - Nathan.Smith 8/17
            // match variable definitions
            //var variable = _variableRegex.Match(text);
            //if(variable.Success)
            //{
            //    var result = GetSpan(span, variable.Groups["name"], _variable);
            //    list.Add(result);
            //}

            // match keywords
            var keywords = _keywordRegex.Matches(text);
            if(keywords.Count > 0)
            {
                foreach (Match m in keywords)
                {
                    var result = new SnapshotSpan(span.Snapshot, span.Start + m.Index, m.Length);
                    list.Add(new ClassificationSpan(result, _keyword));
                }
            }

            // match references
            var references = _referenceRegex.Matches(text);
            if(references.Count > 0)
            {
                foreach (Match m in references)
                {
                    var result = GetSpan(span, m.Groups["start"], _reference);
                    if (result != null)
                        list.Add(result);
                    result = GetSpan(span, m.Groups["value"], _referenceValue);
                    if (result != null)
                        list.Add(result);
                    result = GetSpan(span, m.Groups["end"], _reference);
                    if (result != null)
                        list.Add(result);
                }
            }

            // match preprocess symbol
            var preProcess = _preProcessRegex.Match(text);
            if (preProcess.Success)
            {
                var result = new SnapshotSpan(span.Snapshot, span.Start + preProcess.Index, preProcess.Length);
                list.Add(new ClassificationSpan(result, _preProcess));
            }

            // match operators
            var operators = _logicRegex.Matches(text);
            if(operators.Count > 0)
            {
                foreach (Match m in operators)
                {
                    var result = new SnapshotSpan(span.Snapshot, span.Start + m.Index, m.Length);
                    list.Add(new ClassificationSpan(result, _logic));
                }
            }

            // match strings
            var strings = _stringRegex.Matches(text);
            if(strings.Count > 0)
            {
                foreach (Match m in strings)
                {
                    var result = new SnapshotSpan(span.Snapshot, span.Start + m.Index, m.Length);
                    list.Add(new ClassificationSpan(result, _string));
                }
            }

            // match numbers
            var numbers = _digitRegex.Matches(text);
            if(numbers.Count > 0)
            {
                foreach (Match m in numbers)
                {
                    var result = new SnapshotSpan(span.Snapshot, span.Start + m.Index, m.Length);
                    list.Add(new ClassificationSpan(result, _digit));
                }
            }

            return list;
        }

        private ClassificationSpan GetSpan(SnapshotSpan span, Group group, IClassificationType type)
        {
            if (group.Length > 0)
            {
                var result = new SnapshotSpan(span.Snapshot, span.Start + group.Index, group.Length);
                return new ClassificationSpan(result, type);
            }

            return null;
        }

        #endregion
    }
}
