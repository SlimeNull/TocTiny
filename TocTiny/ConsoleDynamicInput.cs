using System;
using System.Collections.Generic;

namespace Null.Library
{
    public class DynamicScanner
    {
        private int startTop, startLeft, endTop, endLeft, currentTop, currentLeft, tempEndTop, tempEndLeft;
        private int inputIndex, historyIndex;
        private bool insertMode = true, inputting = false, cursorVisible;
        private readonly object printting = false;                // 用于互斥锁
        private ConsoleKeyInfo readedKey;
        private readonly List<List<char>> inputHistory;
        private List<char> inputtingChars;
        private string promptText = string.Empty;


        public delegate bool CharInputEventHandler(DynamicScanner sender, ConsoleKeyInfo c);
        public delegate bool PreviewCharInputEventHandler(DynamicScanner sender, ConsoleKeyInfo c);

        public event CharInputEventHandler CharInput;
        public event PreviewCharInputEventHandler PreviewCharInput;

        public DynamicScanner()
        {
            inputHistory = new List<List<char>>();
        }
        public string InputtingString => inputtingChars == null ? string.Empty : new string(inputtingChars.ToArray());

        public int StartTop => startTop;
        public int StartLeft => startLeft;
        public int EndTop => endTop;
        public int EndLeft => endLeft;
        public int CurrentTop => currentTop;
        public int CurrentLeft => currentLeft;
        public bool IsInputting => inputting;
        public string PromptText { get => promptText; set => promptText = value; }

        public static bool IsControlKey(ConsoleKey k)
        {
            return k == ConsoleKey.Enter ||
                k == ConsoleKey.UpArrow ||
                k == ConsoleKey.DownArrow ||
                k == ConsoleKey.LeftArrow ||
                k == ConsoleKey.RightArrow ||
                k == ConsoleKey.Insert ||
                k == ConsoleKey.Backspace ||
                k == ConsoleKey.Delete ||
                k == ConsoleKey.Home ||
                k == ConsoleKey.End;
        }

        private void InitReadLine()
        {
            startTop = Console.CursorTop;
            startLeft = Console.CursorLeft;
            inputIndex = 0;

            if (inputHistory.Count == 0 || inputHistory[inputHistory.Count - 1].Count != 0)
            {
                historyIndex = inputHistory.Count;
                inputHistory.Add(new List<char>());
            }
            else
            {
                historyIndex = inputHistory.Count - 1;
            }

            inputtingChars = inputHistory[historyIndex];

            inputting = true;
        }
        private bool DealInputChar()
        {
            switch (readedKey.Key)
            {
                case ConsoleKey.Enter:
                    Console.WriteLine();
                    return true;
                case ConsoleKey.UpArrow:
                    if (historyIndex > 0)
                    {
                        historyIndex--;
                        UpdateInputState();
                    }
                    break;
                case ConsoleKey.DownArrow:
                    if (historyIndex < inputHistory.Count - 1)
                    {
                        historyIndex++;
                        UpdateInputState();
                    }
                    break;
                case ConsoleKey.LeftArrow:
                    if (inputIndex > 0)
                    {
                        inputIndex--;
                    }
                    break;
                case ConsoleKey.RightArrow:
                    if (inputIndex < inputtingChars.Count)
                    {
                        inputIndex++;
                    }
                    break;
                case ConsoleKey.Insert:
                    insertMode = !insertMode;
                    break;
                case ConsoleKey.Backspace:
                    if (inputIndex > 0)
                    {
                        inputtingChars.RemoveAt(inputIndex - 1);
                        inputIndex--;
                    }
                    break;
                case ConsoleKey.Delete:
                    if (inputIndex < inputtingChars.Count)
                    {
                        inputtingChars.RemoveAt(inputIndex);
                    }
                    break;
                case ConsoleKey.Home:
                    inputIndex = 0;
                    break;
                case ConsoleKey.End:
                    inputIndex = inputtingChars.Count;
                    break;
                default:
                    if (inputIndex == inputtingChars.Count)
                    {
                        inputtingChars.Add(readedKey.KeyChar);
                    }
                    else
                    {
                        if (insertMode)
                        {
                            inputtingChars.Insert(inputIndex, readedKey.KeyChar);
                        }
                        else
                        {
                            inputtingChars[inputIndex] = readedKey.KeyChar;
                        }
                    }
                    inputIndex++;
                    break;
            }
            return false;
        }
        private void PrintInputString()
        {
            lock (printting)
            {
                cursorVisible = Console.CursorVisible;
                Console.CursorVisible = false;
                Console.SetCursorPosition(startLeft, startTop);

                Console.Write(promptText);

                if (inputIndex == inputtingChars.Count)
                {
                    for (int i = 0; i < inputtingChars.Count; i++)
                    {
                        Console.Write(inputtingChars[i]);
                    }
                    currentLeft = Console.CursorLeft;
                    currentTop = Console.CursorTop;
                }
                else
                {
                    for (int i = 0; i < inputtingChars.Count; i++)
                    {
                        if (inputIndex == i)
                        {
                            currentLeft = Console.CursorLeft;
                            currentTop = Console.CursorTop;
                        }
                        Console.Write(inputtingChars[i]);
                    }
                }

                tempEndLeft = Console.CursorLeft;
                tempEndTop = Console.CursorTop;

                while (Console.CursorTop < endTop || Console.CursorLeft < endLeft)
                {
                    Console.Write(' ');
                }

                endLeft = tempEndLeft;
                endTop = tempEndTop;

                Console.SetCursorPosition(currentLeft, currentTop);
                Console.CursorVisible = cursorVisible;
            }
        }
        private void UpdateInputState()
        {
            inputtingChars = inputHistory[historyIndex];
            inputIndex = inputtingChars.Count;
        }
        public void ClearDisplayBuffer()
        {
            lock (printting)
            {
                cursorVisible = Console.CursorVisible;
                Console.CursorVisible = false;
                Console.SetCursorPosition(startLeft, startTop);
                while (Console.CursorTop < endTop || Console.CursorLeft < endLeft)
                {
                    Console.Write(' ');
                }
                Console.SetCursorPosition(startLeft, startTop);
                Console.CursorVisible = cursorVisible;
            }
        }
        public void DisplayTo(int cursorLeft, int cursorTop)
        {
            lock (printting)
            {
                cursorVisible = Console.CursorVisible;
                Console.CursorVisible = false;
                Console.SetCursorPosition(cursorLeft, cursorTop);
                startLeft = cursorLeft; startTop = cursorTop;

                Console.Write(promptText);

                if (inputIndex == inputtingChars.Count)
                {
                    for (int i = 0; i < inputtingChars.Count; i++)
                    {
                        Console.Write(inputtingChars[i]);
                    }
                    currentLeft = Console.CursorLeft;
                    currentTop = Console.CursorTop;
                }
                else
                {
                    for (int i = 0; i < inputtingChars.Count; i++)
                    {
                        if (inputIndex == i)
                        {
                            currentLeft = Console.CursorLeft;
                            currentTop = Console.CursorTop;
                        }
                        Console.Write(inputtingChars[i]);
                    }
                }

                endLeft = Console.CursorLeft;
                endTop = Console.CursorTop;
                Console.CursorVisible = cursorVisible;
            }
        }
        public void SetInputStart(int cursorLeft, int cursorTop)
        {
            lock (printting)
            {
                cursorVisible = Console.CursorVisible;
                Console.CursorVisible = false;
                Console.SetCursorPosition(startLeft, startTop);
                while (Console.CursorTop < endTop || Console.CursorLeft < endLeft)
                {
                    Console.Write(' ');
                }
                Console.SetCursorPosition(cursorLeft, cursorTop);
                startLeft = cursorLeft; startTop = cursorTop;

                Console.Write(promptText);

                if (inputIndex == inputtingChars.Count)
                {
                    for (int i = 0; i < inputtingChars.Count; i++)
                    {
                        Console.Write(inputtingChars[i]);
                    }
                    currentLeft = Console.CursorLeft;
                    currentTop = Console.CursorTop;
                }
                else
                {
                    for (int i = 0; i < inputtingChars.Count; i++)
                    {
                        if (inputIndex == i)
                        {
                            currentLeft = Console.CursorLeft;
                            currentTop = Console.CursorTop;
                        }
                        Console.Write(inputtingChars[i]);
                    }
                }

                endLeft = Console.CursorLeft;
                endTop = Console.CursorTop;
                Console.CursorVisible = cursorVisible;
            }
        }
        public string ReadLine()
        {
            InitReadLine();
            PrintInputString();

            while (true)
            {
                readedKey = Console.ReadKey(true);
                if (PreviewCharInput != null && PreviewCharInput.Invoke(this, readedKey))
                {
                    continue;
                }

                if (DealInputChar())
                {
                    inputting = false;
                    return InputtingString;
                }

                PrintInputString();

                if (CharInput != null && CharInput.Invoke(this, readedKey))
                {
                    inputting = false;
                    return InputtingString;
                }
            }
        }
        public string QuietReadLine()
        {
            InitReadLine();
            PrintInputString();

            while (true)
            {
                readedKey = Console.ReadKey(true);
                if (PreviewCharInput != null && PreviewCharInput.Invoke(this, readedKey))
                {
                    continue;
                }

                if (DealInputChar())
                {
                    inputting = false;
                    return InputtingString;
                }

                if (CharInput != null && CharInput.Invoke(this, readedKey))
                {
                    inputting = false;
                    return InputtingString;
                }
            }
        }
    }
}
