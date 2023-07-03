using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.Win32.SafeHandles;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;


namespace RazorComponentToPdf
{

    public class Converter<TComponent> where TComponent : Microsoft.AspNetCore.Components.IComponent
	{
		private readonly IConverter _converter;
		private readonly BlazorTemplater.ComponentRenderer<TComponent> _templater;




		public Converter(IConverter converter)
		{
			_converter = converter;
			_templater = new BlazorTemplater.ComponentRenderer<TComponent>();
		}


		public void Parameter<TValue>(System.Linq.Expressions.Expression<Func<TComponent, TValue>> parameterSelector, TValue value)
		{
			_templater.Set(parameterSelector, value);
		}


		public string Render()
		{
			return _templater.Render();
		}



		public byte[] Convert(GlobalSettings globalSettings, ObjectSettings objectSettings)
		{
			HtmlToPdfDocument htmlToPdfDocument = new()
			{

				GlobalSettings = globalSettings,
				Objects =
				{
					objectSettings
				}

			};


			return _converter.Convert(htmlToPdfDocument);
		}





		private SafeFileHandle LoadwkHtmlToPdf()
			{
				CustomAssemblyLoadContext customAssemblyLoadContext = new CustomAssemblyLoadContext();
				var architectureFolder = (IntPtr.Size == 8) ? "64 bit" : "32 bit";


				string? runDir = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
				if (runDir is null) throw new Exception("Running directory not found");


				string wkHtmlToPdfPath;

				if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
				{
					wkHtmlToPdfPath = Path.Combine(runDir, $"Libs\\{architectureFolder}\\libwkhtmltox.dylib");

				}
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
				{
					wkHtmlToPdfPath = Path.Combine(runDir, $"Libs\\{architectureFolder}\\libwkhtmltox.so");

				}
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				{
					wkHtmlToPdfPath = Path.Combine(runDir, $"Libs\\{architectureFolder}\\libwkhtmltox.dll");
				}
				else
				{
					throw new Exception("Unsupported runtime.");
				}


				IntPtr p = customAssemblyLoadContext.LoadUnmanagedLibrary(wkHtmlToPdfPath);
				return new SafeFileHandle(p, true);
			}


			private class CustomAssemblyLoadContext : AssemblyLoadContext
			{
				public IntPtr LoadUnmanagedLibrary(string absolutePath)
				{
					return LoadUnmanagedDll(absolutePath);
				}
				protected override IntPtr LoadUnmanagedDll(String unmanagedDllName)
				{
					return LoadUnmanagedDllFromPath(unmanagedDllName);
				}
				protected override Assembly Load(AssemblyName assemblyName)
				{
					throw new NotImplementedException();
				}
			}





		}

	}

