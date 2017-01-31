using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_to_Pseudocode
{
    public class ProgressChangedEventArgs : EventArgs
    {
        public double percent{get;set;}
    }
    public class LineParser
    {
        FilterOptions options = new FilterOptions();

        public delegate void OnProgressChanged(ProgressChangedEventArgs e);
        public event OnProgressChanged ProgressChanged;
        public string Parse(string code, string type, FilterOptions _fo)
        {
            options = _fo;
            if (type == "Function")
                return ParseFunction(code);
            else
                return "Error: Unable to Parse Code as type: " + type;
        }
        # region Function Helper
        public bool IsFunctionHeadder(string lineofcode)
        {
            if (lineofcode.Contains(";"))
                return false;
            if (HasDatatype(lineofcode))
                return true;

            return false;
        }
        public bool HasDatatype(string lineofcode)
        {
            string type = GetFunctionReturnType(lineofcode);
            switch (type)
            {
                case "void":
                case "int":
                case "int[]":
                case "int*":
                case "float":
                case "float[]":
                case "float*":
                case "char":
                case "char*":
                case "char[]":
                case "Books":
                case "Books[]":
                case "Books*":
                case "FILE*":
                case "struct":
                    return true;
                default:
                    return false;
            }
        }
        public string GetFunctionName(string lineofcode)
        {
            if (lineofcode.Split(' ')[0].Trim().Contains("struct"))
                return lineofcode.Split(' ')[2].Split('(')[0];

            return lineofcode.Split(' ')[1].Split('(')[0];
        }
        public string GetFunctionReturnType(string lineofcode)
        {
            return lineofcode.Split(' ')[0];
        }
        public string DataTypeToPseudocode(string type)
        {
            switch (type)
            {
                case "int":
                    return "integer";
                case "int[]":
                    return "integer array";
                case "int*":
                    return "integer pointer";
                case "char":
                    return "character";
                case "char[]":
                    return "string";
                case "char*":
                    return "character pointer";
                case "float":
                    return "real";
                case "float[]":
                    return "real array";
                case "time_t":
                    return "TIME";
                case "Books":
                    return "Books";
                case "Books*":
                    return "Books pointer";
                case "Books[]":
                    return "Book array";
                case "FILE*":
                    return "FILE pointer";
                case "struct":
                    return "structure";
                case "enum":
                    return "enumeration";
                default: return "unknown type(" + type + ")";
            }
        }
        public string GetFunctionArgs(string lineofcode)
        {
            string format = "";
            string[] temp;
            string[] args = lineofcode.Split('(')[1].Split(')')[0].Split(',');
            if (args[0] == "")
                return "";

            for (int i = 0; i < args.Length; i++)
            {
                temp = args[i].Trim().Split(' ');
                format += temp[1] + "<" + DataTypeToPseudocode(temp[0]) + ">";
                if (i <= args.Length - 2)
                    format += ",";
            }

            return format;
        }
        public string ParseFunctionheadder(string lineofcode)
        {
            string function_name = GetFunctionName(lineofcode);
            string args = "(" + GetFunctionArgs(lineofcode) + ")";

            return "ALGORITHM " + function_name + args + "\nBegin";
        }
        #endregion

        #region Declaration Helper
        public bool isDeclearation(string lineofcode)
        {
            if (HasDatatype(lineofcode) && lineofcode.Contains(";") && !lineofcode.Contains("("))
                return true;
            return false;
        }
        public string ParseDeclearation(string lineofcode)
        {
            string datatype = GetFunctionReturnType(lineofcode);
            string variable_names = lineofcode.Replace(datatype, "").Trim().Replace(";", "");

            return "\tDECLARE " + variable_names + " AS " + DataTypeToPseudocode(datatype).ToUpper() + "\n";
        }
        #endregion

        #region Print Statement Helper
        public bool IsPrintStatement(string lineofcode)
        {
            string function_name;
            if (lineofcode.Contains("printf(") || lineofcode.Contains("print("))
            {
                function_name = lineofcode.Split('(')[0];
                if (function_name == "printf" || function_name == "print")
                    return true;
            }
            return false;
        }
        public string ParsePrintStatement(string lineofcode)
        {
            string stringtoprint = lineofcode.Split('"')[1].Split('"')[0];
            lineofcode = lineofcode.Replace(");", "");
            int args_count = 0;
            string lastarg = "";
            string lastarg2 = "";
            if (lineofcode.Contains("\","))
            {
                string[] args = lineofcode.Split('"')[2].Substring(1).Split(',');
                for (int i = 0; i < args.Length; i++)
                    if (args[i].Contains("(") && !args[i].Contains(")"))
                    {
                        args[i] += "," + args[i + 1];
                        args = args.Where(w => w != args[i + 1]).ToArray();
                        if (LineContainsFunctionCall(args[i]))
                            args[i] = "CALL " + args[i];
                    }

                int count = 0;
                int padding = 0;
                for (int i = 0; i < stringtoprint.Length; i++)
                {
                    if (stringtoprint[i] == '%')
                    {
                        stringtoprint = stringtoprint.Remove(i, 2);
                        if (i != 0)
                        {
                            stringtoprint = stringtoprint.Insert(i, "\"," + args[count]);
                            padding = 2;
                        }
                        else
                        {
                            stringtoprint = stringtoprint.Insert(i, args[count]);
                            padding = 0;
                        }

                        if (i < stringtoprint.Length - (padding + args[count].Length))
                            stringtoprint = stringtoprint.Insert(i + args[count].Length + 2, ",\"");
                        count++;
                    }
                }
                stringtoprint = stringtoprint.Replace(",\"\",", ",");
                args_count = args.Length;
                lastarg = args[args.Length - 1];
            }
            lastarg2 = stringtoprint.Split(',')[stringtoprint.Split(',').Length - 1];
            if (args_count == 1 && lastarg.Replace(";", "") == stringtoprint)
                return "\tPRINT " + RemoveColours(RemoveEscapeSequences(stringtoprint)) + "\n";
            else if (args_count == 1 && stringtoprint.Length >= 1 && lastarg == lastarg2)
                return "\tPRINT \"" + RemoveColours(RemoveEscapeSequences(stringtoprint)) + "\n";
            else
                return "\tPRINT \"" + RemoveColours(RemoveEscapeSequences(stringtoprint)) + "\"\n";

        }
        public string RemoveEscapeSequences(string lineofcode)
        {
            string[] specialchar = { "\\n", "\\t", "\\r" };

            foreach (string s in specialchar)
                lineofcode = lineofcode.Replace(s, "");

            return lineofcode;
        }
        public string RemoveColours(string lineofcode)
        {
            if (options.RemoveColours)
                for (int i = 1; i < 10; i++)
                    lineofcode = lineofcode.Replace("^" + i, "");

            return lineofcode;
        }
        #endregion

        #region Comment Helper
        public bool IsComment(string lineofcode)
        {
                if (lineofcode.Length >= 2)
                    if (lineofcode.Substring(0, 2) == "//")
                        return true;
            return false;
        }
        #endregion

        #region InputStatement Helper
        public bool IsInputStatement(string lineofcode)
        {
            if (lineofcode.Contains("scanf(") && !lineofcode.Contains("fscanf("))
                return true;
            return false;
        }
        public string ParseInputStatement(string lineofcode)
        {
            string format = "";
            lineofcode = lineofcode.Replace(");", "");
            string[] args = lineofcode.Split('"')[2].Substring(1).Split(',');
            for (int i = 0; i < args.Length; i++)
            {
                format += args[i];
                if (i <= args.Length - 2)
                    format += ",";
            }
            format = format.Replace("&", "");
            return "\tINPUT " + format + "\n";
        }
        #endregion

        #region ForStatement Helper
        public bool IsForStatement(string lineofcode)
        {
            if (lineofcode.Replace(" ", "").Contains("for("))
                return true;
            return false;
        }
        public string RemoveComparsionSymbols(string s)
        {
            string format = "";
            string filterout = "<>=!";
            foreach (char c in s)
                if (!filterout.Contains(c.ToString()))
                    format += c.ToString();

            return format.Trim();
        }
        public string ParseForStatement(string lineofcode)
        {
            string format = "";
            string[] parts = lineofcode.Split('(')[1].Split(')')[0].Split(';');

            string index_variable = parts[0].Trim().Split('=')[0].Trim();
            string start_index = parts[0].Trim().Split('=')[1].Trim();

            if (start_index == "0")
                start_index = "1";

            string target_index = RemoveComparsionSymbols(parts[1].Trim().Substring(1));
            string increment = "";
            if (parts[2].Contains("++"))
                increment = "STEP 1";
            else
                increment = parts[2];

            format += "\tFOR " + index_variable + " = " + start_index + " TO " + target_index + " " + increment;

            return format;
        }
        #endregion

        #region SkipIndenting
        public bool IsDontIndent(string lineofcode)
        {

            if (lineofcode.Contains("{") || lineofcode.Contains("}") || lineofcode.Contains("<") && lineofcode.Contains(">") || lineofcode.Contains("#"))
                return true;
            return false;
        }
        #endregion

        #region Excp
        public bool LineContainsFunctionCall(string lineofcode)
        {
            if (lineofcode.Contains("(") && lineofcode.Contains(")"))
                return true;

            return false;
        }

        public bool IsInclude(string lineofcode)
        {
            if (lineofcode.Contains("#include"))
                return true;
            return false;
        }

        public bool IsNotCall(string lineofcode)
        {
            if (lineofcode.Contains("if(") || lineofcode.Contains("while(") || lineofcode.Contains("switch(") || lineofcode.Contains("case"))
                return true;
            return false;
        }

        public string ParseFunctionCall(string lineofcode)
        {
                if (lineofcode.Contains("="))
                    return "\t" + lineofcode.Replace(lineofcode.Split('=')[1], "call " + lineofcode.Split('=')[1]) + "\n";

                return "\tcall " + lineofcode;
        }

        #endregion

        #region SwitchStatementHelper
        public bool IsSwitchStatement(string lineofcode)
        {
            if (lineofcode.Contains("switch"))
                return true;


            return false;
        }

        public string ParseSwitchStatement(string lineofcode)
        {
            string arg = lineofcode.Split('(')[1].Split(')')[0];

            return "Do Case " + arg + "";
        }

        public bool IsCaseSatemnt(string lingofcode)
        {
            lingofcode = lingofcode.Trim();
            if (lingofcode.Split(':')[0].Contains("case"))
                return true;

            return false;
        }
        public string ParseCaseStatement(string lineofcode) //god
        {
            string key = "";
            string statement = lineofcode.Split(':')[1];

            if (lineofcode.Split(':')[0].Contains("\'"))
                key = lineofcode.Split('\'')[1].Split('\'')[0];
            else
                key = lineofcode.Split(':')[0].Trim().Substring(0, lineofcode.Split(':')[0].Trim().Length - 1);

            return "\t    Case " + key + ":" + statement + "\n";
        }

        public bool IsCaseOther(string lineofcode)
        {
            if (lineofcode.Contains("default: "))
                return true;
            return false;
        }
        public string ParseCaseOther(string lineofcode)
        {
            string statement = lineofcode.Split(':')[1];

            return "\tCase other: " + statement + "\n";
        }
        #endregion


        #region Handle Parse
        public string ParseFunction(string codeblock)
        {
            int i = 0;
            bool wasaprint = false;
            string[] codeblocks = codeblock.Split('\n');
            string format = "";
            foreach (string line in codeblocks)
            {
                string temp = line.Trim();
                if (IsFunctionHeadder(temp))
                {
                    codeblocks[i] = ParseFunctionheadder(temp);
                    codeblocks[i + 1] = codeblocks[i + 1].Replace("{", "");
                }
                else if (isDeclearation(temp))
                    codeblocks[i] = ParseDeclearation(temp);
                else if (HasDatatype(temp) && temp.Contains(";") && temp.Contains("(") && temp.Contains("(") && temp.Contains("="))
                    codeblocks[i] = ParseDeclearation(temp);
                else if (HasDatatype(temp) && temp.Contains(";") && temp.Contains("("))
                    if (!options.RemoveFuncPrototype)
                        codeblocks[i] = "";
                    else
                        codeblocks[i] = "Function Declaration ->" + codeblocks[i];
                else if (IsPrintStatement(temp))
                {
                    codeblocks[i] = ParsePrintStatement(RemoveEscapeSequences(temp));
                    wasaprint = true;
                }
                else if (IsComment(temp) && options.RemoveComments)
                    codeblocks[i] = "";
                else if (IsInputStatement(temp))
                    codeblocks[i] = ParseInputStatement(temp);
                else if (IsForStatement(temp))
                {
                    codeblocks[i] = ParseForStatement(temp);
                    if (!codeblocks[i + 1].Contains("{"))
                        codeblocks[i] = codeblocks[i] + "\n";
                }
                else if (!IsDontIndent(temp) && options.EnableAutoIndent)
                {
                    if (LineContainsFunctionCall(temp) && !IsNotCall(temp))
                    {
                        codeblocks[i] = ParseFunctionCall(temp);
                        if (!codeblocks[i].Contains('\n'))
                            codeblocks[i] = codeblocks[i] + "\n";
                    }
                    else if (IsNotCall(temp))
                        codeblocks[i] = "\t" + temp;
                    else
                        codeblocks[i] = "\t" + temp + "\n";
                }

                if (IsInclude(temp) && options.RemoveIncludes)
                {
                    codeblocks[i] = "";
                    i++;
                    continue;
                }

                if (IsSwitchStatement(temp))
                {
                    codeblocks[i] = ParseSwitchStatement(temp);
                    codeblocks[i + 1] = codeblocks[i + 1].Replace("{", "");
                }
                if (IsCaseSatemnt(temp))
                    codeblocks[i] = ParseCaseStatement(temp);
                if (IsCaseOther(temp))
                    codeblocks[i] = ParseCaseOther(temp);
                if (temp.Contains("if("))
                {
                    // codeblocks[i] = codeblocks[i] + "\n";
                    wasaprint = true;
                }
                if (!wasaprint && options.UseArrowEqual)
                    codeblocks[i] = codeblocks[i].Replace("=", "←");
                wasaprint = false;
                if (!codeblocks[i].Contains('\n') && codeblocks[i] != "" && !codeblocks[i].Contains("{") && !codeblocks[i].Contains("FOR") && IsNotCall(codeblocks[i]) && !codeblocks[i + 1].Contains("{"))
                    codeblocks[i] = codeblocks[i] + "\n";

                if (codeblocks[i].Contains("++"))
                    codeblocks[i] = "\t" + codeblocks[i].Split('+')[0].Trim() + " = " + codeblocks[i].Split('+')[0].Trim() + "+ 1";

                if (codeblocks[i].Contains("else"))
                {
                    codeblocks[i] = "\telse";
                    if (codeblocks[i + 1].Trim().Contains("{"))
                        codeblocks[i + 1] = "";
                }
                if (codeblocks[i].Contains("=="))
                    codeblocks[i] = codeblocks[i].Replace("==", "=");

                if (codeblocks[i].Contains("//") && !LineContainsFunctionCall(codeblocks[i]))
                {
                    int startindex = codeblocks[i].IndexOf("//");
                    int lastindex = codeblocks[i].IndexOf('\n');
                    codeblocks[i] = codeblocks[i].Remove(startindex, lastindex - startindex);
                }
                ProgressChanged(new ProgressChangedEventArgs{percent = (i*100)/codeblocks.Length});
                i++;
                //  if (i > codeblocks.Length-1)
                //  break;
            }
            foreach (string line in codeblocks)
                format += line;

            return format.Replace("{", "\r\tBegin").Replace("}", "\rEnd").Replace(";", "").Replace("[", "(").Replace("]", ")").Replace("←←", "←"); ;
        }
        #endregion

    }
}