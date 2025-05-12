using System.Collections.Generic;
using System.Text;      

namespace task
{
    public abstract class RussBlock
    {
        public string BlockType { get; protected set; }
        public int Depth { get; set; }

        protected RussBlock(string blockType)
        {
            BlockType = blockType;
        }

        public abstract string ToRussianString();

        protected string Indent() => new string(' ', Depth * 2);

        protected string OperandToString(RussBlock operand)
        {
            if (operand is IdentifierUsageBlock iub)
            {
                return $"ИДЕНТИФИКАТОР: {iub.Name} (Тип: {iub.ValueType ?? "неизвестен"})";
            }
            if (operand is LiteralBlock lb)
            {
                return $"ЛИТЕРАЛ: {lb.Value} ({lb.LiteralType})";
            }
            if (operand is ExpressionBlock eb)
            {
                return $"РЕЗУЛЬТАТ_ВЫРАЖЕНИЯ: {eb.ResultTempVar} (Тип: {eb.ResultType ?? "неизвестен"})";
            }
            if (operand is WhenExpressionBlock web)
            {
                return $"РЕЗУЛЬТАТ_WHEN_ВЫРАЖЕНИЯ: {web.ResultTempVar} (Тип: {web.ResultType ?? "неизвестен"})";
            }
            return operand?.ToRussianString() ?? "НЕОПРЕДЕЛЕННЫЙ_ОПЕРАНД";
        }
    }

    public class ProgStartBlock : RussBlock
    {
        public string FunctionName { get; }
        public ProgStartBlock(string funcName) : base("НАЧАЛО_ПРОГРАММЫ") { FunctionName = funcName; }
        public override string ToRussianString() => $"{Indent()}НАЧАЛО_ПРОГРАММЫ: {FunctionName}()";
    }

    public class ProgEndBlock : RussBlock
    {
        public string FunctionName { get; }
        public ProgEndBlock(string funcName) : base("КОНЕЦ_ПРОГРАММЫ") { FunctionName = funcName; }
        public override string ToRussianString() => $"{Indent()}КОНЕЦ_ПРОГРАММЫ: {FunctionName}()";
    }

    public class DeclarationBlock : RussBlock
    {
        public string DeclarationNature { get; }
        public string Name { get; }
        public string VarType { get; }        
        public RussBlock InitialValue { get; set; }

        public DeclarationBlock(string declNature, string name, string varType) : base($"ОБЪЯВЛЕНИЕ_{declNature.ToUpper()}")
        {
            DeclarationNature = declNature;
            Name = name;
            VarType = varType;
        }
        public override string ToRussianString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{Indent()}{BlockType}: {Name}");
            sb.AppendLine($"{Indent()}  ТИП: {VarType}");       
            if (InitialValue != null)
            {
                InitialValue.Depth = this.Depth + 1;      
                sb.AppendLine($"{Indent()}  НАЧАЛЬНОЕ_ЗНАЧЕНИЕ:");
                sb.Append(InitialValue.ToRussianString());
            }
            return sb.ToString().TrimEnd();
        }
    }

    public class AssignmentBlock : RussBlock
    {
        public string TargetVariable { get; }
        public RussBlock ValueExpression { get; }
        public AssignmentBlock(string target, RussBlock value) : base("ПРИСВАИВАНИЕ")
        {
            TargetVariable = target;
            ValueExpression = value;
        }
        public override string ToRussianString()
        {
            ValueExpression.Depth = this.Depth + 1;
            return $"{Indent()}ПРИСВАИВАНИЕ: {TargetVariable}\n{Indent()}  ЗНАЧЕНИЕ:\n{ValueExpression.ToRussianString()}";
        }
    }

    public class CompoundAssignmentBlock : RussBlock
    {
        public string TargetVariable { get; }
        public string Operator { get; }
        public RussBlock ValueExpression { get; }
        public CompoundAssignmentBlock(string target, string op, RussBlock value) : base("СОСТАВНОЕ_ПРИСВАИВАНИЕ")
        {
            TargetVariable = target;
            Operator = op;
            ValueExpression = value;
        }
        public override string ToRussianString()
        {
            ValueExpression.Depth = this.Depth + 1;
            return $"{Indent()}СОСТАВНОЕ_ПРИСВАИВАНИЕ: {TargetVariable} {Operator}\n{Indent()}  ЗНАЧЕНИЕ:\n{ValueExpression.ToRussianString()}";
        }
    }

    public class ExpressionBlock : RussBlock
    {
        public string Operation { get; }
        public RussBlock Operand1 { get; }
        public RussBlock Operand2 { get; }
        public string ResultTempVar { get; }
        public string ResultType { get; set; }      

        public ExpressionBlock(string op, RussBlock op1, RussBlock op2, string tempVar) : base("ВЫРАЖЕНИЕ_ОПЕРАЦИЯ")
        {
            Operation = op;
            Operand1 = op1;
            Operand2 = op2;
            ResultTempVar = tempVar;
            ResultType = "НЕИЗВЕСТНЫЙ_ТИП";    
        }
        public override string ToRussianString()
        {
            Operand1.Depth = this.Depth + 1;
            Operand2.Depth = this.Depth + 1;
            var sb = new StringBuilder();
            sb.AppendLine($"{Indent()}ОПЕРАЦИЯ: '{Operation}' (Промежуточный результат в: {ResultTempVar}, Тип результата: {ResultType})");
            sb.AppendLine($"{Indent()}  ОПЕРАНД1:");
            sb.AppendLine(Operand1.ToRussianString());
            sb.AppendLine($"{Indent()}  ОПЕРАНД2:");
            sb.Append(Operand2.ToRussianString());
            return sb.ToString().TrimEnd();
        }
    }

    public class IdentifierUsageBlock : RussBlock
    {
        public string Name { get; }
        public string ValueType { get; set; }      

        public IdentifierUsageBlock(string name) : base("ИСПОЛЬЗОВАНИЕ_ИДЕНТИФИКАТОРА")
        {
            Name = name;
            ValueType = "НЕИЗВЕСТНЫЙ_ТИП";    
        }
        public override string ToRussianString() => $"{Indent()}ИДЕНТИФИКАТОР: {Name} (Тип: {ValueType})";
    }

    public class LiteralBlock : RussBlock
    {
        public string Value { get; }
        public string LiteralType { get; }          
        public LiteralBlock(string val, string type) : base($"ЛИТЕРАЛ")
        {
            Value = val;
            LiteralType = type;
        }
        public override string ToRussianString() => $"{Indent()}{LiteralType}_ЛИТЕРАЛ: {Value}";
    }

    public class WhenExpressionBlock : RussBlock
    {
        public RussBlock InputExpression { get; }
        public List<WhenBranchBlock> Branches { get; }
        public WhenElseBranchBlock ElseBranch { get; set; }
        public string ResultTempVar { get; }
        public string ResultType { get; set; }       

        public WhenExpressionBlock(RussBlock inputExpr, List<WhenBranchBlock> branches, string tempVar) : base("УСЛОВНОЕ_ВЫРАЖЕНИЕ_WHEN")
        {
            InputExpression = inputExpr;
            Branches = branches;
            ResultTempVar = tempVar;
            ResultType = "НЕИЗВЕСТНЫЙ_ТИП";    
        }
        public override string ToRussianString()
        {
            var sb = new StringBuilder();
            InputExpression.Depth = this.Depth + 1;
            sb.AppendLine($"{Indent()}УСЛОВНОЕ_ВЫРАЖЕНИЕ_WHEN (Результат в: {ResultTempVar}, Тип результата: {ResultType})");
            sb.AppendLine($"{Indent()}  ПРОВЕРЯЕМОЕ_ЗНАЧЕНИЕ:");
            sb.AppendLine(InputExpression.ToRussianString());
            foreach (var branch in Branches)
            {
                branch.Depth = this.Depth + 1;
                sb.AppendLine(branch.ToRussianString());
            }
            if (ElseBranch != null)
            {
                ElseBranch.Depth = this.Depth + 1;
                sb.AppendLine(ElseBranch.ToRussianString());
            }
            return sb.ToString().TrimEnd();
        }
    }

    public class WhenStatementBlock : RussBlock
    {
        public RussBlock InputExpression { get; }
        public List<WhenBranchBlock> Branches { get; }

        public WhenStatementBlock(RussBlock inputExpr, List<WhenBranchBlock> branches) : base("ОПЕРАТОР_WHEN")
        {
            InputExpression = inputExpr;
            Branches = branches;
        }
        public override string ToRussianString()
        {
            var sb = new StringBuilder();
            InputExpression.Depth = this.Depth + 1;
            sb.AppendLine($"{Indent()}ОПЕРАТОР_WHEN");
            sb.AppendLine($"{Indent()}  ПРОВЕРЯЕМОЕ_ЗНАЧЕНИЕ:");
            sb.AppendLine(InputExpression.ToRussianString());
            foreach (var branch in Branches)
            {
                branch.Depth = this.Depth + 1;
                sb.AppendLine(branch.ToRussianString());
            }
            return sb.ToString().TrimEnd();
        }
    }

    public class WhenBranchBlock : RussBlock
    {
        public List<ValueConditionBlock> Conditions { get; }
        public RussBlock Action { get; }                

        public WhenBranchBlock(List<ValueConditionBlock> conditions, RussBlock action) : base("ВЕТКА_WHEN")
        {
            Conditions = conditions;
            Action = action;
        }
        public override string ToRussianString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{Indent()}  ВЕТКА_УСЛОВИЙ:");
            foreach (var cond in Conditions)
            {
                cond.Depth = this.Depth + 2;
                sb.AppendLine(cond.ToRussianString());
            }
            Action.Depth = this.Depth + 2;      
            sb.AppendLine($"{Indent()}    ДЕЙСТВИЕ_ВЕТКИ:");
            sb.Append(Action.ToRussianString());
            return sb.ToString().TrimEnd();
        }
    }

    public class ValueConditionBlock : RussBlock
    {
        public RussBlock Value1 { get; }  
        public RussBlock Value2 { get; }    
        public bool IsRange { get; }

        public ValueConditionBlock(RussBlock val1, RussBlock val2 = null) : base("УСЛОВИЕ_ЗНАЧЕНИЯ")
        {
            Value1 = val1;
            Value2 = val2;
            IsRange = (val2 != null);
        }
        public override string ToRussianString()
        {
            Value1.Depth = this.Depth;                  
            string val1Str = (Value1 as LiteralBlock)?.Value ?? OperandToString(Value1);


            if (IsRange)
            {
                Value2.Depth = this.Depth;
                string val2Str = (Value2 as LiteralBlock)?.Value ?? OperandToString(Value2);
                return $"{Indent()}  ДИАПАЗОН: {val1Str} .. {val2Str}";
            }
            return $"{Indent()}  ЗНАЧЕНИЕ: {val1Str}";
        }
    }

    public class WhenElseBranchBlock : RussBlock
    {
        public RussBlock Action { get; }  
        public WhenElseBranchBlock(RussBlock action) : base("ВЕТКА_ELSE_WHEN")
        {
            Action = action;
        }
        public override string ToRussianString()
        {
            Action.Depth = this.Depth + 1;
            return $"{Indent()}  ВЕТКА_ELSE:\n{Indent()}    ДЕЙСТВИЕ_ELSE:\n{Action.ToRussianString()}";
        }
    }

    public class ScopeBlock : RussBlock
    {
        public List<RussBlock> Statements { get; }
        public ScopeBlock(List<RussBlock> statements) : base("БЛОК_ОПЕРАТОРОВ_ВЕТКИ")
        {
            Statements = statements;
        }
        public override string ToRussianString()
        {
            var sb = new StringBuilder();
            foreach (var stmt in Statements)
            {
                stmt.Depth = this.Depth;             
                sb.AppendLine(stmt.ToRussianString());
            }
            return sb.ToString().TrimEnd();
        }
    }


    public class ScopeStartBlock : RussBlock
    {
        public ScopeStartBlock() : base("НАЧАЛО_БЛОКА_КОДА") { }
        public override string ToRussianString() => $"{Indent()}{BlockType}";
    }

    public class ScopeEndBlock : RussBlock
    {
        public ScopeEndBlock() : base("КОНЕЦ_БЛОКА_КОДА") { }
        public override string ToRussianString() => $"{Indent()}{BlockType}";
    }
}