using System;
using System.CodeDom;
using Term = Runtime.RuntimeTerm;
using Identifier = LogicTermDataStructures.Identifier;
namespace CodeGen
{

    public static class ExampleCompUnit {
        public static procedure_1(Term X, Term Y, Action cont_0) {
            Action cont_1 = delegate() {
                ExampleCompUnit.procedure_2(X, cont_0);
            };
            
            ExampleCompUnit.procedure_3(Y, X, cont_1);
        }

        public static procedure_2(Term X);

    }
    
    public class CodeGenerator {

        public Procedure[] procedures;
        public CodeCompileUnit targetUnit;
        public CodeTypeDeclaration targetClass;

        public CodeGenerator(Procedure[] procedures){
            this.procedures = procedures;

            this.targetUnit = new CodeCompileUnit();
            CodeNamespace langauage = new CodeNamespace("Language");
            langauage.Imports.Add(new CodeNamespaceImport("System"));
            langauage.Imports.Add(new CodeNamespaceImport("Runtime"));
            this.targetClass = new CodeTypeDeclaration(
                String.Format("Comp_{0}",
                              Identifier.current_compilation_unit);
            );
            this.targetClass.IsClass = true;
            this.targetClass.TypeAttributes = TypeAttributes.Public;
            langauage.Types.Add(targetClass);
            this.targetUnit.Namespaces.Add(langauage);
        }


        public void add_procedure(int proc_num){
            var proc = this.procedures[proc_num];
            CodeMemberMethod procedure_method = new CodeMemberMethod();
            procedure_method.Attributes = MemberAttributes.Public;
            procedure_method.Name = String.Format("Procedure_{0}",
                              proc.head.id.id_num);
            procedure_method.Comments.Add(proc.head.name);
            for (var variable in proc.head.elements){
                procedure_method.Parameters.Add(
                     new CodeParameterDeclarationExpression(
                         typeof(RuntimeTerm), element.name
                     )
                );
            }
            procedure_method.Parameters.Add(
                new CodeParameterDeclarationExpression(
                    typeof(Action), "cont_0"
                )
            );
            

            StringBuilder snippet = new StringBuilder("//beginning of the procedure code\n", 500);

            for (var variable in proc.variables){
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
            int i = 1;
            for (var continuation in proc.continuations) {
                snippet.AppendFormat(
                    "Action cont_{0} = delegate() { \n", i
                );
                this.write_verb(continuation, snippet, proc_num);
                snippet.Append("}; \n");
                i++;
            }

            this.write_verb(proc.body, snippet, proc_num);

            procedure_method.Statements.Add(
                new CodeSnippetStatement(snippet.ToString())
            );

            this.targetClass.Members.Add(procedure_method);

        }

        public void write_sentence(Sentence s, StringBuilder sb){
            //stub
            throw new NotImplementedException();
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
                    for (var v in proc.variables){
                        if (v.is_head) continue;
                        sb.AppendFormat(
                            "var backup_{0} = {0}.backup_value(); \n", v.name
                        );
                    }
                    sb.Append("Trail.new_choice_point();");
                    for (var arg in d.arguments) {
                        this.write_verb(arg, sb, proc_num);
                        for (var v in proc.variables){
                            if (v.is_head) continue;
                            sb.AppendFormat(
                                "{0}.variable_value = backup_{0}; \n", v.name
                            );
                        }
                        sb.Append("Trail.restore_choice_point();");
                    }
                    sb.Append("Trail.remove_choice_point();");
                    break;
                    
                case SetCondition sc:
                    sb.AppendFormat(
                        "cond_{0} = true; \n", sc.condition
                    );
                    break;
                    
                case Call c:
                    sb.AppendFormat(
                        "Comp_{0}.Procedure_{1}(",
                        c.sentence.id.compilation_number,
                        c.sentence.id.id_num
                    );
                    
                    for (var v in c.sentence.elements) {
                        sb.AppendFormat("{0},", v.name);
                    }
                    sb.AppendFormat("cont_{0}); \n", c.next);
                    break;
                    
                case Conditional c:
                    switch (c.semidet){
                        case Unify u:
                            sb.AppendFormat(
                                "if ({0}.unify_with({1}) cont_{2}(); \n",
                                u.l_term, u.r_term, c.next 
                            );
                            break;
                        case Structure s:
                            sb.Append("{ \n var model = ");
                            this.write_sentence(s.r_term, sb);
                            sb.Append(";\n");
                            sb.AppendFormat(
                                "if ({0}.unify_with(model) cont_{1}(); \n",
                                s.l_term,  c.next 
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
                
                // Generate source code using the code provider.
                provider.GenerateCodeFromCompileUnit(this.targetUnit, tw,
                                                     new CodeGeneratorOptions());
                
                // Close the output file.
                tw.Close();
            }
            
            
        }
        
    }
}
