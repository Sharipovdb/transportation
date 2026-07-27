using System.Reflection;

namespace Transportation.Application;

public static class ApplicationRef
{
    public static Assembly Assembly => typeof(ApplicationRef).Assembly;
}