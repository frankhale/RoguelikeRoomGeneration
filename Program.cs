using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;

namespace RoguelikeRoomGeneration
{
    public enum Orientation
    {
        Vertical,
        Horizontal,
        None
    }

    public enum RoomType
    {
        Room,
        Corridor
    }

    public class RoomInfo
    {
        public Point TopLeft { get; set; }
        public Point TopRight { get; set; }
        public Point BottomLeft { get; set; }
        public Point BottomRight { get; set; }
        public Point HorizontalMiddle { get; set; }
        public Point VerticalMiddle { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Rectangle Rectangle { get; set; }

    }

    public class RoomConnectionInfo
    {
        public Rectangle Room1 { get; set; }
        public Rectangle Room2 { get; set; }
        public RoomInfo RoomInfo1 { get; set; }
        public RoomInfo RoomInfo2 { get; set; }
        public Rectangle DominantRoom { get; set; }
        public Rectangle InferiorRoom { get; set; }
        public List<int> IntersectionRange { get; set; }
        public Orientation Orientation { get; set; }
    }

    public class Room
    {
        public Rectangle Rectangle { get; set; }
        public RoomInfo RoomInfo { get; set; }
        public RoomConnectionInfo RoomConnectionInfo { get; set; }
        public RoomType Type { get; set; }
        public Orientation Orientation { get; set; }
    }

    public class RoguelikeRoomGeneration
    {
        const int HEIGHT = 50;
        const int WIDTH = 100;

        public RoguelikeRoomGeneration()
        {
            var rooms = GenerateRooms(WIDTH, HEIGHT, 15, 15, 15);
            rooms.AddRange(GenerateCorridors(rooms));
            RenderMapToConsole(rooms);
        }

        private Rectangle GenerateDimension(int mapWidth, int mapHeight, int roomMaxWidth, int roomMaxHeight)
        {
            var random = new Random();

            int width = random.Next(5, roomMaxWidth);
            int height = random.Next(5, roomMaxHeight);
            int x = random.Next(2, mapWidth - width - 2);
            int y = random.Next(2, mapHeight - height - 2);

            return new Rectangle
            {
                X = x,
                Y = y,
                Width = width,
                Height = height
            };
        }

        private List<Room> GenerateRooms(int mapWidth, int mapHeight, int roomMaxWidth, int roomMaxHeight, int numRooms)
        {
            var rooms = new List<Room>();

            var gap = 2;

            for (int i = 0; i < numRooms; i++)
            {
                Rectangle dim;

                do
                {
                    dim = GenerateDimension(mapWidth, mapHeight, roomMaxWidth, roomMaxHeight);
                }
                while (rooms.Any(room => (!(dim.X > (room.Rectangle.X + room.Rectangle.Width + gap) || (dim.X + dim.Width + gap) < room.Rectangle.X ||
                                          dim.Y > (room.Rectangle.Y + room.Rectangle.Height + gap) || (dim.Y + dim.Height + gap) < room.Rectangle.Y))));


                rooms.Add(new Room
                {
                    Rectangle = new Rectangle(dim.X, dim.Y, dim.Width, dim.Height),
                    RoomInfo = GetRoomInfo(dim),
                    Type = RoomType.Room,
                    Orientation = Orientation.None
                });
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
                BottomRight = new Point(r.X + r.Width, r.Y + r.Height),
                HorizontalMiddle = new Point((int)(new int[] { r.Left, r.Right }).Average(), r.Left),
                VerticalMiddle = new Point(r.Top, (int)(new int[] { r.Top, r.Bottom }).Average()),
                Width = r.Width,
                Height = r.Height,
                Rectangle = r
            };
        }

        private RoomConnectionInfo AreRoomsConnectable(Room room1, Room room2)
        {
            var rangeX1 = Enumerable.Range(room1.RoomInfo.TopLeft.X, room1.RoomInfo.Width);
            var rangeX2 = Enumerable.Range(room2.RoomInfo.TopLeft.X, room2.RoomInfo.Width);
            var rangeY1 = Enumerable.Range(room1.RoomInfo.TopLeft.Y, room1.RoomInfo.Height);
            var rangeY2 = Enumerable.Range(room2.RoomInfo.TopLeft.Y, room2.RoomInfo.Height);

            var verticalConnection = rangeX1.Intersect(rangeX2);
            var horizontalConnection = rangeY1.Intersect(rangeY2);

            //Console.WriteLine($"X1 = {rangeX1.Count()} | X2 = {rangeX2.Count()} | Y1 = {rangeY1.Count()} | Y2 = {rangeY1.Count()}");

            if (verticalConnection.Any() || horizontalConnection.Any())
            {
                var roomSet = new List<Rectangle> { room1.Rectangle, room2.Rectangle };

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
                    Room1 = room1.Rectangle,
                    Room2 = room2.Rectangle,
                    RoomInfo1 = room1.RoomInfo,
                    RoomInfo2 = room2.RoomInfo,
                    DominantRoom = dominantRoom,
                    InferiorRoom = inferiorRoom,
                    IntersectionRange = intersectionRange.ToList(),
                    Orientation = orientation
                };
            }

            return null;
        }

        private List<Room> GenerateCorridors(List<Room> rooms)
        {
            Console.WriteLine("Generate Corridors is inn the process of being rewritten...");

            var corridors = new List<Room>();

            foreach (var room in rooms)
            {
                //Console.WriteLine($"X = {room.RoomInfo.TopLeft} | Width = {room.RoomInfo.Width} | Height = {room.RoomInfo.Height} | VerticalMiddle = {room.RoomInfo.VerticalMiddle}");
            }

            return corridors;
        }

        private List<Room> GenerateCorridors2(List<Room> rooms)
        {
            var corridors = new List<Room>();
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

            foreach (var cnx in roomConnectionInfos)
            {
                Rectangle corridor = Rectangle.Empty;
                var random = new Random(cnx.IntersectionRange.Count);
                var corridorIntersection = cnx.IntersectionRange[cnx.IntersectionRange.Count / 2];

                if (cnx.Orientation == Orientation.Vertical)
                {
                    corridor = new Rectangle(corridorIntersection, cnx.DominantRoom.Bottom, 1, cnx.InferiorRoom.Top - cnx.DominantRoom.Bottom);
                }
                else if (cnx.Orientation == Orientation.Horizontal)
                {
                    corridor = new Rectangle(cnx.DominantRoom.Right, corridorIntersection, cnx.InferiorRoom.Left - cnx.DominantRoom.Right, 1);
                }

                corridors.Add(new Room
                {
                    Rectangle = corridor,
                    RoomConnectionInfo = cnx,
                    Type = RoomType.Corridor,
                    Orientation = cnx.Orientation
                });
            }

            var removeCorridorsThatOverlapWithRooms = corridors.Where(x =>
                rooms.Any(y => y.Rectangle.IntersectsWith(x.Rectangle))).ToList();

            return corridors.Except(removeCorridorsThatOverlapWithRooms).ToList();
        }

        private void RenderMapToConsole(List<Room> rooms)
        {
            //foreach (var r in rooms)
            //{
            //    Console.WriteLine($"X = {r.X} | Y = {r.Y} | Width = {r.Width} | Height {r.Height}");
            //}

            for (int y = 0; y < HEIGHT; y++)
            {
                for (int x = 0; x < WIDTH; x++)
                {
                    var room = rooms.FirstOrDefault(room => x >= room.Rectangle.X && x <= room.Rectangle.X + room.Rectangle.Width &&
                                                            y >= room.Rectangle.Y && y <= room.Rectangle.Y + room.Rectangle.Height);

                    if (room != null)
                    {
                        if (room.Type == RoomType.Room)
                        {
                            if (room.Rectangle.Left == x ||
                                room.Rectangle.Right == x ||
                                room.Rectangle.Top == y ||
                                room.Rectangle.Bottom == y)
                            {
                                Console.Write("#");
                            }
                            else
                            {
                                Console.Write(".");
                            }
                        }
                        else if (room.Type == RoomType.Corridor && room.Orientation == Orientation.Horizontal)
                        {
                            Console.Write("-");
                        }
                        else if (room.Type == RoomType.Corridor && room.Orientation == Orientation.Vertical)
                        {
                            Console.Write("|");
                        }
                    }
                    else
                    {
                        Console.Write(" ");
                    }
                }

                Console.WriteLine();
            }
        }

        private void RenderMapToConsole2(List<Room> rooms)
        {
            //foreach (var r in rooms)
            //{
            //    Console.WriteLine($"X = {r.X} | Y = {r.Y} | Width = {r.Width} | Height {r.Height}");
            //}

            //var roomPadding = 2;

            //for (int y = 0; y < HEIGHT; y++)
            //{
            //    for (int x = 0; x < WIDTH; x++)
            //    {
            //        var room = rooms.FirstOrDefault(room => x >= room.Rectangle.X - roomPadding &&
            //                                                x <= room.Rectangle.X - roomPadding + room.Rectangle.Width + roomPadding &&
            //                                                y >= room.Rectangle.Y - roomPadding &&
            //                                                y <= room.Rectangle.Y - roomPadding + room.Rectangle.Height + roomPadding);

            //        if (room != null)
            //        {
            //            if (room.Rectangle.Left - roomPadding == x ||
            //                room.Rectangle.Right == x ||
            //                room.Rectangle.Top - roomPadding == y ||
            //                room.Rectangle.Bottom == y)
            //            {
            //                Console.Write("#");
            //            }
            //            else
            //            {
            //                Console.Write(".");
            //            }
            //        }
            //        else
            //        {
            //            Console.Write(" ");
            //        }
            //    }

            //    Console.WriteLine();
            //}
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
            new RoguelikeRoomGeneration();
        }
    }
}
