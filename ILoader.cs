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
    }
}
