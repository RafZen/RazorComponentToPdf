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
        [Inject] private Factory Factory { get; set; }



        private async Task GeneratePdf()
        {
			Converter<PdfDoc1> converter = Factory.CreateConverter<PdfDoc1>();


            List<SampleModel> model = new();
            model.Add(new SampleModel() { id = 1, name = "Rafal", address = "Swindon" });
            model.Add(new SampleModel() { id = 2, name = "John", address = "London" });

            converter.Parameter(c => c.Model, model);
            string html = converter.Render();




            GlobalSettings globalSettings = new GlobalSettings()
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings { Top = 18, Bottom = 18 }

            };

            ObjectSettings objectSettings = new ObjectSettings()
            {
                PagesCount = true,
                HtmlContent = html,
                WebSettings = { DefaultEncoding = "utf-8", EnableIntelligentShrinking = false },
                HeaderSettings = { FontSize = 10, Right = "Page [page] of [toPage]", Line = true, },
                FooterSettings = { FontSize = 8, Center = "ZEN PDF demo", Line = true }
            };


            byte[] bytes = converter.Convert(globalSettings, objectSettings);






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
