using System;
using System.Linq;
using System.Reflection;
using Py_embedded_v37;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            StartFunc();
            PythonAbstractions Python_Functions = new Py_embedded_v37.PythonAbstractions();
            Python_Functions.RunScript();
            dynamic test = Python_Functions.RunFunction(ScriptName: "ConfigLib", FuncName: "main", Args: new dynamic[] { "Hello" });
            Console.WriteLine(test);
            Python_Functions.Python_Console(new string[] { });
        }

        static private void StartFunc()
        {
            AppDomain.CurrentDomain.AssemblyResolve += DLLIntegrator.CurrentDomain_AssemblyResolve;
        }

    }

    class DLLIntegrator
    {
        public static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            // Get assembly name
            var assemblyName = new AssemblyName(args.Name).Name + ".dll";

            // Get resource name
            var resourceName = PythonAbstractions.EmbeddedLibraries.FirstOrDefault(x => x.EndsWith(assemblyName));
            if (resourceName == null)
            {
                return null;
            }

            // Load assembly from resource
            using (var stream = PythonAbstractions.ExecutingAssembly.GetManifestResourceStream(resourceName))
            {
                var bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);
                return Assembly.Load(bytes);
            }
        }
    }
}
