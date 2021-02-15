using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace RoguelikeRoomGeneration.FirstGen
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
        public int Index { get; set; }
    }

    public class RoomGenerator1
    {
        private const int HEIGHT = 30;
        private const int WIDTH = 110;
        private const int NUM_ROOMS = 4;
        private const int MAX_ROOM_WIDTH = 20;
        private const int MAX_ROOM_HEIGHT = 20;
        private bool DEBUG = false;

        public RoomGenerator1(bool debug = false)
        {
            DEBUG = debug;

            var rooms = GenerateRooms(WIDTH, HEIGHT, MAX_ROOM_WIDTH, MAX_ROOM_HEIGHT, NUM_ROOMS);
            rooms.AddRange(GenerateCorridors(rooms));
            RenderMapToConsole(rooms);
        }

        private void Log(string str)
        {
            if (DEBUG)
            {
                Console.WriteLine(str);
            }
        }

        private Rectangle GenerateDimension(int mapWidth, int mapHeight, int roomMaxWidth, int roomMaxHeight)
        {
            var random = new Random();

            int width = random.Next(10, roomMaxWidth);
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

            var gap = 8;

            for (int i = 0; i < numRooms; i++)
            {
                Rectangle dim;

                do
                {
                    dim = GenerateDimension(mapWidth, mapHeight, roomMaxWidth, roomMaxHeight);
                }
                while (rooms.Any(room => (!(dim.X > (room.Rectangle.X + room.Rectangle.Width + gap) || (dim.X + dim.Width + gap) < room.Rectangle.X ||
                                          dim.Y > (room.Rectangle.Y + room.Rectangle.Height + gap) || (dim.Y + dim.Height + gap) < room.Rectangle.Y))));

                Log($"X = {dim.X} | Y = {dim.Y} | Width = {dim.Width} | Height {dim.Height}");

                rooms.Add(new Room
                {
                    Rectangle = new Rectangle(dim.X, dim.Y, dim.Width, dim.Height),
                    RoomInfo = GetRoomInfo(dim),
                    Type = RoomType.Room,
                    Orientation = Orientation.None,
                    Index = rooms.Count + 1
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
                    corridor = new Rectangle(corridorIntersection, cnx.DominantRoom.Bottom, 0, cnx.InferiorRoom.Top - cnx.DominantRoom.Bottom);
                }
                else if (cnx.Orientation == Orientation.Horizontal)
                {
                    corridor = new Rectangle(cnx.DominantRoom.Right, corridorIntersection, cnx.InferiorRoom.Left - cnx.DominantRoom.Right, 0);
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
                        else
                        if (room.Type == RoomType.Corridor && room.Orientation == Orientation.Horizontal)
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
    }
}

