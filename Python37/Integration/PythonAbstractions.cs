using static Python.Runtime.PythonEngine;
using Python.Runtime;
using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Runtime;

namespace Py_embedded_v37
{
    public class PythonAbstractions
    {
        public static Assembly ExecutingAssembly = Assembly.GetExecutingAssembly();
        public static string[] EmbeddedLibraries = ExecutingAssembly.GetManifestResourceNames().Where(x => x.EndsWith(".dll")).ToArray();
        private void Create_Windows_EnvVariables(string custom_PATH)
        {
            string pathToPython = @"\Python37\Windows";
            string path = pathToPython + ";" +
            Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("PATH", path, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("PYTHONHOME", pathToPython, EnvironmentVariableTarget.Process);

            var lib = new[]
                {
                @"\Python37\Windows\Lib",
                @"\Python37\Windows\DLLs",
                @"\Python37\Windows\Lib\site-packages",
                @"\Scripts"
                };

            if (custom_PATH != "")
            {
                lib = new[]
                {
                @"\Python37\Windows\Lib",
                @"\Python37\Windows\DLLs",
                @"\Python37\Windows\Lib\site-packages",
                @"\Scripts",
                custom_PATH
                };
            }

            string paths = string.Join("; ", lib);
            Environment.SetEnvironmentVariable("PYTHONPATH", paths, EnvironmentVariableTarget.Process);
        }

        public void Initpython(string custom_PATH = "")
        {
            Create_Windows_EnvVariables(custom_PATH);
            if (Environment.OSVersion.ToString().Contains("Unix"))
            {
                Console.WriteLine("IsLinux");
            }
            Initialize();
            
        }

        string FSpath_to_PyPath(string FSPath)
        {
            FSPath = FSPath.Replace('\\', '/');
            string[] Path_Split = FSPath.Split('/');
            string pyPath = Path_Split[Path_Split.Length - 1];
            if (pyPath != "/")
            {
                pyPath = pyPath.Trim().Replace("/", "");
            }

            else
            {
                pyPath = Path_Split[Path_Split.Length - 2];
                pyPath = pyPath.Trim().Remove('/');
            }


            return pyPath;
        }

        

        public void TerminatePython()
        {
            try
            {
                Shutdown();
            }
            catch (PythonException Exception)
            {
                Console.WriteLine(Exception.Message);
            }

        }

        public bool RunScript(string ScriptLocation = "Scripts", string ScriptName = "Main.py", string FunctionStart = "")
        {
            ScriptLocation = FSpath_to_PyPath(ScriptLocation);
            if (ScriptLocation != "Scripts")
            {
                Initpython(ScriptLocation);
            }
            else
            {
                Initpython();
            }

            try
            {
                var Script = ImportModule(ScriptLocation + "." + ScriptName);
                if (FunctionStart != "")
                {
                    Script.InvokeMethod(FunctionStart);
                }
            }
            catch (PythonException Exception)
            {
                Console.WriteLine(Exception.Message);
                TerminatePython();
                return false;
            }
            TerminatePython();
            return true;

        }

        public dynamic ToCSharp(PyObject variable)
        {
            Initpython();
            string Pytype = variable.GetPythonType().ToString();
            if (Pytype == "<class 'int'>" || Pytype == "<class 'long'>")
            {
                if (variable.As<long>() >= int.MaxValue)
                {
                    var CSvar = variable.As<long>();
                    TerminatePython();
                    return CSvar;
                }
                else
                {
                    var CSvar = variable.As<int>();
                    TerminatePython();
                    return CSvar;
                }
                
            }
            else if (Pytype == "<class 'short'>")
            {
                var CSvar = variable.As<short>();
                TerminatePython();
                return CSvar;
            }

            else if (Pytype == "<class 'str'>")
            {
                var CSvar = variable.As<string>();
                TerminatePython();
                return CSvar;
            }
            else if (Pytype == "<class 'bool'>")
            {
                var CSvar = variable.As<bool>();
                TerminatePython();
                return CSvar;
            }
            else if (Pytype == "<class 'NoneType'>")
            {
                TerminatePython();
                return null;
            }
            else if (Pytype == "<class 'list'>")
            {
                List<dynamic> list_var = new List<dynamic>();
                foreach (PyObject item in variable)
                {
                    dynamic thing = ToCSharp(item);
                    list_var.Add(thing);
                }
                TerminatePython();
                return list_var;
            }

            return null;
        }

        public int Python_Console(string[] args)
        {
            Initpython();

            string[] cmd = args;

            int i = Runtime.Py_Main(cmd.Length, cmd);
            TerminatePython();

            return i;

        }



        public string Get_python_type(PyObject variable)
        {
            Initpython();
            var type = variable.GetPythonType();
            string stringtype = type.ToString();
            TerminatePython();
            return stringtype;

        }

        public dynamic RunFunction(string ScriptLocation = "Scripts", string ScriptName = "Main.py", string FuncName = "Main", dynamic[] Args = null)
        {
            Initpython(ScriptLocation);
            ScriptLocation = FSpath_to_PyPath(ScriptLocation);

            Initpython();


            dynamic return_value;
            try
            {
                Console.WriteLine("Function Starting");
                dynamic Script = ImportModule(ScriptLocation + "." + ScriptName);
                if (Args == null)
                {
                    return_value = Script.InvokeMethod(FuncName);
                }
                else
                {
                    PyObject[] pyArgs = new PyObject[Args.Length];

                    for (int x = 0; x < Args.Length; x++)
                    {
                        var Argsitem = (object)Args[x];
                        var item = Argsitem.ToPython();
                        pyArgs[x] = item;
                    }

                    return_value = Script.InvokeMethod(FuncName, pyArgs);
                }

            }
            catch (PythonException Exception)
            {
                Console.WriteLine(Exception.Message);
                TerminatePython();
                return null;
            }
            TerminatePython();
            return ToCSharp(return_value);
        }
    }
}
