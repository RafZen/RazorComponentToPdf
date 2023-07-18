

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using RazorComponentToPdfBlazor.Data;
using RazorComponentToPdfBlazor.PDFComponents;
using System;
using System.Diagnostics;



namespace RazorComponentToPdfBlazor.Pages
{

	public partial class PDFCreator
	{
		[Inject] private IJSRuntime _JS { get; set; } = null!;		
		private CancellationTokenSource _cancellationTokenSource;
		private int _PDFCounter = 0;
		private ChromePdfRenderer _chromePdfRenderer;
		private EvoPdf.HtmlToPdfConverter _evoConverter;
		private Stopwatch _stopwatch;
		private List<string> _errors = new List<string>();


        public PDFCreator()
		{
			//IronPdf.Logging.Logger.EnableDebugging = true;
   //         IronPdf.Logging.Logger.LogFilePath = "Default.log";

   //         IronPdf.Logging.Logger.LoggingMode = IronPdf.Logging.Logger.LoggingModes.All;

            _chromePdfRenderer = new ChromePdfRenderer();
			_chromePdfRenderer.RenderingOptions.PaperOrientation = IronPdf.Rendering.PdfPaperOrientation.Landscape;
			_chromePdfRenderer.RenderingOptions.MarginLeft = 10;
			_chromePdfRenderer.RenderingOptions.MarginRight = 10;
			_chromePdfRenderer.RenderingOptions.MarginTop = 10;
			_chromePdfRenderer.RenderingOptions.MarginBottom = 10;
			_chromePdfRenderer.RenderingOptions.Timeout = 3;


			_evoConverter = new EvoPdf.HtmlToPdfConverter();
			_evoConverter.PdfDocumentOptions.PdfPageSize = new EvoPdf.PdfPageSize();
			_evoConverter.PdfDocumentOptions.PdfPageOrientation = EvoPdf.PdfPageOrientation.Landscape;
			_evoConverter.PdfDocumentOptions.BottomMargin = 10;
            _evoConverter.PdfDocumentOptions.TopMargin = 10;
            _evoConverter.PdfDocumentOptions.LeftMargin = 10;
            _evoConverter.PdfDocumentOptions.RightMargin = 10;


			_stopwatch = new Stopwatch();
        }


  //      private async Task DownloadPDF_DinkToPdf()
		//{

  //          string html = await GenerateHtml();

  //          byte[] bytes = await GeneratePdf_Dink(html);


  //          //========== FILE DOWNLOAD ===========

  //          using (MemoryStream ms = new(bytes))
		//	{
		//		using (var streamRef = new DotNetStreamReference(ms))
		//		{
		//			await _JS.InvokeVoidAsync("saveAsFile", "Test.pdf", streamRef);
		//		}
		//	}

		//}


		//private async Task StartStressTest_Dink()
		//{
		//	_cancellationTokenSource = new CancellationTokenSource();

		//	while (!_cancellationTokenSource.IsCancellationRequested)
		//	{
		//		string html = await GenerateHtml();
  //              byte[] bytes = await GeneratePdf_Dink(html);

  //              _PDFCounter++;
		//		StateHasChanged();
		//	}

		//}


		private void StopStressTest()
		{
			_cancellationTokenSource.Cancel();
		}


		//--------------------------------------------------------------------





		private async Task DownloadPDF_Iron()
		{
            _cancellationTokenSource = new CancellationTokenSource();



            string html = await GenerateHtml();



			using (PdfDocument pdfDoc = await _chromePdfRenderer.RenderHtmlAsPdfAsync(html))
			{

				using (MemoryStream ms = pdfDoc.Stream)
				{

					using (var streamRef = new DotNetStreamReference(ms))
					{
						await _JS.InvokeVoidAsync("saveAsFile", "Test.pdf", streamRef);
					}
				}
			}


            _PDFCounter++;
            StateHasChanged();
        }



		private async Task StartStressTest_Iron()
		{

			_cancellationTokenSource = new CancellationTokenSource();
			string html = await GenerateHtml();

			_stopwatch.Start();
			while (!_cancellationTokenSource.IsCancellationRequested)
			{
				try
				{
					using (PdfDocument pdfDoc = await _chromePdfRenderer.RenderHtmlAsPdfAsync(html))
					{
						using (MemoryStream ms = pdfDoc.Stream)
						{
							using (var streamRef = new DotNetStreamReference(ms))
							{
							}
						}

					}
					_PDFCounter++;
				}
				catch (IronPdf.Exceptions.IronPdfNativeException ironPdfNativeException)
				{
					_errors.Add(ironPdfNativeException.Message);

				}

				StateHasChanged();

			}
			_stopwatch.Stop();

		}




        private async Task DownloadPDF_Evo()
        {
            _cancellationTokenSource = new CancellationTokenSource();


            string html = await GenerateHtml();
			

            using (MemoryStream ms = new MemoryStream())
            {
                _evoConverter.ConvertHtmlToStream(html, "/", ms);
				ms.Seek(0, SeekOrigin.Begin);
                using (var streamRef = new DotNetStreamReference(ms))
                {

                    await _JS.InvokeVoidAsync("saveAsFile", "Test.pdf", streamRef);
                }
            }
            
            _PDFCounter++;
			await InvokeAsync(() => StateHasChanged());
        }



		private async Task StartStressTest_Evo()
		{
			_cancellationTokenSource = new CancellationTokenSource();


			string html = await GenerateHtml();

			await Task.Run(() =>
			{

				while (!_cancellationTokenSource.IsCancellationRequested)
				{

					using (MemoryStream ms = new MemoryStream())
					{
						_evoConverter.ConvertHtmlToStream(html, "/", ms);
						ms.Seek(0, SeekOrigin.Begin);
						using (var streamRef = new DotNetStreamReference(ms))
						{

						}
					}

					_PDFCounter++;
					InvokeAsync(() => StateHasChanged());
				}

			});
		}





		private void CollectGarbage()
		{
			GC.Collect();
		}



		private async Task<string> GenerateHtml()
		{
			return await Task.Run(() =>
			{
				BlazorTemplater.ComponentRenderer<PdfDoc1> componentRenderer = new();

                //============ PDF DATA ===========

                List<SampleModel> model = new();
				SampleModel line1 = new()
				{
					Description = "3rd Floor Rear, Scottish Life House, 154-155 Great Charles Street Estate,Birmingham, B3 3LG",
					StartDate = "16/12/2022",
					EndDate = "30/03/2023",
					MeterReadingStart = "00575216",
					MeterReadingCurrent = "00602665",
					Quantity = "27,449",
					Rate = "0.6659",
					Vat = "20",
					Amount = "18,278.29"
				};
				model.Add(line1);

				SampleModel line2 = new()
				{
					Description = "Quorterly charge",
					StartDate = "16/12/2022",
					EndDate = "30/03/2023",
					MeterReadingStart = "",
					MeterReadingCurrent = "",
					Quantity = "1",
					Rate = "50.85",
					Vat = "20",
					Amount = "50.85"
				};
				model.Add(line2);

				SampleModel line3 = new()
				{
					Description = "Feed in Tariff",
					StartDate = "16/12/2022",
					EndDate = "30/03/2023",
					MeterReadingStart = "",
					MeterReadingCurrent = "",
					Quantity = "27,449",
					Rate = "0.00821",
					Vat = "20",
					Amount = "225.36"
				};
				model.Add(line3);



				//=============== RENDER COMPONENT INTO HTML =============

				componentRenderer.Set(p => p.Model, model);
				return componentRenderer.Render();
				
			});
		}



		//private async Task<byte[]> GeneratePdf_Dink(string html)
		//{
		//	return await Task.Run(() =>
		//	{

		//		//=============== PDF SETUP =============

		//		GlobalSettings globalSettings = new GlobalSettings()
		//		{
		//			ColorMode = ColorMode.Color,
		//			Orientation = Orientation.Landscape,
		//			PaperSize = PaperKind.A4,
		//			Margins = new MarginSettings { Top = 18, Bottom = 18 }
		//		};

		//		ObjectSettings objectSettings = new ObjectSettings()
		//		{
		//			PagesCount = true,
		//			HtmlContent = html,
		//			WebSettings = { DefaultEncoding = "utf-8", EnableIntelligentShrinking = false, LoadImages = true },
		//			HeaderSettings = { FontSize = 10, Right = "Page [page] of [toPage]", Line = true, },
		//			FooterSettings = { FontSize = 8, Center = "ZEN PDF demo", Line = true }
		//		};



		//		//=============== CONVERT HTML INTO PDF =============

		//		byte[] bytes = converter.Convert(globalSettings, objectSettings);


		//		return bytes;

		//	});

		//}

	}
}
