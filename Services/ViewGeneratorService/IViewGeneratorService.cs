using Nml.Improve.Me.Dependencies;
using System;
using System.Collections.Generic;
using System.Text;

namespace priapism.worldliness
{
    public interface IViewGeneratorService
    {
        bool GenerateView(string baseUri, Application application, out string view);
    }
}
