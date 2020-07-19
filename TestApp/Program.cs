using System;
using Py_embedded_v37;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            PythonAbstractions Python_Functions = new Py_embedded_v37.PythonAbstractions();
            Python_Functions.RunScript();
            dynamic test = Python_Functions.RunFunction(ScriptName: "ConfigLib", FuncName: "main", Args: new dynamic[] { 10 });
        }

    }
}
