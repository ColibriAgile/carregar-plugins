namespace CarregarPlugins
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    internal static class Program
    {
        private static void Main(string[] args)
        {
            foreach (var arquivo in Directory.EnumerateDirectories(".").ToList().SelectMany(dir => Directory.EnumerateFiles(dir, "Plugin.*.dll")))
            {
                Console.WriteLine(arquivo);
                CarregarPlugin(arquivo);
                Console.WriteLine();
            }

            Console.ReadKey();
        }

        private static void CarregarPlugin(string arquivo)
        {
            try
            {
                Console.WriteLine("*".PadRight(60, '*'));
                var assembly = CaregarAssembly(arquivo);
                var plugin = ObterClassePlugin(arquivo, assembly);
                var instancia = ObterInstancia(plugin);
                var nome = ExecutarMetodo<string>("ObterNome", plugin, instancia);
                var versao = ExecutarMetodo<string>("ObterVersao", plugin, instancia);
                Console.WriteLine($" Plugin: {nome} ({versao}) carregado corretemente! ");
                Console.WriteLine("*".PadRight(60, '*'));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static T ExecutarMetodo<T>(string metodo, Type plugin, object instancia)
        {
            var m = plugin.GetMethod(metodo);
            return (T)m?.Invoke(instancia, null);

        }

        private static object ObterInstancia(Type plugin)
        {
            var obj = Activator.CreateInstance(plugin);
            Console.WriteLine("- classe instanciada...");
            return obj;
        }

        private static Type ObterClassePlugin(string arquivo, Assembly assembly)
        {
            var classe = Path.GetFileNameWithoutExtension(arquivo).Replace(".", "");
            classe = $"{classe}.Plugin";
            var plugin = assembly.GetType(classe);
            Console.WriteLine("- classe plugin encontrada...");
            return plugin;
        }

        private static Assembly CaregarAssembly(string arquivo)
        {
            var a = Assembly.LoadFrom(arquivo);
            Console.WriteLine("- assembly carregado...");
            return a;
        }
    }
}
