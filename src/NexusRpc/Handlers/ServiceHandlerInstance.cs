using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NexusRpc.Handlers
{
    /// <summary>
    /// Representation of a service handler instance with instantiated operation handlers. Users
    /// should use <see cref="FromInstance"/> to get an instance.
    /// </summary>
    public class ServiceHandlerInstance
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceHandlerInstance"/> class.
        /// </summary>
        /// <param name="definition">Definition for this service.</param>
        /// <param name="operationHandlers">All instantiated operation handlers, keyed by operation
        /// name.</param>
        /// <remarks>
        /// This is not commonly used, users should prefer <see cref="FromInstance"/> with an
        /// instantiated instance of a class with a <see cref="NexusServiceHandlerAttribute"/>
        /// attribute. This constructor does minimal validation on operation handlers.
        /// </remarks>
        public ServiceHandlerInstance(
            ServiceDefinition definition,
            IReadOnlyDictionary<string, IOperationHandler<object?, object?>> operationHandlers)
        {
            Definition = definition;
            OperationHandlers = operationHandlers;

            // We need to make sure every definition has a handler
            var missingHandlers = definition.Operations.Keys.Except(operationHandlers.Keys).ToList();
            if (missingHandlers.Count > 0)
            {
                throw new ArgumentException(
                    $"Missing handlers for defined operations: '{string.Join("', '", missingHandlers)}'");
            }
            var extraHandlers = operationHandlers.Keys.Except(definition.Operations.Keys).ToList();
            if (extraHandlers.Count > 0)
            {
                throw new ArgumentException(
                    $"Extra handlers without defined operations: '{string.Join("', '", extraHandlers)}'");
            }
        }

        /// <summary>
        /// Gets the service definition.
        /// </summary>
        public ServiceDefinition Definition { get; private init; }

        /// <summary>
        /// Gets the operation handlers, keyed by operation name.
        /// </summary>
        public IReadOnlyDictionary<string, IOperationHandler<object?, object?>> OperationHandlers { get; private init; }

        /// <summary>
        /// Create a service handler instance from an instance of a class with a
        /// <see cref="NexusServiceHandlerAttribute"/> attribute. This is the primary way to create
        /// a service instance.
        /// </summary>
        /// <param name="instance">Instance of service handler class.</param>
        /// <returns>Service handler instance.</returns>
        public static ServiceHandlerInstance FromInstance(object instance)
        {
            // Make sure the attribute is on the declaring type of the instance
            var handlerAttr = instance.GetType().GetCustomAttribute<NexusServiceHandlerAttribute>() ??
                throw new ArgumentException("Missing NexusServiceHandler attribute");
            var serviceDef = ServiceDefinition.FromType(handlerAttr.ServiceType);

            // Collect all methods recursively
            var methods = new List<MethodInfo>();
            CollectTypeMethods(instance.GetType(), methods);

            // Collect handlers from the method list
            var opHandlers = new Dictionary<string, IOperationHandler<object?, object?>>();
            foreach (var method in methods)
            {
                // Only care about ones with operation attribute
                if (method.GetCustomAttribute<NexusOperationHandlerAttribute>() == null)
                {
                    continue;
                }
                try
                {
                    AddOperationHandler(serviceDef, instance, method, opHandlers);
                }
                catch (Exception e)
                {
                    throw new ArgumentException(
                        $"Failed obtaining operation handler from {method.Name}", e);
                }
            }

            return new(serviceDef, opHandlers);
        }

        private static void AddOperationHandler(
            ServiceDefinition serviceDef,
            object instance,
            MethodInfo method,
            Dictionary<string, IOperationHandler<object?, object?>> opHandlers)
        {
            // Validate
            if (method.GetParameters().Length != 0)
            {
                throw new ArgumentException("Cannot have parameters");
            }
            if (method.ContainsGenericParameters)
            {
                throw new ArgumentException("Cannot be generic");
            }
            if (!method.IsPublic)
            {
                throw new ArgumentException("Must be public");
            }

            // Find definition by the method name
            var opDef = serviceDef.Operations.Values.FirstOrDefault(o => o.MethodInfo?.Name == method.Name) ??
                throw new ArgumentException("No matching NexusOperation on the service interface");

            // Check return
            var goodReturn = false;
            if (method.ReturnType.IsGenericType &&
                method.ReturnType.GetGenericTypeDefinition() == typeof(IOperationHandler<,>))
            {
                var args = method.ReturnType.GetGenericArguments();
                goodReturn = args.Length == 2 &&
                    NoValue.NormalizeVoidType(args[0]) == opDef.InputType &&
                    NoValue.NormalizeVoidType(args[1]) == opDef.OutputType;
            }
            if (!goodReturn)
            {
                var inType = opDef.InputType == typeof(void) ? typeof(NoValue) : opDef.InputType;
                var outType = opDef.OutputType == typeof(void) ? typeof(NoValue) : opDef.OutputType;
                throw new ArgumentException(
                    $"Expected return type of IOperationHandler<{inType.Name}, {outType.Name}>");
            }

            // Confirm not present already
            if (opHandlers.ContainsKey(opDef.Name))
            {
                throw new ArgumentException($"Duplicate operation handler named ${opDef.Name}");
            }

            // Invoke to get handler
            try
            {
                var handler = method.Invoke(method.IsStatic ? null : instance, null) ??
                    throw new ArgumentException("Operation handler was null");
                opHandlers[opDef.Name] = OperationHandler.WrapAsGenericHandler(handler, method.ReturnType);
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException!;
            }
        }

        private static void CollectTypeMethods(Type type, List<MethodInfo> methods)
        {
            // Add all declared public static/instance methods that do not already have one like
            // it present
            foreach (var method in type.GetMethods(
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                // Only add if there isn't already one that matches the base definition
                var baseDef = method.GetBaseDefinition();
                if (!methods.Any(m => baseDef == m.GetBaseDefinition()))
                {
                    methods.Add(method);
                }
            }
            if (type.BaseType is { } baseType)
            {
                CollectTypeMethods(baseType, methods);
            }
        }
    }
}