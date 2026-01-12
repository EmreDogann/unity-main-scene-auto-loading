using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Ems.MainSceneAutoLoading.Utilities
{
    public static class ReflectionUtility
    {
        /// <summary>
        ///     Compile a lambda that will invoke the specified property's getter and return the value.
        /// </summary>
        /// <param name="type">The type of the instance the property is defined in.</param>
        /// <param name="memberName">The name of the property member.</param>
        /// <param name="bindingFlags">The flags used to find the property.</param>
        /// <typeparam name="T">The instance type to access the property from (use 'object' if unknown at compile-time).</typeparam>
        /// <typeparam name="U">The return value type (use 'object' if unknown at compile-time).</typeparam>
        /// <returns>Lambda which can be invoked like a normal function. (e.g. lambdaName(instance))</returns>
        public static Func<T, U> BuildPropertyGetter<T, U>(Type type, string memberName, BindingFlags bindingFlags)
        {
            ParameterExpression instanceParam = Expression.Parameter(typeof(T), "t");

            Expression instanceExpr = null;
            if ((bindingFlags & BindingFlags.Static) != BindingFlags.Static)
            {
                // Cast the instance from "object" to the correct type.
                if (typeof(T) == typeof(object))
                {
                    instanceExpr = Expression.TypeAs(instanceParam, type);
                }
            }

            MemberInfo propertyInfo = type.GetProperty(memberName, bindingFlags);
            // t.[PropertyName]
            MemberExpression propertyAccessExpr = Expression.MakeMemberAccess(instanceExpr, propertyInfo);

            // Convert the return value to the correct type: Convert(t.PropertyName, typeof(U))
            UnaryExpression returnConvert = Expression.Convert(propertyAccessExpr, typeof(U));

            var lambda = Expression.Lambda<Func<T, U>>(returnConvert, instanceParam);
            return lambda.Compile();
        }

        /// <summary>
        ///     Compile a lambda that will invoke the specified property's static getter and return the value.
        /// </summary>
        /// <param name="type">The type of the instance the property is defined in.</param>
        /// <param name="memberName">The name of the property member.</param>
        /// <param name="bindingFlags">The flags used to find the property.</param>
        /// <typeparam name="T">The return value type (use 'object' if unknown at compile-time).</typeparam>
        /// <returns>Lambda which can be invoked like a normal function. (e.g. lambdaName(instance))</returns>
        public static Func<T> BuildPropertyGetter<T>(Type type, string memberName, BindingFlags bindingFlags)
        {
            MemberInfo propertyInfo = type.GetProperty(memberName, bindingFlags);
            // t.[PropertyName]
            MemberExpression propertyAccessExpr = Expression.MakeMemberAccess(null, propertyInfo);

            // Convert the return value to the correct type: Convert(t.PropertyName, typeof(T))
            UnaryExpression returnConvert = Expression.Convert(propertyAccessExpr, typeof(T));

            var lambda = Expression.Lambda<Func<T>>(returnConvert, null);
            return lambda.Compile();
        }

        /// <summary>
        ///     Compile a lambda that will invoke the specified method and return its value.
        /// </summary>
        /// <param name="type">The type of the instance the method is defined in.</param>
        /// <param name="methodName">The name of the method.</param>
        /// <param name="bindingFlags">The flags used to find the method.</param>
        /// <typeparam name="T">The instance type to call the method from (use 'object' if unknown at compile-time).</typeparam>
        /// <typeparam name="U">The return value type (use 'object' if unknown at compile-time).</typeparam>
        /// <returns>Lambda which can be invoked like a normal function. (e.g. lambdaName(instance))</returns>
        public static Func<T, U> BuildMethodInvoker<T, U>(Type type, string methodName, BindingFlags bindingFlags)
        {
            ParameterExpression instanceParam = Expression.Parameter(typeof(T), "t");

            Expression instanceExpr = instanceParam;
            // Cast the instance from "object" to the correct type.
            if (typeof(T) == typeof(object))
            {
                instanceExpr = Expression.TypeAs(instanceParam, type);
            }

            MethodInfo methodInfo = type.GetMethod(methodName, bindingFlags);
            MethodCallExpression methodExpr = Expression.Call(instanceExpr, methodInfo);

            // Convert the return value to the correct type: Convert(t.[methodName](), typeof(U))
            UnaryExpression returnConvert = Expression.Convert(methodExpr, typeof(U));

            // Create delegate
            var lambda = Expression.Lambda<Func<T, U>>(returnConvert, instanceParam);
            return lambda.Compile();
        }

        /// <summary>
        ///     Compile a lambda that will invoke the specified method. Method must be void return and must accept two parameters.
        /// </summary>
        /// <param name="type">The type of the instance the method is defined in.</param>
        /// <param name="methodName">The name of the method.</param>
        /// <param name="bindingFlags">The flags used to find the method.</param>
        /// <typeparam name="T">The instance type to call the method from (use 'object' if unknown at compile-time).</typeparam>
        /// <typeparam name="U">The first parameter type.</typeparam>
        /// <returns>A lambda which can be invoked like a normal function. (e.g. lambdaName(instance, param1))</returns>
        public static Action<T, U> BuildMethodInvoker_VoidReturn<T, U>(Type type, string methodName,
            BindingFlags bindingFlags)
        {
            ParameterExpression instanceParam = Expression.Parameter(typeof(T), "t");
            ParameterExpression param = Expression.Parameter(typeof(U), "param");

            Expression instanceExpr = instanceParam;
            // Cast the instance from "object" to the correct type.
            if (typeof(T) == typeof(object))
            {
                instanceExpr = Expression.TypeAs(instanceParam, type);
            }

            MethodInfo methodInfo = type.GetMethod(methodName, bindingFlags);
            MethodCallExpression methodExpr = Expression.Call(instanceExpr, methodInfo, param);

            // Create delegate
            var lambda = Expression.Lambda<Action<T, U>>(methodExpr, instanceParam, param);
            return lambda.Compile();
        }

        /// <summary>
        ///     Compile a lambda that will invoke the specified method. Method must be void return and must accept two parameters.
        /// </summary>
        /// <param name="type">The type of the instance the method is defined in.</param>
        /// <param name="methodName">The name of the method.</param>
        /// <param name="bindingFlags">The flags used to find the method.</param>
        /// <typeparam name="T">The instance type to call the method from (use 'object' if unknown at compile-time).</typeparam>
        /// <typeparam name="U">The first parameter type.</typeparam>
        /// <typeparam name="V">The second parameter type.</typeparam>
        /// <returns>A lambda which can be invoked like a normal function. (e.g. lambdaName(instance, param1, param2))</returns>
        public static Action<T, U, V> BuildMethodInvoker_VoidReturn<T, U, V>(Type type, string methodName,
            BindingFlags bindingFlags)
        {
            ParameterExpression instanceParam = Expression.Parameter(typeof(T), "t");
            ParameterExpression param1 = Expression.Parameter(typeof(U), "param1");
            ParameterExpression param2 = Expression.Parameter(typeof(V), "param2");

            Expression instanceExpr = instanceParam;
            // Cast the instance from "object" to the correct type.
            if (typeof(T) == typeof(object))
            {
                instanceExpr = Expression.TypeAs(instanceParam, type);
            }

            MethodInfo methodInfo = type.GetMethod(methodName, bindingFlags);
            MethodCallExpression methodExpr = Expression.Call(instanceExpr, methodInfo, param1, param2);

            // Create delegate
            var lambda = Expression.Lambda<Action<T, U, V>>(methodExpr, instanceParam, param1, param2);
            return lambda.Compile();
        }
    }
}