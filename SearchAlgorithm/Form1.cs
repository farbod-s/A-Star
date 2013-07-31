/*
 * TODO: bayad switch case baraye algorithm haye mokhtalef bezarim [-]
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Timers;
using System.Windows.Media.Animation;
using System.Collections;

namespace SearchAlgorithm
{
    public partial class Form1 : Form
    {
        int Y_SIZE = 9;
        int X_SIZE = 6;
        int FORWARD_COST = 10;
        int DIAGONAL_COST = 14;
        Pair<int, int> HOME = new Pair<int, int>();
        Pair<int, int> GOAL = new Pair<int, int>();
        List<Pair<int, int>> OBSTACLES = new List<Pair<int, int>>(); // not used!

        public enum Status { EMPTY, FULL, START, END }

        public struct Tile
        {
            public Status status;
            public Pair<int, int> parent;
            public Pair<int , int> coordinate;
            public int distance;
            public int heuristic;
            public int finalCost;
        }

        Label[][] map;
        Tile[][] logic;
        bool startIsSet;
        bool endIsSet;
       
        public Form1()
        {
            startIsSet = false;
            endIsSet = false;

            InitializeComponent();
        }

        bool Comparator(Tile a, Tile b) { return (a.finalCost < b.finalCost); }

        private void Form1_Load(object sender, EventArgs e)
        {
            numericUpDown1.Value = Y_SIZE;
            numericUpDown2.Value = X_SIZE;

            endIsSet = false;
            startIsSet = false;

            const int spacing = 50;
            map = new Label[X_SIZE][];
            logic = new Tile[X_SIZE][];

            for (int x = 0; x < X_SIZE; x++)
            {
                map[x] = new Label[Y_SIZE];
                logic[x] = new Tile[Y_SIZE];
                for (int y = 0; y < Y_SIZE; y++)
                {
                    logic[x][y].status = Status.EMPTY;
                    logic[x][y].parent = new Pair<int, int>(-1, -1);
                    logic[x][y].coordinate = new Pair<int, int>(x, y);
                    logic[x][y].distance = 0;
                    logic[x][y].heuristic = 0;
                    logic[x][y].finalCost = 0;

                    map[x][y] = new Label();
                    map[x][y].BorderStyle = BorderStyle.FixedSingle;
                    map[x][y].AutoSize = false;
                    map[x][y].Location = new System.Drawing.Point(y * spacing + 15, x * spacing + 20);
                    map[x][y].Name = "map" + x.ToString() + "," + y.ToString();
                    map[x][y].Size = new System.Drawing.Size(spacing, spacing);
                    map[x][y].TabIndex = 0;
                    map[x][y].Text = "(" + x.ToString() + ", " + y.ToString() + ")";
                    map[x][y].MouseClick += new MouseEventHandler(CreateMapDetails);
                }
                this.Controls.AddRange(map[x]);
            }
        }

        private void CreateMapDetails(object sender, EventArgs e)
        {
            Label lbl = (Label)sender;

            if (!startIsSet && Form.ModifierKeys == Keys.Control)
            {
                startIsSet = true;
                HOME.First = Convert.ToInt32(lbl.Text[1].ToString());
                HOME.Second = Convert.ToInt32(lbl.Text[4].ToString());
                lbl.BackColor = Color.Green;
            }

            else if (!endIsSet && Form.ModifierKeys == Keys.Shift)
            {
                endIsSet = true;
                GOAL.First = Convert.ToInt32(lbl.Text[1].ToString());
                GOAL.Second = Convert.ToInt32(lbl.Text[4].ToString());
                lbl.BackColor = Color.Red;
            }

            else if (lbl.BackColor != Color.Green && lbl.BackColor != Color.Red && Form.ModifierKeys == Keys.Alt)
                lbl.BackColor = Color.Black;

            else if (lbl.BackColor == Color.Black)
                lbl.BackColor = System.Drawing.Color.Transparent;

            else if (startIsSet && lbl.BackColor == Color.Green)
            {
                startIsSet = false;
                HOME.First = -1;
                HOME.Second = -1;
                lbl.BackColor = System.Drawing.Color.Transparent;
            }

            else if (endIsSet && lbl.BackColor == Color.Red)
            {
                endIsSet = false;
                GOAL.First = -1;
                GOAL.Second = -1;
                lbl.BackColor = System.Drawing.Color.Transparent;
            }

            else
                return;
        }

        private void ProcessBTN_Click(object sender, EventArgs e)
        {
            // initial details
            if (!endIsSet || !startIsSet)
                return;
            if (HOME.First == -1 || HOME.Second == -1)
                return;
            if (GOAL.First == -1 || GOAL.Second == -1)
                return;
            
            for (int x = 0; x < X_SIZE; x++)
                for (int y = 0; y < Y_SIZE; y++)
                    if(map[x][y].BackColor == Color.Orange)
                        map[x][y].BackColor = Color.Transparent;

            logic[HOME.First][HOME.Second].status = Status.START;
            logic[GOAL.First][GOAL.Second].status = Status.END;

            for (int i = 0; i < X_SIZE; i++)
                for (int j = 0; j < Y_SIZE; j++)
                    if (map[i][j].BackColor == Color.Black)
                        logic[i][j].status = Status.FULL;
                    else if (map[i][j].BackColor != Color.Green && map[i][j].BackColor != Color.Red)
                        logic[i][j].status = Status.EMPTY;

            List<Tile> openList = new List<Tile>();
            List<Tile> closeList = new List<Tile>();

            openList.Clear();
            closeList.Clear();

            Pair<int, int> homePos = new Pair<int, int>(HOME.First, HOME.Second);
            Pair<int, int> goalPos = new Pair<int, int>(GOAL.First, GOAL.Second);

            bool success = false;
            do
            {
                if (homePos.First == goalPos.First && homePos.Second == goalPos.Second)
                {
                    success = true;
                    break;
                }

                if(openList.Count != 0)
                    openList.Remove(openList.First<Tile>());

                for (int i = 0; i < X_SIZE; ++i)
                {
                    for (int j = 0; j < Y_SIZE; ++j)
                    {
                        // neighbours
                        if (i >= (homePos.First - 1) && i <= (homePos.First + 1) && j >= (homePos.Second - 1) && j <= (homePos.Second + 1))
                        {
                            if ((logic[i][j].status != Status.EMPTY && logic[i][j].status != Status.END) || (i == homePos.First && j == homePos.Second))
                            {
                                bool exist = false;
                                if (closeList.Count != 0)
                                    for (int k = 0; k < closeList.Count; ++k)
                                        if (closeList[k].coordinate.First == logic[i][j].coordinate.First && closeList[k].coordinate.Second == logic[i][j].coordinate.Second)
                                            if (closeList[k].finalCost != 0 && closeList[k].finalCost < logic[i][j].finalCost)
                                                exist = true;
                                if (!exist)
                                    closeList.Add(logic[i][j]);
                            }
                            else
                            {
                                bool closed = false;
                                if (closeList.Count != 0)
                                    for (int k = 0; k < closeList.Count; ++k)
                                        if (closeList[k].coordinate.First == logic[i][j].coordinate.First && closeList[k].coordinate.Second == logic[i][j].coordinate.Second)
                                            closed = true;

                                if (!closed)
                                {
                                    // calculate open list costs
                                    int distance = logic[homePos.First][homePos.Second].distance;
                                    Pair<int, int> parent = homePos;
                                    int heuristic = ((Math.Abs(logic[i][j].coordinate.First - GOAL.First)) + (Math.Abs(logic[i][j].coordinate.Second - GOAL.Second))) * FORWARD_COST; ;
                                    if (Math.Abs(logic[i][j].coordinate.First - homePos.First) == 1 && Math.Abs(logic[i][j].coordinate.Second - homePos.Second) == 1)
                                        distance += DIAGONAL_COST;
                                    else
                                        distance += FORWARD_COST;

                                    int finalCost = distance + heuristic;

                                    bool exist = false;
                                    if (openList.Count != 0)
                                        for (int k = 0; k < openList.Count; ++k)
                                            if (openList[k].coordinate.First == logic[i][j].coordinate.First && openList[k].coordinate.Second == logic[i][j].coordinate.Second)
                                                if (openList[k].finalCost != 0 && openList[k].finalCost < finalCost)
                                                    exist = true;

                                    if (!exist)
                                    {
                                        logic[i][j].distance = distance;
                                        logic[i][j].parent = parent;
                                        logic[i][j].heuristic = heuristic;
                                        logic[i][j].finalCost = finalCost;

                                        openList.Add(logic[i][j]);
                                    }
                                }
                            }
                        }
                    }
                }

                // handle empty list
                if (openList.Count == 0)
                    break;

                // sort open list
                openList.Sort(delegate(Tile t1, Tile t2)
                    {
                        return t1.finalCost.CompareTo(t2.finalCost);
                    });

                // update position
                Tile condidateTile = openList[0];
                closeList.Add(condidateTile);
                homePos = condidateTile.coordinate;
            } while (openList.Count != 0);

            Tile currentTile = logic[GOAL.First][GOAL.Second];
            do
            {
                if (!success)
                {
                    for (int x = 0; x < X_SIZE; x++)
                        for (int y = 0; y < Y_SIZE; y++)
                            if (map[x][y].BackColor == Color.Orange)
                                map[x][y].BackColor = Color.Transparent;

                    break;
                }

                if (currentTile.coordinate == logic[GOAL.First][GOAL.Second].coordinate)
                    ; // do nothing!
                else
                    map[currentTile.coordinate.First][currentTile.coordinate.Second].BackColor = Color.Orange;
                currentTile = logic[currentTile.parent.First][currentTile.parent.Second];
            } while (currentTile.coordinate.First != logic[HOME.First][HOME.Second].coordinate.First ||
                currentTile.coordinate.Second != logic[HOME.First][HOME.Second].coordinate.Second);
        }

        private void ExitBTN_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            for (int x = 0; x < X_SIZE; x++)
            {
                for (int y = 0; y < Y_SIZE; y++)
                {
                    logic[x][y].status = Status.EMPTY;
                    logic[x][y].parent = new Pair<int, int>(-1, -1);
                    logic[x][y].coordinate = new Pair<int, int>(x, y);
                    logic[x][y].distance = 0;
                    logic[x][y].heuristic = 0;
                    logic[x][y].finalCost = 0;
                    map[x][y].BackColor = Color.Transparent;
                }
            }

            endIsSet = false;
            startIsSet = false;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < X_SIZE * Y_SIZE; i++)
                this.Controls.RemoveAt(4);

            Y_SIZE = Convert.ToInt32(numericUpDown1.Value.ToString());
            X_SIZE = Convert.ToInt32(numericUpDown2.Value.ToString());

            Form1_Load(sender, e);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            String textReport;
            textReport = "Developers:\n\n" + "Farbod Samsamipour\n" + "Nima Hemati\n" + "Mohammad Abdous";
            // Display the context menu information in a message box.
            MessageBox.Show(textReport, "About"); 
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            String textReport;
            textReport = "Instructions:\n\n" + "CTRL + Left Click = Home\n" + "SHIFT + Left Click = Goal\n" + "ALT + Left Click = Obstacle\n" + "Left Click = Remove Item";
            // Display the context menu information in a message box.
            MessageBox.Show(textReport, "Help"); 
        }
    }

    public class Pair<T, U>
    {
        public Pair() {}

        public Pair(T first, U second)
        {
            this.First = first;
            this.Second = second;
        }

        public T First { get; set; }
        public U Second { get; set; }
    }
}

