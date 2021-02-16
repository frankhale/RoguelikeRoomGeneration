using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace RoguelikeRoomGeneration.SecondGen
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

    public class RoomConnection
    {
        public Point TopMiddle { get; set; }
        public Point LeftMiddle { get; set; }
        public Point RightMiddle { get; set; }
        public Point BottomMiddle { get; set; }
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

    public class Room
    {        
        public Rectangle Rectangle { get; set; }
        public RoomInfo RoomInfo { get; set; }
        public RoomType Type { get; set; }
        public Orientation Orientation { get; set; }
        public RoomConnection Connection { get; set; }
    }

    public class RoomGenerator2
    {
        private const int HEIGHT = 30;
        private const int WIDTH = 120;
        private const int NUM_ROOMS = 4;
        private const int MAX_ROOM_WIDTH = 10;
        private const int MAX_ROOM_HEIGHT = 10;
        private bool DEBUG = false;

        public RoomGenerator2(bool debug = false)
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
            var padding = 8; // put some default space between each room

            for (int i = 0; i < numRooms; i++)
            {
                Rectangle dim;

                do
                {
                    dim = GenerateDimension(mapWidth, mapHeight, roomMaxWidth, roomMaxHeight);
                }
                while (rooms.Any(room => (!(dim.X > (room.Rectangle.Right + padding) || (dim.Right + padding) < room.Rectangle.X ||
                                          dim.Y > (room.Rectangle.Bottom + padding) || (dim.Bottom + padding) < room.Rectangle.Y))));

                Log($"X = {dim.X} | Y = {dim.Y} | Width = {dim.Width} | Height {dim.Height}");

                rooms.Add(CreateRoomFromRect(dim.X, dim.Y, dim.Width, dim.Height, Orientation.None, RoomType.Room));
            }

            return rooms;
        }

        private Room CreateRoomFromRect(int x, int y, int width, int height, Orientation orientation, RoomType roomType)
        {
            var roomInfo = GetRoomInfo(new Rectangle(x, y, width, height));

            return new Room
            {
                Rectangle = roomInfo.Rectangle,
                RoomInfo = roomInfo,
                Orientation = orientation,
                Type = roomType,                
                Connection = new RoomConnection()
            };
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

        private List<Room> GenerateCorridors(List<Room> rooms)
        {
            Log("***");
            Log("NOT COMPLETE: Generate Corridors is in the process of being rewritten...");
            Log("***");

            var corridors = new List<Room>();

            foreach (var room in rooms.OrderBy(x => x.Rectangle.X))
            {
                Log($"room: X = {room.RoomInfo.TopLeft} | Width = {room.RoomInfo.Width} | Height = {room.RoomInfo.Height} | VerticalMiddle = {room.RoomInfo.VerticalMiddle}");

                // start with horizontal corridors
                // are there any rooms east of us? If so, pick the closest one.
                var closestHorizontalNeighbor = rooms.OrderBy(x => x.Rectangle.X)
                                                     .FirstOrDefault(x => room.Rectangle.X < x.Rectangle.X &&
                                                                          (room.Rectangle.Bottom + 3 > x.Rectangle.Y));

                if (closestHorizontalNeighbor != null)
                {
                    Log($"room.X {room.RoomInfo.TopLeft.X} closest horizontal neighbor withn X {closestHorizontalNeighbor.RoomInfo.TopLeft.X}");
                    Log($"room.X {room.RoomInfo.TopLeft.X} corridor vertical differential from room X {closestHorizontalNeighbor.RoomInfo.TopLeft.X} is {Math.Abs(room.RoomInfo.VerticalMiddle.Y - closestHorizontalNeighbor.RoomInfo.VerticalMiddle.Y)}");

                    var distanceToNearestHorizontalNeighbor = closestHorizontalNeighbor.RoomInfo.TopLeft.X - room.RoomInfo.TopRight.X;

                    if (closestHorizontalNeighbor.RoomInfo.VerticalMiddle.Y == room.RoomInfo.VerticalMiddle.Y)
                    {
                        // No need to make a joint in the corridor
                        corridors.Add(CreateRoomFromRect(room.RoomInfo.TopRight.X, room.RoomInfo.VerticalMiddle.Y, distanceToNearestHorizontalNeighbor, 0, Orientation.Horizontal, RoomType.Corridor));
                    }
                    else
                    {
                        var corrRoom1 = CreateRoomFromRect(room.RoomInfo.TopRight.X, room.RoomInfo.VerticalMiddle.Y, distanceToNearestHorizontalNeighbor / 2, 0, Orientation.Horizontal, RoomType.Corridor);
                        var corrJoint = new Rectangle
                        {
                            X = corrRoom1.RoomInfo.TopRight.X,
                            Width = 0
                        };

                        // Decide which direction on the X axis to start the corridor joint
                        if (closestHorizontalNeighbor.RoomInfo.VerticalMiddle.Y > room.RoomInfo.VerticalMiddle.Y)
                        {
                            corrJoint.Y = corrRoom1.RoomInfo.BottomRight.Y;
                            corrJoint.Height = closestHorizontalNeighbor.RoomInfo.VerticalMiddle.Y - corrRoom1.RoomInfo.VerticalMiddle.Y;
                            corridors.Add(CreateRoomFromRect(corrJoint.X, corrJoint.Bottom, distanceToNearestHorizontalNeighbor / 2, 0, Orientation.Horizontal, RoomType.Corridor));
                        }
                        else
                        {
                            corrJoint.Y = closestHorizontalNeighbor.RoomInfo.VerticalMiddle.Y;
                            corrJoint.Height = corrRoom1.RoomInfo.VerticalMiddle.Y - closestHorizontalNeighbor.RoomInfo.VerticalMiddle.Y;
                            corridors.Add(CreateRoomFromRect(corrJoint.X, closestHorizontalNeighbor.RoomInfo.VerticalMiddle.Y, distanceToNearestHorizontalNeighbor / 2, 0, Orientation.Horizontal, RoomType.Corridor));
                        }

                        corridors.Add(corrRoom1);
                        rooms.Add(CreateRoomFromRect(corrJoint.X, corrJoint.Y, corrJoint.Width, corrJoint.Height, Orientation.Horizontal, RoomType.Corridor));
                    }
                }
            }

            return corridors;
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
                                if (x == room.RoomInfo.HorizontalMiddle.X ||
                                    y == room.RoomInfo.VerticalMiddle.Y &&
                                    room.Type == RoomType.Room)
                                {
                                    Console.Write("+");
                                }
                                else
                                {
                                    Console.Write("#");
                                }
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
