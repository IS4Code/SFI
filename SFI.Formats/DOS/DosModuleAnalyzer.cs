using IS4.SFI.Formats.Modules;
using IS4.SFI.Services;
using IS4.SFI.Vocabulary;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace IS4.SFI.Analyzers
{
    /// <summary>
    /// An analyzer of MS-DOS executables, as instances of <see cref="DosModule"/>.
    /// </summary>
    [Description("An analyzer of MS-DOS executables.")]
    public class DosModuleAnalyzer : MediaObjectAnalyzer<DosModule>
    {
        /// <summary>
        /// Whether to attempt to run the executable to obtain additional data.
        /// </summary>
        [Description("Whether to attempt to run the executable to obtain additional data.")]
        public bool Emulate { get; set; } = true;

        /// <summary>
        /// Contains the encoding used to decode texts produced by the module.
        /// </summary>
        [Description("Contains the encoding used to decode texts produced by the module.")]
        public Encoding ConsoleEncoding { get; set; }

        /// <summary>
        /// The number of instructions to emulate in each step.
        /// </summary>
        [Description("The number of instructions to emulate in each step.")]
        public int InstructionStep { get; set; } = 1024;

        /// <summary>
        /// The limit on total number of instructions after which
        /// emulation is stopped.
        /// </summary>
        [Description("The limit on total number of instructions after which emulation is stopped.")]
        public int InstructionLimit { get; set; } = 1048576;

        /// <inheritdoc cref="EntityAnalyzer.EntityAnalyzer"/>
        public DosModuleAnalyzer() : base(Common.ApplicationClasses)
        {
            try{
                ConsoleEncoding = Encoding.GetEncoding(437);
            }catch{
                ConsoleEncoding = Encoding.ASCII;
            }
        }

        /// <inheritdoc/>
        public async override ValueTask<AnalysisResult> Analyze(DosModule module, AnalysisContext context, IEntityAnalyzers analyzers)
        {
            var node = GetNode(context);
            var uncompressed = module.GetCompressedContents();
            if(uncompressed != null)
            {
                await analyzers.Analyze(uncompressed, context.WithParentLink(node, Properties.HasPart));
            }else if(Emulate)
            {
                var text = module.Emulate(ConsoleEncoding, InstructionStep, InstructionLimit);
                if(IsDefined(text, out var value))
                {
                    node.Set(Properties.Description, value);
                }
            }
            return new AnalysisResult(node);
        }
    }
}
