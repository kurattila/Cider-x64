namespace Cider_x64
{
    public interface ILoader
    {
        void PreloadAssembly(string assemblyPath);
        void AddMergedDictionary(string packUriStringOfResDictXaml);
        void Load(string assemblyPath, string namespaceDotType);
        void Show();
        void Hide();
        void CloseWindow();

        /// <summary>
        /// Get list of loaded assembly type names. Assembly must be already loaded to obtain valid list.
        /// </summary>
        /// <returns>list of loaded assembly type names</returns>
        System.Collections.Generic.List<System.String> GetLoadedAssemblyTypeNames();
    }
}
