#if FAKE_XRM_EASY_9
using Microsoft.Xrm.Sdk.Query;

namespace FakeXrmEasy.Query
{
    internal static class ColumnSetExtensions
    {
        /// <summary>
        /// Adds column aliases into the ColumnSet that were not explicitly set so that they can be projected in the query execution
        /// </summary>
        /// <param name="columnSet">The ColumnSet with the current attributes and column aliases</param>
        internal static void AddMissingColumnAliases(this ColumnSet columnSet)
        {
            //Add any attribute expression to the list of columns to be projected in case that was missing too
            foreach (var attributeExpression in columnSet.AttributeExpressions)
            {
                if (attributeExpression.AggregateType == XrmAggregateType.None)
                {
                    if (!columnSet.Columns.Contains(attributeExpression.AttributeName))
                    {
                        columnSet.Columns.Add(attributeExpression.AttributeName);
                    }
                }
            }
        }
    }
}
#endif