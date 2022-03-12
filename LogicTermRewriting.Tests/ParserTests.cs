using System;
using Xunit;
using System.Collections.Generic;
using Parser;
using LogicTermDataStructures;

namespace LogicTermRewriting.Tests {

    public class ParserTests {

        [Fact]
        public void PredicateTest() {
           
            String test_string = @"
    Predicates:
      - (room1) leads to (room2)
      - (room2) leads to (room3)

      - 
       ?RoomA is connected to ?RoomB:
           or:
             - ?RoomA leads to ?RoomB
             - ?RoomB leads to ?RoomA

      - 
       ?RoomA can be walked to from ?RoomB:
           or:
            - ?RoomA is connected to ?RoomB
            - 
             and:
                 - ?RoomA can be walked to from ?RoomC
                 - ?RoomC can be walked to from ?RoomB
";

            Tokenizer t = new Tokenizer("test_string", test_string);
           
            Stack<Token> t_output = t.tokenize_file();
        
            Token [] tokens = t_output.ToArray();

            Array.Reverse(tokens);

            //foreach (Token tok in tokens) Console.WriteLine(tok.ToString() + ":" + tok.line_count.ToString());

            ASTParser p = new ASTParser(tokens);

            p.parse_document();

            var clauses = p.ast_to_clauses();

            foreach (Clause c in clauses){
                //    Console.WriteLine(c);

                VariableExtractor ve = new VariableExtractor();
                var clause = ve.extract_variables(c);
                LogicTermRewriter r = new LogicTermRewriter();
                var procedure = r.rewrite(clause);
                Console.WriteLine(procedure);
            }
        }

    }
}
