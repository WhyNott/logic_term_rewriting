using System;
using System.Collections.Generic;
using CodeGen;
using EmissionDataStructures;
using Parser;
using LogicTermRewriting;
using LogicTermDataStructures;
using LogicTermSentence = LogicTermDataStructures.Sentence;
using System.IO;
using System.Text;


namespace CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2) {
                Console.WriteLine("Usage: cli [input-file] [output-file]");

                Console.WriteLine("input-file: Path to yaml-like file containing the input program");
                Console.WriteLine("output-file: Path to where the .CSX script with the compiled program should be output");
                
                return;
            }
            string source_filename = Path.GetFullPath(args[0]);
            string output_filename = Path.GetFullPath(args[1]);

            string file_content = File.ReadAllText(source_filename);
            
            Tokenizer t = new Tokenizer(source_filename, file_content);

            Stack<Token> t_output = t.tokenize_file();
             
            Token [] tokens = t_output.ToArray();

            Array.Reverse(tokens);
            ASTParser p = new ASTParser(tokens);

            p.parse_document();

            var clauses = ASTParser.join_clauses_of_same_predicate(p.ast_to_clauses().ToArray());

            List<Procedure> procedures = new List<Procedure>();

            foreach (KeyValuePair<string, Clause> entry in clauses){
                VariableExtractor ve = new VariableExtractor();
                var clause = ve.extract_variables(entry.Value);
                Console.WriteLine(string.Join("", ve.clause_variables));
                LogicTermRewriter r = new LogicTermRewriter();
                var procedure = r.rewrite(clause);
                procedures.Add(procedure);
            }

            var cg = new CodeGenerator(procedures.ToArray());

            cg.add_all_procedures();
            cg.output_source_code(output_filename);
           
        }
    }
}
