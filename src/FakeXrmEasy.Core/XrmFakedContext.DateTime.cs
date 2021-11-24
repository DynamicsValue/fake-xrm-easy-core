using FakeXrmEasy.Abstractions;
using System;

namespace FakeXrmEasy
{
    public partial class XrmFakedContext : IXrmFakedContext
    {
        /// <summary>
        /// System TimeZone
        /// </summary>
        public TimeZoneInfo SystemTimeZone { get; set; }
    }
}