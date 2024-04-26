using System;
using System.Collections.Generic;

namespace Mini_Apps
{
    class Program
    {
        private static void Main(string[] args)
        {
            // found bug with win11 update to cmd:
            // bufferheight=window height, so setcursorposition will not work as before.
            // -> bufferheight needs to be increased to allow to write after windowheight without error
            // [scrolling still works with added bufferheight]

            Console.SetBufferSize(Console.BufferWidth, Console.BufferHeight + (16)); // bufferheight increased so all 10 questions fit, as well as title and error messages, etc.
            // increase bufferheight by:
            //      1  (end of 6th question)
            //      + 3(amount of questions left)
            //      * 4(amount of lines used for each question)
            //      + 2 (extra lines at end for error message)
            //      = 15

            
            // generate random colour, hide cursor //
            Display.ColourGen();
            Console.CursorVisible = false;

            bool displayMenu = true;

            // display menu until user asks to exit program, even after using mini app // 
            while (displayMenu)
                displayMenu = Menu();
        }
        private static bool Menu()
        {
            // main menu //
            switch (Display.Menu(0))
            {
                case 0: // start Keep Counting
                    KeepCounting.Play();
                    return true;
                case 1: // start Square Root Calculator
                    SqrRtCalc.Play();
                    return true;
                case 2: // start Caesar Cipher, after choosing type
                    Caesar.Play(Display.Menu(1)); // display caesar selection menu (en/decrypt), start method
                    return true;
                default: // escape key pressed, exit program
                    return false;
            }
        }
    }

    /* ADD:
     *  KeepCounting
        ->take parameter when called from main menu such that the amount of questions can be changed, and adjust the bufferheight from this variable so it will not break
            -rather than manually changing buffer height to match maximum lines needed for questions
        ->stop only minus or only plus being used - don't have it completely random [having only '0 - 0 = ' for all 10 questions is incredibly annoying]
        ->exit to menu if escape key pressed

     *  SqRtCalc
        ->exit to menu if escape key pressed (even if trying to calculate)

     *  Caesar
        ->exit to main menu if escape key pressed

     *  Test bufferheight adjustment on repl.it

     *  escape key from any mini app brings user back to main menu
     
     *  would have each mini app in separate file, to be more readable, but having in one .cs file is fine
     
    */

    // MINI APPS //
    class KeepCounting
    {
        public static void Play()
        {
            // display title with Instructions //
            Display.TitleInfo("Keep Counting", false,
                "You will be presented with 10 arithmetic questions.",
                "After the first question, the left-hand operand is the previous result.");

            // display questions //
            QuestionCount = 10; // amount of questions to be displayed
            int score = DoQuestions(); // display the questions, return the user's score

            // clear console, display score. Wait for key press then return to main menu //
            Console.Clear();
            Display.TitleInfo("Keep Counting Score", true,
                "You scored " + score + "!");
        }
        private static int QuestionCount { get; set; }
        /// <summary>
        /// Generate random number from 0 to max.
        /// </summary>
        /// <param name="max">The largest number that can be generated.</param>
        /// <returns>Randomly generated number.</returns>
        private static int NumGen(int max)
        {
            Random rnd = new Random();
            return rnd.Next(0, max + 1);
        }

        /// <summary>
        /// Displays the questions, and counts user's score.
        /// </summary>
        /// <returns>Returns user's score.</returns>
        private static int DoQuestions()
        {
            int num1 = 0, // first operand
                num2, // second operand used
                answer, // correct answer to current question
                score = 0,
                input;  // user's input
            char sign = '-'; // randomly generated sign [+ / -]
            string question; // current question


            for (int questionNum = 0; questionNum < QuestionCount; questionNum++)
            {
                // if first question, generate num1 //
                if (questionNum == 0)
                    num1 = NumGen(10);

                // sign generation //
                if (NumGen(1) == 0) // 0 => +, 1 => -
                    sign = '+';

                // if +, simply generate num2 <= 10 //
                if (sign == '+' || num1 >= 10) // if num1 >= 10, then num2 <= 10
                    num2 = NumGen(10); 
                
                // if -, make sure num2 !> num1 //
                else
                    num2 = NumGen(num1); // if num1 < 10, then num2 <= num1

                // define question //
                question = $"{num1} {sign} {num2}";

                // calculate answer, define num1 for next question //
                if (sign == '+')
                    num1 = answer = num1 + num2;
                else
                    num1 = answer = num1 - num2;

                // display question //
                input = Display.Question(questionNum * 4,   // placement [each question takes up 4 lines, including error check message]
                    "Question " + (questionNum + 1), true,       // "Question 2:", etc
                    question)[0];                              // actual question - "1 + 3 = ", etc

                // if user is correct, set answer to -1 so Feedback() will say user is correct //
                if (input == answer)
                {
                    answer = -1;
                    score++;
                }

                // display feedback //
                Display.Feedback(questionNum * 4, answer);
            }
            return score;
        }
    }
    class SqrRtCalc
    {
        /* TESTING TABLE
         * 
         * [testing truncation]
         * input1   input2  Expected output     Actual output   Fix
         * 1        3       1.000               1               If not to correct decimal place, add 0s after decimal point. If no decimal point, create decimal point then add the 0s.
         * 1        3       1.000               1.000           Fixed.
         * => forgot to account for whole numbers with first implementation of truncation. Fixed now.
         * 
         * [testing input into "Please enter a positive whole number:"]
         * Test Type        Input1      Input2  Expected Result     Actual Result                   Comments
         * Boundary[upper]  2147483647  1       46340.9             Overflow, cannot calculate      To fix, need to find maximum number user can input without causing overflow, and disallow anything higher
         * Boundary[lower]  0           1       0.0                 Same                            N/A
         * Erroneous        dog         1       Invalid Notice      Same                            N/A
         * Erroneous        -123        1       Invalid Notice      Same                            N/A
         * Normal           2           1       1.4                 Same                            N/A
         * Normal           81          1       9.0                 Same                            N/A
         * Normal           123456      1       351.3               Same                            N/A
         * => issues only arise due to maximum capacity of int32 and decimal data types, otherwise fine
         * 
         * [testing input into "How many places to round the solution [1 - 6]:"]
         * Test Type        Input1      Input2  Expected Output     Actual Output       Comments
         * Boundary[upper]  7           6       Continue            Same                N/A
         * Boundary[lower]  7           1       Continue            Same                N/A
         * Erroneous        7           dog     Invalid Notice      Same                N/A
         * Erroneous        7           -123    Invalid Notice      Same                N/A
         * Normal           7           2       Continue            Same                N/A
         * Normal           7           4       Continue            Same                N/A
         * => number correctly truncated to wanted decimal place
         * 
         * [testing choosing of the initial boundaries]
         * input1   input2  Expected boundaries     Actual boundaries   Comments
         * 1        1       1,0                     1,0                 Found perfect square root (1), upperbound equaling 0 shows it is a perfect sqrt
         * 2        1       1,2                     1,2                 1^2 < sqrt < 2^2. Good Boundaries
         * 1234     1       35,36                   35,36               
         * 0        1       0,0                     0,0
         */

        public static void Play()
        {
            string result;

            // display title //
            Display.TitleInfo("Square Root Calculator");

            // ask for number to square root and decimal places //
            int num = Display.Question(-3, "Please enter a positive whole number", true)[0];
            int decimalPlace = Display.Question(0, "How many places to round the solution [1 - 6]", true, null, 6)[0];

            // calculate result //
            result = Calculate(num, decimalPlace);

            // display truncated result //
            Console.Clear();
            Display.TitleInfo("Square Root Result", true,
                "The square root of " + num + " to " + decimalPlace + " decimal places is " + result);
        }
        /// <summary>
        /// Find square root of number to specified accuracy via finding upper and lower bounds, average of each, and updating in reference to number to find root.
        /// </summary>
        /// <param name="num">The number of which to find the square root.</param>
        /// <param name="accuracy">The accuracy of the answer.</param>
        /// <returns>The answer as a string.</returns>
        private static string Calculate(int num, int accuracy)
        {
            // divide decimalPlaces by 0.1 until wanted decimal place found //
            decimal decimalPlaces = 1; // decimal places in decimal form
            for (int i = 0; i <= accuracy; i++)
                decimalPlaces *= (decimal)0.1;

            // find initial upper, lower bounds [closest square roots to number]//
            var bounds = InitialBounds(num);
            decimal
                lb = bounds.Item1,
                ub = bounds.Item2,
                average,
                square;

            // find sqrt, return //
            while (true)
            {
                // if upper bound = 0, sqrt found
                if (ub == 0)
                    return Truncate(lb, accuracy);

                average = Average(lb, ub);
                square = average * average;

                // change bounds //
                if (square < num)
                    lb = average;
                else // if (square > num)
                    ub = average;


                if (CheckAccuracy(lb, ub, decimalPlaces)) // if to specified degree of accuracy, return sqrt
                    return Truncate(average - (average % decimalPlaces), accuracy);
            }
        }


        /// <summary>
        /// Truncate to desired accuracy.
        /// </summary>
        /// <param name="num">Value to be truncated.</param>
        /// <param name="dec">The accuracy to truncate.</param>
        /// <returns>Truncated value as string.</returns>
        private static string Truncate(decimal num, int dec)
        {
            // hold num as a list of characters //
            List<char> digitsInNum = new List<char>();
            foreach (char item in num.ToString())
                digitsInNum.Add(item);

            int i = 0, // index within list
                digitsAfterDot = -1; // how many digits past dot character in list
            string
                outputText = null; // the text (truncated answer) to be displayed

            while (true)
            {
                // go through list, until find dot. Then continue, until accuracy found. If found, output. Otherwise, add 0s until accuracy found, then output.

                // find dot, continue until desired accuracy. Then output //
                if (i < digitsInNum.Count)
                {
                    if (digitsInNum[i] == '.') // if dot found, update digitsAfterDot
                    {
                        digitsAfterDot = 0;
                        i++;
                        continue;
                    }
                    if (digitsAfterDot >= 0) // add to digitsAfterDot
                        digitsAfterDot++;

                    if (digitsAfterDot == dec) // if accuracy found, output 
                    {
                        for (int k = 0; k <= i; k++)
                            outputText += digitsInNum[k];
                        return outputText;
                    }
                }
                else if (digitsAfterDot >= 0) // if dot found and has surpassed length of list
                {
                    for (int k = 0; k < dec - digitsAfterDot; k++) // add 0s until accuracy reached
                        digitsInNum.Add('0');

                    for (int k = 0; k < digitsInNum.Count; k++)
                        outputText += digitsInNum[k];
                    return outputText;
                }
                else // if dot not found, create and return at wanted accuracy
                {
                    digitsInNum.Add('.');
                    for (int k = 0; k < dec; k++)
                        digitsInNum.Add('0');
                    for (int k = 0; k < digitsInNum.Count; k++)
                        outputText += digitsInNum[k];
                    return outputText;
                }
                i++;
            }

        }


        // following three methods could be in calculate() rather than as seperate functions. They are only called once each //
        /// <summary>
        /// Find lower and upper bounds or square root of number.
        /// </summary>
        /// <param name="num">The number of which to find square root.</param>
        /// <returns>The [upper, lower] bounds. [sqrt, 0] if sqrt found, [0, 0] if error.</returns>
        /// 
        private static Tuple<int,int> InitialBounds(int num)
        {
            int last = 1, lb = 0, ub = 0;
            // go from 1 to num, find lb, ub or sqrt //
            for (int i = 1; i <= num; i++)
            {
                if (i * i < num) // last possible lower bound
                    last = i;

                if (i * i == num) // if i^2 = num, return sqrt
                {
                    lb = i; ub = 0; // i = sqrt [ub == 0 => lb is sqrt]
                    break;
                }
                else if (i * i > num) // if i^2 > num, bounds found; assigned
                {
                    ub = i;
                    lb = last;
                    break;
                }
            }

            return new Tuple<int, int>(lb, ub);
        }
        /// <summary>
        /// Find average of both bounds.
        /// </summary>
        /// <param name="lb">The lower bound.</param>
        /// <param name="ub">The upper bound.</param>
        /// <returns>The average.</returns>
        private static decimal Average(decimal lb, decimal ub)
            => (lb + ub) / 2;
        /// <summary>
        /// Check if difference between bounds is at wanted accuracy
        /// </summary>
        /// <param name="lb">The lower bound.</param>
        /// <param name="ub">The upper bound.</param>
        /// <param name="dec">The accuracy wanted [in form 0.1, 0.01,...].</param>
        /// <returns>A boolean value showing if at wanted accuracy.</returns>
        private static bool CheckAccuracy(decimal lb, decimal ub, decimal dec)
            => (ub - lb) < dec;
        //


    }
    class Caesar
    {
        public static void Play(int type)
        {
            switch (type)
            {
                case 0: // start encryption
                    CalculateCrypt(true);
                    break;
                case 1: // start decryption
                    CalculateCrypt(false);
                    break;
            }



        }
        private static void CalculateCrypt(bool encrypt)
        {
            /*  ->ask for input of alphanumerics and space ["A","B",...,"Z","0","1",...,"9"," "]
                ->convert lowercase to uppercase

                ->dec ascii codes:


                ->ask for shift [1-36] 
                    [all of alphanumeric characters], space = 0
            */

            // ask for inputs //
            int shift;

            // set up display for en/decryption //
            string 
                suffix,
                upOrDown,
                output = null;
            switch (encrypt)
            {
                case true: // encryption
                    suffix = "En";
                    upOrDown = "up";
                    break;
                case false: // decryption
                    suffix = "De";
                    upOrDown = "down";
                    break;
            }


            // display title //
            Display.TitleInfo(suffix + "cryption", false, $"This Caesar Cipher will shift your characters {upOrDown} by your chosen shift");

            // ask for shift user wants, input range 1 - 36 //
            shift = Display.Question(-1, "What shift would you like ? [1 - 36]", true, null, 36)[0]; 

            // if decrypting, shift is backwards //
            if (!encrypt)
                shift *= -1;

            // ask user for text to en/decrypt //
            int[] inputAscii = Display.Question(2, $"What text you want to {suffix.ToLower()}crypt? [Alphanumerics and space only]", false);

            // add shift //
            for (int c = 0; c < inputAscii.Length; c++)
            {
                if (inputAscii[c] != 32)
                    output += (char)(inputAscii[c] + shift);
                else 
                    output += ' ';
            }

            Console.Clear();
            Display.TitleInfo($"{suffix}crypted Text", true, output) ;

        }
    }
    //

    class Display
    {
        // starting position of options/questions //
        const int
            startX = 8, // position from left of screen for all text
            startY_Title = 2, // position of title, number of lines from top of screen
            startY_Options = startY_Title + 5; // position of options and any questions from title text

        static ConsoleColor MenuColour;
        static int ColourIndex;

        /// <summary>
        /// Generate the  colour used within 'Display' methods. 
        /// </summary>
        /// <param name="random">If false, generation is iterative, otherwise random.</param>
        public static void ColourGen(bool random = true)
        {
            Random r = new Random();
            ConsoleColor[] colours = { ConsoleColor.DarkCyan,
                           ConsoleColor.DarkGreen,
                           ConsoleColor.DarkYellow};
            if (random)
                MenuColour = colours[ColourIndex = r.Next(0, colours.Length)];
            else
            {
                try
                {
                    MenuColour = colours[ColourIndex += 1];
                }
                catch
                {
                    MenuColour = colours[ColourIndex = 0];
                }
            }
        }

        // menu system //

        /// <summary>
        /// Display menu for user to choose an option via use of arrow, enter, and escape key. Pressing C changes menu colour.
        /// </summary>
        /// <param name="menuName">The menu title.</param>
        /// <param name="miniMenu">If true, menu states it exits to main menu.</param>
        /// <param name="options">The options to be selected from.</param>
        /// <returns>The index of selected option.</returns>
        private static int MultipleChoice(string menuName, bool miniMenu, params string[] options)
        {
            int currentSelection = 0, // currently selected option
                optionsAmount = options.Length; // amount of options
            ConsoleKey key; // key pressed by user
            
            // clear formatting //
            Console.Clear();
            Console.ResetColor();

            // mini menu //
            string escapeTo = "program"; // main menu => exit program. mini menu (choosing en/decryption) => exit menu
            if (miniMenu)
                escapeTo = "menu";

            // loop until option is selected (enter key or escape key pressed) //
            do
            {

                // display menu name and instruction text //
                TitleInfo(menuName, false,
                    "Arrow keys and enter to choose",
                    "Escape key to exit " + escapeTo);

                // write each option, encasing in coloured -> and <- if selected //
                for (int i = 0; i < optionsAmount; i++) // i is option being displayed
                {
                    Console.SetCursorPosition(startX, startY_Options + i); // display options at startX, downwards from startY_Options

                    if (i == currentSelection) // format currently chosen option
                    {
                        // add first arrow //
                        Console.ForegroundColor = MenuColour;
                        Console.Write(" ->");

                        // move cursor to after option, add next arrow //
                        Console.SetCursorPosition(startX + options[i].Length + 3, startY_Options + i);
                        Console.Write("<-");
                        Console.SetCursorPosition(startX + 3, startY_Options + i);
                        Console.ResetColor();
                    }

                    Console.Write(options[i]); // display option
                }

                // wait for input, reads key //
                key = Console.ReadKey(true).Key;
                switch (key)
                {
                    case ConsoleKey.C:
                        ColourGen(false);
                        break;

                    case ConsoleKey.UpArrow:
                        // check if there is an option above, moves selection if so //
                        if (currentSelection > 0)
                            currentSelection--;
                        break;

                    case ConsoleKey.DownArrow:
                        // check if there is an option below, moves selection if so //
                        if (currentSelection < optionsAmount - 1)
                            currentSelection++;
                        break;

                    case ConsoleKey.Escape:
                        // exit menu system //
                        return -1;
                }
            } while (key != ConsoleKey.Enter);

            Console.Clear();
            return currentSelection;
        }
        /// <summary>
        /// Predefined selection of menus. 0 for Main, 1 for Caesar Select.
        /// </summary>
        /// <param name="type">Main = 0 ; Caesar = 1.</param>
        /// <returns>Index of selected option. -1 returned if incorrect parameters.</returns>
        public static int Menu(int type) => type switch
        {
            // 0 displays the Main Menu, with each option as a mini app //
            0 => MultipleChoice(
                "Mini Apps", false,
                "Keep Counting", "Square Root Calculator", "Caesar Cipher"),

            // 1 displays the menu for selecting encryption or decryption for Caesar cipher //
            1 => MultipleChoice(
                "Caesar Cipher", true,
                "Encryption", "Decryption"),

            // returns -1 with incorrect parameters //
            _ => -1
        };

        // other displaying //

        /// <summary>
        /// Display highlighted title and following info text in a colour randomised upon startup.
        /// </summary>
        /// <param name="title">The title. Background will be highlighted.</param>
        /// <param name="wait">If program should wait for user input before continuing.</param>
        /// <param name="info">The information below the title. Foreground highlighted.</param>
        public static void TitleInfo(string title, bool wait = false, params string[] info)
        {
            // display title //
            Console.Clear();
            Console.BackgroundColor = MenuColour;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.SetCursorPosition(startX, startY_Title);
            Console.WriteLine($" {title} "); // space added so text looks nicer [highlighted background can make text look cramped otherwise]

            // prepare for displaying info text //
            Console.ResetColor();
            Console.ForegroundColor = MenuColour;

            // display info text line by line //
            for (int i = 0; i < info.Length; i++)
            {
                Console.SetCursorPosition(startX, startY_Title + 2 + i); // line between first line and title [+2]
                Console.Write(info[i]);
            }
            Console.ResetColor();

            // if wait is true, display following text and wait for input //
            if (wait)
            {
                Console.SetCursorPosition(startX, startY_Title + 3 + info.Length);
                Console.Write("Press any key to continue...");
                Console.ReadKey();
            }
        }
        

        /// <summary>
        /// Display question, asking for either integer or alphanumeric input depending on question type.
        /// </summary>
        /// <param name="y_Offset">Offset from where options are displayed.</param>
        /// <param name="questionOrNum">Question to be asked [or question number in math type].</param>
        /// <param name="isMathType">If true, asks for integer input, otherwise alphanumeric.</param>
        /// <param name="mathQ">The maths question to be displayed ["0 + 1 = "]</param>
        /// <param name="range">The range for possible input in a maths question.</param>
        /// <returns>The user's input.</returns>
        public static int[] Question(int y_Offset, string questionOrNum, bool isMathType = false, string mathQ = null, int range = 0)
        {
            string userInput;                

            int 
                // cursor positions //
                x_input = startX + 2, // position for cursor when acquiring input
                y_input = startY_Options + y_Offset + 1,

                x_invalText = startX + 2, // position for cursor when displaying invalid input message
                y_invalText = startY_Options + y_Offset + 1;

            int[] answer = { -1 }; // -> inefficient to use array if only storing one integer, will need to fix

            // display question
            Console.SetCursorPosition(startX, startY_Options + y_Offset); // cursor placed at specified start position

            // question //
            Console.ForegroundColor = MenuColour;
            Console.Write(questionOrNum + ":");
            Console.ResetColor();

            bool validInput;

            if (isMathType)
            {
                do
                {
                    validInput = true;

                    // extra question text //
                    Console.SetCursorPosition(x_input, y_input);
                    if (mathQ != null)
                        Console.Write(mathQ + " = "); // for arithmetic questions

                    // make cursor visible, take user input //
                    Console.CursorVisible = true;
                    if (mathQ != null)
                        Console.ForegroundColor = MenuColour;
                    userInput = Console.ReadLine();
                    Console.CursorVisible = false;
                    Console.ResetColor();

                    // input validity check //
                    try
                    { answer[0] = Convert.ToInt32(userInput); } // try to convert to integer
                    catch
                    { validInput = false; } // alter validInput if not possible

                    if (validInput && range != 0)
                    {
                        if (Convert.ToInt32(userInput) > range || Convert.ToInt32(userInput) <= 0)
                            validInput = false;
                    }
                    if (validInput)
                        if (Convert.ToInt32(userInput) <= 0)
                            validInput = false;

                    // displays then removes invalid input message after key press //
                    if (!validInput)
                        InvalNotice(x_invalText, y_invalText, userInput, range);

                } while (!validInput);
                return answer;
            }
            else
            {
                int charCount;

                // add all possible ascii codes to list
                List<int> possibleAscii = new List<int> { 32 }; // space
                for (int i = 48; i <= 57; i++) // uppercase alphabet
                    possibleAscii.Add(i);
                for (int i = 65; i <= 90; i++) // numerics
                    possibleAscii.Add(i);

                char[] chars;
                int[] inputAscii;

                // display question, check validity of user input //
                do
                {
                    validInput = false;
                    // make cursor visible, take user input //
                    Console.SetCursorPosition(x_input, y_input);
                    Console.CursorVisible = true;
                    userInput = Console.ReadLine().ToUpper();
                    Console.CursorVisible = false;

                    // input validity check //

                    // add user input to array as ascii codes //
                    chars = userInput.ToCharArray();
                    inputAscii = new int[chars.Length];
                    charCount = chars.Length;
                    for (int x = 0; x < charCount; x++)
                        inputAscii[x] = (int)chars[x];


                    for (int c = 0; c < inputAscii.Length; c++)
                    {
                        validInput = false; // is invalid until proven otherwise
                        for (int i = 0; i < possibleAscii.Count; i++)
                        {
                            if (inputAscii[c] == possibleAscii[i])
                            {
                                validInput = true; // once character found in array, it is found to be valid. add shift
                                break;
                            }
                        }
                        if (!validInput) // if character not in array, break, show error
                            break;
                    }

                    // displays then removes invalid input message after key press //
                    if (!validInput)
                    {
                        InvalNotice(x_invalText, y_invalText, userInput, -3);
                    }
                } while (!validInput);
                return inputAscii;
            }
        }


        /// <summary>
        /// Display highlighted feedback message dependant on answer.
        /// </summary>
        /// <param name="y">The y coord to set cursor position.</param>
        /// <param name="answer">The answer to display if wrong input [-1 => correct input].</param>
        public static void Feedback(int y, int answer = -1)
        {
            Console.SetCursorPosition(startX + 2, startY_Options + y + 2);
            Console.BackgroundColor = MenuColour;
            Console.ForegroundColor = ConsoleColor.Black;

            Console.Write(answer switch
            {
                -1 => " Correct answer ",
                _ => " Incorrect answer. Correct answer is " + answer + " "
            });

            Console.ResetColor();
        }

        /// <summary>
        /// Display a notice to state there was invalid input from the user.
        /// </summary>
        /// <param name="x">How far to display from the left of the window.</param>
        /// <param name="y">How far to display from the top of the window.</param>
        /// <param name="input">The input from the user.</param>
        /// <param name="inputType">The valid range from 1 or: 0 => integer, -1 => alphanumerics.</param>
        private static void InvalNotice(int x, int y, string input, int inputType = 0)
        {
            string 
                invalNotice = "Incorrect input. Please input",
                invalContinue = "Press any key to continue...";

            invalNotice += (inputType) switch
            {
                0 => " an integer",
                -1 => " only alphanumerics and spaces",
                _ => $" between 1 and {inputType}"
            };

            invalNotice += '.';

            // remove any incorrect input from the user //
            Console.SetCursorPosition(x, y);
            for (int i = 0; i < 11 + input.Length; i++) // adds 11 to input length, as this is the max length of string displayed before [01_+_23_=__]
                Console.Write(" "); // replace with spaces

            // display invalid input message //
            Console.SetCursorPosition(x, y);
            Console.Write(invalNotice);
            Console.SetCursorPosition(x, y + 2);
            Console.Write(invalContinue);

            // wait for key press //
            Console.ReadKey(true);

            // remove invalid input message from console //
            Console.SetCursorPosition(x, y);
            for (int i = 0; i < invalNotice.Length; i++)
                Console.Write(" ");
            Console.SetCursorPosition(x, y + 2);
            for (int i = 0; i < invalContinue.Length; i++)
                Console.Write(" ");
        }
        //
    }
    
}