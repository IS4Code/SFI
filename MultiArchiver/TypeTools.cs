using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;

namespace IS4.MultiArchiver
{
    public static class TypeTools
    {
        public static Exception TryCreate<T>(Expression<Func<T>> constructor, out T result)
        {
            var constructedType = typeof(T);
            result = (T)FormatterServices.GetUninitializedObject(constructedType);
            if(!(constructor.Body is NewExpression newExpr)) throw new ArgumentException(null, nameof(constructor));
            var ctor = newExpr.Constructor;
            if(!ctor.DeclaringType.Equals(constructedType)) throw new ArgumentException(null, nameof(constructor));
            var args = new object[newExpr.Arguments.Count];
            for(int i = 0; i < args.Length; i++)
            {
                switch(newExpr.Arguments[i])
                {
                    case ConstantExpression constant:
                        args[i] = constant.Value;
                        break;
                    case MemberExpression member:
                        if(!(member.Expression is ConstantExpression container)) goto default;
                        switch(member.Member)
                        {
                            case FieldInfo fi:
                                args[i] = fi.GetValue(container.Value);
                                break;
                            case PropertyInfo pi:
                                args[i] = pi.GetValue(container.Value);
                                break;
                            default:
                                throw new ArgumentException(null, nameof(constructor));
                        }
                        break;
                    default:
                        throw new ArgumentException(null, nameof(constructor));
                }
            }
            try{
                ctor.Invoke(result, args);
            }catch(TargetInvocationException e)
            {
                return e.InnerException;
            }
            return null;
        }
    }
}
