using System.Collections.Generic;
using src.Exceptions;
using src.Interfaces;
using src.Tokenizer;

namespace src.Parser.Nodes
{
    public class ProgramRaw : AstNode
    {
        public List<Annotation> Annotations = new List<Annotation>();
        public List<Data> Datas = new List<Data>();
        public List<Routine> Routines = new List<Routine>();
        public List<Module> Modules = new List<Module>();
        public Code code;

        public override void ParseChild(AstNode node)
        {
            switch (node) {
                case Annotation a:
                    Annotations.Add(a);
                    break;
                case Data d:
                    Datas.Add(d);
                    break;
                case Routine r:
                    Routines.Add(r);
                    break;
                case Module m:
                    Modules.Add(m);
                    break;
                case Code c:
                    if (code != null) {
                        throw new SemanticError("todo");
                    }
                    code = c;

                    break;
            }
        }
    }

    public class ProgramCombined : AstNode
    {
    }
}