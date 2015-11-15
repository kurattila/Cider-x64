using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Cider_x64
{
    class GuiTypesExtractor
    {
        public virtual List<Type> GetGuiTypesOnly(AssemblyWrapper assemblyWrapper)
        {
            var filteredTypes = new List<Type>();
            var allTypes = getAllTypesInsideAssembly(assemblyWrapper);
            foreach (Type anyType in allTypes)
            {
                if (   anyType == typeof(Window)
                    || anyType.InheritsFrom(typeof(Window))
                    || anyType == typeof(UserControl)
                    || anyType.InheritsFrom(typeof(UserControl))
                    )
                {
                    filteredTypes.Add(anyType);
                }
            }
            return filteredTypes;
        }

        protected virtual List<Type> getAllTypesInsideAssembly(AssemblyWrapper assembly)
        {
            return new List<Type>(assembly.Assembly.GetTypes());
        }
    }

    static class InheritanceDetector
    {
        // http://stackoverflow.com/questions/8868119/find-all-parent-types-both-base-classes-and-interfaces
        public static bool InheritsFrom(this Type type, Type baseType)
        {
            // null does not have base type
            if (type == null)
            {
                return false;
            }

            // only interface can have null base type
            if (baseType == null)
            {
                return type.IsInterface;
            }

            // check implemented interfaces
            if (baseType.IsInterface)
            {
                return type.GetInterfaces().Contains(baseType);
            }

            // check all base types
            var currentType = type;
            while (currentType != null)
            {
                if (currentType.BaseType == baseType)
                {
                    return true;
                }

                currentType = currentType.BaseType;
            }

            return false;
        }
    }
}
