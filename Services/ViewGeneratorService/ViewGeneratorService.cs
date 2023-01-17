using Nml.Improve.Me;
using Nml.Improve.Me.Dependencies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace priapism.worldliness
{
    public class ViewGeneratorService : IViewGeneratorService
    {
        public IViewGenerator _view_Generator { get; }
        public ILogger<ViewGeneratorService> _logger { get; }
        public IConfiguration _configuration { get; }
        public IPathProvider _templatePathProvider { get; }

        public ViewGeneratorService(IViewGenerator viewGenerator, ILogger<ViewGeneratorService> logger, IConfiguration configuration, IPathProvider templatePathProvider)
        {
            _view_Generator = viewGenerator;
            _logger = logger;
            _configuration = configuration;
            _templatePathProvider = templatePathProvider;
        }
        public bool GenerateView(string baseUri, Application application, out string view)
        {
            view = string.Empty;
            switch (application.State)
            {
                case ApplicationState.Pending:
                    view = GeneratePendingView(baseUri, application);
                    break;
                case ApplicationState.Activated:
                    view = GenerateActivatedView(baseUri, application);
                    break;
                case ApplicationState.InReview:
                    view = GenerateInReviewView(baseUri, application);
                    break;
                default:
                    _logger.LogWarning(
                            $"The application is in state '{application.State}' and no valid document can be generated for it.");
                    return false;
            }

            return true;
        }
        private string GenerateInReviewView(string baseUri, Application application)
        {
            var view = string.Empty;
            var templatePath = _templatePathProvider.Get("InReviewApplication");
            var inReviewMessage = "Your application has been placed in review" +
                                application.CurrentReview.Reason switch
                                {
                                    { } reason when reason.Contains("address") =>
                                        " pending outstanding address verification for FICA purposes.",
                                    { } reason when reason.Contains("bank") =>
                                        " pending outstanding bank account verification.",
                                    _ =>
                                        " because of suspicious account behaviour. Please contact support ASAP."
                                };
            var inReviewApplicationViewModel = new InReviewApplicationViewModel();
            inReviewApplicationViewModel.ReferenceNumber = application.ReferenceNumber;
            inReviewApplicationViewModel.State = application.State.ToDescription();
            inReviewApplicationViewModel.FullName = string.Format(
                "{0} {1}",
                application.Person.FirstName,
                application.Person.Surname);
            inReviewApplicationViewModel.LegalEntity =
                application.IsLegalEntity ? application.LegalEntity : null;
            inReviewApplicationViewModel.PortfolioFunds = application.Products.SelectMany(p => p.Funds);
            inReviewApplicationViewModel.PortfolioTotalAmount = application.Products.SelectMany(p => p.Funds)
                .Select(f => (f.Amount - f.Fees) * _configuration.TaxRate)
                .Sum();
            inReviewApplicationViewModel.InReviewMessage = inReviewMessage;
            inReviewApplicationViewModel.InReviewInformation = application.CurrentReview;
            inReviewApplicationViewModel.AppliedOn = application.Date;
            inReviewApplicationViewModel.SupportEmail = _configuration.SupportEmail;
            inReviewApplicationViewModel.Signature = _configuration.Signature;
            view = _view_Generator.GenerateFromPath($"{baseUri}{templatePath}", inReviewApplicationViewModel);
            return view;
        }

        private string GenerateActivatedView(string baseUri, Application application)
        {
            var view = string.Empty;
            var path = _templatePathProvider.Get("ActivatedApplication");
            ActivatedApplicationViewModel vm = new ActivatedApplicationViewModel
            {
                ReferenceNumber = application.ReferenceNumber,
                State = application.State.ToDescription(),
                FullName = $"{application.Person.FirstName} {application.Person.Surname}",
                LegalEntity = application.IsLegalEntity ? application.LegalEntity : null,
                PortfolioFunds = application.Products.SelectMany(p => p.Funds),
                PortfolioTotalAmount = application.Products.SelectMany(p => p.Funds)
                                                .Select(f => (f.Amount - f.Fees) * _configuration.TaxRate)
                                                .Sum(),
                AppliedOn = application.Date,
                SupportEmail = _configuration.SupportEmail,
                Signature = _configuration.Signature
            };
            view = _view_Generator.GenerateFromPath(baseUri + path, vm);
            return view;
        }

        private string GeneratePendingView(string baseUri, Application application)
        {
            var view = string.Empty;
            var path = _templatePathProvider.Get("PendingApplication");
            PendingApplicationViewModel vm = new PendingApplicationViewModel
            {
                ReferenceNumber = application.ReferenceNumber,
                State = application.State.ToDescription(),
                FullName = application.Person.FirstName + " " + application.Person.Surname,
                AppliedOn = application.Date,
                SupportEmail = _configuration.SupportEmail,
                Signature = _configuration.Signature
            };
            view = _view_Generator.GenerateFromPath($"{baseUri}{path}", vm);
            return view;
        }
    }
}
