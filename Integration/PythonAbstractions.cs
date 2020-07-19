﻿using static Python.Runtime.PythonEngine;
using System;
using System.Reflection;
using Python.Runtime;

namespace Py_embedded_v37
{
    public class PythonAbstractions
    {

        public void Initpython(string custom_PATH = "")
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
            return variable.As<dynamic>();
            
        }

        public string Get_python_type(PyObject variable)
        {
            Initpython();
            var type = variable.GetPythonType();
            string stringtype = type.ToString();
            Console.WriteLine(stringtype);
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
                        pyArgs[x] = (PyObject)Args[x];
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
