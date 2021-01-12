using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {

        Crossword crossword;
        bool is_mouse_down_ = false;
        int screen_width_ = Screen.PrimaryScreen.WorkingArea.Width;
        int screen_height_ = Screen.PrimaryScreen.WorkingArea.Height;
        int grid_size_ = 40;
        int grid_width_ = 20;
        int grid_height_ = 10;
        Point selection_point_1;
        Point selection_point_2;
        Image img_grid_;
        Image img_cross_word_;
        Image img_letters;

        public Form1()
        {
            crossword = new Crossword();
            InitializeComponent();
            img_grid_ = new Bitmap(screen_width_, screen_height_);
            img_cross_word_ = new Bitmap(screen_width_, screen_height_);
            img_letters = new Bitmap(screen_width_, screen_height_);
            DrawGrid();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            crossword.clear_list();
            img_cross_word_ = new Bitmap(screen_width_, screen_height_);
            img_letters = new Bitmap(screen_width_, screen_height_);
            crossword.SortCrossword();
            crossword.Initialize_crossword();
            while (!crossword.Fill_crossword())
            {
                crossword.Initialize_crossword();
            }
            DrawLetters();
            crossword.PrintCrossWord();
            //crossword = new Crossword();
            Refresh();
            //crossword.Download_data("........");
        }
        private void Form1_load(object sender, EventArgs e)
        {

        }


        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (!is_mouse_down_)
            {
                is_mouse_down_ = true;
                selection_point_1 = e.Location;
            }
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            //if (IsMouseDown)
            //{
            //    end_xy_ = e.Location;
            //    Refresh();
            //}
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (is_mouse_down_)
            {
                selection_point_2 = e.Location;
                is_mouse_down_ = false;
                SortSelectionPoints();
                AddCrosswordPiece();
                Refresh();
            }
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            //e.Graphics.DrawRectangle(Pens.Black, GetRect());
            //createmap(e);
            e.Graphics.DrawImage(DrawMap(), 0, 0);
        }

        private Bitmap DrawMap()
        {
            Bitmap bmp = new Bitmap(screen_width_, screen_height_);
            using (Graphics graph = Graphics.FromImage(bmp))
            {
                graph.DrawImage(img_cross_word_, 0, 0);
                graph.DrawImage(img_grid_, 0, 0);
                graph.DrawImage(img_letters, 0, 0);
            }
            return bmp;
        }
        private void AddCrosswordPiece()
        {
            int x;
            int y;
            int w;
            int h;

            Point grid_point_1 = PixelToGrid(selection_point_1);
            if (grid_point_1.X == -1 || grid_point_1.Y == -1)
                return;
            Point grid_point_2 = PixelToGrid(selection_point_2);
            if (grid_point_2.X == -1 || grid_point_2.Y == -1)
                return;

            x = GridToPixel(grid_point_1).X;
            y = GridToPixel(grid_point_1).Y;
            w = grid_size_;
            h = grid_size_;

            if ((grid_point_1.X == grid_point_2.X)
                && (grid_point_2.Y - grid_point_1.Y) >= 2)
                h *= grid_point_2.Y - grid_point_1.Y + 1;
            else if ((grid_point_1.Y == grid_point_2.Y)
                && (grid_point_2.X - grid_point_1.X) >= 2)
                w *= grid_point_2.X - grid_point_1.X + 1;
            else
                return;

            if (!(crossword.add_word(grid_point_1, grid_point_2)))
                return;

            using (Graphics graph = Graphics.FromImage(img_cross_word_))
            {
                Rectangle rect = new Rectangle(x, y, w, h);
                graph.FillRectangle(new SolidBrush(Color.Blue), rect);
            }
        }

        private void DrawGrid()
        {
            using (Graphics graph = Graphics.FromImage(img_grid_))
            {
                for (int i = 0; i < grid_width_; i++)
                    for (int j = 0; j < grid_height_; j++)
                    {
                        Rectangle rect = new Rectangle(i * grid_size_, j * grid_size_, grid_size_, grid_size_);
                        //e.Graphics.FillRectangle(blueBrush, i * 20, j * 20, 20, 20);
                        graph.DrawRectangle(Pens.Black, rect);
                    }
            }
        }
        private void DrawLetters()
        {
            System.Drawing.Font drawFont = new System.Drawing.Font("Arial", 16);
            System.Drawing.SolidBrush drawBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
            System.Drawing.StringFormat drawFormat = new System.Drawing.StringFormat();
            Char sign;
            int offset = 10;
            using (Graphics graph = Graphics.FromImage(img_letters))
            {
                for (int i = 0; i < crossword.array_width; i++)
                {
                    for (int j = 0; j < crossword.array_height; j++)
                    {
                        sign = crossword.letters[i, j];
                        if (!sign.Equals('0'))
                        {
                            graph.DrawString(sign.ToString(), drawFont, drawBrush, offset + i * grid_size_, offset + j * grid_size_, drawFormat);
                        }
                    }
                }
                System.Drawing.Font drawFont2 = new System.Drawing.Font("Arial", 9);
                int word_number = 1;

                for (int i = 0; i < crossword.clues_list.Count; i++)
                {

                    graph.DrawString(word_number.ToString(), drawFont2, drawBrush, crossword.clues_list[i].point_.X * grid_size_, crossword.clues_list[i].point_.Y * grid_size_, drawFormat);
                    graph.DrawString(word_number.ToString() + ". " + crossword.clues_list[i].text, drawFont, drawBrush, 21 * grid_size_, (1 + i) * grid_size_, drawFormat);
                    // wpisywanie małej cyferki w clue.point
                    //w jakimś miejscu wpisać clue.text z daną cyferką lub liczbą
                    //ta liczba to wordnumber
                    word_number++;
                }


                drawFont.Dispose();
                drawBrush.Dispose();
            }
        }

        private void SortSelectionPoints()
        {
            Point new_start = new Point();
            Point new_end = new Point();
            new_start.X = Math.Min(selection_point_1.X, selection_point_2.X);
            new_start.Y = Math.Min(selection_point_1.Y, selection_point_2.Y);
            new_end.X = Math.Max(selection_point_1.X, selection_point_2.X);
            new_end.Y = Math.Max(selection_point_1.Y, selection_point_2.Y);
            selection_point_1 = new_start;
            selection_point_2 = new_end;
        }

        private Point PixelToGrid(Point pixel_point)
        {
            Point grid_point = new Point((int)(pixel_point.X / grid_size_), (int)(pixel_point.Y / grid_size_));
            if (grid_point.X < 0
                || grid_point.Y < 0
                || grid_point.X >= grid_width_
                || grid_point.Y >= grid_height_)
                return new Point(-1, -1);
            else
                return grid_point;
        }

        private Point GridToPixel(Point grid_point)
        {
            Point pixel_point = new Point((int)(grid_point.X * grid_size_), (int)(grid_point.Y * grid_size_));
            if (pixel_point.X < 0
                || pixel_point.Y < 0
                || pixel_point.X >= screen_width_
                || pixel_point.Y >= screen_height_)
                return new Point(-1, -1);
            else
                return pixel_point;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            crossword.hard_clear_list();
            img_letters = new Bitmap(screen_width_, screen_height_);
            img_cross_word_ = new Bitmap(screen_width_, screen_height_);
            Refresh();

        }
    }

    public class Crossword
    {
        public struct Word
        {
            public Point start_point;
            public Point end_point;
            public bool is_horizontal;
            public int Lenght()
            {
                if (is_horizontal)
                {
                    return end_point.X - start_point.X + 1;
                }
                else
                {
                    return end_point.Y - start_point.Y + 1;
                }
            }
        }
        public struct Clue
        {
            public Point point_;
            public string text;
        }

        public Char[,] letters;
        public int array_width = 20, array_height = 20;
        List<Word> word_list;
        public List<Clue> clues_list;

        public Crossword()
        {
            word_list = new List<Word>();
            letters = new char[array_width, array_height];
            clues_list = new List<Clue>();
        }

        public void add_clue(Point point, string str_text)
        {
            Clue new_clue;
            new_clue.point_ = point;
            new_clue.text = str_text;
            clues_list.Add(new_clue);
        }

        public void hard_clear_list()
        {
            clues_list.Clear();
            word_list.Clear();
        }
        public void clear_list()
        {
            clues_list.Clear();
        }
        public bool add_word(Point start, Point end)
        {
            Word new_word;
            new_word.start_point = start;
            new_word.end_point = end;
            if (start.Y == end.Y)
                new_word.is_horizontal = true;
            else
                new_word.is_horizontal = false;

            foreach (Word word in word_list)
            {
                if (new_word.is_horizontal == word.is_horizontal)
                {
                    if (new_word.is_horizontal)
                    {
                        if (Math.Abs(new_word.start_point.Y - word.start_point.Y) <= 1)
                            if (word.start_point.X - new_word.end_point.X <= 1 && new_word.start_point.X - word.end_point.X <= 1)
                                return false;
                    }
                    else
                    {
                        if (Math.Abs(new_word.start_point.X - word.start_point.X) <= 1)
                            if (word.start_point.Y - new_word.end_point.Y <= 1 && new_word.start_point.Y - word.end_point.Y <= 1)
                                return false;
                    }
                }
            }
            word_list.Add(new_word);
            return true;
        }

        public void Initialize_crossword()
        {
            clues_list.Clear();
            for (int i = 0; i < array_width; i++)
            {
                for (int j = 0; j < array_height; j++)
                {
                    letters[i, j] = '0';
                }
            }
            foreach (Word word in word_list)
            {
                if (word.is_horizontal)
                {
                    for (int i = word.start_point.X; i <= word.end_point.X; i++)
                        letters[i, word.start_point.Y] = '.';
                }
                else
                {
                    for (int i = word.start_point.Y; i <= word.end_point.Y; i++)
                        letters[word.start_point.X, i] = '.';
                }
            }
        }

        public void SortCrossword()
        {
            word_list = word_list.OrderByDescending(word => word.Lenght()).ToList();
        }

        public bool Fill_crossword()
        {
            string char_word;
            string[] found_word;
            int word_counter = 0;
            foreach (Word word in word_list)
            {
                word_counter++;
                int j = 0;
                if (word.is_horizontal)
                {

                    Char[] letter = new Char[word.end_point.X - word.start_point.X + 1];
                    for (int i = word.start_point.X; i <= word.end_point.X; i++)
                    {
                        letter[j] = letters[i, word.start_point.Y];
                        j++;
                    }
                    char_word = new string(letter);
                    try
                    {
                        found_word = Download_data(char_word);
                        if (found_word[0].Equals(""))
                            return false;
                    }
                    catch (Exception e)
                    {
                        return false;
                    }

                    letter = found_word[0].ToCharArray();
                    j = 0;
                    for (int i = word.start_point.X; i <= word.end_point.X; i++)
                    {
                        letters[i, word.start_point.Y] = letter[j];
                        j++;
                    }
                }
                else
                {
                    Char[] letter = new Char[word.end_point.Y - word.start_point.Y + 1];
                    for (int i = word.start_point.Y; i <= word.end_point.Y; i++)
                    {
                        letter[j] = letters[word.start_point.X, i];
                        j++;
                    }

                    char_word = new string(letter);
                    try
                    {
                        found_word = Download_data(char_word);
                        if (found_word[0].Equals(""))
                            return false;
                    }
                    catch (Exception e)
                    {
                        return false;
                    }
                    letter = found_word[0].ToCharArray();

                    j = 0;
                    for (int i = word.start_point.Y; i <= word.end_point.Y; i++)
                    {
                        letters[word.start_point.X, i] = letter[j];
                        j++;
                    }
                }
                add_clue(word.start_point, found_word[1]);
            }
            return true;
        }

        public string[] Download_data(string search_word)
        {
            //System.Net.WebClient wc = new System.Net.WebClient();
            //string webData = wc.DownloadString("https://krzyzowka.net/szukaj?h=&l=8&d=&t=1&p=0");
            //Console.WriteLine(webData);
            string[] word_definition = new string[] { "", "" };
            Random rand = new Random();
            int rand_counter;
            int try_counter = 5;
            while (try_counter > 0)
            {
                try_counter--;
                try
                {
                    rand_counter = rand.Next(1, 80);
                    string page = rand_counter.ToString();
                    page = page + "/";
                    System.Net.HttpWebRequest rqst = (System.Net.HttpWebRequest)System.Net.WebRequest.Create("http://www.lexic.pl/szukaj/search/by_missing_letters/" + page + search_word);
                    var rspns = (System.Net.HttpWebResponse)rqst.GetResponse();
                    using (StreamReader sr = new StreamReader(rspns.GetResponseStream()))
                    {
                        string line;
                        int counter = 0;
                        rand_counter = rand.Next(1, 20);
                        // Read and display lines from the file until the end of
                        // the file is reached.
                        while ((line = sr.ReadLine()) != null)
                        {
                            if (line.StartsWith("            <b>"))
                            {
                                string word = line.Split(new string[] { "            <b>", "</b>:" }, StringSplitOptions.None)[1];
                                if (!IsWordValid(word))
                                {
                                    continue;
                                }
                                word_definition[0] = word;
                                line = sr.ReadLine();
                                word_definition[1] = line.Split(new string[] { "      " }, StringSplitOptions.None)[1];
                                counter++;
                                if (counter > rand_counter)
                                {
                                    break;
                                }
                            }
                        }
                        if (word_definition[0].Equals(""))
                        {
                            continue;
                        }
                        else
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    continue;
                }
            }
            Console.WriteLine(word_definition[0]);
            Console.WriteLine(word_definition[1]);
            return word_definition;
        }

        private bool IsWordValid(string word)
        {
            foreach (char character in word)
            {
                if (!Char.IsLetter(character))
                    return false;
            }
            return true;
        }

        public void PrintCrossWord()
        {
            for (int i = 0; i < array_height; i++)
            {
                for (int j = 0; j < array_width; j++)
                {
                    Console.Write(letters[i, j]);

                }
                Console.WriteLine("/n");
            }

        }

    }
    
}
