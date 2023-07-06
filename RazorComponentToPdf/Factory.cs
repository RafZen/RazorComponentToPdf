using DinkToPdf.Contracts;
using DinkToPdf;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

namespace RazorComponentToPdf
{
	public class Factory
	{
		private readonly IConverter _converter;

		public Factory()
		{
			LoadwkHtmlToPdf();
			_converter = new SynchronizedConverter(new PdfTools());
			_converter.Error += _converter_Error;
			_converter.Warning += _converter_Warning;
			_converter.ProgressChanged += _converter_ProgressChanged;
			_converter.PhaseChanged += _converter_PhaseChanged;
			_converter.Finished += _converter_Finished;
		}

		private void _converter_Finished(object? sender, DinkToPdf.EventDefinitions.FinishedArgs e)
		{

		}

		private void _converter_PhaseChanged(object? sender, DinkToPdf.EventDefinitions.PhaseChangedArgs e)
		{

		}

		private void _converter_ProgressChanged(object? sender, DinkToPdf.EventDefinitions.ProgressChangedArgs e)
		{

		}

		private void _converter_Warning(object? sender, DinkToPdf.EventDefinitions.WarningArgs e)
		{

		}

		private void _converter_Error(object? sender, DinkToPdf.EventDefinitions.ErrorArgs e)
		{
		}



		public Converter<TComponent> CreateConverter<TComponent>() where TComponent : Microsoft.AspNetCore.Components.IComponent
		{
			return new Converter<TComponent>(_converter);
		}



		private void LoadwkHtmlToPdf()
		{
			CustomAssemblyLoadContext wkhtmltopdfAssemblyLoadContext = new CustomAssemblyLoadContext();

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


			wkhtmltopdfAssemblyLoadContext.LoadUnmanagedLibrary(wkHtmlToPdfPath);
			
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
