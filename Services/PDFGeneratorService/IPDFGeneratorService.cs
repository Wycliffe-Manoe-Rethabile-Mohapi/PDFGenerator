using System;
using System.Collections.Generic;
using System.Text;

namespace priapism.worldliness
{
    public interface IPDFGeneratorService
    {
        byte[] GeneratePDF(string view);
    }
}
