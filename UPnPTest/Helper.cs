using System;
using System.Collections.Generic;

//// = documentation
// = per-step working comments


namespace TileBasedSurvivalGame {
    //// Helper class for console interaction
    static class Helper {
        public static bool Ask(string yesNoQuestion, bool defaultYes) {
            // ask the question
            Console.Write($"{yesNoQuestion} {(defaultYes ? "[Y/n]" : "[y/N]")}\n> ");
            // get the answer, normalize the answer
            string answer = Console.ReadLine().TrimStart(' ').ToLower();

            // return true if the answer starts with y
            if (answer.StartsWith("y")) {
                return true;
            }
            // return false if the answer starts with n
            else if (answer.StartsWith("n")) {
                return false;
            }
            // otherwise, use the default
            if (defaultYes) {
                return true;
            }
            return false;
        }
        public static int Ask(string numericalAnswerQuestion, int defaultAnswer) {
            // ask the question
            Console.Write($"{numericalAnswerQuestion} [{defaultAnswer}]\n> ");
            // get the answer, remove any spaces
            string answerString = Console.ReadLine().TrimStart(' ').TrimEnd(' ');

            // return the default if nothing is entered
            if (answerString.Length == 0) {
                return defaultAnswer;
            }

            // attempt to parse the answer to an integer, ask again if it can't be parsed
            if (int.TryParse(answerString, out int numericalAnswer)) {
                return numericalAnswer;
            }
            else {
                Console.WriteLine("Invalid integer, please try again.");
                return Ask(numericalAnswerQuestion, defaultAnswer);
            }
        }

        #region WaitingActions
        //// action list
        static List<Action> _waitingActions = new List<Action>();

        //// do all waiting actions
        public static void DoAllWaitingActions() {
            foreach (Action action in _waitingActions) {
                action();
            }

            _waitingActions.Clear();
        }

        //// add an action to be executed with all other waiting methods
        //// .. when "DoAllWaitingActions" is called
        public static void AddToWaitingActions(Action action) {
            _waitingActions.Add(action);
        }

        #endregion WaitingActions
    }
}
