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






		}

	}

