using System;
using Xunit;

using CodeGen;
using EmissionDataStructures;

using LogicTermDataStructures;
using LogicTermSentence = LogicTermDataStructures.Sentence;

namespace LogicTermRewriting.Tests {

    public class TestingData {
        public static Context fake_context = new Context(-1, -1, "fake file");

        
        //son(X, Y) :- father(Y,X), male(X).
        public static Clause son_relationship(){
            var X = new Variable("X", TestingData.fake_context);
            var Y = new Variable("Y", TestingData.fake_context);

            var contents = new LogicVerb[] {
                new PredicateCall(
                    new LogicTermSentence("father", new Term[] {Y, X}, TestingData.fake_context)
                ),
                new PredicateCall(
                    new LogicTermSentence("male", new Term[] {X}, TestingData.fake_context)
                ),
            };
            
            return new Clause(
                new LogicTermSentence("son", new Term[] {X, Y}, TestingData.fake_context),
                new And(contents),
                TestingData.fake_context
            );
        }

        //father(terach,abraham).
        //father(terach,nachor).
        //father(terach,haran).
        //father(abraham,isaac).
        //father(haran,lot).
        //father(haran,milcah).
        //father(haran,yiscah).
        public static Clause father_data() {
            var X = new Variable("X", TestingData.fake_context);
            var Y = new Variable("Y", TestingData.fake_context);

            Func<string, string, LogicVerb> quickpair = (a, b) => new And(
                    new LogicVerb[] {
                        new PredicateCall(
                            new LogicTermSentence("{} = {}", new Term[] {
                                    X,
                                    new LogicTermSentence(a,
                                                          new Term[] {}, TestingData.fake_context)
                                }, TestingData.fake_context)
                        ),
                        new PredicateCall(
                            new LogicTermSentence("{} = {}", new Term[] {
                                    Y,
                                    new LogicTermSentence(b,
                                                          new Term[] {}, TestingData.fake_context)
                                }, TestingData.fake_context)
                        ),
                    });
            
            

            var contents = new LogicVerb[] {
                quickpair("terach","abraham"),
                quickpair("terach","nachor"),
                quickpair("terach","haran"),
                quickpair("abraham","isaac"),
                quickpair("haran","lot"),
                quickpair("haran","milcah"),
                quickpair("haran","yiscah")  
            };
            
            return new Clause(
                new LogicTermSentence("father", new Term[] {X, Y}, TestingData.fake_context),
                new Or(contents),
                TestingData.fake_context
            );

        }

        //male(terach).
        //male(abraham).
        //male(nachor).
        //male(haran).
        //male(isaac).
        //male(lot).

        public static Clause male_data() {
            var X = new Variable("X", TestingData.fake_context);

            Func<string, LogicVerb> quick = (a) => 
                        new PredicateCall(
                            new LogicTermSentence("{} = {}", new Term[] {
                                    X,
                                    new LogicTermSentence(a,
                                                          new Term[] {}, TestingData.fake_context)
                                }, TestingData.fake_context)
                        );

            var contents = new LogicVerb[] {
                quick("terach"),
                quick("abraham"),
                quick("nachor"),
                quick("haran"),
                quick("isaac"),
                quick("lot")
            };

            return new Clause(
                new LogicTermSentence("male", new Term[] {X}, TestingData.fake_context),
                new Or(contents),
                TestingData.fake_context
            );
            
        }

    }
    
    public class UnitTest1 {
        
        [Fact]
        public void Test1() {

            Func<Clause, Procedure> do_processing = (cl) => {
                VariableExtractor ve = new VariableExtractor();
                var clause = ve.extract_variables(cl);
                //Assert.True(clause.ToString() == "son(?.X(0),?.Y(1),)<>:-And(PredicateCallVariableExtracted(father(?.Y(1),?.X(0),),)PredicateCallVariableExtracted(male(?.X(0),),),)");
                LogicTermRewriter r = new LogicTermRewriter();
                var procedure = r.rewrite(clause);
                //Assert.True(procedure.ToString() == "son(?.X(0),?.Y(1),){\nvariables: [];\ncontinuations: [Call(male(?.X(0),),0,)];\nnumber of conditions: 0;\nCall(father(?.Y(1),?.X(0),),1,);\n}");
                return procedure;
            };
            

            var proc_son = do_processing(TestingData.son_relationship());
            var proc_father = do_processing(TestingData.father_data());
            var proc_male = do_processing(TestingData.male_data());

            var cg = new CodeGenerator(new Procedure[] {
                    proc_son,
                    proc_father,
                    proc_male
                });
            cg.add_procedure(0);
            cg.add_procedure(1);
            cg.add_procedure(2);
            cg.output_source_code("Test.cs");


        }
    }
}
