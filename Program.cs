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

        // ReSharper disable once InconsistentNaming
        private static readonly Dictionary<string, object> Funcoes = new Dictionary<string, object>()
        {
            ["ObterHandleJanelaPrincipal"] = (Func<uint>)(() => 0),
            ["ObterModoLicenciado"] = (Func<string,int>)(m => 1),
            ["ObterChaveLicenciada"] = (Func<string,int>)(c => 1)
        };

        [STAThread]
        private static void Main(string[] args)
        {
            foreach (var arquivo in Directory.EnumerateDirectories(".")
                .ToList()
                .SelectMany(dir => Directory.EnumerateFiles(dir, "Plugin.*.dll"))
                .Where(arquivo => args.Length == 0 || arquivo.ToLower().EndsWith(args[0].ToLower())))
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

                ImprimirReferencias(assembly);

                var nome = ExecutarMetodo<string>("ObterNome", pluginClass, _plugin);
                var versao = ExecutarMetodo<string>("ObterVersao", pluginClass, _plugin);

                Console.WriteLine("*".PadRight(60, '*'));
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($" Plugin: {nome} ({versao}) carregado corretemente! ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("*".PadRight(60, '*'));

                if (_metodosPlugin?.FirstOrDefault(m => m.Name == "Configurar") is null)
                    return;

                Console.Write("Deseja tentar executar o método de configurações? [Y/N]:");
                var resposta = Console.ReadKey();
                if (resposta.KeyChar.ToString().ToLower() == "y")
                    Configurar();
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red; 
                Console.WriteLine(e);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        private static void ImprimirReferencias(Assembly assembly)
        {
            Console.WriteLine($"- Tipos exportados: ({assembly.GetExportedTypes().Length})");
            foreach (var t in assembly.GetExportedTypes())
            {
                Console.Out.WriteLine($"  - {t.FullName}");
            }

            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            Console.WriteLine($"- Referencias: ({assembly.GetReferencedAssemblies().Length})");
            foreach (var ra in assembly.GetReferencedAssemblies())
            {
                var la = loadedAssemblies.SingleOrDefault(a => a.FullName == ra.FullName);
                var dll = la is null ? null : Assembly.Load(la.FullName);
                var name = dll?.GetName().Name ?? ra.Name;
                Console.WriteLine($"  - {name,-25} => {la?.CodeBase ?? "<costura>" }");
            }
        }

        private static void Configurar()
        {
            _metodosColibri?.FirstOrDefault(m => m.Name == "AtribuirFuncoes")?.Invoke(null, new object[] { Funcoes });
            _metodosPlugin?.FirstOrDefault(m => m.Name == "ConfigurarDb")?.Invoke(_plugin, new object[] { ".", "ncrcolibri", "sa", "1234", "" });
            _metodosPlugin?.FirstOrDefault(m => m.Name == "Configurar")?.Invoke(_plugin, new object[] { "{'maquinas': {1:'maquina1'}}" });
        }

        private static string ObterNamespace(string arquivo)
            => Path.GetFileNameWithoutExtension(arquivo).Replace(".", "");

        private static MethodInfo[] ObterMetodos(Type classe)
        {
            var metodos = classe?.GetMethods();

            if (metodos == null)
                return null;

            Console.WriteLine($"  - Metodos: ({classe.GetMethods().Length})");
            foreach (var methodInfo in metodos)
                Console.WriteLine($"    - {methodInfo.Name}");

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
