using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Runtime {
    public delegate void Procedure(RuntimeTerm[] arguments, System.Action cont_0);

    public static class Toplevel {
        //A very simplifed parsing function that can parse a single sentence
        public static RuntimeTerm parse_query(
            string query, ref int index,
            Dictionary<string, RuntimeTerm> v_map){
            StringBuilder sentence_name = new StringBuilder();
            List<RuntimeTerm> elements = new List<RuntimeTerm>();
            Debug.Assert(query[index] == '<');
            index++;
            for (; index < query.Length; index++){
                int i = index;
                if (query[i] == '>'){
                    if (elements.Count == 0)
                    {
                        return RuntimeTerm.make_atom(sentence_name.ToString(), false);
                    }
                    else
                    {
                        return RuntimeTerm.make_structured_term(sentence_name.ToString(), elements.ToArray(), false);
                    }
                }
                else if (query[i] == '<'){
                    sentence_name = sentence_name.Append("{}");
                    elements.Add(
                        Toplevel.parse_query(
                            query, ref index, v_map));
                } else if (query[i] == '?' && (query[i-1] == ' ' || query[i-1] == '<')){
                    sentence_name = sentence_name.Append("{}");
                    var variable_name = new StringBuilder();
                    for (; query[index] != ' ' && query[index] != '>'; index++){
                        variable_name = variable_name.Append(query[index]);
                    }
                    //@todo: this will change because insert is custom for you
                    if (v_map.ContainsKey(variable_name.ToString())){
                        elements.Add(v_map[variable_name.ToString()]);
                    } else {
                        var variable = RuntimeTerm.make_empty_variable(variable_name.ToString(), null);
                        v_map.Add(variable_name.ToString(), variable);
                        elements.Add(variable);
                        
                    }
                    index--; //the ending space or bracket should be parsed too
                }
                else {
                    sentence_name = sentence_name.Append(query[i]);
                    
                }
                
                
            }
            Debug.Fail("Unreachable code reached");
            throw new InvalidOperationException();
        }
        
        public static void run_REPL(Dictionary<string, Procedure> procedure_map){
            bool run = true;
            while (run) {
                Console.Write("query> ");
                //load string from STDIN
                string input = Console.ReadLine();
                if (input == "quit")
                    break;
                else if (input.Length > 0 && input[0] == '<') {
                    //parse it
                    Dictionary<string, RuntimeTerm> v_map = new Dictionary<string, RuntimeTerm>();
                    int index = 0;
                    RuntimeTerm query = Toplevel.parse_query(input, ref index, v_map);


                
                    //find the delegate corresponding to the functor in the table
                    Procedure proc = procedure_map[query.name];
                    bool wait_for_answer = true;
                    Action print_variables = delegate (){
                        Console.WriteLine("yes");
                        foreach (KeyValuePair<string, RuntimeTerm> entry in v_map) {
                            Console.WriteLine(
                                String.Format(
                                    "{0}: {1}",
                                    entry.Key,
                                    entry.Value
                                ));
                        
                        }
                        if (wait_for_answer) {
                            Console.Write("answers> ");
                            string answer_input = Console.ReadLine();
                            if (answer_input == "a") {
                                wait_for_answer = false;
                            } else if (answer_input == "quit"){
                                System.Environment.Exit(0);
                            }
                        }
                        
                    };

                    //Call it with the parsed parameters
                    proc(query.arguments, print_variables);
                    Console.WriteLine("no");
                } else {
                    Console.WriteLine("Invalid query!");
                }
            }

        }
    }
}
