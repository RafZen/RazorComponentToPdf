using DinkToPdf;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using RazorComponentToPdf;
using RazorComponentToPdfBlazor.Data;
using RazorComponentToPdfBlazor.PDFComponents;
using System;
using IronPdf;


namespace RazorComponentToPdfBlazor.Pages
{

	public partial class PDFCreator
	{
		[Inject] private IJSRuntime JS { get; set; } = null!;
		[Inject] private Factory Factory { get; set; } = null!;
		private CancellationTokenSource cancellationTokenSource;
		private int PDFCounter = 0;
		private ChromePdfRenderer ironRenderer;

        private Converter<PdfDoc1> converter;


		public PDFCreator()
		{
			ironRenderer = new ChromePdfRenderer();
			ironRenderer.RenderingOptions.PaperOrientation = IronPdf.Rendering.PdfPaperOrientation.Landscape;
			ironRenderer.RenderingOptions.MarginLeft = 10;
			ironRenderer.RenderingOptions.MarginRight = 10;
			ironRenderer.RenderingOptions.MarginTop = 10;
			ironRenderer.RenderingOptions.MarginBottom = 10;


		}


        private async Task DownloadPDF_DinkToPdf()
		{

            string html = await GenerateHtml();

            byte[] bytes = await GeneratePdf_Dink(html);


            //========== FILE DOWNLOAD ===========

            using (MemoryStream ms = new(bytes))
			{
				using (var streamRef = new DotNetStreamReference(ms))
				{
					await JS.InvokeVoidAsync("saveAsFile", "Test.pdf", streamRef);
				}
			}

		}


		private async Task StartStressTest_Dink()
		{
			cancellationTokenSource = new CancellationTokenSource();

			while (!cancellationTokenSource.IsCancellationRequested)
			{
				string html = await GenerateHtml();
                byte[] bytes = await GeneratePdf_Dink(html);

                PDFCounter++;
				StateHasChanged();
			}

		}


		private void StopStressTest()
		{
			cancellationTokenSource.Cancel();
		}


		//--------------------------------------------------------------------





		private async Task DownloadPDF_Iron()
		{
            cancellationTokenSource = new CancellationTokenSource();

            //ironRenderer = new ChromePdfRenderer();
            //ironRenderer.RenderingOptions.PaperOrientation = IronPdf.Rendering.PdfPaperOrientation.Landscape;
            //ironRenderer.RenderingOptions.MarginLeft = 10;
            //ironRenderer.RenderingOptions.MarginRight = 10;
            //ironRenderer.RenderingOptions.MarginTop = 10;
            //ironRenderer.RenderingOptions.MarginBottom = 10;


            string html = await GenerateHtml();



			using (PdfDocument pdfDoc = await ironRenderer.RenderHtmlAsPdfAsync(html))
			{

				using (MemoryStream ms = pdfDoc.Stream)
				{

					using (var streamRef = new DotNetStreamReference(ms))
					{
						await JS.InvokeVoidAsync("saveAsFile", "Test.pdf", streamRef);
					}
				}
			}

		}



        private async Task StartStressTest_Iron()
        {
            cancellationTokenSource = new CancellationTokenSource();

            string html = await GenerateHtml();


            while (!cancellationTokenSource.IsCancellationRequested)
            {

				
				using (PdfDocument pdfDoc = await ironRenderer.RenderHtmlAsPdfAsync(html))
				{

					using (MemoryStream ms = pdfDoc.Stream)
					{

						using (var streamRef = new DotNetStreamReference(ms))
						{
							// await JS.InvokeVoidAsync("saveAsFile", "Test.pdf", streamRef);
						}
					}
				}

				PDFCounter++;
                StateHasChanged();
            }

        }





        private void CollectGarbage()
		{
			GC.Collect();
		}







		private async Task<string> GenerateHtml()
		{
			return await Task.Run(() =>
			{
                converter = Factory.CreateConverter<PdfDoc1>();

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



				//=============== INSTANTIATE LIBRARY =============


				



				//=============== RENDER COMPONENT INTO HTML =============

				converter.Parameter(c => c.Model, model);
				return converter.Render();

			});
		}



		private async Task<byte[]> GeneratePdf_Dink(string html)
		{
			return await Task.Run(() =>
			{

				//=============== PDF SETUP =============

				GlobalSettings globalSettings = new GlobalSettings()
				{
					ColorMode = ColorMode.Color,
					Orientation = Orientation.Landscape,
					PaperSize = PaperKind.A4,
					Margins = new MarginSettings { Top = 18, Bottom = 18 }
				};

				ObjectSettings objectSettings = new ObjectSettings()
				{
					PagesCount = true,
					HtmlContent = html,
					WebSettings = { DefaultEncoding = "utf-8", EnableIntelligentShrinking = false, LoadImages = true },
					HeaderSettings = { FontSize = 10, Right = "Page [page] of [toPage]", Line = true, },
					FooterSettings = { FontSize = 8, Center = "ZEN PDF demo", Line = true }
				};



				//=============== CONVERT HTML INTO PDF =============

				byte[] bytes = converter.Convert(globalSettings, objectSettings);


				return bytes;

			});

		}

	}
}
