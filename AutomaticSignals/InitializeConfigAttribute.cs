using System;

namespace AutomaticSignals;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class InitializeConfigAttribute : Attribute {
}