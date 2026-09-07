using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

// Loads a mod DLL against the real game assemblies and verifies, without launching
// the game, the two crash classes that kill ONI mods after game updates:
//   1. Type load / vtable setup ("VTable setup of type X failed") — forced by
//      touching every type, its interface maps, and its static constructor.
//   2. Missing members at call time (MissingMethodException/MissingFieldException) —
//      forced by JIT-compiling every method body so all member references resolve.
//
// Must run on .NET 8+ (not .NET Framework): the game's interfaces use default
// interface methods, which the desktop CLR rejects but CoreCLR and the game's
// Unity 6 Mono runtime both support.
//
// Usage: dotnet run --project common/tools/ModCheck -- <game Managed dir> <mod dll>
internal static class Program
{
	private static int Main(string[] args)
	{
		if (args.Length < 2)
		{
			Console.WriteLine("usage: ModCheck <game Managed dir> <mod dll>");
			return 2;
		}
		string managedDir = args[0];
		string modDll = Path.GetFullPath(args[1]);

		AppDomain.CurrentDomain.AssemblyResolve += (_, e) =>
		{
			string path = Path.Combine(managedDir, new AssemblyName(e.Name).Name + ".dll");
			return File.Exists(path) ? Assembly.LoadFrom(path) : null;
		};

		int failures = 0;
		try
		{
			var asm = Assembly.LoadFrom(modDll);
			foreach (var type in asm.GetTypes())
			{
				try
				{
					foreach (var iface in type.GetInterfaces())
						type.GetInterfaceMap(iface);
					RuntimeHelpers.RunClassConstructor(type.TypeHandle);
				}
				catch (Exception ex)
				{
					Console.WriteLine($"FAIL type {type.FullName}: {Unwrap(ex)}");
					failures++;
					continue;
				}

				const BindingFlags all = BindingFlags.Public | BindingFlags.NonPublic |
					BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
				foreach (MethodBase method in AllMethods(type, all))
				{
					if (method.IsAbstract || method.ContainsGenericParameters)
						continue;
					try
					{
						RuntimeHelpers.PrepareMethod(method.MethodHandle);
					}
					catch (Exception ex)
					{
						Console.WriteLine($"FAIL jit {type.FullName}.{method.Name}: {Unwrap(ex)}");
						failures++;
					}
				}
				Console.WriteLine("OK " + type.FullName);
			}
		}
		catch (ReflectionTypeLoadException rtle)
		{
			foreach (var le in rtle.LoaderExceptions)
			{
				Console.WriteLine("FAIL loader: " + ((le as TypeLoadException)?.TypeName ?? "?") + " :: " + le?.Message);
				failures++;
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine("FAIL: " + ex);
			failures++;
		}

		Console.WriteLine(failures == 0 ? "ALL CHECKS PASSED" : $"{failures} FAILURE(S)");
		return failures == 0 ? 0 : 1;
	}

	private static MethodBase[] AllMethods(Type type, BindingFlags flags)
	{
		MethodInfo[] methods = type.GetMethods(flags);
		ConstructorInfo[] ctors = type.GetConstructors(flags);
		var result = new MethodBase[methods.Length + ctors.Length];
		methods.CopyTo(result, 0);
		ctors.CopyTo(result, methods.Length);
		return result;
	}

	private static string Unwrap(Exception ex)
	{
		while (ex is TypeInitializationException && ex.InnerException != null)
			ex = ex.InnerException;
		return ex.GetType().Name + ": " + ex.Message;
	}
}
