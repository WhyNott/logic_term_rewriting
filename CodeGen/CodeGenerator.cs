using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Text;
using Term = Runtime.RuntimeTerm;
using Identifier = LogicTermDataStructures.Identifier;
using Variable = LogicTermDataStructures.Variable;
using LogicTermSentence = LogicTermDataStructures.Sentence;
using EmissionDataStructures;
using System.Reflection;
using Microsoft.CSharp;
using System.IO;


namespace CodeGen {

    
    public class CodeGenerator {

        public Procedure[] procedures;
        public CodeCompileUnit targetUnit;
        public CodeTypeDeclaration targetClass;

        public CodeGenerator(Procedure[] procedures){
            this.procedures = procedures;

            this.targetUnit = new CodeCompileUnit();
            CodeNamespace langauage = new CodeNamespace();
            langauage.Imports.Add(new CodeNamespaceImport("System"));
            langauage.Imports.Add(new CodeNamespaceImport("Runtime"));
            this.targetClass = new CodeTypeDeclaration(
                String.Format("Comp_{0}",
                              Identifier.current_compilation_unit)
            );
            this.targetClass.IsClass = true;
            this.targetClass.TypeAttributes = TypeAttributes.Public;
            langauage.Types.Add(targetClass);
            this.targetUnit.Namespaces.Add(langauage);
        }
        
        public string generate_procedure_table(){
            StringBuilder table = new StringBuilder(
                "var procedure_map = new Dictionary<string, Procedure> {\n"
            );
            foreach (var proc in this.procedures){
                table.AppendFormat(
                    "[\"{0}\"] = Comp_{1}.Procedure_{2},\n",
                    proc.head.name,
                    Identifier.current_compilation_unit,
                    proc.head.id.id_num
                );
            }
            table.Append("};\n");
            return table.ToString();
            
        }

        public void add_procedure(int proc_num){
            var proc = this.procedures[proc_num];
            CodeMemberMethod procedure_method = new CodeMemberMethod();
            procedure_method.Attributes =
                MemberAttributes.Public | MemberAttributes.Static;
            procedure_method.Name = String.Format("Procedure_{0}",
                              proc.head.id.id_num);
            procedure_method.Comments.Add(
                new CodeCommentStatement(new CodeComment(proc.head.name, false))
            );

            procedure_method.Parameters.Add(
                new CodeParameterDeclarationExpression(
                    new CodeTypeReference("RuntimeTerm[]"), "arguments"
                )
            );
            
            
            
            procedure_method.Parameters.Add(
                new CodeParameterDeclarationExpression(
                    typeof(Action), "cont_0"
                )
            );
            

            StringBuilder snippet = new StringBuilder("//beginning of the procedure code\n", 500);

            int arg_i = 0;
            foreach (var variable in proc.head.elements){
                snippet.AppendFormat(
                    "var {0} = arguments[{1}];\n",
                    variable.name,
                    arg_i
                );
                arg_i++;
            }
            
             
            
            foreach (var variable in proc.variables){
                snippet.AppendFormat(
                    "var {0} = RuntimeTerm.make_empty_variable(\"{0}\", null);\n",
                    variable.name
                );
            }

            for (int i = 0; i <= proc.conditions; i++){
                snippet.AppendFormat(
                    "bool cond_{0} = false; \n", i
                );
            }
            int j = 1;
            foreach (var continuation in proc.continuations) {
                snippet.AppendFormat(
                    "Action cont_{0} = delegate() {{ \n", j
                );
                this.write_verb(continuation, snippet, proc_num);
                snippet.Append("}; \n");
                j++;
            }

            this.write_verb(proc.body, snippet, proc_num);

            procedure_method.Statements.Add(
                new CodeSnippetStatement(snippet.ToString())
            );

            this.targetClass.Members.Add(procedure_method);

        }

        public void write_sentence(LogicTermSentence s, StringBuilder sb){

            if (s.elements.Length == 0) {
                sb.AppendFormat("RuntimeTerm.make_atom(\"{0}\", false)", s.name);
            } else {
                sb.AppendFormat("RuntimeTerm.make_structured_term(\"{0}\"", s.name);
                foreach (var element in s.elements) {
                    sb.Append(", ");
                    switch (element) {
                        case Variable v:
                            sb.AppendFormat("{0}", v.name);
                            break;
                        case LogicTermSentence ls:
                            this.write_sentence(ls, sb);
                            break;

                    }
                }
                sb.Append("false)");
            }
        }
        
        public void write_verb(EmissionVerb verb, StringBuilder sb, int proc_num){
            var proc = this.procedures[proc_num];
            switch (verb) {
                case Fail f:
                    break;
                    
                case Succeed s:
                    sb.AppendFormat(
                        "cont_{0}(); \n", s.next
                    );
                    break;
                    
                case Disjunction d:
                    foreach (var v in proc.variables){
                        if (v.is_head) continue;
                        sb.AppendFormat(
                            "var backup_{0} = {0}.backup_value(); \n", v.name
                        );
                    }
                    sb.Append("Trail.new_choice_point();\n");
                    foreach (var arg in d.arguments) {
                        this.write_verb(arg, sb, proc_num);
                        foreach (var v in proc.variables){
                            if (v.is_head) continue;
                            sb.AppendFormat(
                                "{0}.variable_value = backup_{0}; \n", v.name
                            );
                        }
                        sb.Append("Trail.restore_choice_point();\n");
                    }
                    sb.Append("Trail.remove_choice_point();\n");
                    break;
                    
                case SetCondition sc:
                    sb.AppendFormat(
                        "cond_{0} = true; \n", sc.condition
                    );
                    break;
                    
                case Call c:
                    sb.AppendFormat(
                        "Comp_{0}.Procedure_{1}(new RuntimeTerm[] {{",
                        c.sentence.id.compilation_number,
                        c.sentence.id.id_num
                    );
                    
                    foreach (var v in c.sentence.elements) {
                        sb.AppendFormat("{0},", v.name);
                    }
                    sb.AppendFormat("}}, cont_{0}); \n", c.next);
                    break;
                    
                case Conditional c:
                    switch (c.semidet){
                        case Unify u:
                            sb.AppendFormat(
                                "if ({0}.unify_with({1})) cont_{2}(); \n",
                                u.l_term.name, u.r_term.name, c.next 
                            );
                            break;
                        case Structure s:
                            sb.Append("{ \n var model = ");
                            this.write_sentence(s.r_term, sb);
                            sb.Append(";\n");
                            sb.AppendFormat(
                                "if ({0}.unify_with(model)) cont_{1}(); \n",
                                s.l_term.name,  c.next 
                            );
                            sb.Append("\n}\n");
                            break;
                        case CheckCondition ch:
                            sb.AppendFormat(
                                "if (cond_{0}) cont_{2}(); \n",
                                ch.condition, c.next 
                            );
                            break;
                    }
                    break;
                    
            }

        }
         
        public void output_source_code(string file){
            CSharpCodeProvider provider = new CSharpCodeProvider();

            // Create a TextWriter to a StreamWriter to the output file.
            using (StreamWriter sw = new StreamWriter(file, false))
            {
                IndentedTextWriter tw = new IndentedTextWriter(sw, "    ");

                tw.WriteLine("#r \"Runtime.dll\"");
                
                

                // Generate source code using the code provider.
                provider.GenerateCodeFromCompileUnit(this.targetUnit, tw,
                                                     new CodeGeneratorOptions());

                tw.Write(this.generate_procedure_table());

                tw.WriteLine("Toplevel.run_REPL(procedure_map);");

                // Close the output file.
                tw.Close();
            }
            
            
        }
        
    }
}





