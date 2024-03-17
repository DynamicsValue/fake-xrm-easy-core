using System;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace FakeXrmEasy.Query
{
    public static partial class ConditionExpressionExtensions
    {
        internal static Expression ToLikeExpression(this TypedConditionExpression tc, Expression getAttributeValueExpr, Expression containsAttributeExpr)
        {
            var c = tc.CondExpression;
            BinaryExpression expOrValues = Expression.Or(Expression.Constant(false), Expression.Constant(false));
            Expression convertedValueToStr = Expression.Convert(tc.AttributeType.GetAppropiateCastExpressionBasedOnType(getAttributeValueExpr, c.Values[0]), typeof(string));

            Expression convertedValueToStrAndToLower = convertedValueToStr.ToCaseInsensitiveExpression();


            foreach (object value in c.Values)
            {
                //convert a like into a regular expression
                string input = value.ToString();
                string result = "^";
                int lastMatch = 0;
                var regex = new Regex("([^\\[]*)(\\[[^\\]]*\\])", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                foreach (Match match in regex.Matches(input))
                {
                    if (match.Groups[1].Success)
                    {
                        result += ConvertToRegexDefinition(match.Groups[1].Value);
                    }
                    result += match.Groups[2].Value.Replace("\\", "\\\\");
                    lastMatch = match.Index + match.Length;
                }
                if (input.Length != lastMatch)
                {
                    result += ConvertToRegexDefinition(input.Substring(lastMatch));
                }
                result += "$";

                regex = new Regex(result, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

                expOrValues = Expression.Or(expOrValues, Expression.Call(
                    Expression.Constant(regex),
                    typeof(Regex).GetMethod("IsMatch", new Type[] { typeof(string) }),
                    convertedValueToStrAndToLower) //Linq2CRM adds the percentage value to be executed as a LIKE operator, here we are replacing it to just use the appropiate method
                );
            }

            return Expression.AndAlso(
                            containsAttributeExpr,
                            expOrValues);
        }

        private static string ConvertToRegexDefinition(string value)
        {
            return value.Replace("\\", "\\\\").Replace("(", "\\(").Replace("{", "\\{").Replace(".", "\\.").Replace("*", "\\*").Replace("+", "\\+").Replace("?", "\\?").Replace("%", ".*").Replace("_", ".");
        }
    }
}
