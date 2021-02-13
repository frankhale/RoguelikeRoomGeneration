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

    public class RoomInfo
    {
        public Point TopLeft { get; set; }
        public Point TopRight { get; set; }
        public Point BottomLeft { get; set; }
        public Point BottomRight { get; set; }
    }

    public enum Orientation
    {
        Vertical,
        Horizontal
    }

    //public class RoomPair
    //{
    //    public Rectangle Room1 { get; set; }
    //    public Rectangle Room2 { get; set; }
    //}

    public class RoomConnectionInfo
    {
        public Rectangle Room1 { get; set; }
        public Rectangle Room2 { get; set; }
        public Rectangle DominantRoom { get; set; }
        public Rectangle InferiorRoom { get; set; }
        public List<int> IntersectionRange { get; set; }
        public Orientation Orientation { get; set; }
    }

    public class ProceduralLevelGenerator
    {
        const int HEIGHT = 40;
        const int WIDTH = 100;

        public ProceduralLevelGenerator()
        {
            //int[,] map = InitializeMap(WIDTH, HEIGHT);
            var rooms = GenerateRooms(WIDTH, HEIGHT, 10, 10, 20);

            //RenderMap(rooms);

            rooms.AddRange(GenerateCorridors(rooms));

            //Console.WriteLine("---");
            //Console.WriteLine("WITH CORRIDORS");
            //Console.WriteLine("---");

            RenderMap(rooms);
        }

        //private int[,] InitializeMap(int width, int height)
        //{
        //    int[,] map = new int[height, width];

        //    for (int y = 0; y < height; y++)
        //        for (int x = 0; x < width; x++)
        //        {
        //            map[y, x] = 0;
        //        }

        //    return map;
        //}

        private Dimension GenerateDimension(int mapWidth, int mapHeight, int roomMaxWidth, int roomMaxHeight)
        {
            var random = new Random();

            int width = random.Next(4, roomMaxWidth);
            int height = random.Next(4, roomMaxHeight);
            int x = random.Next(1, mapWidth - width - 1);
            int y = random.Next(1, mapHeight - height - 1);

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

        private RoomInfo GetRoomInfo(Rectangle r)
        {
            return new RoomInfo
            {
                TopLeft = new Point(r.X, r.Y),
                TopRight = new Point(r.X + r.Width, r.Y),
                BottomLeft = new Point(r.X, r.Y + r.Height),
                BottomRight = new Point(r.X + r.Width, r.Y + r.Height)
            };
        }

        private RoomConnectionInfo AreRoomsConnectable(Rectangle room1, Rectangle room2)
        {
            var roomInfo1 = GetRoomInfo(room1);
            var roomInfo2 = GetRoomInfo(room2);

            var rangeX1 = Enumerable.Range(roomInfo1.TopLeft.X, room1.Width);
            var rangeX2 = Enumerable.Range(roomInfo2.TopLeft.X, room2.Width);
            var rangeY1 = Enumerable.Range(roomInfo1.TopLeft.Y, room1.Height);
            var rangeY2 = Enumerable.Range(roomInfo2.TopLeft.Y, room2.Height);

            var verticalConnection = rangeX1.Intersect(rangeX2);
            var horizontalConnection = rangeY1.Intersect(rangeY2);

            if (verticalConnection.Any() || horizontalConnection.Any())
            {
                var roomSet = new List<Rectangle> { room1, room2 };

                Orientation orientation;
                IEnumerable<int> intersectionRange;
                Rectangle dominantRoom = Rectangle.Empty, inferiorRoom = Rectangle.Empty;

                if (verticalConnection.Any())
                {
                    var roomSetOrdered = roomSet.OrderBy(x => x.Y);
                    dominantRoom = roomSetOrdered.First();
                    inferiorRoom = roomSetOrdered.Last();

                    orientation = Orientation.Vertical;
                    intersectionRange = verticalConnection;
                }
                else
                {
                    var roomSetOrdered = roomSet.OrderBy(x => x.X);
                    dominantRoom = roomSetOrdered.First();
                    inferiorRoom = roomSetOrdered.Last();

                    orientation = Orientation.Horizontal;
                    intersectionRange = horizontalConnection;
                }

                return new RoomConnectionInfo
                {
                    Room1 = room1,
                    Room2 = room2,
                    DominantRoom = dominantRoom,
                    InferiorRoom = inferiorRoom,
                    IntersectionRange = intersectionRange.ToList(),
                    Orientation = orientation
                };
            }

            return null;
        }

        private List<Rectangle> GenerateCorridors(List<Rectangle> rooms)
        {
            var corridors = new List<Rectangle>();
            var roomConnectionInfos = new List<RoomConnectionInfo>();

            foreach (var room in rooms)
            {
                foreach (var r in rooms.Where(x => x != room))
                {
                    var cnx = AreRoomsConnectable(room, r);
                    if (cnx != null)
                    {
                        roomConnectionInfos.Add(cnx);
                    }
                }
            }

            var cnxGroups = roomConnectionInfos.DistinctBy(x => x.DominantRoom).GroupBy(x => x.DominantRoom);

            foreach (var cnxGroup in cnxGroups)
            {
                foreach (var cnx in cnxGroup)
                {
                    Rectangle corridor = Rectangle.Empty;
                    var random = new Random();
                    var pos = cnx.IntersectionRange[random.Next(0, cnx.IntersectionRange.Count)];

                    if (cnx.Orientation == Orientation.Vertical)
                    {
                        corridor = new Rectangle(pos, cnx.DominantRoom.Bottom, 1, cnx.InferiorRoom.Top - cnx.DominantRoom.Bottom);
                    }
                    else if (cnx.Orientation == Orientation.Horizontal)
                    {
                        corridor = new Rectangle(cnx.DominantRoom.Right, pos, cnx.InferiorRoom.Left - cnx.DominantRoom.Right, 1);
                    }

                    corridors.Add(corridor);
                }
            }

            return corridors;
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
                    var room = rooms.FirstOrDefault(room => x >= room.X && x < room.X + room.Width &&
                                                            y >= room.Y && y < room.Y + room.Height);

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

    
    public static class LinqExtensions
    {
        // https://stackoverflow.com/questions/438188/split-a-collection-into-n-parts-with-linq
        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> list, int parts)
        {
            int i = 0;
            var splits = from item in list
                         group item by i++ % parts into part
                         select part.AsEnumerable();
            return splits;
        }

        // https://stackoverflow.com/a/489421/170217
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>
            (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
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
