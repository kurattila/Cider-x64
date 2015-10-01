using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace Cider_x64
{
    public class Loader : MarshalByRefObject, ILoader
    {
        Window m_Win = new Window();

        public void AddMergedDictionary(string packUriStringOfResDictXaml)
        {
            ResourceDictionary xamlDictionaryToMerge = new ResourceDictionary();
            xamlDictionaryToMerge.Source = new Uri(packUriStringOfResDictXaml);
            Application.Current.Resources.MergedDictionaries.Add(xamlDictionaryToMerge);
        }

        public void PreloadAssembly(string assemblyPath)
        {
            Assembly.LoadFrom(System.IO.Path.Combine(assemblyPath));
        }

        public void Show(string assemblyPath, string namespaceDotType)
        {
            Assembly assembly = Assembly.LoadFrom(assemblyPath);
            Type typeToCreate = assembly.GetType(namespaceDotType);
            object instanceCreated = Activator.CreateInstance(typeToCreate);

            displayWpfGuiPreview(instanceCreated as Window);
            displayWpfGuiPreview(instanceCreated as UserControl);
        }

        private void displayWpfGuiPreview(Window instanceCreated)
        {
            if (instanceCreated == null)
                return;

            instanceCreated.Show();
        }

        private void displayWpfGuiPreview(UserControl instanceCreated)
        {
            if (instanceCreated == null)
                return;

            m_Win.Content = instanceCreated;
            m_Win.Show();
        }
    }
}
