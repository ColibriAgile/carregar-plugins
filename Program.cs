namespace CarregarPlugins
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    class Program
    {
        static void Main(string[] args)
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
                var classe = Path.GetFileNameWithoutExtension(arquivo);
                classe = classe.Replace("Plugin.", "");
                classe = classe.Replace(".", "");
                classe = $"Plugin{classe}.Plugin";
                Console.WriteLine("*".PadRight(60, '*'));
                var a = Assembly.LoadFrom(arquivo);
                Console.WriteLine("- assembly carregado...");
                var plugin = a.GetType(classe);
                Console.WriteLine("- classe plugin encontrada...");
                var obj = Activator.CreateInstance(plugin);
                Console.WriteLine("- classe instanciada...");
                var obterNome = plugin.GetMethod("ObterNome");
                var obterVersao = plugin.GetMethod("ObterVersao");
                Console.WriteLine("- metodos encontrados...");
                var nome = obterNome?.Invoke(obj, null);
                var versao = obterVersao?.Invoke(obj, null);
                Console.WriteLine($" Plugin: {nome} ({versao}) carregado corretemente! ");
                Console.WriteLine("*".PadRight(60, '*'));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
