

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
		
		private Stopwatch _stopwatch;
		private List<string> _errors = new List<string>();
		private int _maxConcurrentRequests;
		private SemaphoreSlim _semaphore;

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



			_stopwatch = new Stopwatch();

            _maxConcurrentRequests = Environment.ProcessorCount;
            _semaphore = new SemaphoreSlim(_maxConcurrentRequests);
        }


  

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

			bool semaphoreTaken = false;
            _stopwatch.Start();
			while (!_cancellationTokenSource.IsCancellationRequested)
			{								
			 semaphoreTaken = await _semaphore.WaitAsync(-1);

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
					Thread.Sleep(10);
				}
				catch (IronPdf.Exceptions.IronPdfNativeException ironPdfNativeException)
				{
					_errors.Add(ironPdfNativeException.Message);
				}
				finally
				{
					_semaphore.Release();
				}

				StateHasChanged();
			}
			_stopwatch.Stop();

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



	}
}
