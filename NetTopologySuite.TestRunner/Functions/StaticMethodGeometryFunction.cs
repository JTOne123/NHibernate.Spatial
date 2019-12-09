﻿using NetTopologySuite.Geometries;
using Open.Topology.TestRunner.Utility;
using System;
using System.Diagnostics;
using System.Reflection;

namespace Open.Topology.TestRunner.Functions
{
    ///<summary>
    /// A <see cref="IGeometryFunction"/> which calls a static <see cref="MethodInfo"/>.
    /// </summary>
    /// <author>Martin Davis</author>
    public class StaticMethodGeometryFunction
        : BaseGeometryFunction
    {
        public static StaticMethodGeometryFunction CreateFunction(MethodInfo method)
        {
            var pi = method.GetParameters();
            Debug.Assert(typeof(Geometry).IsAssignableFrom(pi[0].GetType()));

            Type clz = method.DeclaringType;

            String category = ExtractCategory(ClassUtility.GetClassname(clz));
            String funcName = method.Name;
            String[] paramNames = ExtractParamNames(method);
            Type[] paramTypes = ExtractParamTypes(method);
            Type returnType = method.ReturnType;
            return new StaticMethodGeometryFunction(category, funcName, paramNames, paramTypes,
                                                    returnType, method);
        }

        private static String ExtractCategory(String className)
        {
            String trim = StringUtil.RemoveFromEnd(className, "Functions");
            return trim;
        }

        private static String[] ExtractParamNames(MethodInfo method)
        {
            var pi = method.GetParameters();
            String[] name = new String[pi.Length - 1];
            for (int i = 1; i < name.Length; i++)
                name[i] = "arg" + i;
            return name;
        }

        private static Type[] ExtractParamTypes(MethodInfo method)
        {
            var methodParamTypes = method.GetParameters();
            Type[] types = new Type[methodParamTypes.Length - 1];
            for (int i = 1; i < methodParamTypes.Length; i++)
                types[i - 1] = methodParamTypes[i].ParameterType;
            return types;
        }

        private MethodInfo method;

        public StaticMethodGeometryFunction(
            String category,
            String name,
            String[] parameterNames,
            Type[] parameterTypes,
            Type returnType,
            MethodInfo method)
            : base(category, name, parameterNames, parameterTypes, returnType)
        {
            this.method = method;
        }

        public override object Invoke(Geometry g, Object[] arg)
        {
            return Invoke(method, null, CreateFullArgs(g, arg));
        }

        /**
         * Creates an arg array which includes the target geometry as the first argument
         *
         * @param g
         * @param arg
         * @return
         */

        private static Object[] CreateFullArgs(Geometry g, Object[] arg)
        {
            var fullArgLen = 1;
            if (arg != null)
                fullArgLen = arg.Length + 1;
            var fullArg = new Object[fullArgLen];
            fullArg[0] = g;
            for (var i = 1; i < fullArgLen; i++)
            {
                fullArg[i] = arg[i - 1];
            }
            return fullArg;
        }

        public static Object Invoke(MethodInfo method, Object target, Object[] args)
        {
            Object result;
            try
            {
                result = method.Invoke(target, args);
            }
            catch (TargetInvocationException ex)
            {
                var t = ex.InnerException;
                throw t;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw new Exception(ex.Message);
            }
            return result;
        }

        public static String GetClassname(Type javaClass)
        {
            String jClassName = javaClass.Name;
            int lastDotPos = jClassName.LastIndexOf(".");
            return jClassName.Substring(lastDotPos + 1, jClassName.Length);
        }
    }
}