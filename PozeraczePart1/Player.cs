using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PozeraczePart1
{
    internal class Player
    {
        public string Name;
        public string Color;
        public int[] pieceNumbers;

        public Player() 
        {
            Name = "player";
            Color = "Black";
            pieceNumbers = new int[] { -1, 2, 2 };
        }

        public Player(int pieceCount)
        {
            Name = "player";
            Color = "Black";
            pieceNumbers = new int[] { -1, pieceCount, pieceCount };
        }

    }
}
