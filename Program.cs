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
        private int depth = 5;
        private double gamma = 0.98;
        private int botId = 0;

        static void Main(string[] args)
        {
            Connect4Parser parser = new Connect4Parser(new Connect4Bot());
            parser.Run();
        }

        public void SetBotId (int id)
        {
            botId = id;
        }

        public int MakeTurn(Connect4Board b)
        {
            double max = -10000.0;
            int bestMoveSoFar = 0;
            for (int i = 0; i < b.GetNrColumns(); i++)
            {
                if (b.IsValidMove(i))
                {
                    Connect4Board testBoard = new Connect4Board(b);
                    testBoard.AddDisc(i, botId);
                    double moveScore = AlphaBeta(testBoard, depth, -10000.0, 10000.0, false);
                    if (moveScore > max)
                    {
                        max = moveScore;
                        bestMoveSoFar = i;
                    }
                }
            }
            return bestMoveSoFar;
        }

        public double Score(Connect4Board b)
        {
            if (CheckForWin(b) == botId) return 100.0;
            else if (CheckForWin(b) != 0) return -100.0;
            else if (CheckForDraw(b)) return 0;

            //evaluation function
            else
            {
                return 0;
            }
        }

        public double AlphaBeta(Connect4Board oldBoard, int depth, double alpha, double beta, bool player)
        {
            if ((depth == 0) || (CheckForWin(oldBoard) != 0 || CheckForDraw(oldBoard)))
            {
                return Score(oldBoard);
            }
            if (player)
            {
                double val = -10000.0;
                for (int i = 0; i < oldBoard.GetNrColumns(); i++)
                {
                    if (oldBoard.IsValidMove(i))
                    {
                        Connect4Board newBoard = new Connect4Board(oldBoard);
                        newBoard.AddDisc(i, botId);
                        val = Math.Max(val, gamma * AlphaBeta(newBoard, depth-1, alpha, beta, !player));
                        alpha = Math.Max(alpha, val);
                        if (beta <= alpha) break;
                    }
                }
                return alpha;
            }
            else
            {
                double val = 10000.0;
                for (int i = 0; i < oldBoard.GetNrColumns(); i++)
                {
                    if (oldBoard.IsValidMove(i))
                    {
                        Connect4Board newBoard = new Connect4Board(oldBoard);
                        newBoard.AddDisc(i, botId);
                        val = Math.Min(val, gamma * AlphaBeta(newBoard, depth-1, alpha, beta, !player));
                        beta = Math.Min(val, beta);
                        if (beta <= alpha) break;
                    }
                }
                return beta;
            }
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
                for (int i = 0; i < b.GetNrRows()-3; i++)
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
                for (int j = 0; j < b.GetNrColumns()-3; j++)
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
                if (line.Length == 0) continue;

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
            }
        }
    }
}
