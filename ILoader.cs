namespace Cider_x64
{
    public interface ILoader
    {
        void PreloadAssembly(string assemblyPath);
        void AddMergedDictionary(string packUriStringOfResDictXaml);
        void Show(string assemblyPath, string namespaceDotType);
        void CloseWindow();
    }
}
