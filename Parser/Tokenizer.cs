using System;
using System.Collections.Generic;
using System.Text;

namespace Parser {

    public class SyntaxErrorException : Exception {
        public SyntaxErrorException(){
        }

        public SyntaxErrorException(string message)
            : base(message){
        }
    }


    public class Token {
        public int line_count;
        public int column_count;
        public String filename;

        public Token(){}

        public Token(String filename, int line_count, int column_count) {
            this.filename = filename;
            this.line_count = line_count;
            this.column_count = column_count;
        }
        

        public override string ToString() {
            var name = base.ToString().Split('.')[1];
            return "[" + name + "]";
            
        }
    }

    public class StringToken : Token {
        public String content;
        
        public StringToken(String filename, int line_count, int column_count, String content)
            :base(filename, line_count, column_count){
            
            this.content = content;
            
        }

        public override string ToString() {
            return  base.ToString() + ":(" + this.content + ")";
        }
    }

    public class Indent : Token{
        public Indent(){}
        public Indent(String filename, int line_count, int column_count)
           :base(filename, line_count, column_count) {}
    }
    

    public class Dedent : Token{
        public Dedent(){}
        public Dedent(String filename, int line_count, int column_count)
            :base(filename, line_count, column_count) {}
    }
    
    public class Dash : Token{
        public Dash(){}
        public Dash(String filename, int line_count, int column_count)
            :base(filename, line_count, column_count) {}
    }
    public class Semicolon : Token{
        public Semicolon(){}
        public Semicolon(String filename, int line_count, int column_count)
            :base(filename, line_count, column_count) {}
    }
    public class BeginSentence : Token{
        public BeginSentence(){}
        public BeginSentence(String filename, int line_count, int column_count)
           :base(filename, line_count, column_count) {}
    }
    public class EndSentence : Token{
        public EndSentence(){}
        public EndSentence(String filename, int line_count, int column_count)
            :base(filename, line_count, column_count) {}
    }


    public class SentencePiece : StringToken{
        public SentencePiece(String filename, int line_count, int column_count, String content)
            :base(filename, line_count, column_count, content) {} 
    }
    public class SentencePieceVerbatim : StringToken{
        public SentencePieceVerbatim(String filename, int line_count, int column_count, String content)
            :base(filename, line_count, column_count, content) {} 
    }
    public class Variable : StringToken{
        public Variable(String filename, int line_count, int column_count, String content)
            :base(filename, line_count, column_count, content) {} 
    }
    
    

    public class Tokenizer {
        public Stack<Token> tokens = new Stack<Token>();
        Stack<int> indentation_stack = new Stack<int>();
        private int line_count = 1;
        private int column_count = 0;
        private int index = 0;
        private String filename;
        private String file;
        private StringBuilder sentence_piece;
    

        public Tokenizer(String filename, String file){
            this.filename = filename;
            this.file = file.Replace(Environment.NewLine, "\n"); //Remove carriage returns from newlines for easier handling
            this.indentation_stack.Push(0);
            this.sentence_piece = new StringBuilder();
        }

        private void incr(){
            index++;
            if (this.index  < this.file.Length && file[index] == '\n'){
                this.line_count++;
                this.column_count = 0;
            } else {
                this.column_count++;
            }
        }

        private void decr(){
            index--;
            //the column number will be wrong sometimes
            //but no one looks at column numbers in compiler errors anyway
            if (this.column_count == 0)
                this.line_count--;
            else
                this.column_count--;
        }

        private void handle_comment(){
            for (; this.index+1  < this.file.Length; this.incr()){
                if (this.file[this.index+1] == '\n')
                    break;
            }
        }

        private void emmit_sentence_piece(){
            if (this.sentence_piece.Length != 0){
                tokens.Push(
                    new SentencePiece(
                        this.filename,
                        this.line_count,
                        this.column_count,
                        this.sentence_piece.ToString()
                    )
                );
                this.sentence_piece.Clear();
            }
        }

        private void emmit_verbatim_sentence_piece(){
            if (this.sentence_piece.Length != 0){
                tokens.Push(
                    new SentencePieceVerbatim(
                        this.filename,
                        this.line_count,
                        this.column_count,
                        this.sentence_piece.ToString()
                    )
                );   
                this.sentence_piece.Clear();
            }
        }
        
        private bool tokenize_variable(bool verbatim=false){
            
            if (Char.IsLetterOrDigit(this.file[this.index+1])) {
                this.incr();
                //previous sentence piece is over
                if (verbatim)
                    this.emmit_verbatim_sentence_piece();
                else
                    this.emmit_sentence_piece();
                
                for (; this.index  < this.file.Length; this.incr()){
                    if (Char.IsLetterOrDigit(this.file[this.index])){
                        this.sentence_piece.Append(this.file[this.index]);
                    } else {
                        break;
                    }
                }
                //reaching end of file is same as reaching a non-alphanumeric char
                this.decr();
                tokens.Push(
                    new Variable(
                        this.filename,
                        this.line_count,
                        this.column_count,
                        this.sentence_piece.ToString()
                    )
                );
                this.sentence_piece.Clear();
               
                return true;

            
            } else {
                //regular question mark, not a variable
                return false;
            }
        }

        public void handle_indentation(){
            //If there is no next line or the line begins with a comment,
            //there is no indentation to handle
            if (this.index+1 >= this.file.Length || this.file[this.index+1] == '#') {
                return;
            }
            
            //Move on to the first character of the line
            this.incr();
            
            
            int indentation_counter = 0;

            //The kind of whitespace used to indent a given line
            //(we do not allow mixing eg. tabs and spaces due to ambiguous lengths) 
            char indentation_kind = this.file[this.index];
            

            //Iterate until the end of file or the beginning of actual line content
            for (; this.index < this.file.Length && Char.IsWhiteSpace(this.file[this.index]); this.incr()){
                //lines that end without any content don't interest us
                if (this.file[this.index] == '\n'){
                    this.decr();
                    return;
                }

                if (this.file[this.index] == indentation_kind)
                    if (indentation_kind == ' ')
                        indentation_counter++;
                    else if (indentation_kind == '\t')
                        //we assume a tab is equal to 4 spaces
                        indentation_counter += 4;
                    else //all other whitespace counts as 1 space (for now)
                        indentation_counter++;
                else
                    throw new SyntaxErrorException(String.Format("Inconsistent whitespace in indentation for file {0} at line {1}:{2} \nHint: have you accidentally mixed tabs and spaces?", this.filename, this.line_count, this.column_count));
            }
            //If file has ended or the line is just a comment, we don't need to handle indentation
            if (this.index >= this.file.Length || this.file[this.index] == '#') {
                //we move the index one before to ensure tokenize_file method
                //can handle both the EOF and comment cases
                this.decr();
                return;
            }

            //current line has deeper indentation then the previous line
            if (indentation_counter > this.indentation_stack.Peek()) {
                this.tokens.Push(new Indent(
                                       this.filename,
                                       this.line_count,
                                       this.column_count
                                   ));
                this.indentation_stack.Push(indentation_counter);
            }
            //current line has smaller indentation then the previous line
            else if (indentation_counter < indentation_stack.Peek()) {
                //continue emmiting dedentation tokens until we encounter
                //an old indentation that matches
                while (indentation_stack.Count > 0) {
                    
                    int indentation = this.indentation_stack.Peek();
                    if (indentation == indentation_counter) {
                        break;
                    } else {
                        this.indentation_stack.Pop();
                    }
                    this.tokens.Push(new Dedent(
                                         this.filename,
                                          this.line_count,
                                          this.column_count
                                     ));
                                      
                }
                
                //once the stack is empty
                if (indentation_stack.Count == 0) {
                    if (indentation_counter == 0){
                        indentation_stack.Push(0);
                        this.tokens.Push(new Dedent(
                                         this.filename,
                                         this.line_count,
                                         this.column_count
                                         ));
                    } else {
                        throw new SyntaxErrorException(String.Format("Inconsistent indentation in file {0} at line {1}:{2}", this.filename, this.line_count, this.column_count));
                    }
                }
                
            }
            //back the index one character to allow for line content to be
            //tokenized properly by tokenize_file
            this.decr();
        }

        public Stack<Token> tokenize_file(){
            for (; this.index < this.file.Length; this.incr()){
                if (this.file[this.index] == '#'){
                    this.handle_comment();
                } else if (this.file[this.index] == '-'){
                    tokens.Push(
                        new Dash(
                            this.filename,
                            this.line_count,
                            this.column_count
                        )
                    );
                } else if (this.file[this.index] == ':'){
                    tokens.Push(
                        new Semicolon(
                            this.filename,
                            this.line_count,
                            this.column_count
                        )
                    );
                } else if (this.file[this.index] == '<'){
                    //@todo: shouldn't this be doing a simillar scan to the
                    //parenthesis one?
                    this.tokenize_prose_sentence();
                } else if (this.file[this.index] == '#'){
                    this.handle_comment();
                } else if (this.file[this.index] == '\n'){
                    this.handle_indentation();
                } else if (Char.IsWhiteSpace(this.file[this.index])){
                    continue;
                } else if (this.file[this.index] == '('){
                
                    int paren_counter = 1;
                
                    
                    bool unbound = false;
                    //scan until the end of line
                    for (int i = this.index+1; index < this.file.Length && this.file[i] != '\n'; i++){
                        if (paren_counter == 0) {
                            unbound = true;
                            break;
                        }
                        if (this.file[i] == '(') {
                            paren_counter++;
                    
                        }

                        if (this.file[i] == ')') {
                            paren_counter--;
                        }
                    }

                    var bounded = !unbound;

                    if (bounded)
                        this.tokenize_code_sentence();
                    else
                        this.tokenize_inline_sentence();
                } else {
                    this.tokenize_inline_sentence();
                } 
            }
            while (indentation_stack.Count > 0 && indentation_stack.Peek() != 0) {
                this.tokens.Push(new Dedent(
                                     this.filename,
                                     this.line_count,
                                     this.column_count
                                 ));
                this.indentation_stack.Pop();
            }
            
            return this.tokens;
            
        
        
        }
        private void tokenize_inline_sentence(){
            tokens.Push(
                new BeginSentence(
                    this.filename,
                    this.line_count,
                    this.column_count
                )
            );
            for (; this.index < this.file.Length; this.incr()){
                if (this.file[this.index] == '#'){
                    this.handle_comment();
                } else if (this.file[this.index] == '\n'){
                    break;
                } else if (this.file[this.index] == ':'){
                    break;
                } else if (this.file[this.index] == '?'){
                    if (!this.tokenize_variable()){
                        this.sentence_piece.Append(this.file[this.index]);
                    } 
                } else if (this.file[this.index] == '('){
                    this.tokenize_code_sentence();
                } else if (this.file[this.index] == '<'){
                    this.tokenize_prose_sentence();
                } else if (Char.IsWhiteSpace(this.file[this.index])){
                    if (this.sentence_piece.Length != 0)
                        this.emmit_sentence_piece();
                } else {
                    this.sentence_piece.Append(this.file[this.index]);
                }
            }
            this.emmit_sentence_piece();
            this.decr();
            tokens.Push(
                new EndSentence(
                    this.filename,
                    this.line_count,
                    this.column_count
                )
            );
        }

        public void tokenize_code_sentence(){
            var begin_lc = this.line_count;
            var begin_cc = this.column_count; 
            tokens.Push(
                new BeginSentence(
                    this.filename,
                    begin_lc,
                    begin_cc
                )
            );
            this.incr();//skip the initial '('
            for (; this.index < this.file.Length; this.incr()){
                if (this.file[this.index] == '#'){
                    this.emmit_sentence_piece();
                    this.handle_comment();
                } else if (this.file[this.index] == '('){
                    this.emmit_sentence_piece();
                    this.tokenize_code_sentence();
                } else if (this.file[this.index] == ')'){
                    this.emmit_sentence_piece();
                    tokens.Push(
                        new EndSentence(
                            this.filename,
                            begin_lc,
                            begin_cc
                        )
                    );
                    return;
                } else if (this.file[this.index] == '<'){
                    this.emmit_sentence_piece();
                    this.tokenize_prose_sentence();
                } else if (this.file[this.index] == '?'){
                    if (!this.tokenize_variable()){
                        this.sentence_piece.Append(this.file[this.index]);
                    } 
                } else if (Char.IsWhiteSpace(this.file[this.index])){
                        this.emmit_sentence_piece();
                } else {
                    this.sentence_piece.Append(this.file[this.index]);
                }
            }
            
            throw new SyntaxErrorException(String.Format("EOF while parsing file {0} for sentence beginning at {1}:{2} \nHint: have you forgotten a closing parenthesis somewhere?", this.filename, begin_lc, begin_cc));
            
        }

        public void tokenize_prose_sentence(){
            var begin_lc = this.line_count;
            var begin_cc = this.column_count; 
            tokens.Push(
                new BeginSentence(
                    this.filename,
                    begin_lc,
                    begin_cc
                )
            );
            this.incr();//skip the initial '<'
            for (; this.index < this.file.Length; this.incr()){
                if (this.file[this.index] == '#'){
                    this.emmit_verbatim_sentence_piece();
                    this.handle_comment();
                } else if (this.file[this.index] == '<'){
                    this.emmit_verbatim_sentence_piece();
                    this.tokenize_prose_sentence();
                } else if (this.file[this.index] == '>'){
                    this.emmit_verbatim_sentence_piece();
                    tokens.Push(
                        new EndSentence(
                            this.filename,
                            begin_lc,
                            begin_cc
                        )
                    );
                    return;
                } else if (this.file[this.index] == '?'){
                    if (!this.tokenize_variable(verbatim:true)){
                        this.sentence_piece.Append(this.file[this.index]);
                    }
                } else {
                    this.sentence_piece.Append(this.file[this.index]);
                }
            }

            throw new SyntaxErrorException(String.Format("EOF while parsing file {0} for sentence beginning at {1}:{2} \nHint: have you forgotten a closing angle bracket (>) somewhere?", this.filename, begin_lc, begin_cc));
            
        }
        
    }   
}



