using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Arcade.Dsl;

namespace Arcade.Tests
{
    public interface IAmAnInterface
    {
        void DoSomething(string input);
    }

    public sealed class InterfaceImplementation : IAmAnInterface
    {
        public void DoSomething(string input)
        {
            Debug.WriteLine(input);
        }
    }

    #region Flows

    public sealed class IntToStringContinuation : IFlowContinuation<int, string>
    {
        public void Process(int input, Action<string> continueWith)
        {
            continueWith(input.ToString());
        }
    }
    
    public sealed class StringFromChars : IFlow<IEnumerable<char>, string>
    {
        public void Process(IEnumerable<char> input)
        {
            Result(new string(input.ToArray()));
        }
        
        public event Action<string> Result;
    }

    public sealed class DependingOnInterfaceFlow : IFlow<string, string>
    {
        private readonly IAmAnInterface _dependency;

        public DependingOnInterfaceFlow(IAmAnInterface dependency)
        {
            _dependency = dependency;
        }

        public void Process(string input)
        {
            _dependency.DoSomething(input);
            Result(input);
        }

        public event Action<string> Result;
    }
    
    public sealed class StringSplitter : IFlow<string, IEnumerable<char>>
    {
        public void Process(string input)
        {
            Result(input.ToCharArray());
        }
        
        public event Action<IEnumerable<char>> Result;
    }
    
    public sealed class CharShifter : IFlow<char, char>
    {
        public void Process(char input)
        {
            Result(input);
        }
        
        public event Action<char> Result;
    }

    public sealed class FailingCharShifter : IFlowContinuation<char, char>
    {
        public void Process(char input, Action<char> continueWith)
        {
            var charValue = (int) input;

            if((charValue % 2) != 0)
                throw new InvalidOperationException("Thrown on purpose!");

            continueWith(input);
        }
    }

    public sealed class Aggregator : IFlowContinuation<int[], int>
    {
        public void Process(int[] input, Action<int> continueWith)
        {
            var result = 0;

            foreach (var i in input)
            {
                result += i;
            }

            continueWith(result);
        }
    }
    
    public sealed class BoolToStringTranslator : IFlow<bool, string>
    {
        public void Process(bool input)
        {
            Result(input.ToString());
        }
        
        public event Action<string> Result;
    }
    
    public sealed class FailingFlow : IFlow<int, string>
    {
        public void Process(int input)
        {
            throw new InvalidOperationException("Failing on purpose!");
        }
        
        public event Action<string> Result;
    }
    
    public class InitialOutflow : IOutflow<string>
    {
        public void Process()
        {
			Result("Scrambled");
        }
        
        public event Action<string> Result;
    }
    
    [CustomTimeout(10)]
    public class ContinueFlow : IFlow<string, int>
    {
        public void Process(string input)
        {
            Debug.WriteLine("Executing Process of ContinueFlow with input " + input);
            Result(input.Length);
        }
        
        public event Action<int> Result;
    }

    public sealed class TranslateNumberToLocaleMonth : IFlow<int, string>
    {
        private static readonly DateTimeFormatInfo DateTimeFormatInfo =
            DateTimeFormatInfo.GetInstance(CultureInfo.GetCultureInfo("de-DE"));

        public void Process(int input)
        {
            Result(DateTimeFormatInfo.GetMonthName(input));
        }

        public event Action<string> Result;
    }
    
    public class CheckFlow : IFlow<int, bool>
    {
        public void Process(int input)
        {
			Result((input % 2) != 0);
        }
        
        public event Action<bool> Result;
    }

    public class Functions
    {
        public static async Task<string> IntToString(int input)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            return await Task.Run(() => input.ToString(CultureInfo.CurrentCulture));
        }

        //public static TaskAwaiter<string> IntToString(int input)
        //{
            
        //}
    }
    
    public class Receiver
    {
        public void Process(int input)
        {
			Debug.WriteLine("The result measured at the receiver is: " + input.ToString());
        }
    }
    
    public interface ISomthing { }
    
    public class Something : ISomthing { }
    
    public class SomeOtherFlow : IFlow<bool, string>
    {
        public void Process(bool input)
        {
			Result("Hi from SimpleFlow!");
        }
        
        public event Action<string> Result;
    }
    
    public class FlowOutputDependingOnInput : IFlow<bool, string>
    {
        private readonly string _ifTrue;
        private readonly string _ifFalse;
        
        public FlowOutputDependingOnInput(string ifTrue, string ifFalse)
        {
            _ifTrue = ifTrue;
            _ifFalse = ifFalse;
        }

        public void Process(bool input)
        {
            Result(input ? _ifTrue : _ifFalse);
        }
        
        public event Action<string> Result;
    }
    
    public sealed class IntToString
    {
        public void Process(int i)
        {
            Result(i.ToString(CultureInfo.InvariantCulture));
        }
        
        public event Action<string> Result;
    }

    [CustomTimeout(1)]
    public sealed class TimingOutContinuation : IFlowContinuation<string, string>
    {
        public void Process(string input, Action<string> continueWith)
        {
            Thread.Sleep(2.Seconds());
            continueWith(input);
        }
    }

#endregion
    
}
