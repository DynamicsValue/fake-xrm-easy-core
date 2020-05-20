using FakeXrmEasy.Abstractions;
using System;

namespace FakeXrmEasy
{
    public partial class XrmFakedContext : IXrmFakedContext
    {
        public TimeZoneInfo SystemTimeZone { get; set; }
    }
}