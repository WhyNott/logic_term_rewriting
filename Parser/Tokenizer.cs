using System;

namespace Parser {

    public class SyntaxErrorException : Exception {
        public SyntaxErrorException(){
        }

        public SyntaxErrorException(string message)
            : base(message){
        }
    }

    //todo: all of these should probably handle comments as well
    
    public class Tokenizer {
        public Stack<Token> tokens = new Stack();
        Stack<int> indentation_stack = new Stack<int>();
        private int line_count = 1;
        private int column_count = 0;
        private int index = 0;
        private String filename;
        private String file;
        private StringBuilder sentence_piece;
    

        public Tokenizer(String filename, String file){
            this.filename = filename;
            this.file = file;
            this.indentation_stack.Push(0);
        }

        private void incr(){
            index++;
            if (file[index] == '\n'){
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
            for (; this.index  < this.file.Length; this.incr()){
                if (this.file[this.index] == '\n')
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
                //previous sentence piece is over
                if (verbatim)
                    this.emmit_verbatim_sentence_piece();
                else
                    this.emmit_sentence_piece();
                
                for (; this.index  < this.file.Length; this.incr()){
                    if (Char.IsLetterOrDigit(this.file[this.index])){
                        this.sentence_piece.Insert(this.file[this.index]);
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
            //this starts at newline, so lets go to the next character
            this.incr();
            int indentation_counter = 0;
            char indentation_kind = this.file[this.index];
            for (; this.index < this.file.Length && Char.IsWhiteSpace(this.file[this.index]); this.incr()){
                //lines that end without any content don't interest us
                if (this.file[this.index] == '\n')
                    return;
                if (this.file[this.index] == indentation_kind)
                    if (indentation_kind == ' ')
                        indentation_counter++;
                    else if (indentation_kind == '\t')
                        indentation_counter += 4;
                    else //for now, anway
                        indentation_counter++;
                else
                    throw new SyntaxErrorException(String.Format("Inconsistent whitespace in indentation for file {0} at line {1}:{2} \nHint: have you accidentally mixed tabs and spaces?", this.filename, this.line_count, this.column_count));
            }
            if (indent_counter > indentation_stack.Peek()) {
                output_tokens.Push(new Indent(
                                       this.filename,
                                       this.line_count,
                                       this.column_count
                                   ));
                indentation_stack.Push(indent_counter);
            } else if (indent_counter < indentation_stack.Peek()) {
                while (indentation_stack.Count > 0) {
                    output_tokens.Add(new Dedent(
                                          this.filename,
                                          this.line_count,
                                          this.column_count
                                      ));
                    int indentation = indentation_stack.Pop();
                    if (indentation == indent_counter) {
                        break;
                    }
                }
                
                if (indentation_stack.Count == 0) {
                    if (indent_counter == 0){
                        indentation_stack.Push(0);
                    } else {
                        throw new SyntaxErrorException(String.Format("Inconsistent indentation in file {0} at line {1}:{2}", this.filename, this.line_count, this.column_count));
                    }
                }
                
            }
            
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
                } else if (this.file[this.index] == '\n'){
                    this.handle_indentation();
                } else if (Char.IsWhiteSpace(this.file[this.index])){
                    continue;
                } else if (this.file[this.index] == '('){
                
                    int paren_counter = 1;
                
                    bool inner_parens = false;
                    bool bounded = false;
                    //scan until the end of line
                    for (int i = this.index+1; index < this.file.Length && this.file[i] != "\n"; i++){
                        if (paren_counter == 0) {
                            bounded = true;
                            break;
                        }
                        if (this.file[i] == "(") {
                            paren_counter++;
                            inner_parens = true;
                        }

                        if (this.file[i] == ")") {
                            paren_counter--;
                        }
                    }

                    bounded = bounded || !inner_parens;

                    if (bounded)
                        this.tokenize_code_sentence();
                    else
                        this.tokenize_inline_sentence();
                } else {
                    this.tokenize_inline_sentence();
                } 
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
                        this.sentence_piece.Insert(this.file[this.index]);
                    } 
                } else if (this.file[this.index] == '('){
                    this.tokenize_code_sentence();
                } else if (this.file[this.index] == '<'){
                    this.tokenize_prose_sentence();
                } else if (Char.IsWhiteSpace(this.file[this.index])){
                    if (this.sentence_piece.Length != 0)
                        this.emmit_sentence_piece();
                } else {
                    this.sentence_piece.Insert(this.file[this.index]);
                }
            }
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
                        this.sentence_piece.Insert(this.file[this.index]);
                    } 
                } else if (Char.IsWhiteSpace(this.file[this.index])){
                        this.emmit_sentence_piece();
                } else {
                    this.sentence_piece.Insert(this.file[this.index]);
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
            for (; this.index < this.file.Length; this.incr()){
                if (this.file[this.index] == '#'){
                    this.emmit_verbatim_sentence_piece();
                    this.handle_comment();
                } else if (this.file[this.index] == '<'){
                    this.emmit_verbatim_sentence_piece();
                    this.tokenize_prose_sentence();
                } else if (this.file[this.index] == '?'){
                    if (!this.tokenize_variable(verbatim=true)){
                        this.sentence_piece.Insert(this.file[this.index]);
                    }
                } else {
                    this.sentence_piece.Insert(this.file[this.index]);
                }
            }

            throw new SyntaxErrorException(String.Format("EOF while parsing file {0} for sentence beginning at {1}:{2} \nHint: have you forgotten a closing angle bracket (>) somewhere?", this.filename, begin_lc, begin_cc));
            
        }
        
    }   
}
