using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Extensions;
using FakeXrmEasy.Extensions.FetchXml;
using FakeXrmEasy.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;

namespace FakeXrmEasy
{
    public partial class XrmFakedContext : IXrmFakedContext
    {
        public Type FindReflectedType(string logicalName)
        {
            var types =
                ProxyTypesAssemblies.Select(a => FindReflectedType(logicalName, a))
                                    .Where(t => t != null);

            if (types.Count() > 1)
            {
                var errorMsg = $"Type { logicalName } is defined in multiple assemblies: ";
                foreach (var type in types)
                {
                    errorMsg += type.Assembly
                                    .GetName()
                                    .Name + "; ";
                }
                var lastIndex = errorMsg.LastIndexOf("; ");
                errorMsg = errorMsg.Substring(0, lastIndex) + ".";
                throw new InvalidOperationException(errorMsg);
            }

            return types.SingleOrDefault();
        }

        /// <summary>
        /// Finds reflected type for given entity from given assembly.
        /// </summary>
        /// <param name="logicalName">
        /// Entity logical name which type is searched from given
        /// <paramref name="assembly"/>.
        /// </param>
        /// <param name="assembly">
        /// Assembly where early-bound type is searched for given
        /// <paramref name="logicalName"/>.
        /// </param>
        /// <returns>
        /// Early-bound type of <paramref name="logicalName"/> if it's found
        /// from <paramref name="assembly"/>. Otherwise null is returned.
        /// </returns>
        private static Type FindReflectedType(string logicalName,
                                              Assembly assembly)
        {
            try
            {
                if (assembly == null)
                {
                    throw new ArgumentNullException(nameof(assembly));
                }

                var subClassType = assembly.GetTypes()
                        .Where(t => typeof(Entity).IsAssignableFrom(t))
                        .Where(t => t.GetCustomAttributes(typeof(EntityLogicalNameAttribute), true).Length > 0)
                        .Where(t => ((EntityLogicalNameAttribute)t.GetCustomAttributes(typeof(EntityLogicalNameAttribute), true)[0]).LogicalName.Equals(logicalName.ToLower()))
                        .FirstOrDefault();

                return subClassType;
            }
            catch (ReflectionTypeLoadException exception)
            {
                // now look at ex.LoaderExceptions - this is an Exception[], so:
                var s = "";
                foreach (var innerException in exception.LoaderExceptions)
                {
                    // write details of "inner", in particular inner.Message
                    s += innerException.Message + " ";
                }

                throw new Exception("XrmFakedContext.FindReflectedType: " + s);
            }
        }

        
        public Type FindReflectedAttributeType(Type earlyBoundType, string sEntityName, string attributeName)
        {
            //Get that type properties
            var attributeInfo = earlyBoundType.GetEarlyBoundTypeAttribute(attributeName);
            if (attributeInfo == null && attributeName.EndsWith("name"))
            {
                // Special case for referencing the name of a EntityReference
                attributeName = attributeName.Substring(0, attributeName.Length - 4);
                attributeInfo = earlyBoundType.GetEarlyBoundTypeAttribute(attributeName);

                if (attributeInfo.PropertyType != typeof(EntityReference))
                {
                    // Don't mess up if other attributes follow this naming pattern
                    attributeInfo = null;
                }
            }

            if (attributeInfo == null || attributeInfo.PropertyType.FullName == null)
            {
                //Try with metadata
                var injectedType = this.FindAttributeTypeInInjectedMetadata(sEntityName, attributeName);

                if (injectedType == null)
                {
                    throw new Exception($"XrmFakedContext.FindReflectedAttributeType: Attribute {attributeName} not found for type {earlyBoundType}");
                }

                return injectedType;
            }

            if (attributeInfo.PropertyType.FullName.EndsWith("Enum") || attributeInfo.PropertyType.BaseType.FullName.EndsWith("Enum"))
            {
                return typeof(int);
            }

            if (!attributeInfo.PropertyType.FullName.StartsWith("System."))
            {
                try
                {
                    var instance = Activator.CreateInstance(attributeInfo.PropertyType);
                    if (instance is Entity)
                    {
                        return typeof(EntityReference);
                    }
                }
                catch
                {
                    // ignored
                }
            }
#if FAKE_XRM_EASY_2015 || FAKE_XRM_EASY_2016 || FAKE_XRM_EASY_365 || FAKE_XRM_EASY_9
            else if (attributeInfo.PropertyType.FullName.StartsWith("System.Nullable"))
            {
                return attributeInfo.PropertyType.GenericTypeArguments[0];
            }
#endif

            return attributeInfo.PropertyType;
        }

        

        public IQueryable<Entity> CreateQuery(string entityLogicalName)
        {
            return this.CreateQuery<Entity>(entityLogicalName);
        }

        public IQueryable<T> CreateQuery<T>()
            where T : Entity
        {
            var typeParameter = typeof(T);

            if (ProxyTypesAssemblies.Count() == 0)
            {
                //Try to guess proxy types assembly
                var assembly = Assembly.GetAssembly(typeof(T));
                if (assembly != null)
                {
                    EnableProxyTypes(assembly);
                }
            }

            var logicalName = "";

            if (typeParameter.GetCustomAttributes(typeof(EntityLogicalNameAttribute), true).Length > 0)
            {
                logicalName = (typeParameter.GetCustomAttributes(typeof(EntityLogicalNameAttribute), true)[0] as EntityLogicalNameAttribute).LogicalName;
            }

            return this.CreateQuery<T>(logicalName);
        }

        protected IQueryable<T> CreateQuery<T>(string entityLogicalName)
            where T : Entity
        {
            var subClassType = FindReflectedType(entityLogicalName);
            if (subClassType == null && !(typeof(T) == typeof(Entity)) || (typeof(T) == typeof(Entity) && string.IsNullOrWhiteSpace(entityLogicalName)))
            {
                throw new Exception($"The type {entityLogicalName} was not found");
            }

            var lst = new List<T>();
            if (!Data.ContainsKey(entityLogicalName))
            {
                return lst.AsQueryable(); //Empty list
            }

            foreach (var e in Data[entityLogicalName].Values)
            {
                if (subClassType != null)
                {
                    var cloned = e.Clone(subClassType);
                    lst.Add((T)cloned);
                }
                else
                    lst.Add((T)e.Clone());
            }

            return lst.AsQueryable();
        }

        public IQueryable<Entity> CreateQueryFromEntityName(string entityName)
        {
            return Data[entityName].Values.AsQueryable();
        }      


        protected static XElement RetrieveFetchXmlNode(XDocument xlDoc, string sName)
        {
            return xlDoc.Descendants().Where(e => e.Name.LocalName.Equals(sName)).FirstOrDefault();
        }

        public static XDocument ParseFetchXml(string fetchXml)
        {
            try
            {
                return XDocument.Parse(fetchXml);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("FetchXml must be a valid XML document: {0}", ex.ToString()));
            }
        }

        public static QueryExpression TranslateFetchXmlToQueryExpression(XrmFakedContext context, string fetchXml)
        {
            return TranslateFetchXmlDocumentToQueryExpression(context, ParseFetchXml(fetchXml));
        }

        public static QueryExpression TranslateFetchXmlDocumentToQueryExpression(XrmFakedContext context, XDocument xlDoc)
        {
            //Validate nodes
            if (!xlDoc.Descendants().All(el => el.IsFetchXmlNodeValid()))
                throw new Exception("At least some node is not valid");

            //Root node
            if (!xlDoc.Root.Name.LocalName.Equals("fetch"))
            {
                throw new Exception("Root node must be fetch");
            }

            var entityNode = RetrieveFetchXmlNode(xlDoc, "entity");
            var query = new QueryExpression(entityNode.GetAttribute("name").Value);

            query.ColumnSet = xlDoc.ToColumnSet();

            // Ordering is done after grouping/aggregation
            if (!xlDoc.IsAggregateFetchXml())
            {
                var orders = xlDoc.ToOrderExpressionList();
                foreach (var order in orders)
                {
                    query.AddOrder(order.AttributeName, order.OrderType);
                }
            }

            query.Distinct = xlDoc.IsDistincFetchXml();

            query.Criteria = xlDoc.ToCriteria(context);

            query.TopCount = xlDoc.ToTopCount();

            query.PageInfo.Count = xlDoc.ToCount() ?? 0;
            query.PageInfo.PageNumber = xlDoc.ToPageNumber() ?? 1;
            query.PageInfo.ReturnTotalRecordCount = xlDoc.ToReturnTotalRecordCount();

            var linkedEntities = xlDoc.ToLinkEntities(context);
            foreach (var le in linkedEntities)
            {
                query.LinkEntities.Add(le);
            }

            return query;
        }


        protected static Expression TranslateConditionExpressionIn(TypedConditionExpression tc, Expression getAttributeValueExpr, Expression containsAttributeExpr)
        {
            var c = tc.CondExpression;

            BinaryExpression expOrValues = Expression.Or(Expression.Constant(false), Expression.Constant(false));

#if FAKE_XRM_EASY_9
            if (tc.AttributeType == typeof(OptionSetValueCollection))
            {
                var leftHandSideExpression = GetAppropiateCastExpressionBasedOnType(tc.AttributeType, getAttributeValueExpr, null);
                var rightHandSideExpression = Expression.Constant(ConvertToHashSetOfInt(c.Values, isOptionSetValueCollectionAccepted: false));

                expOrValues = Expression.Equal(
                    Expression.Call(leftHandSideExpression, typeof(HashSet<int>).GetMethod("SetEquals"), rightHandSideExpression),
                    Expression.Constant(true));
            }
            else
#endif
            {
                foreach (object value in c.Values)
                {
                    if (value is Array)
                    {
                        foreach (var a in ((Array)value))
                        {
                            expOrValues = Expression.Or(expOrValues, Expression.Equal(
                                GetAppropiateCastExpressionBasedOnType(tc.AttributeType, getAttributeValueExpr, a),
                                GetAppropiateTypedValueAndType(a, tc.AttributeType)));
                        }
                    }
                    else
                    {
                        expOrValues = Expression.Or(expOrValues, Expression.Equal(
                                    GetAppropiateCastExpressionBasedOnType(tc.AttributeType, getAttributeValueExpr, value),
                                    GetAppropiateTypedValueAndType(value, tc.AttributeType)));
                    }
                }
            }

            return Expression.AndAlso(
                            containsAttributeExpr,
                            Expression.AndAlso(Expression.NotEqual(getAttributeValueExpr, Expression.Constant(null)),
                                expOrValues));
        }

        //protected static Expression TranslateConditionExpressionOn(ConditionExpression c, Expression getAttributeValueExpr, Expression containsAttributeExpr)
        //{
        //    BinaryExpression expOrValues = Expression.Or(Expression.Constant(false), Expression.Constant(false));
        //    foreach (object value in c.Values)
        //    {

        //        expOrValues = Expression.Or(expOrValues, Expression.Equal(
        //                    GetAppropiateCastExpressionBasedOnValue(getAttributeValueExpr, value),
        //                    GetAppropiateTypedValue(value)));


        //    }
        //    return Expression.AndAlso(
        //                    containsAttributeExpr,
        //                    Expression.AndAlso(Expression.NotEqual(getAttributeValueExpr, Expression.Constant(null)),
        //                        expOrValues));
        //}

        protected static Expression TranslateConditionExpressionGreaterThanOrEqual(XrmFakedContext context, TypedConditionExpression tc, Expression getAttributeValueExpr, Expression containsAttributeExpr)
        {
            //var c = tc.CondExpression;

            return Expression.Or(
                                TranslateConditionExpressionEqual(context, tc, getAttributeValueExpr, containsAttributeExpr),
                                TranslateConditionExpressionGreaterThan(tc, getAttributeValueExpr, containsAttributeExpr));

        }
        protected static Expression TranslateConditionExpressionGreaterThan(TypedConditionExpression tc, Expression getAttributeValueExpr, Expression containsAttributeExpr)
        {
            var c = tc.CondExpression;

            if (c.Values.Count(v => v != null) != 1)
            {
                throw new FaultException(new FaultReason($"The ConditonOperator.{c.Operator} requires 1 value/s, not {c.Values.Count(v => v != null)}. Parameter Name: {c.AttributeName}"), new FaultCode(""), "");
            }

            if (tc.AttributeType == typeof(string))
            {
                return TranslateConditionExpressionGreaterThanString(tc, getAttributeValueExpr, containsAttributeExpr);
            }
            else if (GetAppropiateTypeForValue(c.Values[0]) == typeof(string))
            {
                return TranslateConditionExpressionGreaterThanString(tc, getAttributeValueExpr, containsAttributeExpr);
            }
            else
            {
                BinaryExpression expOrValues = Expression.Or(Expression.Constant(false), Expression.Constant(false));
                foreach (object value in c.Values)
                {
                    var leftHandSideExpression = GetAppropiateCastExpressionBasedOnType(tc.AttributeType, getAttributeValueExpr, value);
                    var transformedExpression = leftHandSideExpression.TransformValueBasedOnOperator(tc.CondExpression.Operator);

                    expOrValues = Expression.Or(expOrValues,
                            Expression.GreaterThan(
                                transformedExpression,
                                TransformExpressionValueBasedOnOperator(tc.CondExpression.Operator, GetAppropiateTypedValueAndType(value, tc.AttributeType))));
                }
                return Expression.AndAlso(
                                containsAttributeExpr,
                                Expression.AndAlso(Expression.NotEqual(getAttributeValueExpr, Expression.Constant(null)),
                                    expOrValues));
            }

        }

        protected static Expression TranslateConditionExpressionGreaterThanString(TypedConditionExpression tc, Expression getAttributeValueExpr, Expression containsAttributeExpr)
        {
            var c = tc.CondExpression;

            BinaryExpression expOrValues = Expression.Or(Expression.Constant(false), Expression.Constant(false));
            foreach (object value in c.Values)
            {
                var leftHandSideExpression = GetAppropiateCastExpressionBasedOnType(tc.AttributeType, getAttributeValueExpr, value);
                var transformedExpression = TransformExpressionValueBasedOnOperator(tc.CondExpression.Operator, leftHandSideExpression);

                var left = transformedExpression;
                var right = TransformExpressionValueBasedOnOperator(tc.CondExpression.Operator, GetAppropiateTypedValueAndType(value, tc.AttributeType));

                var methodCallExpr = GetCompareToExpression<string>(left, right);

                expOrValues = Expression.Or(expOrValues,
                        Expression.GreaterThan(
                            methodCallExpr,
                            Expression.Constant(0)));
            }
            return Expression.AndAlso(
                            containsAttributeExpr,
                            Expression.AndAlso(Expression.NotEqual(getAttributeValueExpr, Expression.Constant(null)),
                                expOrValues));
        }

        protected static Expression TranslateConditionExpressionLessThanOrEqual(XrmFakedContext context, TypedConditionExpression tc, Expression getAttributeValueExpr, Expression containsAttributeExpr)
        {
            //var c = tc.CondExpression;

            return Expression.Or(
                                TranslateConditionExpressionEqual(context, tc, getAttributeValueExpr, containsAttributeExpr),
                                TranslateConditionExpressionLessThan(tc, getAttributeValueExpr, containsAttributeExpr));

        }

        protected static Expression GetCompareToExpression<T>(Expression left, Expression right)
        {
            return Expression.Call(left, typeof(T).GetMethod("CompareTo", new Type[] { typeof(string) }), new[] { right });
        }

        protected static Expression TranslateConditionExpressionLessThanString(TypedConditionExpression tc, Expression getAttributeValueExpr, Expression containsAttributeExpr)
        {
            var c = tc.CondExpression;

            BinaryExpression expOrValues = Expression.Or(Expression.Constant(false), Expression.Constant(false));
            foreach (object value in c.Values)
            {
                var leftHandSideExpression = GetAppropiateCastExpressionBasedOnType(tc.AttributeType, getAttributeValueExpr, value);
                var transformedLeftHandSideExpression = TransformExpressionValueBasedOnOperator(tc.CondExpression.Operator, leftHandSideExpression);

                var rightHandSideExpression = TransformExpressionValueBasedOnOperator(tc.CondExpression.Operator, GetAppropiateTypedValueAndType(value, tc.AttributeType));

                //var compareToMethodCall = Expression.Call(transformedLeftHandSideExpression, typeof(string).GetMethod("CompareTo", new Type[] { typeof(string) }), new[] { rightHandSideExpression });
                var compareToMethodCall = GetCompareToExpression<string>(transformedLeftHandSideExpression, rightHandSideExpression);

                expOrValues = Expression.Or(expOrValues,
                        Expression.LessThan(compareToMethodCall, Expression.Constant(0)));
            }
            return Expression.AndAlso(
                            containsAttributeExpr,
                            Expression.AndAlso(Expression.NotEqual(getAttributeValueExpr, Expression.Constant(null)),
                                expOrValues));
        }

        protected static Expression TranslateConditionExpressionLessThan(TypedConditionExpression tc, Expression getAttributeValueExpr, Expression containsAttributeExpr)
        {
            var c = tc.CondExpression;

            if (c.Values.Count(v => v != null) != 1)
            {
                throw new FaultException(new FaultReason($"The ConditonOperator.{c.Operator} requires 1 value/s, not {c.Values.Count(v => v != null)}. Parameter Name: {c.AttributeName}"), new FaultCode(""), "");
            }

            if (tc.AttributeType == typeof(string))
            {
                return TranslateConditionExpressionLessThanString(tc, getAttributeValueExpr, containsAttributeExpr);
            }
            else if (GetAppropiateTypeForValue(c.Values[0]) == typeof(string))
            {
                return TranslateConditionExpressionLessThanString(tc, getAttributeValueExpr, containsAttributeExpr);
            }
            else
            {
                BinaryExpression expOrValues = Expression.Or(Expression.Constant(false), Expression.Constant(false));
                foreach (object value in c.Values)
                {
                    var leftHandSideExpression = GetAppropiateCastExpressionBasedOnType(tc.AttributeType, getAttributeValueExpr, value);
                    var transformedExpression = TransformExpressionValueBasedOnOperator(tc.CondExpression.Operator, leftHandSideExpression);

                    expOrValues = Expression.Or(expOrValues,
                            Expression.LessThan(
                                transformedExpression,
                                TransformExpressionValueBasedOnOperator(tc.CondExpression.Operator, GetAppropiateTypedValueAndType(value, tc.AttributeType))));
                }
                return Expression.AndAlso(
                                containsAttributeExpr,
                                Expression.AndAlso(Expression.NotEqual(getAttributeValueExpr, Expression.Constant(null)),
                                    expOrValues));
            }

        }

        protected static Expression TranslateConditionExpressionLast(TypedConditionExpression tc, Expression getAttributeValueExpr, Expression containsAttributeExpr)
        {
            var c = tc.CondExpression;

            var beforeDateTime = default(DateTime);
            var currentDateTime = DateTime.UtcNow;
            switch (c.Operator)
            {
                case ConditionOperator.LastXHours:
                    beforeDateTime = currentDateTime.AddHours(-(int)c.Values[0]);
                    break;
                case ConditionOperator.LastXDays:
                    beforeDateTime = currentDateTime.AddDays(-(int)c.Values[0]);
                    break;
                case ConditionOperator.Last7Days:
                    beforeDateTime = currentDateTime.AddDays(-7);
                    break;
                case ConditionOperator.LastXWeeks:
                    beforeDateTime = currentDateTime.AddDays(-7 * (int)c.Values[0]);
                    break;
                case ConditionOperator.LastXMonths:
                    beforeDateTime = currentDateTime.AddMonths(-(int)c.Values[0]);
                    break;
                case ConditionOperator.LastXYears:
                    beforeDateTime = currentDateTime.AddYears(-(int)c.Values[0]);
                    break;
            }

            c.Values.Clear();          
            c.Values.Add(beforeDateTime);
            c.Values.Add(currentDateTime);
            
            return TranslateConditionExpressionBetween(tc, getAttributeValueExpr, containsAttributeExpr);
        }

        /// <summary>
        /// Takes a condition expression which needs translating into a 'between two dates' expression and works out the relevant dates
        /// </summary>        
        protected static Expression TranslateConditionExpressionBetweenDates(TypedConditionExpression tc, Expression getAttributeValueExpr, Expression containsAttributeExpr, XrmFakedContext context)
        {
            var c = tc.CondExpression;

            DateTime? fromDate = null;
            DateTime? toDate = null;

            var today = DateTime.Today;
            var thisYear = today.Year;
            var thisMonth = today.Month;


            switch (c.Operator)
            {
                case ConditionOperator.ThisYear: // From first day of this year to last day of this year
                    fromDate = new DateTime(thisYear, 1, 1);
                    toDate = new DateTime(thisYear, 12, 31);
                    break;
                case ConditionOperator.LastYear: // From first day of last year to last day of last year
                    fromDate = new DateTime(thisYear - 1, 1, 1);
                    toDate = new DateTime(thisYear - 1, 12, 31);
                    break;
                case ConditionOperator.NextYear: // From first day of next year to last day of next year
                    fromDate = new DateTime(thisYear + 1, 1, 1);
                    toDate = new DateTime(thisYear + 1, 12, 31);
                    break;
                case ConditionOperator.ThisMonth: // From first day of this month to last day of this month                    
                    fromDate = new DateTime(thisYear, thisMonth, 1);
                    // Last day of this month: Add one month to the first of this month, and then remove one day
                    toDate = new DateTime(thisYear, thisMonth, 1).AddMonths(1).AddDays(-1);
                    break;
                case ConditionOperator.LastMonth: // From first day of last month to last day of last month                    
                    fromDate = new DateTime(thisYear, thisMonth, 1).AddMonths(-1);
                    // Last day of last month: One day before the first of this month
                    toDate = new DateTime(thisYear, thisMonth, 1).AddDays(-1);
                    break;
                case ConditionOperator.NextMonth: // From first day of next month to last day of next month
                    fromDate = new DateTime(thisYear, thisMonth, 1).AddMonths(1);
                    // LAst day of Next Month: Add two months to the first of this month, and then go back one day
                    toDate = new DateTime(thisYear, thisMonth, 1).AddMonths(2).AddDays(-1);
                    break;
                case ConditionOperator.ThisWeek:
                    fromDate = today.ToFirstDayOfDeltaWeek();
                    toDate = today.ToLastDayOfDeltaWeek().AddDays(1);
                    break;
                case ConditionOperator.LastWeek:
                    fromDate = today.ToFirstDayOfDeltaWeek(-1);
                    toDate = today.ToLastDayOfDeltaWeek(-1).AddDays(1);
                    break;
                case ConditionOperator.NextWeek:
                    fromDate = today.ToFirstDayOfDeltaWeek(1);
                    toDate = today.ToLastDayOfDeltaWeek(1).AddDays(1);
                    break;
                case ConditionOperator.InFiscalYear:
                    var fiscalYear = (int)c.Values[0];
                    c.Values.Clear();
                    var fiscalYearDate = context.FiscalYearSettings?.StartDate ?? new DateTime(fiscalYear, 4, 1);
                    fromDate = fiscalYearDate;
                    toDate = fiscalYearDate.AddYears(1).AddDays(-1);
                    break;
            }

            c.Values.Add(fromDate);
            c.Values.Add(toDate);

            return TranslateConditionExpressionBetween(tc, getAttributeValueExpr, containsAttributeExpr);
        }


        protected static Expression TranslateConditionExpressionOlderThan(TypedConditionExpression tc, Expression getAttributeValueExpr, Expression containsAttributeExpr)
        {
            var c = tc.CondExpression;

            var valueToAdd = 0;

            if (!int.TryParse(c.Values[0].ToString(), out valueToAdd))
            {
                throw new Exception(c.Operator + " requires an integer value in the ConditionExpression.");
            }

            if (valueToAdd <= 0)
            {
                throw new Exception(c.Operator + " requires a value greater than 0.");
            }

            DateTime toDate = default(DateTime);

            switch (c.Operator)
            {
                case ConditionOperator.OlderThanXMonths:
                    toDate = DateTime.UtcNow.AddMonths(-valueToAdd);
                    break;
#if !FAKE_XRM_EASY && !FAKE_XRM_EASY_2013
                case ConditionOperator.OlderThanXMinutes:      
                    toDate = DateTime.UtcNow.AddMinutes(-valueToAdd);
                    break;
                case ConditionOperator.OlderThanXHours: 
                    toDate = DateTime.UtcNow.AddHours(-valueToAdd);
                    break;
                case ConditionOperator.OlderThanXDays: 
                    toDate = DateTime.UtcNow.AddDays(-valueToAdd);
                    break;
                case ConditionOperator.OlderThanXWeeks:              
                    toDate = DateTime.UtcNow.AddDays(-7 * valueToAdd);
                    break;              
                case ConditionOperator.OlderThanXYears: 
                    toDate = DateTime.UtcNow.AddYears(-valueToAdd);
                    break;
#endif
            }
                        
            return TranslateConditionExpressionOlderThan(tc, getAttributeValueExpr, containsAttributeExpr, toDate);
        }
     

        protected static Expression TranslateConditionExpressionBetween(TypedConditionExpression tc, Expression getAttributeValueExpr, Expression containsAttributeExpr)
        {
            var c = tc.CondExpression;

            object value1, value2;
            value1 = c.Values[0];
            value2 = c.Values[1];

            //Between the range... 
            var exp = Expression.And(
                Expression.GreaterThanOrEqual(
                            GetAppropiateCastExpressionBasedOnType(tc.AttributeType, getAttributeValueExpr, value1),
                            GetAppropiateTypedValueAndType(value1, tc.AttributeType)),

                Expression.LessThanOrEqual(
                            GetAppropiateCastExpressionBasedOnType(tc.AttributeType, getAttributeValueExpr, value2),
                            GetAppropiateTypedValueAndType(value2, tc.AttributeType)));


            //and... attribute exists too
            return Expression.AndAlso(
                            containsAttributeExpr,
                            Expression.AndAlso(Expression.NotEqual(getAttributeValueExpr, Expression.Constant(null)),
                                exp));
        }

        protected static Expression TranslateConditionExpressionNull(TypedConditionExpression tc, Expression getAttributeValueExpr, Expression containsAttributeExpr)
        {
            var c = tc.CondExpression;

            return Expression.Or(Expression.AndAlso(
                                    containsAttributeExpr,
                                    Expression.Equal(
                                    getAttributeValueExpr,
                                    Expression.Constant(null))),   //Attribute is null
                                 Expression.AndAlso(
                                    Expression.Not(containsAttributeExpr),
                                    Expression.Constant(true)));   //Or attribute is not defined (null)
        }

        protected static Expression TranslateConditionExpressionOlderThan(TypedConditionExpression tc, Expression getAttributeValueExpr, Expression containsAttributeExpr, DateTime olderThanDate)
        {
            var lessThanExpression = Expression.LessThan(
                            GetAppropiateCastExpressionBasedOnType(tc.AttributeType, getAttributeValueExpr, olderThanDate),
                            GetAppropiateTypedValueAndType(olderThanDate, tc.AttributeType));

            return Expression.AndAlso(containsAttributeExpr,
                            Expression.AndAlso(Expression.NotEqual(getAttributeValueExpr, Expression.Constant(null)),
                                lessThanExpression));
        }

        protected static Expression TranslateConditionExpressionEndsWith(TypedConditionExpression tc, Expression getAttributeValueExpr, Expression containsAttributeExpr)
        {
            var c = tc.CondExpression;

            //Append a ´%´at the end of each condition value
            var computedCondition = new ConditionExpression(c.AttributeName, c.Operator, c.Values.Select(x => "%" + x.ToString()).ToList());
            var typedComputedCondition = new TypedConditionExpression(computedCondition);
            typedComputedCondition.AttributeType = tc.AttributeType;

            return TranslateConditionExpressionLike(typedComputedCondition, getAttributeValueExpr, containsAttributeExpr);
        }



        protected static Expression TranslateConditionExpressionLike(TypedConditionExpression tc, Expression getAttributeValueExpr, Expression containsAttributeExpr)
        {
            var c = tc.CondExpression;

            BinaryExpression expOrValues = Expression.Or(Expression.Constant(false), Expression.Constant(false));
            Expression convertedValueToStr = Expression.Convert(GetAppropiateCastExpressionBasedOnType(tc.AttributeType, getAttributeValueExpr, c.Values[0]), typeof(string));

            Expression convertedValueToStrAndToLower = GetCaseInsensitiveExpression(convertedValueToStr);

            string sLikeOperator = "%";
            foreach (object value in c.Values)
            {
                var strValue = value.ToString();
                string sMethod = "";

                if (strValue.EndsWith(sLikeOperator) && strValue.StartsWith(sLikeOperator))
                    sMethod = "Contains";

                else if (strValue.StartsWith(sLikeOperator))
                    sMethod = "EndsWith";

                else
                    sMethod = "StartsWith";

                expOrValues = Expression.Or(expOrValues, Expression.Call(
                    convertedValueToStrAndToLower,
                    typeof(string).GetMethod(sMethod, new Type[] { typeof(string) }),
                    Expression.Constant(value.ToString().ToLowerInvariant().Replace("%", "")) //Linq2CRM adds the percentage value to be executed as a LIKE operator, here we are replacing it to just use the appropiate method
                ));
            }

            return Expression.AndAlso(
                            containsAttributeExpr,
                            expOrValues);
        }

        protected static Expression TranslateConditionExpressionContains(TypedConditionExpression tc, Expression getAttributeValueExpr, Expression containsAttributeExpr)
        {
            var c = tc.CondExpression;

            //Append a ´%´at the end of each condition value
            var computedCondition = new ConditionExpression(c.AttributeName, c.Operator, c.Values.Select(x => "%" + x.ToString() + "%").ToList());
            var computedTypedCondition = new TypedConditionExpression(computedCondition);
            computedTypedCondition.AttributeType = tc.AttributeType;

            return TranslateConditionExpressionLike(computedTypedCondition, getAttributeValueExpr, containsAttributeExpr);

        }

        

        

        
        
        
        protected static Expression TranslateConditionExpressionNext(TypedConditionExpression tc, Expression getAttributeValueExpr, Expression containsAttributeExpr)
        {
            var c = tc.CondExpression;

            var nextDateTime = default(DateTime);
            var currentDateTime = DateTime.UtcNow;
            switch (c.Operator)
            {
                case ConditionOperator.NextXHours:
                    nextDateTime = currentDateTime.AddHours((int)c.Values[0]);
                    break;
                case ConditionOperator.NextXDays:
                    nextDateTime = currentDateTime.AddDays((int)c.Values[0]);
                    break;
                case ConditionOperator.Next7Days:
                    nextDateTime = currentDateTime.AddDays(7);
                    break;
                case ConditionOperator.NextXWeeks:                  
                    nextDateTime = currentDateTime.AddDays(7 * (int)c.Values[0]);
                    break;              
                case ConditionOperator.NextXMonths:
                    nextDateTime = currentDateTime.AddMonths((int)c.Values[0]);
                    break;
                case ConditionOperator.NextXYears:
                    nextDateTime = currentDateTime.AddYears((int)c.Values[0]);
                    break;
            }

            c.Values.Clear();
            c.Values.Add(currentDateTime);
            c.Values.Add(nextDateTime);


            return TranslateConditionExpressionBetween(tc, getAttributeValueExpr, containsAttributeExpr);
        }

#if FAKE_XRM_EASY_9
        protected static Expression TranslateConditionExpressionContainValues(TypedConditionExpression tc, Expression getAttributeValueExpr, Expression containsAttributeExpr)
        {
            var leftHandSideExpression = GetAppropiateCastExpressionBasedOnType(tc.AttributeType, getAttributeValueExpr, null);
            var rightHandSideExpression = Expression.Constant(ConvertToHashSetOfInt(tc.CondExpression.Values, isOptionSetValueCollectionAccepted: false));

            return Expression.AndAlso(
                       containsAttributeExpr,
                       Expression.AndAlso(
                           Expression.NotEqual(getAttributeValueExpr, Expression.Constant(null)),
                           Expression.Equal(
                               Expression.Call(leftHandSideExpression, typeof(HashSet<int>).GetMethod("Overlaps"), rightHandSideExpression),
                               Expression.Constant(true))));
        }
#endif
    }
}