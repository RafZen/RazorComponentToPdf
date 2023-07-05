using DinkToPdf;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using RazorComponentToPdf;
using RazorComponentToPdfBlazor.Data;
using RazorComponentToPdfBlazor.PDFComponents;


namespace RazorComponentToPdfBlazor.Pages
{
   
    public partial class PDFCreator
    {
        [Inject] private IJSRuntime JS { get; set; } = null!;
        [Inject] private Factory Factory { get; set; } = null!;


		private async Task GeneratePdf()
        {
			

            //============ PDF DATA ===========

            List<SampleModel> model = new();
            SampleModel line1 = new()
            {
				Description="3rd Floor Rear, Scottish Life House, 154-155 Great Charles Street Estate,Birmingham, B3 3LG",
                StartDate="16/12/2022",
                EndDate="30/03/2023",
                MeterReadingStart="00575216",
                MeterReadingCurrent="00602665",
                Quantity =  "27,449",
                Rate = "0.6659",
                Vat="20",
                Amount="18,278.29"
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


            Converter<PdfDoc1> converter = Factory.CreateConverter<PdfDoc1>();



			//=============== RENDER COMPONENT INTO HTML =============
			
			converter.Parameter(c => c.Model, model);
            string html = converter.Render();



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




            //========== FILE DOWNLOAD ===========

            using (MemoryStream ms = new(bytes))
            {
                using (var streamRef = new DotNetStreamReference(ms))
                {
                    await JS.InvokeVoidAsync("saveAsFile", "Test.pdf", streamRef);
                }
            }

        }



    }
}
