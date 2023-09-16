#nullable disable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Force.DeepCloner.Helpers
{
	internal static class DeepClonerExprGenerator
	{
		private static readonly ConcurrentDictionary<FieldInfo, bool> readonlyFields = new ();

		private static readonly bool canFastCopyReadonlyFields = false;

		private static readonly MethodInfo fieldSetMethod;
		static DeepClonerExprGenerator()
		{
			typeof(DeepClonerExprGenerator).GetTypeInfo().GetDeclaredField(nameof(canFastCopyReadonlyFields))!.SetValue(null, true);
			fieldSetMethod = typeof(FieldInfo).GetMethod("SetValue", new[] {typeof(object), typeof(object)});

			if (fieldSetMethod == null)
				throw new ArgumentNullException();
		}
		
		internal static Delegate GenerateClonerInternal(Type realType, bool asObject)
            => GenerateProcessMethod(realType, asObject && realType.IsValueType);

        private static Delegate GenerateProcessMethod(Type type, bool unboxStruct)
		{
			if (type.IsArray)
				return GenerateProcessArrayMethod(type);

			if (type.FullName != null && type.FullName.StartsWith("System.Tuple`"))
			{
				// if not safe type it is no guarantee that some type will contain reference to
				// this tuple. In usual way, we're creating new object, setting reference for it
				// and filling data. For tuple, we will fill data before creating object
				// (in constructor arguments)
				var genericArguments = type.GetTypeInfo().GenericTypeArguments;
				// current tuples contain only 8 arguments, but may be in future...
				// we'll write code that works with it
				if (genericArguments.Length < 10 && genericArguments.All(DeepClonerSafeTypes.CanReturnSameObject))
					return GenerateProcessTupleMethod(type);
			}

			var methodType = unboxStruct || type.GetTypeInfo().IsClass ? typeof(object) : type;

			var expressionList = new List<Expression>();

			ParameterExpression from = Expression.Parameter(methodType);
			var fromLocal = from;
			var toLocal = Expression.Variable(type);
			var state = Expression.Parameter(typeof(DeepCloneState));

			if (!type.GetTypeInfo().IsValueType)
			{
				var methodInfo = typeof(object).GetTypeInfo().GetDeclaredMethod(nameof(MemberwiseClone))!;

				// to = (T)from.MemberwiseClone()
				expressionList.Add(Expression.Assign(toLocal, Expression.Convert(Expression.Call(from, methodInfo), type)));

				fromLocal = Expression.Variable(type);
				// fromLocal = (T)from
				expressionList.Add(Expression.Assign(fromLocal, Expression.Convert(from, type)));

				// added from -> to binding to ensure reference loop handling
				// structs cannot loop here
				// state.AddKnownRef(from, to)
				expressionList.Add(Expression.Call(state, typeof(DeepCloneState).GetMethod(nameof(DeepCloneState.AddKnownRef))!, from, toLocal));
			}
			else
			{
				if (unboxStruct)
				{
					// toLocal = (T)from;
					expressionList.Add(Expression.Assign(toLocal, Expression.Unbox(from, type)));
					fromLocal = Expression.Variable(type);
					// fromLocal = toLocal; // structs, it is ok to copy
					expressionList.Add(Expression.Assign(fromLocal, toLocal));
				}
				else
				{
					// toLocal = from
					expressionList.Add(Expression.Assign(toLocal, from));
				}
			}

			List<FieldInfo> fi = new List<FieldInfo>();
			var tp = type;
			do
			{
				if (tp.Name == "ContextBoundObject") break;

				fi.AddRange(tp.GetDeclaredFields());
				tp = tp.GetTypeInfo().BaseType;
			}
			while (tp != null);

			foreach (var fieldInfo in fi)
			{
				if (!DeepClonerSafeTypes.CanReturnSameObject(fieldInfo.FieldType))
				{
					var methodInfo = fieldInfo.FieldType.GetTypeInfo().IsValueType
										? typeof(DeepClonerGenerator).GetTypeInfo().GetDeclaredMethod(nameof(DeepClonerGenerator.CloneStructInternal))!
																	.MakeGenericMethod(fieldInfo.FieldType)
										: typeof(DeepClonerGenerator).GetTypeInfo().GetDeclaredMethod(nameof(DeepClonerGenerator.CloneClassInternal))!;

					var get = Expression.Field(fromLocal, fieldInfo);

					// toLocal.Field = Clone...Internal(fromLocal.Field)
					var call = (Expression)Expression.Call(methodInfo, get, state);
					if (!fieldInfo.FieldType.GetTypeInfo().IsValueType)
						call = Expression.Convert(call, fieldInfo.FieldType);

					// should handle specially
					// todo: think about optimization, but it rare case
					var isReadonly = readonlyFields.GetOrAdd(fieldInfo, f => f.IsInitOnly);
					if (isReadonly)
					{
						if (!canFastCopyReadonlyFields)
                            throw new NotSupportedException();

						expressionList.Add(Expression.Call(
							Expression.Constant(fieldInfo),
							fieldSetMethod,
							Expression.Convert(toLocal, typeof(object)),
							Expression.Convert(call, typeof(object))));
					}
					else
					{
						expressionList.Add(Expression.Assign(Expression.Field(toLocal, fieldInfo), call));
					}
				}
			}

			expressionList.Add(Expression.Convert(toLocal, methodType));

			var funcType = typeof(Func<,,>).MakeGenericType(methodType, typeof(DeepCloneState), methodType);

			var blockParams = new List<ParameterExpression>();
			if (from != fromLocal) blockParams.Add(fromLocal);
			blockParams.Add(toLocal);

			return Expression.Lambda(funcType, Expression.Block(blockParams, expressionList), from, state).Compile();
		}

		private static Delegate GenerateProcessArrayMethod(Type type)
		{
			var elementType = type.GetElementType()!;
			var rank = type.GetArrayRank();

			MethodInfo methodInfo;

			// multidim or not zero-based arrays
			if (rank != 1 || type != elementType.MakeArrayType())
			{
				if (rank == 2 && type == elementType.MakeArrayType(2))
				{
					// small optimization for 2 dim arrays
					methodInfo = typeof(DeepClonerGenerator).GetTypeInfo()
                        .GetDeclaredMethod(nameof(DeepClonerGenerator.Clone2DimArrayInternal))!
                        .MakeGenericMethod(elementType);
				}
				else
				{
					methodInfo = typeof(DeepClonerGenerator).GetTypeInfo()
                        .GetDeclaredMethod(nameof(DeepClonerGenerator.CloneAbstractArrayInternal));
				}
			}
			else
			{
				var methodName = nameof(DeepClonerGenerator.Clone1DimArrayClassInternal);
				if (DeepClonerSafeTypes.CanReturnSameObject(elementType))
                    methodName = nameof(DeepClonerGenerator.Clone1DimArraySafeInternal);
				else if (elementType.GetTypeInfo().IsValueType)
                    methodName = nameof(DeepClonerGenerator.Clone1DimArrayStructInternal);
				methodInfo = typeof(DeepClonerGenerator).GetTypeInfo().GetDeclaredMethod(methodName)!.MakeGenericMethod(elementType);
			}

			ParameterExpression from = Expression.Parameter(typeof(object));
			var state = Expression.Parameter(typeof(DeepCloneState));
			var call = Expression.Call(methodInfo!, Expression.Convert(from, type), state);

			var funcType = typeof(Func<,,>).MakeGenericType(typeof(object), typeof(DeepCloneState), typeof(object));

			return Expression.Lambda(funcType, call, from, state).Compile();
		}

		private static Delegate GenerateProcessTupleMethod(Type type)
		{
			ParameterExpression from = Expression.Parameter(typeof(object));
			var state = Expression.Parameter(typeof(DeepCloneState));
			
			var local = Expression.Variable(type);
			var assign = Expression.Assign(local, Expression.Convert(from, type));

			var funcType = typeof(Func<object, DeepCloneState, object>);

			var tupleLength = type.GetTypeInfo().GenericTypeArguments.Length;
			
			var constructor = Expression.Assign(local, Expression.New(type.GetPublicConstructors().First(x => x.GetParameters().Length == tupleLength),
				type.GetPublicProperties().OrderBy(x => x.Name)
					.Where(x => x.CanRead && x.Name.StartsWith("Item") && char.IsDigit(x.Name[4]))
					.Select(x => Expression.Property(local, x.Name))));
			
			return Expression.Lambda(funcType, Expression.Block(new[] { local },
				assign, constructor, Expression.Call(state, typeof(DeepCloneState).GetMethod(nameof(DeepCloneState.AddKnownRef))!, from, local),
					from),
				from, state).Compile();
		}
		
	}
}
