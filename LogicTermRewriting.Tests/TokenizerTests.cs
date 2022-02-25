using System;
using Xunit;
using System.Collections.Generic;
using Parser;

namespace LogicTermRewriting.Tests {

    public class TokenizerTests {
        
        
        [Fact]
        public void SentenceTest() {

            String test_string = @"
    Predicates:
      - (Trude) is mother of (Sally)
      - (Tom) is the father of (Sally)


    Blah blah:
      - (This predicate <Should be parsed correctly
      as a code mode term containing> a single prose mode term)";

            Tokenizer t = new Tokenizer("test_string", test_string);

            Stack<Token> output = t.tokenize_file();

            Token [] tokens = output.ToArray();

            Array.Reverse(tokens);

            int indent_counter = 0;
            
            foreach (var token in tokens) {
                switch (token) {
                    case Indent:
                        indent_counter++;
                        break;
                    case Dedent:
                        indent_counter--;
                        break;

                }
            }
            //foreach (Token tok in tokens) Console.WriteLine(tok.ToString() + ":" + tok.line_count.ToString());
            
            Assert.True(indent_counter == 0);

            string[] expected_tokens = {
                "[Indent]",
                "[BeginSentence]",
                "[SentencePiece]:(Predicates)",
                "[EndSentence]",
                "[Semicolon]",
                "[Indent]",
                "[Dash]",
                "[BeginSentence]",
                "[BeginSentence]",
                "[SentencePiece]:(Trude)",
                "[EndSentence]",
                "[SentencePiece]:(is)",
                "[SentencePiece]:(mother)",
                "[SentencePiece]:(of)",
                "[BeginSentence]",
                "[SentencePiece]:(Sally)",
                "[EndSentence]",
                "[EndSentence]",
                "[Dash]",
                "[BeginSentence]",
                "[BeginSentence]",
                "[SentencePiece]:(Tom)",
                "[EndSentence]",
                "[SentencePiece]:(is)",
                "[SentencePiece]:(the)",
                "[SentencePiece]:(father)",
                "[SentencePiece]:(of)",
                "[BeginSentence]",
                "[SentencePiece]:(Sally)",
                "[EndSentence]",
                "[EndSentence]",
                "[Dedent]",
                "[BeginSentence]",
                "[SentencePiece]:(Blah)",
                "[SentencePiece]:(blah)",
                "[EndSentence]",
                "[Semicolon]",
                "[Indent]",
                "[Dash]",
                "[BeginSentence]",
                "[SentencePiece]:(This)",
                "[SentencePiece]:(predicate)",
                "[BeginSentence]",
 @"[SentencePieceVerbatim]:(Should be parsed correctly
      as a code mode term containing)",
                "[EndSentence]",
                "[SentencePiece]:(a)",
                "[SentencePiece]:(single)",
                "[SentencePiece]:(prose)",
                "[SentencePiece]:(mode)",
                "[SentencePiece]:(term)",
                "[EndSentence]",
                "[Dedent]",
                "[Dedent]"
          
            };

            
            for (int i = 0; i < tokens.Length; i++) {
                Assert.True(expected_tokens[i] == tokens[i].ToString());
            }
            

            

        }

        
        [Fact]
       public void PredicateTest() {
           
            String test_string = @"
    Predicates:
      - (room1) leads to (room2)
      - (room2) leads to (room3)

      - ?RoomA is connected to ?RoomB:
         or:
           - ?RoomA leads to ?RoomB
           - ?RoomB leads to ?RoomA

      - ?RoomA can be walked to from ?RoomB:
         or:
          - ?RoomA is connected to ?RoomB
          - and:
               - ?RoomA can be walked to from ?RoomC
               - ?RoomC can be walked to from ?RoomB
";

           Tokenizer t = new Tokenizer("test_string", test_string);
           
           Stack<Token> output = t.tokenize_file();
        
           Token [] tokens = output.ToArray();

           Array.Reverse(tokens);

           int indent_counter = 0;
           
           foreach (var token in tokens) {
               switch (token) {
                   case Indent:
                       indent_counter++;
                       break;
                   case Dedent:
                       indent_counter--;
                       break;

               }
           }
           
           Assert.True(indent_counter == 0);

           string[] expected_tokens = {
               "[Indent]",
               "[BeginSentence]",
               "[SentencePiece]:(Predicates)",
               "[EndSentence]",
               "[Semicolon]",
               "[Indent]",
               "[Dash]",
               "[BeginSentence]",
               "[BeginSentence]",
               "[SentencePiece]:(room1)",
               "[EndSentence]",
               "[SentencePiece]:(leads)",
               "[SentencePiece]:(to)",
               "[BeginSentence]",
               "[SentencePiece]:(room2)",
               "[EndSentence]",
               "[EndSentence]",
               "[Dash]",
               "[BeginSentence]",
               "[BeginSentence]",
               "[SentencePiece]:(room2)",
               "[EndSentence]",
               "[SentencePiece]:(leads)",
               "[SentencePiece]:(to)",
               "[BeginSentence]",
               "[SentencePiece]:(room3)",
               "[EndSentence]",
               "[EndSentence]",
               "[Dash]",
               "[BeginSentence]",
               "[Variable]:(RoomA)",
               "[SentencePiece]:(is)",
               "[SentencePiece]:(connected)",
               "[SentencePiece]:(to)",
               "[Variable]:(RoomB)",
               "[EndSentence]",
               "[Semicolon]",
               "[Indent]",
               "[BeginSentence]",
               "[SentencePiece]:(or)",
               "[EndSentence]",
               "[Semicolon]",
               "[Indent]",
               "[Dash]",
               "[BeginSentence]",
               "[Variable]:(RoomA)",
               "[SentencePiece]:(leads)",
               "[SentencePiece]:(to)",
               "[Variable]:(RoomB)",
               "[EndSentence]",
               "[Dash]",
               "[BeginSentence]",
               "[Variable]:(RoomB)",
               "[SentencePiece]:(leads)",
               "[SentencePiece]:(to)",
               "[Variable]:(RoomA)",
               "[EndSentence]",
               "[Dash]",
               "[BeginSentence]",
               "[Variable]:(RoomA)",
               "[SentencePiece]:(can)",
               "[SentencePiece]:(be)",
               "[SentencePiece]:(walked)",
               "[SentencePiece]:(to)",
               "[SentencePiece]:(from)",
               "[Variable]:(RoomB)",
               "[EndSentence]",
               "[Semicolon]",
               "[Dedent]",
               "[BeginSentence]",
               "[SentencePiece]:(or)",
               "[EndSentence]",
               "[Semicolon]",
               "[Indent]",
               "[Dash]",
               "[BeginSentence]",
               "[Variable]:(RoomA)",
               "[SentencePiece]:(is)",
               "[SentencePiece]:(connected)",
               "[SentencePiece]:(to)",
               "[Variable]:(RoomB)",
               "[EndSentence]",
               "[Dash]",
               "[BeginSentence]",
               "[SentencePiece]:(and)",
               "[EndSentence]",
               "[Semicolon]",
               "[Indent]",
               "[Dash]",
               "[BeginSentence]",
               "[Variable]:(RoomA)",
               "[SentencePiece]:(can)",
               "[SentencePiece]:(be)",
               "[SentencePiece]:(walked)",
               "[SentencePiece]:(to)",
               "[SentencePiece]:(from)",
               "[Variable]:(RoomC)",
               "[EndSentence]",
               "[Dash]",
               "[BeginSentence]",
               "[Variable]:(RoomC)",
               "[SentencePiece]:(can)",
               "[SentencePiece]:(be)",
               "[SentencePiece]:(walked)",
               "[SentencePiece]:(to)",
               "[SentencePiece]:(from)",
               "[Variable]:(RoomB)",
               "[EndSentence]",
               "[Dedent]",
               "[Dedent]",
               "[Dedent]",
               "[Dedent]",
               "[Dedent]"

           };

           for (int i = 0; i < tokens.Length; i++) {
               Assert.True(expected_tokens[i] == tokens[i].ToString());
           }

        }

        
        [Fact]
        public void CommentTest() {
            String test_string = @"
#Section 1
    Predicates: #This is where the predicates are stored
      - (Trude) is mother of (Sally) 
      #TODO: fix the father predicate
  #   - (Tom) is the father of (Sally)

";

            Tokenizer t = new Tokenizer("test_string", test_string);

            Stack<Token> output = t.tokenize_file();

            Token [] tokens = output.ToArray();

            Array.Reverse(tokens);

            int indent_counter = 0;
            
            foreach (var token in tokens) {
                switch (token) {
                    case Indent:
                        indent_counter++;
                        break;
                    case Dedent:
                        indent_counter--;
                        break;

                }
            }
            
            Assert.True(indent_counter == 0);

            //foreach (Token tok in tokens) {
            //    Console.WriteLine(tok.ToString() + ":" + tok.line_count.ToString());
            //}

            string[] expected_tokens = {
                "[Indent]",
                "[BeginSentence]",
                "[SentencePiece]:(Predicates)",
                "[EndSentence]",
                "[Semicolon]",
                "[Indent]",
                "[Dash]",
                "[BeginSentence]",
                "[BeginSentence]",
                "[SentencePiece]:(Trude)",
                "[EndSentence]",
                "[SentencePiece]:(is)",
                "[SentencePiece]:(mother)",
                "[SentencePiece]:(of)",
                "[BeginSentence]",
                "[SentencePiece]:(Sally)",
                "[EndSentence]",
                "[EndSentence]",
                "[Dedent]",
                "[Dedent]",            
                
            };

            for (int i = 0; i < tokens.Length; i++) {
                Assert.True(expected_tokens[i] == tokens[i].ToString());
            }

        }
        
    }
}
