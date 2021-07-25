using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chess2
{
    class ChessGameLauncher
    {
        static void Main(string[] args)
        {
            new ChessGame().PlayGame();
        }
    }

    class ChessGame
    {
        public void PlayGame()
        {
            int thereWasNoPieceCaptureOrAPawnMovedCount = 0;
            int[] input = new int[4],
                  enPassantLocation = new int[2] { 9, 9 };
            bool whiteTurn = true;
            bool[] castlingParticipants = new bool[6];
            string message = "nothing's wrong",
                   inputArchive = "",
                   history = "";
            Piece[] chessPieces = chessPieces = new Piece[33];
            chessPieces = InitiatePieceArray(chessPieces, whiteTurn);
            PrintBoard(chessPieces);
            while (message == "nothing's wrong" || message == "Check on white King" || message == "Check on black King")
            {
                input = GetAValidAMove(whiteTurn, (whiteTurn ? "White" : "Black") + Instructions(), chessPieces, enPassantLocation, castlingParticipants);//קבלת קלט מהמהשתמש, בדיקת תקינות הקלט ובדיקת חוקיות המהלך
                castlingParticipants = CastlingParticipantsUpdate(castlingParticipants, input);//עדכון תנועה ראשונה של המשתתפים בהצרכה
                inputArchive += ProcessInputToString(input);//תיעוד מהלכי המשחק לטובת הכאת רגלי דרך הילוכו
                EnPassant(enPassantLocation, chessPieces, inputArchive, input);//פעולות מיוחדות למקרה של הכאה דרך הילוכו
                thereWasNoPieceCaptureOrAPawnMovedCount = (chessPieces[GetPieceIndex(chessPieces, input[1], input[0])].ToString()[1] == 'P' || //ההסבר מופיע באנגלית בשם המשתנה
                                                           chessPieces[GetPieceIndex(chessPieces, input[3], input[2])].ToString() != "EE ") ?
                                                           0 : thereWasNoPieceCaptureOrAPawnMovedCount + 1;
                UpdateBoard(input, chessPieces, whiteTurn);//עדכון והדפסת הלוח לאחר המהלך
                history += (whiteTurn ? "w" : "b") + ProcessBoardToString(chessPieces, castlingParticipants, enPassantLocation);//תיעוד ההעמדה האחרונה של הלוח בהיסטורית העמדות הלוח, לטובת תיקו אפשרי
                whiteTurn = whiteTurn ? false : true;//מעבר תור
                message = GetAfterMoveMessage(whiteTurn, chessPieces, enPassantLocation, castlingParticipants, thereWasNoPieceCaptureOrAPawnMovedCount, history);//הדפסת הודעה מיוחדת: שח, שחמט או תיקו
                if (message != "nothing's wrong")
                    Console.WriteLine(message + "\n");
            }
        }

        string Instructions()
        {
            return "'s turn:\n" +
                   "Please enter the origin and the destination and press ENTER.\n" +
                   "Please write four signs only, in the following order:\n" +
                   "Upper or lower case letter of the origin, and after the letter the number of the origin;\n" +
                   "Upper or lower case letter of the destination, and finally the number of the destination.\n";
        }

        Piece[] InitiatePieceArray(Piece[] chessPieces, bool whiteTurn)
        {            
            for (int i = 0; i < 17; i += 16)
            {
                chessPieces[0 + i] = new Rook(whiteTurn ? 7 : 0, 0, whiteTurn);
                chessPieces[1 + i] = new Knight(whiteTurn ? 7 : 0, 1, whiteTurn);
                chessPieces[2 + i] = new Bishop(whiteTurn ? 7 : 0, 2, whiteTurn);
                chessPieces[3 + i] = new Queen(whiteTurn ? 7 : 0, 3, whiteTurn);
                chessPieces[4 + i] = new King(whiteTurn ? 7 : 0, 4, whiteTurn);
                chessPieces[5 + i] = new Bishop(whiteTurn ? 7 : 0, 5, whiteTurn);
                chessPieces[6 + i] = new Knight(whiteTurn ? 7 : 0, 6, whiteTurn);
                chessPieces[7 + i] = new Rook(whiteTurn ? 7 : 0, 7, whiteTurn);
                for (int j = 8; j < 16; j++)
                    chessPieces[i + j] = new Pawn(whiteTurn ? 6 : 1, j - 8, whiteTurn);
                whiteTurn = whiteTurn ? false : true;
            }
            chessPieces[32] = new Piece("EE ");
            return chessPieces;
        }

        int[] GetAValidAMove(bool whiteTurn, string instructions, Piece[] chessPieces, int[] enPassantLocation, bool[] castlingParticipants)
        {
            string whatWasWrong = "",
                   spacelessInput = "";
            int[] positions = new int[4];
            while (whatWasWrong != "nothing's wrong")
            {
                positions = new int[4];
                whatWasWrong = "nothing's wrong";
                spacelessInput = GetSpacelessInput(instructions);//קבלת הקלט
                whatWasWrong = GetLegalInputLengthMessage(spacelessInput);
                if (whatWasWrong == "nothing's wrong")//בדיקת תקינות התוים בקלט
                    whatWasWrong = GetLegalInputCharsMessage(spacelessInput);
                if (whatWasWrong == "nothing's wrong")//המרת הקלט למערך מספרים
                    positions = TurnStringInputToInt(spacelessInput);
                if (whatWasWrong == "nothing's wrong")//בדיקת תקינות התנועה
                    whatWasWrong = GetLegalMoveMessage(whiteTurn, positions, chessPieces, enPassantLocation, castlingParticipants);
                if (whatWasWrong == "nothing's wrong")//ווידוא שהמהלך לא מפקיר את המלך לשח
                    whatWasWrong = GetKingSafeMessage(whiteTurn, positions, chessPieces, enPassantLocation, castlingParticipants);
                if (whatWasWrong != "nothing's wrong")//הדפסת השגיאה
                    Console.WriteLine(whatWasWrong + ", please try again.\n");
            }
            return positions;
        }

        string GetSpacelessInput(string instructions)
        {
            string originalinput = "",                   
                   spaceLessInput = "";
            Console.WriteLine(instructions);
            originalinput = Console.ReadLine();
            originalinput = originalinput.Trim();//התעלמות מהרווחים בתחילת וסוף הקלט
            originalinput = originalinput.ToLower();
            for (int i = 0; i < originalinput.Length; i++)//דילוג על רווחים באמצע הקלט
                if (originalinput[i] != ' ')
                    spaceLessInput += originalinput[i];
            return spaceLessInput;
        }

        string GetLegalInputLengthMessage(string spaceLessInput)
        {
            string whatWasWrong = "nothing's wrong";
            if (spaceLessInput.Length > 4)//ווידוא אורך מחרוזת תקין
                    whatWasWrong = "You entered too many characters";
                if (spaceLessInput.Length < 4)
                    whatWasWrong = "You didnt enter enough characters";
            return whatWasWrong;
        }

        string GetLegalInputCharsMessage(string spaceLessInput)
        {
            string whatWasWrong = "nothing's wrong";
            if (!(IsLegalLetter(spaceLessInput[0]) && IsLegalNumber(spaceLessInput[1])))
                whatWasWrong = "You entered an illegal location for the origin";
            else if (!(IsLegalLetter(spaceLessInput[2]) && IsLegalNumber(spaceLessInput[3])))
                whatWasWrong = "You entered an illegal location for the destination";
            return whatWasWrong;
        }

        bool IsLegalLetter(char letter)
        {
            string legalLetters = "abcdefgh";
            bool legal = false;
            for (int i = 0; i < legalLetters.Length && !legal; i++)
                if (letter == legalLetters[i])
                    legal = true;
            return legal;
        }

        bool IsLegalNumber(char letter)
        {
            string legalNumbers = "12345678";
            bool legal = false;
            for (int i = 0; i < legalNumbers.Length && !legal; i++)
                if (letter == legalNumbers[i])
                    legal = true;
            return legal;
        }

        int[] TurnStringInputToInt(string input)
        {
            string letters = "abcdefgh";
            int[] result = new int[4];
            for (int i = 0; i < letters.Length; i++)
            {
                if (input[0] == letters[i])
                    result[0] = letters.IndexOf(letters[i]);
                if (input[2] == letters[i])
                    result[2] = letters.IndexOf(letters[i]);
            }
            result[1] = int.Parse("" + input[1]) - 1;
            result[3] = int.Parse("" + input[3]) - 1;
            return result;
        }

        public string GetLegalMoveMessage(bool whiteTurn, int[] input, Piece[] chessPieces, int[] enPassantLocation, bool[] castlingParticipants)
        {
            string pieceIsInTheOriginIndex = chessPieces[GetPieceIndex(chessPieces, input[1], input[0])].ToString(),
                whatIsInTheDestinationIndex = chessPieces[GetPieceIndex(chessPieces, input[3], input[2])].ToString(),
             whatWasWrong = "nothing's wrong";
            if (pieceIsInTheOriginIndex[0] == 'E')//וידוא כלי קיים בנקודת המוצא
                whatWasWrong = "The origin spot is enpty";
            else if ((whiteTurn && pieceIsInTheOriginIndex[0] == 'B') || (!whiteTurn && pieceIsInTheOriginIndex[0] == 'W'))//וידוא כלי המוצא כצבע בעל התור
                whatWasWrong = "You picked up a piece with the wrong color";
            else if (input[0] == input[2] && input[1] == input[3])//וידוא מוצא ויעד שונים
                whatWasWrong = "The origin and the destination are in the same spot";
            else if (whatWasWrong == "nothing's wrong")//כללי תנועה לכלים השונים
                whatWasWrong = chessPieces[GetPieceIndex(chessPieces, input[1], input[0])].TheWayItMoves(whatWasWrong, input, chessPieces, castlingParticipants, whiteTurn, enPassantLocation);
            if (whatWasWrong == "nothing's wrong")//ווידוא שהכלי שביעד עם צבע שונה
                if ((whiteTurn && whatIsInTheDestinationIndex[0] == 'W') || (!whiteTurn && whatIsInTheDestinationIndex[0] == 'B'))
                    whatWasWrong = "The destination already has a fellow piece, with the same color";
            return whatWasWrong;
        }

        public int GetPieceIndex(Piece[] chessPieces, int row, int column)
        {
            int index = 32;
            for (int i = 0; i < 32; i++)
                if (chessPieces[i].GetPieceLocation()[0] == row && chessPieces[i].GetPieceLocation()[1] == column)
                {
                    index = i;
                    break;
                }
            return index;
        }

        string GetKingSafeMessage(bool whiteTurn, int[] input, Piece[] chessPieces, int[] enPassantLocation, bool[] castlingParticipants)
        {
            string whatWasWrong = "nothing's wrong";
            int pieceIsInTheOriginIndex = GetPieceIndex(chessPieces, input[1], input[0]),
                whatIsInTheDestinationIndex = GetPieceIndex(chessPieces, input[3], input[2]);
            chessPieces[whatIsInTheDestinationIndex].SetPieceLocation(9, 9);
            chessPieces[pieceIsInTheOriginIndex].SetPieceLocation(input[3], input[2]);
            whatWasWrong = CheckMessage(whiteTurn, chessPieces, enPassantLocation, castlingParticipants);
            if ((whiteTurn && whatWasWrong == "Check on white King") || (!whiteTurn && whatWasWrong == "Check on black King"))
                whatWasWrong = "You made an illegal move and left your king in check";
            chessPieces[whatIsInTheDestinationIndex].SetPieceLocation(input[3], input[2]);
            chessPieces[pieceIsInTheOriginIndex].SetPieceLocation(input[1], input[0]);
            return whatWasWrong;
        }

        public string CheckMessage(bool whiteTurn, Piece[] chessPieces, int[] enPassantLocation, bool[] castlingParticipants)
        {
            string whatWasWrong = "nothing's wrong";
            int[] search = new int[4];
            for (int i = 0; i < 8 && !(chessPieces[GetPieceIndex(chessPieces, search[3], search[2])].ToString() == (whiteTurn ? "WK " : "BK ")); i++)
                for (int j = 0; j < 8; j++)//שיבוץ המלך בנקודת היעד
                    if (chessPieces[GetPieceIndex(chessPieces, i, j)].ToString() == (whiteTurn ? "WK " : "BK "))
                    {
                        search[2] = j;
                        search[3] = i;
                        break;
                    }
            for (int i = 0; i < 8 && whatWasWrong == "nothing's wrong"; i++)//סקירת הלוח, מי יכול להכות את המלך
                for (int j = 0; j < 8 && whatWasWrong == "nothing's wrong"; j++)
                {
                    search[0] = j;
                    search[1] = i;
                    whatWasWrong = GetLegalMoveMessage(!whiteTurn, search, chessPieces, enPassantLocation, castlingParticipants);
                    whatWasWrong = (whatWasWrong == "nothing's wrong") ? (whiteTurn ? "Check on white King" : "Check on black King") : "nothing's wrong";
                }
            return whatWasWrong;
        }

        public bool[] CastlingParticipantsUpdate(bool[] castlingParticipants, int[] input)
        {
            /*
            castlingParticipants[0] => right white rook first move
            castlingParticipants[1] => white king first move
            castlingParticipants[2] => left white rook first move
            castlingParticipants[3] => right black rook first move
            castlingParticipants[4] => black king first move
            castlingParticipants[5] => left black rook first move
            */
            if (!castlingParticipants[0] && (input[0] == 7 && input[1] == 7 || input[2] == 7 && input[3] == 7))
                castlingParticipants[0] = true;
            if (!castlingParticipants[1] && (input[0] == 4 && input[1] == 7))
                castlingParticipants[1] = true;
            if (!castlingParticipants[2] && (input[0] == 0 && input[1] == 7 || input[2] == 0 && input[3] == 7))
                castlingParticipants[2] = true;
            if (!castlingParticipants[3] && (input[0] == 7 && input[1] == 0 || input[2] == 7 && input[3] == 0))
                castlingParticipants[3] = true;
            if (!castlingParticipants[4] && (input[0] == 4 && input[1] == 0))
                castlingParticipants[4] = true;
            if (!castlingParticipants[5] && (input[0] == 0 && input[1] == 0 || input[2] == 0 && input[3] == 0))
                castlingParticipants[5] = true;
            return castlingParticipants;
        }

        public void EnPassant(int[] enPassantLocation, Piece[] chessPieces, string inputArchive, int[] input)
        {
            if (enPassantLocation[0] != 9)//הסרת רגלי שהוכה דרך הילוכו
            {
                CaptureEnPassant(chessPieces, inputArchive);
                enPassantLocation[0] = 9;
            }
            if (chessPieces[GetPieceIndex(chessPieces, input[1], input[0])].ToString()[1] == 'P')//פתיחת אפשרות לרגלי יריב להכות דרך הילוכו
                OpenEnPassantCaptureOption(input, enPassantLocation);
        }

        public void CaptureEnPassant(Piece[] chessPieces, string inputArchive)//הסרת רגלי שהוכה דרך הילוכו
        {
            if (inputArchive.Length > 9 && ((inputArchive[inputArchive.Length - 9] == inputArchive[inputArchive.Length - 7] + 2) ||
                                            (inputArchive[inputArchive.Length - 9] == inputArchive[inputArchive.Length - 7] - 2)))
            {
                int previousMoveFromColumn = int.Parse("" + inputArchive[inputArchive.Length - 10]),
                    previousMoveFromRow = int.Parse("" + inputArchive[inputArchive.Length - 9]),
                    previousMoveToColumn = int.Parse("" + inputArchive[inputArchive.Length - 8]),
                    previousMoveToRow = int.Parse("" + inputArchive[inputArchive.Length - 7]),
                    presentMoveFromColumn = int.Parse("" + inputArchive[inputArchive.Length - 5]),
                    presentMoveFromRow = int.Parse("" + inputArchive[inputArchive.Length - 4]),
                    presentMoveToColumn = int.Parse("" + inputArchive[inputArchive.Length - 3]),
                    presentMoveToRow = int.Parse("" + inputArchive[inputArchive.Length - 2]);
                if (chessPieces[GetPieceIndex(chessPieces, presentMoveFromRow, presentMoveFromColumn)].ToString() == "WP " && presentMoveToRow > previousMoveFromRow &&
                    presentMoveToRow < previousMoveToRow && presentMoveToColumn == previousMoveFromColumn &&
                    presentMoveToColumn == previousMoveToColumn)//בדיקה אם הפעולה האחרונה נעשתה עם רגלי, ואם הוא הונח בין המוצא והיעד של הפעולה הקודמת
                        chessPieces[GetPieceIndex(chessPieces, previousMoveToRow, previousMoveToColumn)].SetPieceLocation(9,9);//הסרת רגלי היריב מהלוח
                if (chessPieces[GetPieceIndex(chessPieces, presentMoveFromRow, presentMoveFromColumn)].ToString() == "BP " && presentMoveToRow < previousMoveFromRow &&
                    presentMoveToRow > previousMoveToRow && presentMoveToColumn == previousMoveFromColumn &&
                    presentMoveToColumn == previousMoveToColumn)
                        chessPieces[GetPieceIndex(chessPieces, previousMoveToRow, previousMoveToColumn)].SetPieceLocation(9, 9);//הסרת רגלי היריב מהלוח
            }
        }

        public int[] OpenEnPassantCaptureOption(int[] input, int[] enPassantLocation)//פתיחת אפשרות לרגלי יריב להכות דרך הילוכו
        {
            if (input[1] == 6 && input[3] == 4)//מציאת הנקודה אליה ליריב מותר להזיז את הרגלי באופן חריג
            {
                enPassantLocation[0] = input[0];
                enPassantLocation[1] = 5;
            }
            if (input[1] == 1 && input[3] == 3)
            {
                enPassantLocation[0] = input[0];
                enPassantLocation[1] = 2;
            }
            return enPassantLocation;
        }

        string ProcessInputToString(int[] input)
        {
            string inputArchive = "";
            for (int i = 0; i < 4; i++)
                inputArchive += input[i];
            inputArchive += ",";
            return inputArchive;
        }

        void UpdateBoard(int[] input, Piece[] chessPieces, bool whiteTurn)
        {
            int index = GetPieceIndex(chessPieces, input[1], input[0]);
            string pieceIsInTheOrigin = chessPieces[index].ToString();
            chessPieces[GetPieceIndex(chessPieces, input[3], input[2])].SetPieceLocation(9,9);
            chessPieces[GetPieceIndex(chessPieces, input[1], input[0])].SetPieceLocation(input[3], input[2]);
            PromotePawn(pieceIsInTheOrigin, index, input, chessPieces, whiteTurn);
            RelocateRookWhenCastling(pieceIsInTheOrigin, input, chessPieces, whiteTurn);
            PrintBoard(chessPieces);
        }

        void PromotePawn(string pieceIsInTheOrigin, int index, int[] input, Piece[] chessPieces, bool whiteTurn)
        {
            bool isLegalInput = false;
            string promote = "";
            if ((pieceIsInTheOrigin[1] == 'P' && input[3] == 0) || (pieceIsInTheOrigin[1] == 'P' && input[3] == 7))//שדרוג רגלי שהגיע לקצה הנגדי של הלוח
                while (!isLegalInput)
                {
                    isLegalInput = true;
                    Console.WriteLine("Please choose a piece to promote your pawn to.\n" +
                                        "Select a number and press ENTER:\n" +
                                        "1 for Queen\n2 for Rook\n3 for Bishop\n4 for Knight");
                    promote = Console.ReadLine();
                    promote = promote.Trim();
                    switch (promote)
                    {
                        case "1":
                            chessPieces[index] = new Queen(input[3], input[2], whiteTurn);
                            break;
                        case "2":
                            chessPieces[index] = new Rook(input[3], input[2], whiteTurn);
                            break;
                        case "3":
                            chessPieces[index] = new Bishop(input[3], input[2], whiteTurn);
                            break;
                        case "4":
                            chessPieces[index] = new Knight(input[3], input[2], whiteTurn);
                            break;
                        default:
                            isLegalInput = false;
                            Console.WriteLine("You entered an illegal input, please try again.");
                            break;
                    }
                }
        }

        void RelocateRookWhenCastling(string pieceIsInTheOrigin, int[] input, Piece[] chessPieces, bool whiteTurn)
        {
            if (pieceIsInTheOrigin[1] == 'K')
            {
                if (input[2] == input[0] + 2)//עדכון מיקום הצריח בהצרכה קטנה
                {
                    chessPieces[whiteTurn ? 7 : 23].SetPieceLocation(input[1], 5);
                }
                if (input[2] == input[0] - 2)//עדכון מיקום הצריח בהצרכה גדולה
                {
                    chessPieces[whiteTurn ? 0 : 16].SetPieceLocation(input[1], 3);
                }
            }
        }

        void PrintBoard(Piece[] chessPieces)
        {
            Console.Clear();
            string result = "";
            result = "  A  B  C  D  E  F  G  H\n";
            for (int row = 0; row < 8; row++)
            {
                result += row + 1 + " ";
                for (int col = 0; col < 8; col++)
                    result += chessPieces[GetPieceIndex(chessPieces, row, col)].ToString() != "EE "? chessPieces[GetPieceIndex(chessPieces, row, col)].ToString() : "EE ";
                result += "\n";
            }
            Console.WriteLine(result);
        }

        string ProcessBoardToString(Piece[] chessPieces, bool[] castlingParticipants, int[] enPassantLocation)
        {
            string history = "";
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                    history += chessPieces[GetPieceIndex(chessPieces, i, j)].ToString();
            for (int c = 0; c < 4; c++)
                history += CastlingPossibilitiesList(castlingParticipants)[c];
            for (int e = 0; e < 2; e++)
                history += enPassantLocation[e];
            return history;
        }

        char[] CastlingPossibilitiesList(bool[] castlingParticipants)
        {
            char[] castlingCapability = new char[4];
            /*
            castlingCapability[0] = small white castling
            castlingCapability[1] = big white castling
            castlingCapability[2] = small black castling
            castlingCapability[3] = big black castling
            */
            castlingCapability[0] = (castlingParticipants[0] && castlingParticipants[1]) ? 'f' : 't';
            castlingCapability[1] = (castlingParticipants[2] && castlingParticipants[1]) ? 'f' : 't';
            castlingCapability[2] = (castlingParticipants[3] && castlingParticipants[4]) ? 'f' : 't';
            castlingCapability[3] = (castlingParticipants[5] && castlingParticipants[4]) ? 'f' : 't';
            return castlingCapability;
        }

        string GetAfterMoveMessage(bool whiteTurn, Piece[] chessPieces, int[] enPassantLocation, bool[] castlingParticipants, int thereWasNoPieceCaptureOrAPawnMovedCount, string history)
        {
            int countIdenticalSituations = HistoryComparison(whiteTurn, chessPieces, history);
            string announcement = CheckMessage(whiteTurn, chessPieces, enPassantLocation, castlingParticipants);//בדיקת שח
            bool isCheckmate = IsCheckmate(whiteTurn, chessPieces, enPassantLocation, castlingParticipants);
            if (announcement != "nothing's wrong" && isCheckmate)//תנאי להכרזת שחמט
                announcement = announcement != "Check on white King" ? "Checkmate.\nWhite wins!" : "Checkmate.\nBlack wins!";
            else if (announcement == "nothing's wrong" && isCheckmate)//תנאי להכרזת תיקו מסוג פט
                announcement = "The game is drawn, it ended up in stalemate.";
            else if (thereWasNoPieceCaptureOrAPawnMovedCount == 50) //תנאי להכרזת תיקו מסוג 50 מסעים רצופים ללא כל הכאה או הסעת רגלי
                announcement = "The game is drawn, there was no piece capture or a pawn movement for 50 turns in a row.";
            else if (NotEnoughPiecesDraw(chessPieces))//תנאי להכרזת תיקו כאשר לא נותר לאף אחד משני השחקנים חומר מספיק כדי להנחית מט
                announcement = "The game is drawn, there are not enough pieces on the board. No one can defeat the other.";
            else if (countIdenticalSituations > 2)//תנאי להכרזת תיקו מסוג אותה העמדה מופיעה בפעם השלישית
                announcement = "The game is drawn, it repeated itself three times and not going anywhere. No one is going to defeat the other.";
            return announcement;
        }

        int HistoryComparison(bool whiteTurn, Piece[] chessPieces, string history)
        {
            bool isSame = true;
            int numOfChars = 0,
                countIdenticalSituations = 0;
            while (numOfChars < history.Length && countIdenticalSituations < 3)//ספירת מספר ההעמדות של הלוח שחוזרות על עצמן
            {
                for (int k = 0; k < 199; k++)
                {
                    isSame = (history[numOfChars + k] == history[history.Length - 199 + k]) ? true : false;
                    if (!isSame)
                        break;
                }
                if (isSame)
                    countIdenticalSituations++;
                numOfChars += 199;
            }
            return countIdenticalSituations;
        }

        bool IsCheckmate(bool whiteTurn, Piece[] chessPieces, int[] enPassantLocation, bool[] castlingParticipants)
        {
            int[] nextPossibleMove = new int[4];
            for (int fromRow = 0; fromRow < 8; fromRow++)
                for (int fromCol = 0; fromCol < 8; fromCol++)
                {
                    nextPossibleMove[0] = fromCol;
                    nextPossibleMove[1] = fromRow;
                    for (int toRow = 0; toRow < 8; toRow++)
                        for (int toCol = 0; toCol < 8; toCol++)
                        {
                            nextPossibleMove[2] = toCol;
                            nextPossibleMove[3] = toRow;
                            if (GetLegalMoveMessage(whiteTurn, nextPossibleMove, chessPieces, enPassantLocation, castlingParticipants) == "nothing's wrong")//בדיקת תקינות התנועה
                                if (GetKingSafeMessage(whiteTurn, nextPossibleMove, chessPieces, enPassantLocation, castlingParticipants) == "nothing's wrong")//ווידוא שהמהלך לא מפקיר את המלך לשח
                                    return false;
                        }
                }
            return true;
        }

        bool NotEnoughPiecesDraw(Piece[] chessPieces)
        {
            bool result = false;
            int[,] drawCases = new int[,] {  { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                             { 0, 1, 0, 0, 0, 0, 0, 0, 0, 0 },
                                             { 0, 0, 0, 0, 0, 0, 1, 0, 0, 0 },
                                             { 0, 1, 0, 0, 0, 0, 1, 0, 0, 0 },
                                             { 0, 0, 1, 0, 0, 0, 0, 0, 0, 0 },
                                             { 0, 0, 0, 0, 0, 0, 0, 1, 0, 0 },
                                             { 0, 0, 1, 0, 0, 0, 0, 1, 0, 0 },
                                             { 0, 1, 0, 0, 0, 0, 0, 1, 0, 0 },
                                             { 0, 0, 1, 0, 0, 0, 1, 0, 0, 0 }  };
            int[] piecesCount = new int[10];
            /*
            piecesCount[0] => WR
            piecesCount[1] => WB
            piecesCount[2] => WN
            piecesCount[3] => WQ
            piecesCount[4] => WP
            piecesCount[5] => BR
            piecesCount[6] => BB
            piecesCount[7] => BN
            piecesCount[8] => BQ
            piecesCount[9] => BP
            */
            for (int row = 0; row < 8; row++)
                for (int col = 0; col < 8; col++)
                {
                    char piece = chessPieces[GetPieceIndex(chessPieces, row, col)].ToString()[1];
                    switch (piece)
                    {
                        case 'R':
                            piecesCount[chessPieces[GetPieceIndex(chessPieces, row, col)].ToString()[0] == 'W' ? 0 : 5]++;
                            break;
                        case 'B':
                            piecesCount[chessPieces[GetPieceIndex(chessPieces, row, col)].ToString()[0] == 'W' ? 1 : 6]++;
                            break;
                        case 'N':
                            piecesCount[chessPieces[GetPieceIndex(chessPieces, row, col)].ToString()[0] == 'W' ? 2 : 7]++;
                            break;
                        case 'Q':
                            piecesCount[chessPieces[GetPieceIndex(chessPieces, row, col)].ToString()[0] == 'W' ? 3 : 8]++;
                            break;
                        case 'P':
                            piecesCount[chessPieces[GetPieceIndex(chessPieces, row, col)].ToString()[0] == 'W' ? 4 : 9]++;
                            break;
                    }
                }
            for (int i = 0; i < drawCases.GetLength(0) && !result; i++)
            {
                result = true;
                for (int j = 0; j < piecesCount.Length && result; j++)
                    result = (piecesCount[j] == drawCases[i, j]) ? true : false;
            }
            return result;
        }
    }
    
    class Piece : ChessGame
    {
        string name = "";
        int[] location;
        public Piece(string name)
        {
            this.name = name;
        }
        public Piece(int row, int column, bool colorIsWhite, string name)
        {
            location = new int[]{ row, column };
            this.name += (colorIsWhite ? 'W' : 'B') + name;
        }
        public override string ToString()
        {
            return name;
        }
        public int[] GetPieceLocation()
        {
            return location;
        }
        public void SetPieceLocation(int row, int column)
        {
            location = new int[] { row, column };
        }
        public virtual string TheWayItMoves(string whatWasWrong, int[] input, Piece[] chessPieces, bool[] castlingParticipants, bool whiteTurn, int[] enPassantLocation)
        {
            return whatWasWrong;
        }
    }

    class Rook : Piece
    {
        public Rook(int row, int column, bool colorIsWhite) : base(row, column, colorIsWhite, "R ") { }
        public override string TheWayItMoves(string whatWasWrong, int[] input, Piece[] chessPieces, bool[] castlingParticipants, bool whiteTurn, int[] enPassantLocation)
        {
            if (input[0] == input[2])//אם צריח זז אנכית
                whatWasWrong = RookGoesVertically(input, chessPieces, whatWasWrong);
            else if (input[1] == input[3])//אם צריח זז אופקית
                whatWasWrong = RookGoesSideways(input, chessPieces, whatWasWrong);
            else
                whatWasWrong = "That's not the way a rook moves";
            return base.TheWayItMoves(whatWasWrong, input, chessPieces, castlingParticipants, whiteTurn, enPassantLocation);
        }
        string RookGoesVertically(int[] input, Piece[] chessPieces, string whatWasWrong)
        {
            if (input[1] > input[3])//אם צריח עולה
                for (int i = input[1] - 1; i > input[3]; i--)
                    if (chessPieces[GetPieceIndex(chessPieces, i, input[0])].ToString() != "EE ")
                    {
                        whatWasWrong = "There is a piece in the way";
                        break;
                    }
            if (input[1] < input[3])//אם צריח יורד
                for (int i = input[1] + 1; i < input[3]; i++)
                    if (chessPieces[GetPieceIndex(chessPieces, i, input[0])].ToString() != "EE ")
                    {
                        whatWasWrong = "There is a piece in the way";
                        break;
                    }
            return whatWasWrong;
        }
        string RookGoesSideways(int[] input, Piece[] chessPieces, string whatWasWrong)
        {
            if (input[0] > input[2])//אם צריח זז שמאלה
                for (int i = input[0] - 1; i > input[2]; i--)
                    if (chessPieces[GetPieceIndex(chessPieces, input[1], i)].ToString() != "EE ")
                    {
                        whatWasWrong = "There is a piece in the way";
                        break;
                    }
            if (input[0] < input[2])//אם צריח זז ימינה
                for (int i = input[0] + 1; i < input[2]; i++)
                    if (chessPieces[GetPieceIndex(chessPieces, input[1], i)].ToString() != "EE ")
                    {
                        whatWasWrong = "There is a piece in the way";
                        break;
                    }
            return whatWasWrong;
        }
    }

    class Knight : Piece
    {
        public Knight(int row, int column, bool colorIsWhite) : base(row, column, colorIsWhite, "N ") { }
        public override string TheWayItMoves(string whatWasWrong, int[] input, Piece[] chessPieces, bool[] castlingParticipants, bool whiteTurn, int[] enPassantLocation)
        {
            if (input[0] + 2 == input[2] && input[1] + 1 == input[3]) ;
            else if (input[0] + 2 == input[2] && input[1] - 1 == input[3]) ;
            else if (input[0] - 2 == input[2] && input[1] + 1 == input[3]) ;
            else if (input[0] - 2 == input[2] && input[1] - 1 == input[3]) ;
            else if (input[0] + 1 == input[2] && input[1] + 2 == input[3]) ;
            else if (input[0] + 1 == input[2] && input[1] - 2 == input[3]) ;
            else if (input[0] - 1 == input[2] && input[1] + 2 == input[3]) ;
            else if (input[0] - 1 == input[2] && input[1] - 2 == input[3]) ;
            else
                whatWasWrong = "That's not the way a knight moves";
            return base.TheWayItMoves(whatWasWrong, input, chessPieces, castlingParticipants, whiteTurn, enPassantLocation);
        }
    }

    class Bishop : Piece
    {
        public Bishop(int row, int column, bool colorIsWhite) : base(row, column, colorIsWhite, "B ") { }
        public override string TheWayItMoves(string whatWasWrong, int[] input, Piece[] chessPieces, bool[] castlingParticipants, bool whiteTurn, int[] enPassantLocation)
        {
            if (input[2] - input[0] == input[3] - input[1] && input[2] > input[0] && input[3] > input[1] && whatWasWrong == "nothing's wrong")//ימינה ולמטה
                for (int i = input[0] + 1, j = input[1] + 1; i < input[2] && j < input[3] && whatWasWrong == "nothing's wrong"; i++, j++)
                    whatWasWrong = PieceInTheWayMessage(chessPieces, whatWasWrong, i, j);
            else if (input[0] - input[2] == input[3] - input[1] && input[0] > input[2] && input[3] > input[1] && whatWasWrong == "nothing's wrong")//שמאלה ולמטה
                for (int i = input[0] - 1, j = input[1] + 1; i > input[2] && j < input[3] && whatWasWrong == "nothing's wrong"; i--, j++)
                    whatWasWrong = PieceInTheWayMessage(chessPieces, whatWasWrong, i, j);
            else if (input[2] - input[0] == input[1] - input[3] && input[2] > input[0] && input[1] > input[3] && whatWasWrong == "nothing's wrong")//ימינה ולמעלה
                for (int i = input[0] + 1, j = input[1] - 1; i < input[2] && j > input[3] && whatWasWrong == "nothing's wrong"; i++, j--)
                    whatWasWrong = PieceInTheWayMessage(chessPieces, whatWasWrong, i, j);
            else if (input[0] - input[2] == input[1] - input[3] && input[0] > input[2] && input[1] > input[3] && whatWasWrong == "nothing's wrong")//שמאלה ולמעלה
                for (int i = input[0] - 1, j = input[1] - 1; i > input[2] && j > input[3] && whatWasWrong == "nothing's wrong"; i--, j--)
                    whatWasWrong = PieceInTheWayMessage(chessPieces, whatWasWrong, i, j);
            else
                whatWasWrong = "That's not the way a bishop moves";
            return base.TheWayItMoves(whatWasWrong, input, chessPieces, castlingParticipants, whiteTurn, enPassantLocation);
        }
        string PieceInTheWayMessage(Piece[] chessPieces, string whatWasWrong, int i, int j)
        {
            if (chessPieces[GetPieceIndex(chessPieces, j, i)].ToString() != "EE ")
                whatWasWrong = "There is a piece in the way";
            return whatWasWrong;
        }
    }

    class Queen : Piece
    {
        public Queen(int row, int column, bool colorIsWhite) : base(row, column, colorIsWhite, "Q ") { }
        public override string TheWayItMoves(string whatWasWrong, int[] input, Piece[] chessPieces, bool[] castlingParticipants, bool whiteTurn, int[] enPassantLocation)
        {
            Rook queenAsRook = new Rook(9,9,whiteTurn);
            Bishop queenAsBishop = new Bishop(9, 9, whiteTurn);
            if (queenAsRook.TheWayItMoves(whatWasWrong, input, chessPieces, castlingParticipants, whiteTurn, enPassantLocation) == "nothing's wrong" ||
                queenAsBishop.TheWayItMoves(whatWasWrong, input, chessPieces, castlingParticipants, whiteTurn, enPassantLocation) == "nothing's wrong") ;
            else if (queenAsRook.TheWayItMoves(whatWasWrong, input, chessPieces, castlingParticipants, whiteTurn, enPassantLocation) == "There is a piece in the way" ||
                     queenAsBishop.TheWayItMoves(whatWasWrong, input, chessPieces, castlingParticipants, whiteTurn, enPassantLocation) == "There is a piece in the way")
                whatWasWrong = "There is a piece in the way";
            else if (queenAsRook.TheWayItMoves(whatWasWrong, input, chessPieces, castlingParticipants, whiteTurn, enPassantLocation) == "That's not the way a rook moves" &&
                     queenAsBishop.TheWayItMoves(whatWasWrong, input, chessPieces, castlingParticipants, whiteTurn, enPassantLocation) == "That's not the way a bishop moves")
                whatWasWrong = "That's not the way a queen moves";
            return base.TheWayItMoves(whatWasWrong, input, chessPieces, castlingParticipants, whiteTurn, enPassantLocation);
        }
    }

    class King : Piece
    {
        public King(int row, int column, bool colorIsWhite) : base(row, column, colorIsWhite, "K ") { }
        public override string TheWayItMoves(string whatWasWrong, int[] input, Piece[] chessPieces, bool[] castlingParticipants, bool whiteTurn, int[] enPassantLocation)
        {
            string pieceIsInTheOrigin = chessPieces[GetPieceIndex(chessPieces, input[1], input[0])].ToString(),
                   whatIsInTheDestination = chessPieces[GetPieceIndex(chessPieces, input[3], input[2])].ToString();
            if (input[3] == input[1] + 1 && input[2] == input[0] + 1) ;
            else if (input[3] == input[1] + 1 && input[2] == input[0] - 1) ;
            else if (input[3] == input[1] + 1 && input[2] == input[0]) ;
            else if (input[3] == input[1] && input[2] == input[0] + 1) ;
            else if (input[3] == input[1] - 1 && input[2] == input[0] + 1) ;
            else if (input[3] == input[1] - 1 && input[2] == input[0] - 1) ;
            else if (input[3] == input[1] - 1 && input[2] == input[0]) ;
            else if (input[3] == input[1] && input[2] == input[0] - 1) ;
            else if (input[3] == input[1] && input[2] == input[0] + 2)//הצרכה קטנה
                whatWasWrong = SmallCastlingMessage(whatWasWrong, input, chessPieces, enPassantLocation, castlingParticipants, whiteTurn);
            else if (input[3] == input[1] && input[2] == input[0] - 2)//הצרכה גדולה
                whatWasWrong = BigCastlingMessage(whatWasWrong, input, chessPieces, enPassantLocation, castlingParticipants, whiteTurn);
            else
                whatWasWrong = "That's not the way a king moves";
            return base.TheWayItMoves(whatWasWrong, input, chessPieces, castlingParticipants, whiteTurn, enPassantLocation);
        }
        string SmallCastlingMessage(string whatWasWrong, int[] input, Piece[] chessPieces, int[] enPassantLocation, bool[] castlingParticipants, bool whiteTurn)
        {
            if (whiteTurn ? castlingParticipants[0] || castlingParticipants[1] : castlingParticipants[3] || castlingParticipants[4])//וידוא תנאי שהכלים טרם זזו
                whatWasWrong = "Illegal castling: it must be the first move for the king and for the rook";
            else if (chessPieces[GetPieceIndex(chessPieces, whiteTurn ? 7 : 0, 5)].ToString() == "EE " &&
                     chessPieces[GetPieceIndex(chessPieces, whiteTurn ? 7 : 0, 6)].ToString() == "EE ")//וידוא תנאי שהמעבר פנוי
                whatWasWrong = CastlingWayThreatsMessage(whiteTurn, chessPieces, input, enPassantLocation, castlingParticipants);
            else
                whatWasWrong = "Illegal castling: there is a piece in the way";
            return whatWasWrong;
        }
        string BigCastlingMessage(string whatWasWrong, int[] input, Piece[] chessPieces, int[] enPassantLocation, bool[] castlingParticipants, bool whiteTurn)
        {
            if (whiteTurn ? castlingParticipants[2] || castlingParticipants[1] : castlingParticipants[5] || castlingParticipants[4])//וידוא תנאי שהכלים טרם זזו
                whatWasWrong = "Illegal castling: it must be the first move for the king and for the rook";
            else if (chessPieces[GetPieceIndex(chessPieces, whiteTurn ? 7 : 0, 1)].ToString() == "EE " &&
                        chessPieces[GetPieceIndex(chessPieces, whiteTurn ? 7 : 0, 2)].ToString() == "EE " &&
                        chessPieces[GetPieceIndex(chessPieces, whiteTurn ? 7 : 0, 3)].ToString() == "EE ")//וידוא תנאי שהמעבר פנוי
                whatWasWrong = CastlingWayThreatsMessage(whiteTurn, chessPieces, input, enPassantLocation, castlingParticipants);
            else
                whatWasWrong = "Illegal castling: there is a piece in the way";
            return whatWasWrong;
        }
        string CastlingWayThreatsMessage(bool whiteTurn, Piece[] chessPieces, int[] input, int[] enPassantLocation, bool[] castlingParticipants)
        {
            int index = GetPieceIndex(chessPieces, input[1], input[0]);
            string whatWasWrong = "nothing's wrong";
            if (CheckMessage(whiteTurn, chessPieces, enPassantLocation, castlingParticipants) != "nothing's wrong")// וידוא תנאי שהמלך לא מאויים בנקודת המוצא
                whatWasWrong = "Illegal castling: the origin is in check";
            chessPieces[index].SetPieceLocation(input[3], input[2] > input[0] ? 5 : 3);
            if (CheckMessage(whiteTurn, chessPieces, enPassantLocation, castlingParticipants) != "nothing's wrong")//וידוא תנאי שהמעבר לא מאויים
                whatWasWrong = "Illegal castling: passing by a square that is in check";
            chessPieces[index].SetPieceLocation(input[1], input[0]);
            return whatWasWrong;
        }
    }

    class Pawn : Piece
    {
        public Pawn(int row, int column, bool colorIsWhite) : base(row, column, colorIsWhite, "P ") { }
        public override string TheWayItMoves(string whatWasWrong, int[] input, Piece[] chessPieces, bool[] castlingParticipants, bool whiteTurn, int[] enPassantLocation)
        {
            string pieceIsInTheOrigin = chessPieces[GetPieceIndex(chessPieces, input[1], input[0])].ToString(),
                   whatIsInTheDestination = chessPieces[GetPieceIndex(chessPieces, input[3], input[2])].ToString();
            if (pieceIsInTheOrigin[0] == 'W')//כללי תנועה לרגלי לבן
                whatWasWrong = TheWayWhitePawnMoves(input, chessPieces, enPassantLocation, whatWasWrong, whatIsInTheDestination);
            if (pieceIsInTheOrigin[0] == 'B')//כללי תנועה לרגלי שחור
                whatWasWrong = TheWayBlackPawnMoves(input, chessPieces, enPassantLocation, whatWasWrong, whatIsInTheDestination);
            return base.TheWayItMoves(whatWasWrong, input, chessPieces, castlingParticipants, whiteTurn, enPassantLocation);
        }
        public string TheWayWhitePawnMoves(int[] input, Piece[] chessPieces, int[] enPassantLocation, string whatWasWrong, string whatIsInTheDestination)
        {
            if (input[0] == input[2] && input[1] == 6 && input[3] == 4)//כלל מיוחד לתנועת רגלי לבן במהלך הראשון
            {
                if (chessPieces[GetPieceIndex(chessPieces, 5, input[0])].ToString() != "EE ")
                    whatWasWrong = "There is a piece in the way";
                if (whatIsInTheDestination[0] == 'B')
                    whatWasWrong = "That's not the way a pawn can capture an opponent's piece";
            }
            else if (input[3] == input[1] - 1 && input[2] == input[0])//תנועה רגילה של רגלי לבן
            {
                if (whatIsInTheDestination[0] == 'B')
                    whatWasWrong = "That's not the way a pawn can capture an opponent's piece";
            }
            else if (input[3] == input[1] - 1 && input[2] == input[0] + 1 && whatIsInTheDestination[0] != 'E') ;//איך רגלי לבן מכה
            else if (input[3] == input[1] - 1 && input[2] == input[0] - 1 && whatIsInTheDestination[0] != 'E') ;
            else if (input[3] == input[1] - 1 && input[2] == input[0] + 1 && input[2] == enPassantLocation[0] && input[3] == enPassantLocation[1]) ;//הכאת רגלי דרך הילוכו
            else if (input[3] == input[1] - 1 && input[2] == input[0] - 1 && input[2] == enPassantLocation[0] && input[3] == enPassantLocation[1]) ;
            else
                whatWasWrong = "That's not the way a white pawn moves";
            return whatWasWrong;
        }
        public string TheWayBlackPawnMoves(int[] input, Piece[] chessPieces, int[] enPassantLocation, string whatWasWrong, string whatIsInTheDestination)
        {
                if (input[1] == 1 && input[3] == 3 && input[2] == input[0])//כלל מיוחד לתנועת רגלי שחור במהלך הראשון
                {
                    if (chessPieces[GetPieceIndex(chessPieces, 2, input[0])].ToString() != "EE ")
                        whatWasWrong = "There is a piece in the way";
                    if (whatIsInTheDestination[0] == 'W')
                        whatWasWrong = "That's not the way a pawn can capture an opponent's piece";
                }
                else if (input[3] == input[1] + 1 && input[2] == input[0])//תנועה רגילה של רגלי שחור
                {
                    if (whatIsInTheDestination[0] == 'W')
                        whatWasWrong = "That's not the way a pawn can capture an opponent's piece";
                }
                else if (input[3] == input[1] + 1 && input[2] == input[0] + 1 && whatIsInTheDestination[0] != 'E') ;//איך רגלי שחור מכה
                else if (input[3] == input[1] + 1 && input[2] == input[0] - 1 && whatIsInTheDestination[0] != 'E') ;
                else if (input[3] == input[1] + 1 && input[2] == input[0] + 1 && input[2] == enPassantLocation[0] && input[3] == enPassantLocation[1]) ;//הכאת רגלי דרך הילוכו                    
                else if (input[3] == input[1] + 1 && input[2] == input[0] - 1 && input[2] == enPassantLocation[0] && input[3] == enPassantLocation[1]) ;
                else
                    whatWasWrong = "That's not the way a black pawn moves";
            return whatWasWrong;
        }
    }
}