using Nml.Improve.Me.Dependencies;
using System;
using System.Collections.Generic;
using System.Text;

namespace priapism.worldliness
{
    public class PDFGeneratorService : IPDFGeneratorService
    {
        public IPdfGenerator _pdfGenerator { get; }

        public PDFGeneratorService(IPdfGenerator pdfGenerator)
        {
            _pdfGenerator = pdfGenerator;
        }
        public byte[] GeneratePDF(string view)
        {
            var pdfOptions = ConfigurePdfOptions();
            var pdf = _pdfGenerator.GenerateFromHtml(view, pdfOptions);
            return pdf.ToBytes();
        }

        private static PdfOptions ConfigurePdfOptions()
        {
            return new PdfOptions
            {
                PageNumbers = PageNumbers.Numeric,
                HeaderOptions = new HeaderOptions
                {
                    HeaderRepeat = HeaderRepeat.FirstPageOnly,
                    HeaderHtml = PdfConstants.Header
                }
            };
        }
    }
}
