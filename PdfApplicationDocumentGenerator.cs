using System;
using System.Linq;
using Nml.Improve.Me.Dependencies;
using priapism.worldliness;

namespace Nml.Improve.Me
{
	public class PdfApplicationDocumentGenerator : IApplicationDocumentGenerator
	{
		private readonly IDataContext _dataContext;
		private readonly ILogger<PdfApplicationDocumentGenerator> _logger;
        private readonly IViewGeneratorService _viewGeneratorService;
        private readonly IPDFGeneratorService _pDFGeneratorService;

		public PdfApplicationDocumentGenerator(
			IDataContext dataContext,
			ILogger<PdfApplicationDocumentGenerator> logger,
            IViewGeneratorService viewGeneratorService,
            IPDFGeneratorService pDFGeneratorService)
		{
			if (dataContext != null)
				throw new ArgumentNullException(nameof(dataContext));

            _dataContext = dataContext;
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _viewGeneratorService = viewGeneratorService;
            _pDFGeneratorService = pDFGeneratorService;
		}
		
		public byte[] Generate(Guid applicationId, string baseUri)
        {
            var application = GetApplicationById(applicationId);

            if (application != null)
            {
                baseUri = RemoveTrailingSlash(baseUri);

                string view;
                if (!_viewGeneratorService.GenerateView(baseUri, application, out view))
                {
                    return null;
                }

                return _pDFGeneratorService.GeneratePDF(view);
            }
            else
            {
                _logger.LogWarning(
                    $"No application found for id '{applicationId}'");
                return null;
            }
        }

        

        private Application GetApplicationById(Guid applicationId)
        {
            return _dataContext.Applications.Single(app => app.Id == applicationId);
        }

        private static string RemoveTrailingSlash(string baseUri)
        {
            if (baseUri.EndsWith("/"))
                baseUri = baseUri.Substring(baseUri.Length - 1);
            return baseUri;
        }
    }
}
