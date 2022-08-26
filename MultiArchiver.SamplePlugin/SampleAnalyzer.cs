using IS4.MultiArchiver;
using IS4.MultiArchiver.Services;
using System;
using System.Threading.Tasks;

namespace MultiArchiver.SamplePlugin
{
    public class SampleAnalyzer : IEntityAnalyzer<SampleAnalyzer>
    {
        public SampleAnalyzer(Archiver archiver)
        {
            Console.WriteLine("Hello!");
        }

        public ValueTask<AnalysisResult> Analyze(SampleAnalyzer entity, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            throw new NotImplementedException();
        }
    }
}
