
using Xunit;



using LogicTermDataStructures;

namespace LogicTermRewriting.Tests {

    public class TestingData {
        public static Context fake_context = new Context(-1, -1, "fake file");

        
        //son(X, Y) :- father(Y,X), male(X).
        public static Clause son_relationship(){
            var X = new Variable("X", TestingData.fake_context);
            var Y = new Variable("Y", TestingData.fake_context);

            var contents = new LogicVerb[] {
                new PredicateCall(
                    new Sentence("father", new Term[] {Y, X}, TestingData.fake_context)
                ),
                new PredicateCall(
                    new Sentence("male", new Term[] {X}, TestingData.fake_context)
                ),
            };
            
            return new Clause(
                new Sentence("son", new Term[] {X, Y}, TestingData.fake_context),
                new And(contents),
                TestingData.fake_context
            );
        } 
        

    }


    public class UnitTest1 {
      
        [Fact]
        public void Test1() {
            
            VariableExtractor ve = new VariableExtractor();
            var clause = ve.extract_variables(TestingData.son_relationship());
            Assert.True(clause.ToString() == "son(?.X(0),?.Y(1),)<>:-And(PredicateCallVariableExtracted(father(?.Y(1),?.X(0),),)PredicateCallVariableExtracted(male(?.X(0),),),)");
            LogicTermRewriter r = new LogicTermRewriter();
            var procedure = r.rewrite(clause);
            Assert.True(procedure.ToString() == "son(?.X(0),?.Y(1),){\nvariables: [];\ncontinuations: [Call(male(?.X(0),),0,)];\nnumber of conditions: 0;\nCall(father(?.Y(1),?.X(0),),1,);\n}");
            
        }
    }
}
