using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace task
{
    public enum ErrorCode
    {
        ExpectedOpenParenthesis,
        ExpectedCloseParenthesis,
        ExpectedOpenBrace,
        ExpectedCloseBrace,
        ExpectedComma,
        ExpectedColon,
        ExpectedSemicolon,
        ExpectedArrow,
        ExpectedDoubleDot,
        ExpectedEquals,
        ExpectedFun,
        ExpectedIn,
        ExpectedMain,
        ExpectedWhen,
        ExpectedElse,
        ExpectedVarOrVal,
        ExpectedIntOrChar,
        ExpectedIdentifier,
        ExpectedLiteral,
        InvalidExpression,
        InvalidStatement,
        InvalidDeclaration,
        InvalidOperator,
        UnexpectedToken,
        InvalidLiteral,
        UndeclaredIdentifier,
        RedeclaredIdentifier,
        LiteralOutOfRange,
        VariableNotInitialized,
        IncompatibleTypesInOperation,
        AssignmentToConstant,
        ValMustBeInitialized,
        IncompatibleTypesInAssignment,
        WhenBranchesHaveDifferentTypes,
        OperationNotDefinedForType,
        CannotInferType,
    }

    public class SyntLLParser
    {
        List<Token> Tokens;
        int index;
        private List<(ErrorType Type, string Message)> errors;

        private Stack<RussBlock> operandStack;
        private Stack<Token> operatorStack;

        private class VariableInfo
        {
            public string Type { get; set; }
            public bool IsInitialized { get; set; }
            public bool IsConstant { get; set; }
            public int DeclarationTokenIndex { get; set; }
            public string OriginalKeyword { get; set; }

            public VariableInfo(string type, bool isInitialized, bool isConstant, int declarationTokenIndex, string keyword)
            {
                Type = type;
                IsInitialized = isInitialized;
                IsConstant = isConstant;
                DeclarationTokenIndex = declarationTokenIndex;
                OriginalKeyword = keyword;
            }
        }
        private Stack<Dictionary<string, VariableInfo>> scopeStack;
        private List<RussBlock> russianCodeMatrix;

        private int tempCounter;
        private string currentProcessingVarId;        
        private string currentVarOrValKeyword;

        private int currentDepth = 0;

        public SyntLLParser(List<Token> tokens, out List<(ErrorType Type, string Message)> analysisErrors)
        {
            Tokens = tokens;
            index = 0;

            errors = new List<(ErrorType, string)>();
            analysisErrors = errors;

            operandStack = new Stack<RussBlock>();
            operatorStack = new Stack<Token>();
            russianCodeMatrix = new List<RussBlock>();
            tempCounter = 0;
            scopeStack = new Stack<Dictionary<string, VariableInfo>>();
        }

        private void AddToMatrix(RussBlock block)
        {
            block.Depth = currentDepth;
            russianCodeMatrix.Add(block);
        }

        private void EnterScope()
        {
            scopeStack.Push(new Dictionary<string, VariableInfo>());
            AddToMatrix(new ScopeStartBlock());
            currentDepth++;
        }

        private void ExitScope()
        {
            currentDepth--;
            AddToMatrix(new ScopeEndBlock());
        }

        private ErrorCode DeclareIdentifier(string id, string type, bool isConstant, bool isInitialized, int tokenIndex, string keyword)
        {
            var currentScope = scopeStack.Peek();
            if (currentScope.ContainsKey(id))
            {
                Error(ErrorCode.RedeclaredIdentifier, tokenIndex, id);
                return ErrorCode.RedeclaredIdentifier;
            }

            currentScope.Add(id, new VariableInfo(type, isInitialized, isConstant, tokenIndex, keyword));
            return ErrorCode.ExpectedCloseBrace;   
        }

        private VariableInfo GetVariableInfo(string id, int usageTokenIndex)
        {
            foreach (var scope in scopeStack)
            {
                if (scope.TryGetValue(id, out VariableInfo varInfo))
                {
                    return varInfo;
                }
            }
            Error(ErrorCode.UndeclaredIdentifier, usageTokenIndex, id);
            return null;
        }

        private void SetIdentifierInitialized(string id, int usageTokenIndex)
        {
            foreach (var scope in scopeStack)
            {
                if (scope.TryGetValue(id, out VariableInfo varInfo))
                {
                    varInfo.IsInitialized = true;
                    return;
                }
            }
            Error(ErrorCode.UndeclaredIdentifier, usageTokenIndex, id);
        }


        private int GetOperatorPriority(Token opToken)
        {
            if (opToken == null || opToken.Type != 'S')
            {
                return -1;
            }

            switch (opToken.Value)
            {
                case "(": return 0;
                case "+":
                case "-": return 1;
                case "*":
                case "/":
                case "%": return 2;
                default: return -1;
            }
        }

        public List<RussBlock> GetRussianCodeMatrix()
        {
            return russianCodeMatrix;
        }

        private ErrorType Classify(ErrorCode code)
        {
            switch (code)
            {
                case ErrorCode.UndeclaredIdentifier:
                case ErrorCode.RedeclaredIdentifier:
                case ErrorCode.LiteralOutOfRange:
                case ErrorCode.VariableNotInitialized:
                case ErrorCode.IncompatibleTypesInOperation:
                case ErrorCode.AssignmentToConstant:
                case ErrorCode.ValMustBeInitialized:
                case ErrorCode.IncompatibleTypesInAssignment:
                case ErrorCode.WhenBranchesHaveDifferentTypes:
                case ErrorCode.OperationNotDefinedForType:
                case ErrorCode.CannotInferType:
                    return ErrorType.Semantic;
                default:
                    return ErrorType.Syntactic;
            }
        }

        private void Error(ErrorCode code, int tokenIndex, params string[] details)
        {
            var type = Classify(code);
            string baseMessage = $"Ошибка в токене №{tokenIndex}: {code}";
            string detailMessage = "";

            switch (code)
            {
                case ErrorCode.UndeclaredIdentifier:
                    detailMessage = $" - Идентификатор '{details[0]}' использован до объявления.";
                    break;
                case ErrorCode.RedeclaredIdentifier:
                    detailMessage = $" - Идентификатор '{details[0]}' уже объявлен в этом блоке.";
                    break;
                case ErrorCode.LiteralOutOfRange:
                    detailMessage = $" - Литерал '{GetToken(tokenIndex).Value}' выходит за допустимый диапазон [0, 65535].";
                    break;
                case ErrorCode.VariableNotInitialized:
                    detailMessage = $" - Переменная '{details[0]}' использована до присвоения значения.";
                    break;
                case ErrorCode.IncompatibleTypesInOperation:
                    detailMessage = $" - Несовместимые типы в операции {details[0]}.";
                    break;
                case ErrorCode.AssignmentToConstant:
                    detailMessage = $" - Невозможно присвоить значение константе '{details[0]}' после инициализации.";
                    break;
                case ErrorCode.ValMustBeInitialized:
                    detailMessage = $" - Константа '{details[0]}' (val) должна быть инициализирована при объявлении.";
                    break;
                case ErrorCode.IncompatibleTypesInAssignment:
                    detailMessage = $" - Несовместимые типы при присваивании переменной {details[0]}. Ожидался {details[1]}, получен {details[2]}.";
                    break;
                case ErrorCode.WhenBranchesHaveDifferentTypes:
                    detailMessage = $" - Ветки when-выражения возвращают разные или несовместимые типы.";
                    if (details.Length > 0) detailMessage += $" Общий ожидаемый тип: {details[0]}, тип ветки: {details[1]}.";
                    break;
                case ErrorCode.OperationNotDefinedForType:
                    detailMessage = $" - Операция {details[0]} не определена для типа/типов {details[1]}.";
                    break;
                case ErrorCode.CannotInferType:
                    detailMessage = $" - Невозможно вывести тип для переменной '{details[0]}'. Укажите тип явно или инициализируйте.";
                    break;
                default:
                    string tokenInfo = tokenIndex < Tokens.Count ?
                    $"'{GetToken(tokenIndex).Value}' (тип: {GetToken(tokenIndex).Type})" : "конец ввода";
                    detailMessage = $" - Неожиданный {tokenInfo}";
                    break;
            }
            errors.Add((type, baseMessage + detailMessage));
        }


        public ErrorCode Parse()
        {
            try
            {
                return prog();
            }
            catch (IndexOutOfRangeException)
            {
                var frame = new StackTrace(new Exception(), true).GetFrame(0);
                string method = frame.GetMethod().Name;
                errors.Add((ErrorType.Syntactic,
                    $"Неожиданный конец ввода в '{method}'"));
                return ErrorCode.UnexpectedToken;
            }
        }

        private Token GetToken(int idx)
        {
            if (idx < Tokens.Count)
                return Tokens[idx];

            throw new IndexOutOfRangeException("Достигнут конец потока токенов");
        }

        private string DetermineLiteralType(Token litToken)
        {
            if (litToken.Value.StartsWith("'") && litToken.Value.EndsWith("'"))
            {
                return "СИМВОЛЬНЫЙ";
            }
            if (int.TryParse(litToken.Value, out _))
            {
                return "ЦЕЛЫЙ";
            }
            return "НЕИЗВЕСТНЫЙ";
        }

        private string GetResultType(RussBlock block, int tokenIndexOfUsage)
        {
            if (block is LiteralBlock lb)
            {
                return lb.LiteralType;
            }
            if (block is IdentifierUsageBlock iub)
            {
                return iub.ValueType;
            }
            if (block is ExpressionBlock eb)
            {
                return eb.ResultType;      
            }
            if (block is WhenExpressionBlock web)
            {
                return web.ResultType;      
            }
            Error(ErrorCode.InvalidExpression, tokenIndexOfUsage, "Не удалось определить тип операнда");
            return "НЕИЗВЕСТНЫЙ_ТИП";
        }


        private ErrorCode IsValidLiteralFormatAndRange(Token literalToken)
        {
            string valueStr = literalToken.Value;

            if (valueStr.Length >= 2 && valueStr[0] == '\'' && valueStr[valueStr.Length - 1] == '\'')
            {
                if (valueStr.Length == 3)  
                {
                    return ErrorCode.ExpectedCloseBrace;
                }
                else if (valueStr.Length == 4 && valueStr[1] == '\\')  
                {
                    return ErrorCode.ExpectedCloseBrace;
                }
                else
                {
                    Error(ErrorCode.InvalidLiteral, index, valueStr);
                    return ErrorCode.InvalidLiteral;
                }
            }
            else if (int.TryParse(valueStr, out int value))
            {
                if (value < 0 || value > 65535)
                {
                    Error(ErrorCode.LiteralOutOfRange, index, valueStr);
                    return ErrorCode.LiteralOutOfRange;
                }
                else
                {
                    return ErrorCode.ExpectedCloseBrace;
                }
            }
            else
            {
                Error(ErrorCode.InvalidLiteral, index, valueStr);
                return ErrorCode.InvalidLiteral;
            }
        }

        public ErrorCode prog()
        {
            index = 0;
            currentDepth = 0;

            if (!(GetToken(index).Type == 'T' && GetToken(index).Number == 0)) { Error(ErrorCode.ExpectedFun, index); return ErrorCode.ExpectedFun; }
            index++;
            if (!(GetToken(index).Type == 'T' && GetToken(index).Number == 1)) { Error(ErrorCode.ExpectedMain, index); return ErrorCode.ExpectedMain; }
            string mainFuncName = GetToken(index).Value;
            AddToMatrix(new ProgStartBlock(mainFuncName));
            index++;
            if (!(GetToken(index).Type == 'S' && GetToken(index).Number == 2)) { Error(ErrorCode.ExpectedOpenParenthesis, index); return ErrorCode.ExpectedOpenParenthesis; }
            index++;
            if (!(GetToken(index).Type == 'S' && GetToken(index).Number == 3)) { Error(ErrorCode.ExpectedCloseParenthesis, index); return ErrorCode.ExpectedCloseParenthesis; }
            index++;
            if (!(GetToken(index).Type == 'S' && GetToken(index).Number == 0)) { Error(ErrorCode.ExpectedOpenBrace, index); return ErrorCode.ExpectedOpenBrace; }
            index++;

            EnterScope();

            ErrorCode code = spis_oper();
            if (code != ErrorCode.ExpectedCloseBrace)
            {
            }

            if (index < Tokens.Count && !(GetToken(index).Type == 'S' && GetToken(index).Number == 1)) { Error(ErrorCode.ExpectedCloseBrace, index); return ErrorCode.ExpectedCloseBrace; }
            else if (index >= Tokens.Count) { Error(ErrorCode.ExpectedCloseBrace, index > 0 ? index - 1 : 0); return ErrorCode.ExpectedCloseBrace; }


            ExitScope();
            index++;

            return ErrorCode.ExpectedCloseBrace;
        }


        private ErrorCode spis_oper()
        {
            if (index >= Tokens.Count || (GetToken(index).Type == 'S' && GetToken(index).Number == 1))
            {
                return ErrorCode.ExpectedCloseBrace;    
            }

            ErrorCode code = oper();
            if (code != ErrorCode.ExpectedCloseBrace) return code;

            return bolshe_oper();
        }

        private ErrorCode oper()
        {
            if (index >= Tokens.Count) { Error(ErrorCode.UnexpectedToken, index); return ErrorCode.UnexpectedToken; }

            Token currentFirstToken = GetToken(index);

            if (currentFirstToken.Type == 'T' && (currentFirstToken.Number == 2 || currentFirstToken.Number == 3))    
            {
                currentVarOrValKeyword = currentFirstToken.Value;
                index++;
                if (index >= Tokens.Count || GetToken(index).Type != 'I') { Error(ErrorCode.ExpectedIdentifier, index); return ErrorCode.ExpectedIdentifier; }

                currentProcessingVarId = GetToken(index).Value;
                index++;
                return opis_id();
            }
            else if (currentFirstToken.Type == 'I')
            {
                currentProcessingVarId = currentFirstToken.Value;
                int idTokenIndex = index;     

                VariableInfo varInfo = GetVariableInfo(currentProcessingVarId, idTokenIndex);
                if (varInfo == null) return ErrorCode.UndeclaredIdentifier;    

                index++;
                return deistv_id();
            }
            else if (currentFirstToken.Type == 'T' && currentFirstToken.Number == 8)   
            {
                index++;
                if (index >= Tokens.Count || !(GetToken(index).Type == 'S' && GetToken(index).Number == 2)) { Error(ErrorCode.ExpectedOpenParenthesis, index); return ErrorCode.ExpectedOpenParenthesis; }
                index++;

                int exprStartIndex = index;
                ErrorCode code = vyr();
                if (code != ErrorCode.ExpectedCloseBrace) return code;
                RussBlock whenInputExpr = operandStack.Pop();
                string inputExprType = GetResultType(whenInputExpr, exprStartIndex);

                if (inputExprType != "ЦЕЛЫЙ" && inputExprType != "СИМВОЛЬНЫЙ" && inputExprType != "НЕИЗВЕСТНЫЙ_ТИП")
                {
                    Error(ErrorCode.IncompatibleTypesInOperation, exprStartIndex, $"when (входное выражение должно быть ЦЕЛЫЙ или СИМВОЛЬНЫЙ, получено {inputExprType})");
                }


                if (index >= Tokens.Count || !(GetToken(index).Type == 'S' && GetToken(index).Number == 3)) { Error(ErrorCode.ExpectedCloseParenthesis, index); return ErrorCode.ExpectedCloseParenthesis; }
                index++;
                if (index >= Tokens.Count || !(GetToken(index).Type == 'S' && GetToken(index).Number == 0)) { Error(ErrorCode.ExpectedOpenBrace, index); return ErrorCode.ExpectedOpenBrace; }
                index++;

                EnterScope();

                List<WhenBranchBlock> branches = new List<WhenBranchBlock>();
                code = when_vetvi_bez(branches, inputExprType);
                if (code != ErrorCode.ExpectedCloseBrace) { ExitScope(); return code; }

                if (index >= Tokens.Count || !(GetToken(index).Type == 'S' && GetToken(index).Number == 1)) { Error(ErrorCode.ExpectedCloseBrace, index); ExitScope(); return ErrorCode.ExpectedCloseBrace; }

                AddToMatrix(new WhenStatementBlock(whenInputExpr, branches));
                ExitScope();
                index++;
                return ErrorCode.ExpectedCloseBrace;
            }
            else
            {
                Error(ErrorCode.InvalidStatement, index); return ErrorCode.InvalidStatement;
            }
        }

        private ErrorCode bolshe_oper()
        {
            if (index >= Tokens.Count ||
                (GetToken(index).Type == 'S' && GetToken(index).Number == 1))  
            {
                return ErrorCode.ExpectedCloseBrace;
            }

            ErrorCode code = oper();
            if (code != ErrorCode.ExpectedCloseBrace) return code;

            return bolshe_oper();
        }

        private ErrorCode opis_id()
        {
            bool isConstant = currentVarOrValKeyword.Equals("val", StringComparison.OrdinalIgnoreCase);
            string declNature = isConstant ? "КОНСТАНТА" : "ПЕРЕМЕННАЯ";
            DeclarationBlock declBlock = null;
            string varName = currentProcessingVarId;      
            int idTokenIndex = index - 1;    

            string declaredType = null;
            RussBlock initialValueExpr = null;
            bool isInitialized = false;


            if (GetToken(index).Type == 'S' && GetToken(index).Number == 4)     
            {
                index++;  
                ErrorCode typeCode = tip();    
                if (typeCode != ErrorCode.ExpectedCloseBrace) return typeCode;
                declaredType = (GetToken(index - 1).Value.Equals("Int", StringComparison.OrdinalIgnoreCase)) ? "ЦЕЛЫЙ" : "СИМВОЛЬНЫЙ";

                if (index < Tokens.Count && GetToken(index).Type == 'S' && GetToken(index).Number == 12)  
                {
                    index++;  
                    int exprTokenIndex = index;
                    ErrorCode exprCode = vyr();
                    if (exprCode != ErrorCode.ExpectedCloseBrace) return exprCode;
                    initialValueExpr = operandStack.Pop();
                    isInitialized = true;

                    string exprType = GetResultType(initialValueExpr, exprTokenIndex);
                    if (exprType != "НЕИЗВЕСТНЫЙ_ТИП" && declaredType != exprType)
                    {
                        Error(ErrorCode.IncompatibleTypesInAssignment, exprTokenIndex, varName, declaredType, exprType);
                    }
                }
            }
            else if (GetToken(index).Type == 'S' && GetToken(index).Number == 12)        
            {
                index++;  
                isInitialized = true;
                int exprOrWhenTokenIndex = index;

                if (index < Tokens.Count && GetToken(index).Type == 'T' && GetToken(index).Number == 8)   
                {
                    index++;  
                    if (index >= Tokens.Count || !(GetToken(index).Type == 'S' && GetToken(index).Number == 2)) { Error(ErrorCode.ExpectedOpenParenthesis, index); return ErrorCode.ExpectedOpenParenthesis; }
                    index++;

                    int whenInputExprTokenIndex = index;
                    ErrorCode paramCode = vyr();
                    if (paramCode != ErrorCode.ExpectedCloseBrace) return paramCode;
                    RussBlock whenInputExpr = operandStack.Pop();
                    string inputExprType = GetResultType(whenInputExpr, whenInputExprTokenIndex);

                    if (inputExprType != "ЦЕЛЫЙ" && inputExprType != "СИМВОЛЬНЫЙ" && inputExprType != "НЕИЗВЕСТНЫЙ_ТИП")
                    {
                        Error(ErrorCode.IncompatibleTypesInOperation, whenInputExprTokenIndex, $"when (входное выражение должно быть ЦЕЛЫЙ или СИМВОЛЬНЫЙ, получено {inputExprType})");
                    }


                    if (index >= Tokens.Count || !(GetToken(index).Type == 'S' && GetToken(index).Number == 3)) { Error(ErrorCode.ExpectedCloseParenthesis, index); return ErrorCode.ExpectedCloseParenthesis; }
                    index++;
                    if (index >= Tokens.Count || !(GetToken(index).Type == 'S' && GetToken(index).Number == 0)) { Error(ErrorCode.ExpectedOpenBrace, index); return ErrorCode.ExpectedOpenBrace; }
                    index++;

                    List<WhenBranchBlock> branches = new List<WhenBranchBlock>();
                    ErrorCode branchesCode = when_vetvi(branches, inputExprType);
                    if (branchesCode != ErrorCode.ExpectedCloseBrace) return branchesCode;

                    WhenElseBranchBlock elseBranch = null;
                    ErrorCode elseCode = inache(ref elseBranch);        
                    if (elseCode != ErrorCode.ExpectedCloseBrace) return elseCode;

                    if (index >= Tokens.Count || !(GetToken(index).Type == 'S' && GetToken(index).Number == 1)) { Error(ErrorCode.ExpectedCloseBrace, index); return ErrorCode.ExpectedCloseBrace; }
                    index++;

                    string tempVarForWhen = "T_WHEN_" + (++tempCounter);
                    WhenExpressionBlock whenExprBlock = new WhenExpressionBlock(whenInputExpr, branches, tempVarForWhen);
                    if (elseBranch != null) whenExprBlock.ElseBranch = elseBranch;

                    string commonBranchType = null;
                    bool firstBranch = true;
                    int errorTokenIndexForWhenType = exprOrWhenTokenIndex;

                    foreach (var branch in branches)
                    {
                        string branchActionType = GetResultType(branch.Action, errorTokenIndexForWhenType);   
                        if (firstBranch) { commonBranchType = branchActionType; firstBranch = false; }
                        else if (commonBranchType != branchActionType && branchActionType != "НЕИЗВЕСТНЫЙ_ТИП")
                        {
                            Error(ErrorCode.WhenBranchesHaveDifferentTypes, errorTokenIndexForWhenType, commonBranchType, branchActionType);
                            commonBranchType = "НЕИЗВЕСТНЫЙ_ТИП";
                            break;
                        }
                    }
                    if (elseBranch != null)
                    {
                        string elseActionType = GetResultType(elseBranch.Action, errorTokenIndexForWhenType);   
                        if (firstBranch && commonBranchType == null) { commonBranchType = elseActionType; }
                        else if (commonBranchType != elseActionType && elseActionType != "НЕИЗВЕСТНЫЙ_ТИП" && commonBranchType != "НЕИЗВЕСТНЫЙ_ТИП")
                        {
                            Error(ErrorCode.WhenBranchesHaveDifferentTypes, errorTokenIndexForWhenType, commonBranchType, elseActionType);
                            commonBranchType = "НЕИЗВЕСТНЫЙ_ТИП";
                        }
                    }

                    if (commonBranchType == null || commonBranchType == "НЕИЗВЕСТНЫЙ_ТИП")
                    {
                        Error(ErrorCode.CannotInferType, errorTokenIndexForWhenType, "when expression (все ветви должны иметь согласуемый тип)");
                        commonBranchType = "НЕИЗВЕСТНЫЙ_ТИП";
                    }

                    whenExprBlock.ResultType = commonBranchType;
                    declaredType = commonBranchType;
                    initialValueExpr = new IdentifierUsageBlock(tempVarForWhen) { ValueType = commonBranchType };       
                    AddToMatrix(whenExprBlock);     
                }
                else   
                {
                    ErrorCode exprCode = vyr();
                    if (exprCode != ErrorCode.ExpectedCloseBrace) return exprCode;
                    initialValueExpr = operandStack.Pop();
                    declaredType = GetResultType(initialValueExpr, exprOrWhenTokenIndex);
                }
            }
            else                  
            {
                if (!isConstant)   
                {
                    Error(ErrorCode.CannotInferType, idTokenIndex, varName);
                    declaredType = "НЕИЗВЕСТНЫЙ_ТИП";     
                }
            }

            if (isConstant && !isInitialized)
            {
                Error(ErrorCode.ValMustBeInitialized, idTokenIndex, varName);
            }

            if (declaredType == null)         
            {
                declaredType = "НЕИЗВЕСТНЫЙ_ТИП";
            }


            ErrorCode declResult = DeclareIdentifier(varName, declaredType, isConstant, isInitialized, idTokenIndex, currentVarOrValKeyword);
            if (declResult != ErrorCode.ExpectedCloseBrace) return declResult;

            declBlock = new DeclarationBlock(declNature, varName, declaredType);
            if (initialValueExpr != null) declBlock.InitialValue = initialValueExpr;
            AddToMatrix(declBlock);

            if (index < Tokens.Count && GetToken(index).Type == 'S' && GetToken(index).Number == 5)  
            {
                index++;
            }
            else if (!(initialValueExpr != null && index < Tokens.Count && GetToken(index - 1).Type == 'S' && GetToken(index - 1).Number == 1))      
            {
                if (!(initialValueExpr is IdentifierUsageBlock && ((IdentifierUsageBlock)initialValueExpr).Name.StartsWith("T_WHEN_")))
                {
                    if (index >= Tokens.Count || !(GetToken(index).Type == 'S' && GetToken(index).Number == 5)) { Error(ErrorCode.ExpectedSemicolon, index); return ErrorCode.ExpectedSemicolon; }
                    index++;
                }
            }


            currentProcessingVarId = null;
            currentVarOrValKeyword = null;
            return ErrorCode.ExpectedCloseBrace;
        }


        private ErrorCode tip()
        {
            if (index < Tokens.Count && GetToken(index).Type == 'T' && (GetToken(index).Number == 4 || GetToken(index).Number == 5))    
            {
                index++;
                return ErrorCode.ExpectedCloseBrace;
            }
            else
            {
                Error(ErrorCode.ExpectedIntOrChar, index); return ErrorCode.ExpectedIntOrChar;
            }
        }

        private ErrorCode vyr()
        {
            ErrorCode code = slag();
            if (code != ErrorCode.ExpectedCloseBrace) return code;

            code = bolshe_slag();
            if (code != ErrorCode.ExpectedCloseBrace) return code;

            while (operatorStack.Count > 0 && operatorStack.Peek().Value != "(")
            {
                Token opToken = operatorStack.Pop();
                int opTokenIndex = Tokens.Contains(opToken) ? Tokens.IndexOf(opToken) : index;     

                RussBlock op2Block = operandStack.Pop();
                RussBlock op1Block = operandStack.Pop();

                string type1 = GetResultType(op1Block, opTokenIndex);
                string type2 = GetResultType(op2Block, opTokenIndex);
                string resultType = "НЕИЗВЕСТНЫЙ_ТИП";

                if (type1 == "ЦЕЛЫЙ" && type2 == "ЦЕЛЫЙ")
                {
                    resultType = "ЦЕЛЫЙ";
                }
                else
                {
                    if (type1 != "НЕИЗВЕСТНЫЙ_ТИП" && type2 != "НЕИЗВЕСТНЫЙ_ТИП")           
                    {
                        Error(ErrorCode.IncompatibleTypesInOperation, opTokenIndex, $"{opToken.Value} ({type1} vs {type2})");
                    }
                    Error(ErrorCode.OperationNotDefinedForType, opTokenIndex, opToken.Value, $"{type1} и {type2}");
                }


                tempCounter++;
                string resultTempVar = "T" + tempCounter;
                ExpressionBlock expr = new ExpressionBlock(opToken.Value, op1Block, op2Block, resultTempVar);
                expr.ResultType = resultType;
                AddToMatrix(expr);
                operandStack.Push(new IdentifierUsageBlock(resultTempVar) { ValueType = resultType });
            }
            return ErrorCode.ExpectedCloseBrace;
        }


        private ErrorCode slag()
        {
            ErrorCode code = fakt();
            if (code != ErrorCode.ExpectedCloseBrace) return code;

            return bolshe_fakt();
        }

        private ErrorCode fakt()
        {
            if (index >= Tokens.Count) { Error(ErrorCode.UnexpectedToken, index); return ErrorCode.UnexpectedToken; }
            Token currentToken = GetToken(index);
            int currentTokenIndex = index;

            if (currentToken.Type == 'I')
            {
                VariableInfo varInfo = GetVariableInfo(currentToken.Value, currentTokenIndex);
                if (varInfo == null) return ErrorCode.UndeclaredIdentifier;    

                if (!varInfo.IsInitialized)
                {
                    Error(ErrorCode.VariableNotInitialized, currentTokenIndex, currentToken.Value);
                }

                IdentifierUsageBlock iub = new IdentifierUsageBlock(currentToken.Value);
                iub.ValueType = varInfo.Type;
                operandStack.Push(iub);
                index++;
                return ErrorCode.ExpectedCloseBrace;
            }
            else if (currentToken.Type == 'L')
            {
                ErrorCode semCode = IsValidLiteralFormatAndRange(currentToken);
                if (semCode != ErrorCode.ExpectedCloseBrace) return semCode;

                string litType = DetermineLiteralType(currentToken);
                LiteralBlock lb = new LiteralBlock(currentToken.Value, litType);
                operandStack.Push(lb);
                index++;
                return ErrorCode.ExpectedCloseBrace;
            }
            else if (currentToken.Type == 'S' && currentToken.Number == 2)  
            {
                operatorStack.Push(currentToken);
                index++;
                ErrorCode code = vyr();
                if (code != ErrorCode.ExpectedCloseBrace) return code;

                if (index >= Tokens.Count || !(GetToken(index).Type == 'S' && GetToken(index).Number == 3))  
                { Error(ErrorCode.ExpectedCloseParenthesis, index); return ErrorCode.ExpectedCloseParenthesis; }

                if (operatorStack.Count == 0 || operatorStack.Peek().Value != "(")
                {
                    Error(ErrorCode.InvalidExpression, index, "Дисбаланс скобок при обработке выражения");
                    return ErrorCode.InvalidExpression;
                }
                operatorStack.Pop();  
                index++;
                return ErrorCode.ExpectedCloseBrace;
            }
            else
            {
                Error(ErrorCode.InvalidExpression, index, $"Неожиданный токен {currentToken.Value} в начале фактора"); return ErrorCode.InvalidExpression;
            }
        }


        private ErrorCode bolshe_fakt()    
        {
            if (index >= Tokens.Count) return ErrorCode.ExpectedCloseBrace;
            Token currentToken = GetToken(index);

            if (currentToken.Type == 'S' && (currentToken.Number == 9 || currentToken.Number == 10 || currentToken.Number == 11))    
            {
                int currentOpPriority = GetOperatorPriority(currentToken);
                while (operatorStack.Count > 0 && GetOperatorPriority(operatorStack.Peek()) >= currentOpPriority && operatorStack.Peek().Value != "(")
                {
                    Token op = operatorStack.Pop();
                    int opTokenIndex = Tokens.Contains(op) ? Tokens.IndexOf(op) : index;

                    RussBlock op2Block = operandStack.Pop();
                    RussBlock op1Block = operandStack.Pop();

                    string type1 = GetResultType(op1Block, opTokenIndex);
                    string type2 = GetResultType(op2Block, opTokenIndex);
                    string resultType = "НЕИЗВЕСТНЫЙ_ТИП";

                    if (type1 == "ЦЕЛЫЙ" && type2 == "ЦЕЛЫЙ")
                    {
                        resultType = "ЦЕЛЫЙ";
                    }
                    else
                    {
                        if (type1 != "НЕИЗВЕСТНЫЙ_ТИП" && type2 != "НЕИЗВЕСТНЫЙ_ТИП")
                        {
                            Error(ErrorCode.IncompatibleTypesInOperation, opTokenIndex, $"{op.Value} ({type1} vs {type2})");
                        }
                        Error(ErrorCode.OperationNotDefinedForType, opTokenIndex, op.Value, $"{type1} и {type2}");
                    }

                    tempCounter++;
                    string resultTempVar = "T" + tempCounter;
                    ExpressionBlock expr = new ExpressionBlock(op.Value, op1Block, op2Block, resultTempVar);
                    expr.ResultType = resultType;
                    AddToMatrix(expr);
                    operandStack.Push(new IdentifierUsageBlock(resultTempVar) { ValueType = resultType });
                }
                operatorStack.Push(currentToken);
                index++;
                ErrorCode code = fakt();
                if (code != ErrorCode.ExpectedCloseBrace) return code;
                return bolshe_fakt();
            }
            return ErrorCode.ExpectedCloseBrace;
        }

        private ErrorCode bolshe_slag()   
        {
            if (index >= Tokens.Count) return ErrorCode.ExpectedCloseBrace;
            Token currentToken = GetToken(index);

            if (currentToken.Type == 'S' && (currentToken.Number == 7 || currentToken.Number == 8))   
            {
                int currentOpPriority = GetOperatorPriority(currentToken);
                while (operatorStack.Count > 0 && GetOperatorPriority(operatorStack.Peek()) >= currentOpPriority && operatorStack.Peek().Value != "(")
                {
                    Token op = operatorStack.Pop();
                    int opTokenIndex = Tokens.Contains(op) ? Tokens.IndexOf(op) : index;

                    RussBlock op2Block = operandStack.Pop();
                    RussBlock op1Block = operandStack.Pop();

                    string type1 = GetResultType(op1Block, opTokenIndex);
                    string type2 = GetResultType(op2Block, opTokenIndex);
                    string resultType = "НЕИЗВЕСТНЫЙ_ТИП";

                    if (type1 == "ЦЕЛЫЙ" && type2 == "ЦЕЛЫЙ")
                    {
                        resultType = "ЦЕЛЫЙ";
                    }
                    else
                    {
                        if (type1 != "НЕИЗВЕСТНЫЙ_ТИП" && type2 != "НЕИЗВЕСТНЫЙ_ТИП")
                        {
                            Error(ErrorCode.IncompatibleTypesInOperation, opTokenIndex, $"{op.Value} ({type1} vs {type2})");
                        }
                        Error(ErrorCode.OperationNotDefinedForType, opTokenIndex, op.Value, $"{type1} и {type2}");
                    }

                    tempCounter++;
                    string resultTempVar = "T" + tempCounter;
                    ExpressionBlock expr = new ExpressionBlock(op.Value, op1Block, op2Block, resultTempVar);
                    expr.ResultType = resultType;
                    AddToMatrix(expr);
                    operandStack.Push(new IdentifierUsageBlock(resultTempVar) { ValueType = resultType });
                }
                operatorStack.Push(currentToken);
                index++;
                ErrorCode code = slag();
                if (code != ErrorCode.ExpectedCloseBrace) return code;
                return bolshe_slag();
            }
            return ErrorCode.ExpectedCloseBrace;
        }

        private ErrorCode when_vetvi(List<WhenBranchBlock> branches, string inputExprType)
        {
            ErrorCode code = when_vetv(branches, inputExprType);
            if (code != ErrorCode.ExpectedCloseBrace) return code;
            return bolshe_vetv(branches, inputExprType);
        }

        private ErrorCode bolshe_vetv(List<WhenBranchBlock> branches, string inputExprType)
        {
            if (index >= Tokens.Count ||
                (GetToken(index).Type == 'S' && GetToken(index).Number == 1) ||  
                (GetToken(index).Type == 'T' && GetToken(index).Number == 7))    
            {
                return ErrorCode.ExpectedCloseBrace;
            }
            ErrorCode code = when_vetv(branches, inputExprType);
            if (code != ErrorCode.ExpectedCloseBrace) return code;
            return bolshe_vetv(branches, inputExprType);
        }

        private ErrorCode when_vetv(List<WhenBranchBlock> branches, string inputExprType)
        {
            List<ValueConditionBlock> conditions = new List<ValueConditionBlock>();
            int conditionsStartIndex = index;
            ErrorCode code = spis_znach(conditions, inputExprType);      
            if (code != ErrorCode.ExpectedCloseBrace) return code;

            if (index >= Tokens.Count || !(GetToken(index).Type == 'S' && GetToken(index).Number == 19)) { Error(ErrorCode.ExpectedArrow, index); return ErrorCode.ExpectedArrow; }
            int arrowTokenIndex = index;
            index++;

            code = fakt();           
            if (code != ErrorCode.ExpectedCloseBrace) return code;
            RussBlock action = operandStack.Pop();
            if (index >= Tokens.Count || !(GetToken(index).Type == 'S' && GetToken(index).Number == 5)) { Error(ErrorCode.ExpectedSemicolon, index); return ErrorCode.ExpectedSemicolon; }
            index++;

            branches.Add(new WhenBranchBlock(conditions, action));
            return ErrorCode.ExpectedCloseBrace;
        }


        private ErrorCode spis_znach(List<ValueConditionBlock> conditions, string expectedValueType)
        {
            ErrorCode code = znach(conditions, expectedValueType);
            if (code != ErrorCode.ExpectedCloseBrace) return code;
            return bolshe_znach(conditions, expectedValueType);
        }

        private ErrorCode bolshe_znach(List<ValueConditionBlock> conditions, string expectedValueType)
        {
            if (index >= Tokens.Count || (GetToken(index).Type == 'S' && GetToken(index).Number == 19))  
            {
                return ErrorCode.ExpectedCloseBrace;
            }
            if (!(GetToken(index).Type == 'S' && GetToken(index).Number == 6)) { Error(ErrorCode.ExpectedComma, index); return ErrorCode.ExpectedComma; }  
            index++;
            ErrorCode code = znach(conditions, expectedValueType);
            if (code != ErrorCode.ExpectedCloseBrace) return code;
            return bolshe_znach(conditions, expectedValueType);
        }

        private ErrorCode znach(List<ValueConditionBlock> conditions, string expectedValueType)
        {
            if (index >= Tokens.Count) { Error(ErrorCode.UnexpectedToken, index); return ErrorCode.UnexpectedToken; }
            int valueTokenIndex = index;

            if (GetToken(index).Type == 'L')  
            {
                Token litToken = GetToken(index);
                IsValidLiteralFormatAndRange(litToken);      
                string litType = DetermineLiteralType(litToken);
                if (expectedValueType != "НЕИЗВЕСТНЫЙ_ТИП" && litType != expectedValueType)
                {
                    Error(ErrorCode.IncompatibleTypesInOperation, valueTokenIndex, $"when branch condition (ожидался тип {expectedValueType}, получен {litType} для литерала {litToken.Value})");
                }
                conditions.Add(new ValueConditionBlock(new LiteralBlock(litToken.Value, litType)));
                index++;
                return ErrorCode.ExpectedCloseBrace;
            }
            else if (GetToken(index).Type == 'T' && GetToken(index).Number == 6)    
            {
                index++;  
                if (index >= Tokens.Count || GetToken(index).Type != 'L') { Error(ErrorCode.ExpectedLiteral, index); return ErrorCode.ExpectedLiteral; }
                Token lit1Token = GetToken(index);
                int lit1TokenIndex = index;
                IsValidLiteralFormatAndRange(lit1Token);
                string lit1Type = DetermineLiteralType(lit1Token);
                if (expectedValueType != "НЕИЗВЕСТНЫЙ_ТИП" && lit1Type != expectedValueType)
                {
                    Error(ErrorCode.IncompatibleTypesInOperation, lit1TokenIndex, $"when branch range condition (ожидался тип {expectedValueType}, получен {lit1Type} для литерала {lit1Token.Value})");
                }
                LiteralBlock lit1Block = new LiteralBlock(lit1Token.Value, lit1Type);
                index++;

                if (index >= Tokens.Count || !(GetToken(index).Type == 'S' && GetToken(index).Number == 20)) { Error(ErrorCode.ExpectedDoubleDot, index); return ErrorCode.ExpectedDoubleDot; }  
                index++;

                if (index >= Tokens.Count || GetToken(index).Type != 'L') { Error(ErrorCode.ExpectedLiteral, index); return ErrorCode.ExpectedLiteral; }
                Token lit2Token = GetToken(index);
                int lit2TokenIndex = index;
                IsValidLiteralFormatAndRange(lit2Token);
                string lit2Type = DetermineLiteralType(lit2Token);
                if (expectedValueType != "НЕИЗВЕСТНЫЙ_ТИП" && lit2Type != expectedValueType)
                {
                    Error(ErrorCode.IncompatibleTypesInOperation, lit2TokenIndex, $"when branch range condition (ожидался тип {expectedValueType}, получен {lit2Type} для литерала {lit2Token.Value})");
                }
                LiteralBlock lit2Block = new LiteralBlock(lit2Token.Value, lit2Type);
                index++;

                conditions.Add(new ValueConditionBlock(lit1Block, lit2Block));
                return ErrorCode.ExpectedCloseBrace;
            }
            else
            {
                Error(ErrorCode.InvalidExpression, index, "Ожидался литерал или 'in' в условии when"); return ErrorCode.InvalidExpression;
            }
        }

        private ErrorCode inache(ref WhenElseBranchBlock elseBranch)         
        {
            if (index >= Tokens.Count || (GetToken(index).Type == 'S' && GetToken(index).Number == 1))  
            {
                elseBranch = null;
                return ErrorCode.ExpectedCloseBrace;
            }

            if (!(GetToken(index).Type == 'T' && GetToken(index).Number == 7)) { Error(ErrorCode.ExpectedElse, index); return ErrorCode.ExpectedElse; }  
            int elseTokenIndex = index;
            index++;
            if (index >= Tokens.Count || !(GetToken(index).Type == 'S' && GetToken(index).Number == 19)) { Error(ErrorCode.ExpectedArrow, index); return ErrorCode.ExpectedArrow; }  
            index++;

            ErrorCode code = fakt();         
            if (code != ErrorCode.ExpectedCloseBrace) return code;
            RussBlock action = operandStack.Pop();
            if (index >= Tokens.Count || !(GetToken(index).Type == 'S' && GetToken(index).Number == 5)) { Error(ErrorCode.ExpectedSemicolon, index); return ErrorCode.ExpectedSemicolon; }  
            index++;

            elseBranch = new WhenElseBranchBlock(action);
            return ErrorCode.ExpectedCloseBrace;
        }


        private ErrorCode deistv_id()              
        {
            if (index >= Tokens.Count) { Error(ErrorCode.UnexpectedToken, index); return ErrorCode.UnexpectedToken; }

            string targetVarName = currentProcessingVarId;       
            int assignmentOpTokenIndex = index - 1;      

            VariableInfo varInfo = GetVariableInfo(targetVarName, assignmentOpTokenIndex);
            if (varInfo == null) return ErrorCode.UndeclaredIdentifier;    

            if (GetToken(index).Type == 'S' && GetToken(index).Number == 12)  
            {
                if (varInfo.IsConstant)
                {
                    Error(ErrorCode.AssignmentToConstant, assignmentOpTokenIndex, targetVarName);
                }
                index++;
                int valueExprTokenIndex = index;
                ErrorCode code = vyr();
                if (code != ErrorCode.ExpectedCloseBrace) return code;

                RussBlock valueExpr = operandStack.Pop();
                string valueType = GetResultType(valueExpr, valueExprTokenIndex);

                if (varInfo.Type != "НЕИЗВЕСТНЫЙ_ТИП" && valueType != "НЕИЗВЕСТНЫЙ_ТИП" && varInfo.Type != valueType)
                {
                    Error(ErrorCode.IncompatibleTypesInAssignment, valueExprTokenIndex, targetVarName, varInfo.Type, valueType);
                }

                SetIdentifierInitialized(targetVarName, assignmentOpTokenIndex);
                AddToMatrix(new AssignmentBlock(targetVarName, valueExpr));
            }
            else if (GetToken(index).Type == 'S' &&
                    (GetToken(index).Number >= 14 && GetToken(index).Number <= 18))      
            {
                if (varInfo.IsConstant)
                {
                    Error(ErrorCode.AssignmentToConstant, assignmentOpTokenIndex, targetVarName);
                }
                if (!varInfo.IsInitialized)
                {
                    Error(ErrorCode.VariableNotInitialized, assignmentOpTokenIndex, targetVarName + " (в составном присваивании)");
                }


                string compoundOperator = GetToken(index).Value;
                int compoundOpTokenIndex = index;
                index++;

                int valueExprTokenIndex = index;
                ErrorCode code = vyr();
                if (code != ErrorCode.ExpectedCloseBrace) return code;

                RussBlock valueExpr = operandStack.Pop();
                string valueType = GetResultType(valueExpr, valueExprTokenIndex);

                if (varInfo.Type != "ЦЕЛЫЙ")
                {
                    Error(ErrorCode.OperationNotDefinedForType, compoundOpTokenIndex, compoundOperator, $"{varInfo.Type} (левый операнд)");
                }
                if (valueType != "ЦЕЛЫЙ")
                {
                    Error(ErrorCode.OperationNotDefinedForType, compoundOpTokenIndex, compoundOperator, $"{valueType} (правый операнд)");
                }

                SetIdentifierInitialized(targetVarName, assignmentOpTokenIndex);        
                AddToMatrix(new CompoundAssignmentBlock(targetVarName, compoundOperator, valueExpr));
            }
            else
            {
                Error(ErrorCode.InvalidOperator, index, $"Ожидался оператор присваивания (=, +=, ...) после идентификатора {targetVarName}"); return ErrorCode.InvalidOperator;
            }

            if (index >= Tokens.Count || !(GetToken(index).Type == 'S' && GetToken(index).Number == 5)) { Error(ErrorCode.ExpectedSemicolon, index); return ErrorCode.ExpectedSemicolon; }  
            index++;

            currentProcessingVarId = null;      
            return ErrorCode.ExpectedCloseBrace;
        }

        private ErrorCode when_vetvi_bez(List<WhenBranchBlock> branches, string inputExprType)
        {
            ErrorCode code = when_vetv_bez(branches, inputExprType);
            if (code != ErrorCode.ExpectedCloseBrace) return code;
            return bolshe_vetv_bez(branches, inputExprType);
        }

        private ErrorCode bolshe_vetv_bez(List<WhenBranchBlock> branches, string inputExprType)
        {
            if (index >= Tokens.Count || (GetToken(index).Type == 'S' && GetToken(index).Number == 1))  
            {
                return ErrorCode.ExpectedCloseBrace;
            }
            ErrorCode code = when_vetv_bez(branches, inputExprType);
            if (code != ErrorCode.ExpectedCloseBrace) return code;
            return bolshe_vetv_bez(branches, inputExprType);
        }

        private ErrorCode when_vetv_bez(List<WhenBranchBlock> branches, string inputExprType)
        {
            List<ValueConditionBlock> conditions = new List<ValueConditionBlock>();
            ErrorCode code = spis_znach(conditions, inputExprType);      
            if (code != ErrorCode.ExpectedCloseBrace) return code;

            if (index >= Tokens.Count || !(GetToken(index).Type == 'S' && GetToken(index).Number == 19)) { Error(ErrorCode.ExpectedArrow, index); return ErrorCode.ExpectedArrow; }  
            index++;

            int startIndexForBlockCapture = russianCodeMatrix.Count;

            int originalDepthForBlock = currentDepth;
            List<RussBlock> blockStatements = new List<RussBlock>();

            code = blok();          
            if (code != ErrorCode.ExpectedCloseBrace)
            {
                return code;
            }

            for (int i = startIndexForBlockCapture; i < russianCodeMatrix.Count; ++i)
            {
                russianCodeMatrix[i].Depth -= (originalDepthForBlock + 1);      
                if (russianCodeMatrix[i].Depth < 0) russianCodeMatrix[i].Depth = 0;
                blockStatements.Add(russianCodeMatrix[i]);
            }
            if (blockStatements.Any())
            {
                russianCodeMatrix.RemoveRange(startIndexForBlockCapture, blockStatements.Count);
            }

            branches.Add(new WhenBranchBlock(conditions, new ScopeBlock(blockStatements)));
            return ErrorCode.ExpectedCloseBrace;
        }

        private ErrorCode blok()         
        {
            if (index >= Tokens.Count) { Error(ErrorCode.UnexpectedToken, index); return ErrorCode.UnexpectedToken; }

            if (GetToken(index).Type == 'S' && GetToken(index).Number == 0)  
            {
                index++;
                EnterScope();   

                ErrorCode code = spis_oper();     
                if (index < Tokens.Count && GetToken(index).Type == 'S' && GetToken(index).Number == 1)
                {
                }
                else if (code != ErrorCode.ExpectedCloseBrace)
                {
                    ExitScope();     
                    return code;
                }


                if (index >= Tokens.Count || !(GetToken(index).Type == 'S' && GetToken(index).Number == 1))  
                {
                    Error(ErrorCode.ExpectedCloseBrace, index);
                    return ErrorCode.ExpectedCloseBrace;
                }

                ExitScope();     
                index++;
                return ErrorCode.ExpectedCloseBrace;
            }
            else   
            {
                return oper();
            }
        }
    }
}