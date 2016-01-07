using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Connect4Bot
{
    class Connect4Bot
    {
        private double gamma = 0.98;
        private int botId = 0;
        private int round = 0;

        static void Main(string[] args)
        {
            Connect4Parser parser = new Connect4Parser(new Connect4Bot());
            parser.Run();
        }

        public void SetBotId (int id)
        {
            botId = id;
        }

        public void SetRound (int i)
        {
            round = i;
        }

        public void DisplayScore(double s, int m)
        {
            Console.WriteLine("Move " + m + " has a score of " + s);

        }

        public int MakeTurn(Connect4Board b)
        {
            int depth = 5;
            if (round >= 25) depth = 7;
            else if (round >= 32) depth = 100;
            double max = -10000.0;
            int bestMoveSoFar = 0;
            for (int i = 0; i < b.GetNrColumns(); i++)
            {
                if (b.IsValidMove(i))
                {
                    Connect4Board testBoard = new Connect4Board(b);
                    testBoard.AddDisc(i, botId);
                    double moveScore = AlphaBeta(testBoard, depth, -100000.0, 100000.0, false);
                    //DisplayScore(moveScore, i);
                    if (moveScore > max)
                    {
                        max = moveScore;
                        bestMoveSoFar = i;
                    }
                }
            }
            //b.DisplayBoard();
            return bestMoveSoFar;
        }

        public double Score(Connect4Board b)
        {
            if (CheckForDraw(b)) return 0.0;
            else return EvalBoardPosition(b);
        }

        public double AlphaBeta(Connect4Board oldBoard, int d, double alpha, double beta, bool player)
        {
            if (d == 0 || (CheckForWin(oldBoard) != 0 || CheckForDraw(oldBoard)))
            {
                return Score(oldBoard);
            }
            if (player)
            {
                double val = -100000.0;
                for (int i = 0; i < oldBoard.GetNrColumns(); i++)
                {
                    if (oldBoard.IsValidMove(i))
                    {
                        Connect4Board newBoard = new Connect4Board(oldBoard);
                        newBoard.AddDisc(i, botId);
                        val = Math.Max(val, gamma * AlphaBeta(newBoard, d-1, alpha, beta, !player));
                        alpha = Math.Max(alpha, val);
                        if (beta <= alpha) break;
                    }
                }
                return alpha;
            }
            else
            {
                double val = 100000.0;
                for (int i = 0; i < oldBoard.GetNrColumns(); i++)
                {
                    if (oldBoard.IsValidMove(i))
                    {
                        Connect4Board newBoard = new Connect4Board(oldBoard);
                        newBoard.AddDisc(i, (3- botId));
                        val = Math.Min(val, gamma * AlphaBeta(newBoard, d-1, alpha, beta, !player));
                        beta = Math.Min(val, beta);
                        if (beta <= alpha) break;
                    }
                }
                return beta;
            }
        }

        public double EvalSingle(Connect4Board b, int i, int j, int iplus, int jplus, int player)
        {
            int count = 0;
            bool opp = false;
            for (int x = 0; x < 4; x++)
            {
                int piece = b.GetDisc(i + iplus * x, j + jplus * x);
                if (piece == 3 - player) return 0.0;
                else if (piece == player) count++;
            }

            if (count == 2) return 5.0;
            else if (count == 3) return 25.0 + IsGoodTriple(b, i, j, iplus, jplus, player);
            else if (count == 4) return 2500.0;
            else return 0.0;
        }

        public double IsGoodTriple(Connect4Board b, int i, int j, int iplus, int jplus, int player)
        {
            if (iplus == 0) return 0.0; // if vertical, can't be good triple
            int maxj = b.GetNrRows() - 1;
            if (j == maxj) return 0.0; // if bottom line can't be good triple

            int gap = -1;
            for (int x = 0; x < 4; x++)
            {
                int piece = b.GetDisc(i + iplus * x, j + jplus * x);
                if (piece == 3 - player) return 0.0; // make sure there's a gap and not another player
                if (piece == 0) gap = x; // get position of gap piece
            }

            if (gap == -1) return 0.0; // making sure nothing went wrong

            if (j + jplus * gap + 1 <= maxj && b.GetDisc(i + iplus * gap, j + jplus * gap + 1) == 0) // if piece below is within boundaries and is missing 
            {
                if (j + jplus * gap + 2 <= maxj && b.GetDisc(i + iplus * gap, j + jplus * gap + 2) == 0) return 100.0; // if piece below that is also within boundaries and is missing, return good score
                else return 200.0; // else return great score
            }
            return 0.0; // return nothing otherwise
        }

        public double EvalBoardPosition(Connect4Board b)
        {
            double accumScore = 0.0;

            //Vertical
            for (int i = 0; i < b.GetNrColumns(); i++)
            {
                for (int j = 0; j < b.GetNrRows()-3; j++)
                {
                    accumScore += EvalSingle(b, i, j, 0, 1, botId);
                    accumScore -= EvalSingle(b, i, j, 0, 1, 3 - botId);
                }
            }

            //Horizontal
            for (int j = 0; j < b.GetNrRows(); j++)
            {
                for (int i = 0; i < b.GetNrColumns()-3; i++)
                {
                    accumScore += EvalSingle(b, i, j, 1, 0, botId);
                    accumScore -= EvalSingle(b, i, j, 1, 0, 3 - botId);
                }
            }

            for (int i = 0; i < b.GetNrColumns()-3; i++)
            {
                for (int j = 0; j < b.GetNrRows()-3; j++)
                {
                    accumScore += EvalSingle(b, i, j, 1, 1, botId);
                    accumScore -= EvalSingle(b, i, j, 1, 1, 3 - botId);
                    accumScore += EvalSingle(b, i + 3, j, -1, 1, botId);
                    accumScore -= EvalSingle(b, i + 3, j, -1, 1, 3 - botId);
                }
            }

            return accumScore;
        }

        public bool CheckForDraw(Connect4Board b)
        {
            for (int i = 0; i < b.GetNrColumns(); i++)
            {
                if (b.IsValidMove(i)) return false;
            }
            return true;
        }

        public int CheckForWin(Connect4Board b)
        {
            int piece;
            //Checking vertically
            for (int i = 0; i < b.GetNrColumns(); i++)
            {
                for (int j = 0; j < b.GetNrRows()-3; j++)
                {
                    if ((piece = b.GetDisc(i, j)) != 0)
                    {
                        if (piece == b.GetDisc(i, j+1))
                        {
                            if (piece == b.GetDisc(i, j+2))
                            {
                                if (piece == b.GetDisc(i, j+3)) return piece;

                                else j += 2;
                            }
                            else j += 1;
                        }
                    }
                }
            }
            //Checking horizontally
            for (int j = 0; j < b.GetNrRows(); j++)
            {
                for (int i = 0; i < b.GetNrColumns()-3; i++)
                {
                    if ((piece = b.GetDisc(i, j)) != 0)
                    {
                        if (piece == b.GetDisc(i+1, j))
                        {
                            if (piece == b.GetDisc(i+2, j))
                            {
                                if (piece == b.GetDisc(i+3, j)) return piece;

                                else i += 2;
                            }
                            else i += 1;
                        }
                    }
                }
            }

            //Checking diagonally
            for (int i = 0; i < b.GetNrColumns()-3; i++)
            {
                for (int j = 0; j < b.GetNrRows()-3; j++)
                {
                    if ((piece = b.GetDisc(i, j)) != 0)
                    {
                        if (piece == b.GetDisc(i + 1, j + 1) && piece == b.GetDisc(i + 2, j + 2) && piece == b.GetDisc(i + 3, j + 3)) return piece;
                    }
                    if ((piece = b.GetDisc(i+3, j)) != 0)
                    {
                        if (piece == b.GetDisc(i + 2, j + 1) && piece == b.GetDisc(i + 1, j + 2) && piece == b.GetDisc(i, j + 3)) return piece;
                    }
                }
            }
            return 0;
        }
    }

    class Connect4Board
    {
        private int[,] board;
        private int cols = 0, rows = 0;
        private String lastError = "";
        public int lastColumn = 0;

        public Connect4Board(int c, int r)
        {
            board = new int[c, r];
            cols = c;
            rows = r;
            ClearBoard();
        }

        public Connect4Board(Connect4Board oldBoard)
        {
            cols = oldBoard.GetNrColumns();
            rows = oldBoard.GetNrRows();
            board = new int[cols, rows];
            for (int i = 0; i < cols; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    board[i, j] = oldBoard.GetDisc(i, j);
                }
            }
        }

        public void SetColumns(int c)
        {
            cols = c;
            board = new int[cols, rows];
        }

        public void SetRows(int r)
        {
            rows = r;
            board = new int[cols, rows];
        }

        public void ClearBoard()
        {
            for (int i = 0; i < cols; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    board[i, j] = 0;
                }
            }
        }

        public bool AddDisc(int c, int player)
        {
            lastError = "";
            if (c < cols)
            {
                for (int i = rows-1; i >= 0; i--)
                {
                    if (board[c, i] == 0)
                    {
                        board[c, i] = player;
                        lastColumn = c;
                        return true;
                    }
                }
                lastError = "Column is full.";
            }
            else
            {
                lastError = "Move out of bounds.";
            }
            return false;
        }

        public void ParseFromString(String s)
        {
            s = s.Replace(';', ',');
            String[] r = s.Split(',');
            int counter = 0;

            for (int j = 0; j < rows; j++)
            {
                for (int i = 0; i < cols; i++)
                {
                    Int32.TryParse(r[counter], out board[i,j]);
                    counter++;
                }
            }
        }

        public void DisplayBoard()
        {
            for (int j = 0; j < rows; j++)
            {
                for (int i = 0; i < cols; i++)
                {
                    if (board[i, j] == 1) Console.Write("1");
                    else if (board[i, j] == 2) Console.Write("2");
                    else Console.Write(".");
                }
                Console.WriteLine();
            }
        }

        public int GetDisc(int c, int r)
        {
            return board[c, r];
        }

        public bool IsValidMove(int c)
        {
            return (board[c,0] == 0);
        }

        public String GetLastError()
        {
            return lastError;
        }

        public override String ToString()
        {
            String r = "";
            int counter = 0;
            for (int j = 0; j < rows; j++)
            {
                for (int i = 0; i < cols; i++)
                {
                    if (counter > 0) r += ",";
                    r += board[i, j];
                    counter++;
                }
            }
            return r;
        }

        public bool IsFull(int c)
        {
            return (board[c, 0] != 0);
        }

        public int GetNrColumns()
        {
            return cols;
        }

        public int GetNrRows()
        {
            return rows;
        }
    }

    class Connect4Parser
    {

        readonly Connect4Bot bot;
        readonly TextReader input;

        private Connect4Board board;
        public static int connect4BotId = 0;

        public Connect4Parser(Connect4Bot bot)
        {
            input = new StreamReader(Console.OpenStandardInput());
            this.bot = bot;
        }

        public void Run()
        {
            board = new Connect4Board(0, 0);
            String line = input.ReadLine();
            while (line != null)
            {
                String[] parts = line.Split(' ');

                if (parts[0].Equals("settings"))
                {
                    if (parts[1].Equals("field_columns"))
                    {
                        board.SetColumns(Int32.Parse(parts[2]));
                    }
                    if (parts[1].Equals("field_rows"))
                    {
                        board.SetRows(Int32.Parse(parts[2]));
                    }
                    if (parts[1].Equals("your_botid"))
                    {
                        connect4BotId = Int32.Parse(parts[2]);
                        bot.SetBotId(connect4BotId);
                    }
                }
                else if (parts[0].Equals("update"))
                {
                    if (parts[2].Equals("field"))
                    {
                        String data = parts[3];
                        board.ParseFromString(data); 
                    }
                    if (parts[2].Equals("round"))
                    {
                        bot.SetRound(Int32.Parse(parts[3]));
                    }
                }
                else if (parts[0].Equals("action"))
                {
                    if (parts[1].Equals("move"))
                    { /* move requested */
                        int column = bot.MakeTurn(board);
                        Console.WriteLine("place_disc " + column);
                    }
                }
                else {
                    Console.WriteLine("unknown command");
                }
                line = input.ReadLine();
            }
        }
    }
}
