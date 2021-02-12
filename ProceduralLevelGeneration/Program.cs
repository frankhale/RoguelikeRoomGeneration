using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ProceduralLevelGeneration
{
    public class Dimension
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class ProceduralLevelGenerator
    {
        const int HEIGHT = 40;
        const int WIDTH = 100;

        public ProceduralLevelGenerator()
        {
            //int[,] map = InitializeMap(WIDTH, HEIGHT);
            var rooms = GenerateRooms(WIDTH, HEIGHT, 20, 20, 40);

            RenderMap(rooms);
        }

        private int[,] InitializeMap(int width, int height)
        {
            int[,] map = new int[height, width];

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    map[y, x] = 0;
                }

            return map;
        }

        private Dimension GenerateDimension(int mapWidth, int mapHeight, int roomMaxWidth, int roomMaxHeight)
        {
            var random = new Random();

            int width = random.Next(1, roomMaxWidth);
            int height = random.Next(1, roomMaxHeight);
            int x = random.Next(1, mapWidth - width);            
            int y = random.Next(1, mapHeight - height);

            return new Dimension
            {
                X = x,
                Y = y,
                Width = width,
                Height = height
            };
        }

        private List<Rectangle> GenerateRooms(int mapWidth, int mapHeight, int roomMaxWidth, int roomMaxHeight, int numRooms)
        {
            var rooms = new List<Rectangle>();

            for (int i = 0; i < numRooms; i++)
            {
                Dimension dim;

                do
                {
                    dim = GenerateDimension(mapWidth, mapHeight, roomMaxWidth, roomMaxHeight);
                }
                while (rooms.Any(room => (!(dim.X > (room.X + room.Width + 2) || (dim.X + dim.Width + 2) < room.X ||
                                          dim.Y > (room.Y + room.Height + 2) || (dim.Y + dim.Height + 2) < room.Y))));
                                         

                rooms.Add(new Rectangle(dim.X, dim.Y, dim.Width, dim.Height));
            }

            return rooms;
        }

        private void RenderMap(List<Rectangle> rooms)
        {
            //foreach (var r in rooms)
            //{
            //    Console.WriteLine($"X = {r.X} | Y = {r.Y} | Width = {r.Width} | Height {r.Height}");
            //}

            for (int y = 0; y < HEIGHT; y++)
            {
                for (int x = 0; x < WIDTH; x++)
                {
                    var room = rooms.FirstOrDefault(room => x >= room.X && x <= room.X + room.Width &&
                                                            y >= room.Y && y <= room.Y + room.Height);

                    if (!room.IsEmpty)
                    {
                        Console.Write("#");
                    }
                    else
                    {
                        Console.Write("-");
                    }
                }

                Console.WriteLine();
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            new ProceduralLevelGenerator();
        }
    }
}
