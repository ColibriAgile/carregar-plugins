namespace CarregarPlugins
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    internal static class Program
    {
        private static MethodInfo[] _metodosColibri;
        private static MethodInfo[] _metodosPlugin;
        private static object _plugin;
        private static readonly Dictionary<string, object> Funcoes = new Dictionary<string, object>()
        {
            ["ObterHandleJanelaPrincipal"] =  (Func<uint>) (() => 0)
        };

        private static void Main()
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
                var colibriClass = ObterClasse("Ncr.Plugin.Colibri", assembly);
                _metodosColibri = ObterMetodos(colibriClass);
                
                var @namespace = ObterNamespace(arquivo);
                var pluginClass = ObterClasse($"{@namespace}.Plugin", assembly);
                _plugin = ObterInstancia(pluginClass);
                _metodosPlugin = ObterMetodos(pluginClass);

                Configurar();

                var nome = ExecutarMetodo<string>("ObterNome", pluginClass, _plugin);
                var versao = ExecutarMetodo<string>("ObterVersao", pluginClass, _plugin);
                Console.WriteLine($" Plugin: {nome} ({versao}) carregado corretemente! ");
                Console.WriteLine("*".PadRight(60, '*'));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void Configurar()
        {
            _metodosColibri?.FirstOrDefault(m => m.Name == "AtribuirFuncoes")?.Invoke(null, new object[] { Funcoes });
            _metodosPlugin?.FirstOrDefault(m => m.Name == "Configurar")?.Invoke(_plugin, new object[] { "" });
        }

        private static string ObterNamespace(string arquivo)
        {
            return Path.GetFileNameWithoutExtension(arquivo).Replace(".", "");
        }

        private static MethodInfo[] ObterMetodos(Type classe)
        {
            Console.WriteLine(" - Metodos:");
            var metodos = classe?.GetMethods();
            if (metodos == null)
                return null;

            foreach (var methodInfo in metodos)
            {
                Console.WriteLine($"   - {methodInfo.Name}");
            }
            return metodos;
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

        private static Type ObterClasse(string classe, Assembly assembly)
        {
            var plugin = assembly.GetType(classe);
            Console.WriteLine($"- classe {classe} encontrada...");
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
