using IS4.SFI.Formats.Modules;
using IS4.SFI.Services;
using IS4.SFI.Vocabulary;
using System.Text;
using System.Threading.Tasks;

namespace IS4.SFI.Analyzers
{
    /// <summary>
    /// An analyzer of MS-DOS modules, as instances of <see cref="DosModule"/>.
    /// </summary>
    public class DosModuleAnalyzer : MediaObjectAnalyzer<DosModule>
    {
        /// <summary>
        /// Contains the encoding used to decode texts produced by the module.
        /// </summary>
        public Encoding ConsoleEncoding { get; set; }

        /// <summary>
        /// Whether to attempt to run the executable to obtain additional data.
        /// </summary>
        public bool Emulate { get; set; } = true;

        /// <summary>
        /// The number of instruction to emulate in each step.
        /// </summary>
        public int InstructionStep { get; set; } = 1024;

        /// <summary>
        /// The limit on total number of instructions after which
        /// emulation is stopped.
        /// </summary>
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
                var infoNode = (await analyzers.Analyze<IFileInfo>(uncompressed, context.WithParent(node))).Node;
                if(infoNode != null)
                {
                    node.Set(Properties.BelongsToContainer, infoNode);
                }
            }else if(Emulate)
            {
                var text = module.Emulate(ConsoleEncoding, InstructionStep, InstructionLimit);
                if(text != null)
                {
                    node.Set(Properties.Description, text);
                }
            }
            return new AnalysisResult(node);
        }
    }
}
