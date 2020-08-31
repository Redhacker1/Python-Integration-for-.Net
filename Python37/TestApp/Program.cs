using System;
using System.Linq;
using System.Reflection;
using Py_embedded;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            PythonAbstractions Python_Functions = new PythonAbstractions();
            //Python_Functions.RunScript();
            dynamic test = Python_Functions.RunFunction(ScriptName: "ConfigLib", FuncName: "main", Args: new dynamic[] { "Hello" });
            Python_Functions.Python_Console(new string[] { });
        }

    }

    class DLLIntegrator
    {
    }
}
