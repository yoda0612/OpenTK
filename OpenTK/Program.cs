using System;


using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL4;
using System.Drawing;

namespace OpenTK
{
    class MainWindow : GameWindow
    {       
        private int _program;
        private int _vertexArray;
        private double _time;
        private Matrix4 _modelView;
        private Matrix4 _projectionMatrix;
        private float _fov = 60f;

        float rX = 1;
        float rY = 1;
        float rZ = 1;
        float scaling = 1f;
        private List<RenderObject> _renderObjects = new List<RenderObject>();
        private List<RenderObject> _otherRenderObjects = new List<RenderObject>();
        int frameSpeedy = 0;
        int frameNum = 0;
        public static SciColorMaps.ColorMap colorMap;
        public static int totalTimepoint =1;

        Camera camera = new Camera();

        int pressedX;
        int pressedY;
        bool startRotate = false;

        public MainWindow(): base(700, // initial width
            700, // initial height
            GraphicsMode.Default,
            "dreamstatecoding",  // initial title
            GameWindowFlags.Default,
            DisplayDevice.Default,
            4, // OpenGL major version
            0, // OpenGL minor version
            GraphicsContextFlags.ForwardCompatible)
        {
            Title += ": OpenGL Version: " + GL.GetString(StringName.Version);
        }
        private void CreateProjection()
        {

            var aspectRatio = (float)Width / Height;
            _projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(
                _fov * ((float)Math.PI / 180f), // field of view angle, in radians
                aspectRatio,                // current window aspect ratio
                0.1f,                       // near plane
                3000f);                     // far plane
        }
        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(100, 100, Width-200, Height-200);
            CreateProjection();
        }
        protected override void OnLoad(EventArgs e)
        {
            CreateProjection();
            _renderObjects.Add(new RenderObject(ObjectFactory.LoadModel(@"D:\TestModel\Sars.obj", Color4.Pink, 0.5f), PrimitiveType.Triangles));
            //_renderObjects.Add(new RenderObject(ObjectFactory.CreateSolidCube(0.3f, Color4.Red), PrimitiveType.Triangles));
            //_renderObjects.Add(new RenderObject(ObjectFactory.CreateTransparentCube(0.4f, Color4.Gray, 0.5f), PrimitiveType.Triangles));

            //_renderObjects.Add(new RenderObject(ObjectFactory.CreateSphere(Color4.Green, 0.4, 0.5), PrimitiveType.Triangles));
            //_renderObjects.Add(new RenderObject(ObjectFactory.CreateSphere(Color4.Red, 1,0.3), PrimitiveType.Triangles));

            //_renderObjects.Add(new RenderObject(ObjectFactory.CreateCellDecom(Color4.Red),PrimitiveType.Triangles));
            //Vertex[] vertices = ObjectFactory.SphericalHarmonicPureShape(Color4.Red);
            //_renderObjects.Add(new RenderObject(ObjectFactory.CreateEnterprise(),PrimitiveType.Triangles));

            //for (int t = 0; t < MainWindow.totalTimepoint; t++)
            //{
            //    Console.WriteLine(t);
            //    Vertex[] vertices = ObjectFactory.CreateCell(t, Color4.Red);
            //    _renderObjects.Add(new RenderObject(vertices, PrimitiveType.Triangles));
            //}
            //_otherRenderObjects.Add(new RenderObject(ObjectFactory.DrawAxis(), PrimitiveType.Lines));

            CursorVisible = true;
            _program = CompileShaders();
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.PatchParameter(PatchParameterInt.PatchVertices, 3);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            
            Closed += OnClosed;
        }
        private void OnClosed(object sender, EventArgs eventArgs)
        {
            Exit();
        }
        public override void Exit()
        {
            foreach (var obj in _renderObjects)
                obj.Dispose();
            GL.DeleteProgram(_program);
            base.Exit();
        }
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            //_time += e.Time;
            //var k = (float)_time * 0.05f;
            //var r1 = Matrix4.CreateRotationX(k * 13.0f);
            //var r2 = Matrix4.CreateRotationY(1 * 13.0f);
            //var r3 = Matrix4.CreateRotationZ(1 * 3.0f);
            //_modelView = r1 * r2 * r3;
            KeyRotate();
            HandleKeyboard();
        }
        private void KeyRotate()
        {
            if (Mouse.GetState().LeftButton==ButtonState.Pressed)
            {
                if (!startRotate)
                {
                    pressedX = Mouse.GetState().X;
                    pressedY = Mouse.GetState().Y;
                    startRotate = true;
                }
                rY -= (pressedX - Mouse.GetState().X) * 0.0001f;
                rX -= (pressedY - Mouse.GetState().Y) * 0.0001f;

            }

            if (Mouse.GetState().LeftButton == ButtonState.Released)
            {
                startRotate = false;
            }

            if (Mouse.GetState().WheelPrecise != 0)
            {                
                scaling = 1f + (Mouse.GetState().Wheel * 0.05f);                
            }

            if (Keyboard.GetState().IsKeyDown(Key.Left))
            {
                rX += 0.01f;
            }
            if (Keyboard.GetState().IsKeyDown(Key.Right))
            {
                rX -= 0.01f;
            }
            if (Keyboard.GetState().IsKeyDown(Key.Up))
            {
                //rY += 0.01f;
                _fov -= 10f;
                CreateProjection();
            }
            if (Keyboard.GetState().IsKeyDown(Key.Down))
            {
                rY -= 0.01f;
            }
            var r1 = Matrix4.CreateRotationX(rX);
            var r2 = Matrix4.CreateRotationY(rY);
            var r3 = Matrix4.CreateRotationZ(rZ);
            var scalingM = Matrix4.CreateScale(scaling, scaling, scaling);

            var t = Matrix4.CreateTranslation(
                       0f,
                       0f,
                       0f);

            //_modelView = r1 * r2 * r3 * scalingM;
            _modelView = r1 * r2 * scalingM * t * camera.LookAtMatrix;
        }
        private void HandleKeyboard()
        {
            var keyState = Keyboard.GetState();

            if (keyState.IsKeyDown(Key.Escape))
            {
                Exit();
            }
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            _time += e.Time;

            Title = $"(Vsync: {VSync}) FPS: {1f / e.Time:0}";
            Color4 backColor;
            backColor.A = 1.0f;
            backColor.R = 0.1f;
            backColor.G = 0.1f;
            backColor.B = 0.3f;
            
            GL.ClearColor(backColor);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(_program);
            GL.UniformMatrix4(21,              // match the layout location in the shader
                                 false,           // transpose
                                 ref _modelView); // our matrix

            GL.UniformMatrix4(20, false, ref _projectionMatrix);
            var model = Matrix4.Identity;
            GL.UniformMatrix4(22, false, ref model);


            
            GL.Uniform3(23, new Vector3(0.0f, 1.0f, 0.0f));
            GL.Uniform3(24, new Vector3(1.0f, 1.0f, 1.0f));
            GL.Uniform3(25, new Vector3(0f, 1.0f, 12.0f));
            
            GL.Uniform3(26, camera.position);

            

            //if (frameSpeedy == 30) 
            //{
            //    frameSpeedy = 0;
            //    frameNum++;
            //    Console.WriteLine(frameNum);
            //}

            //if (frameNum == _renderObjects.Count)
            //    frameNum = 0;

            foreach (var renderObject in _renderObjects)
                renderObject.Render();
            foreach (var otherRenderObject in _otherRenderObjects)
            {
                otherRenderObject.Render();
            }
            
            SwapBuffers();

            frameSpeedy++;
        }
        private int CompileShaders()
        {
            var vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, File.ReadAllText(@"vertexShader.vert"));
            GL.CompileShader(vertexShader);

            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            //GL.ShaderSource(fragmentShader, File.ReadAllText(@"fragmentShader.frag"));
            GL.ShaderSource(fragmentShader, File.ReadAllText(@"lightingShader.frag"));

            GL.CompileShader(fragmentShader);

            var program = GL.CreateProgram();
            GL.AttachShader(program, vertexShader);
            GL.AttachShader(program, fragmentShader);
            GL.LinkProgram(program);

            GL.DetachShader(program, vertexShader);
            GL.DetachShader(program, fragmentShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
            return program;
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
         
            new MainWindow().Run(60);
        
        }
    }


    public class RenderObject : IDisposable
    {
        private bool _initialized;
        private readonly int _vertexArray;
        private readonly int _buffer;
        private readonly int _verticeCount;
        private PrimitiveType _primitiveType;
        public RenderObject(Vertex[] vertices, PrimitiveType primitiveType)
        {
            _primitiveType = primitiveType;
            _verticeCount = vertices.Length;
            _vertexArray = GL.GenVertexArray();
            _buffer = GL.GenBuffer();

            GL.BindVertexArray(_vertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexArray);

            // create first buffer: vertex
            GL.NamedBufferStorage(
                _buffer,
                Vertex.Size * vertices.Length,        // the size needed by this buffer
                vertices,                           // data to initialize with
                BufferStorageFlags.MapWriteBit);    // at this point we will only write to the buffer


            GL.VertexArrayAttribBinding(_vertexArray, 0, 0);
            GL.EnableVertexArrayAttrib(_vertexArray, 0);
            GL.VertexArrayAttribFormat(
                _vertexArray,
                0,                      // attribute index, from the shader location = 0
                4,                      // size of attribute, vec4
                VertexAttribType.Float, // contains floats
                false,                  // does not need to be normalized as it is already, floats ignore this flag anyway
                0);                     // relative offset, first item


            GL.VertexArrayAttribBinding(_vertexArray, 1, 0);
            GL.EnableVertexArrayAttrib(_vertexArray, 1);
            GL.VertexArrayAttribFormat(
                _vertexArray,
                1,                      // attribute index, from the shader location = 1
                4,                      // size of attribute, vec4
                VertexAttribType.Float, // contains floats
                false,                  // does not need to be normalized as it is already, floats ignore this flag anyway
                16);                     // relative offset after a vec4

            GL.VertexArrayAttribBinding(_vertexArray, 2, 0);
            GL.EnableVertexArrayAttrib(_vertexArray, 2);
            GL.VertexArrayAttribFormat(
                _vertexArray,
                2,                      // attribute index, from the shader location = 1
                3,                      // size of attribute, vec4
                VertexAttribType.Float, // contains floats
                false,                  // does not need to be normalized as it is already, floats ignore this flag anyway
                32);

            // link the vertex array and buffer and provide the stride as size of Vertex
            GL.VertexArrayVertexBuffer(_vertexArray, 0, _buffer, IntPtr.Zero, Vertex.Size);
            _initialized = true;
        }
        public void Render()
        {
            GL.BindVertexArray(_vertexArray);
            GL.DrawArrays(_primitiveType, 0, _verticeCount) ;
            
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_initialized)
                {
                    GL.DeleteVertexArray(_vertexArray);
                    GL.DeleteBuffer(_buffer);
                    _initialized = false;
                }
            }
        }
    }
    public struct Vertex
    {
        public const int Size = (4 + 4+3) * 4; // size of struct in bytes

        public readonly Vector4 _position;
        public readonly Color4 _color;
        public Vector3 _normal;

        public Vertex(Vector4 position, Color4 color, Vector3 normal)
        {
            _position = position;
            _color = color;
            _normal = normal;
        }
        public void SetNormal(Vector3 normal)
        {
            _normal = normal;
        }
    }
    public class ObjectFactory
    {

        public static Vertex[] LoadModel(string filename, Color4 color, float alpha)
        {
            MeshGeometry3D geometry3D = new MeshGeometry3D();
            using(StreamReader sr = new StreamReader(filename))
            {
                string line;
                int vn = 0;
                while ((line = sr.ReadLine()) != null)
                {
                    if(line.StartsWith("v "))
                    {
                        string subline = line.Substring(line.IndexOf('v') + 1).Trim();
                        double x = double.Parse(subline.Split(' ')[0]);
                        double y = double.Parse(subline.Split(' ')[1]);
                        double z = double.Parse(subline.Split(' ')[2]);
                        geometry3D.Positions.Add(new Point3D(x, y, z));

                    }
                    if (line.StartsWith("vn "))
                    {
                        string subline = line.Substring(line.IndexOf("vn") + 2).Trim();

                        float x = float.Parse(subline.Split(' ')[0]);
                        float y = float.Parse(subline.Split(' ')[1]);
                        float z = float.Parse(subline.Split(' ')[2]);
                        geometry3D.Normals.Add(vn++, new Vector3(x, y, z));
                    }
                    if (line.StartsWith("f"))
                    {
                        foreach(var vline in line.Split(' ').Skip(1))
                        {
                            if (vline.Trim().Length > 0)
                            {
                                geometry3D.TriangleIndices.Add(int.Parse(vline.Split('/')[0]));
                                geometry3D.NormalIndices.Add(int.Parse(vline.Split('/')[2]));
                            }
                        }
                    }
                }
            }
            //Normalize
            double maxPosition = geometry3D.Positions.Max(p => Math.Abs(p.X));
            if (geometry3D.Positions.Max(p => p.Y) > maxPosition)
            {
                maxPosition = geometry3D.Positions.Max(p => Math.Abs(p.Y));
            }
            if (geometry3D.Positions.Max(p => p.Z) > maxPosition)
            {
                maxPosition = geometry3D.Positions.Max(p => p.Z);
            }

            for (int p = 0; p < geometry3D.Positions.Count(); p++)
            {
                geometry3D.Positions[p].X /= maxPosition;
                geometry3D.Positions[p].Y /= maxPosition;
                geometry3D.Positions[p].Z /= maxPosition;
            }
            //To zero
            Point3D center = new Point3D(geometry3D.Positions.Average(c => c.X), geometry3D.Positions.Average(c => c.Y), geometry3D.Positions.Average(c => c.Z));
            geometry3D.Positions = geometry3D.Positions.Select(c => new Point3D(c.X - center.X, c.Y - center.Y, c.Z - center.Z)).ToList();

            List <Vertex> vertices = new List<Vertex>();

            for (int f = 0; f < geometry3D.TriangleIndices.Count; f++) 
            {
                var p = geometry3D.Positions[geometry3D.TriangleIndices[f]-1];
                var nor = geometry3D.Normals[geometry3D.NormalIndices[f]-1];
                color.A = (float)alpha;
                vertices.Add(new Vertex(new Vector4((float)p.X, (float)p.Y, (float)p.Z, 1.0f), color, nor));
            }
            return vertices.ToArray();
        }
        public static Vertex[] CreateSphere(Color4 color, double size, double alpha)
        {
            IcoSphereCreator icoSphereCreator = new IcoSphereCreator();
            MeshGeometry3D geometry = icoSphereCreator.Create(3);
            for (int v = 0; v < geometry.Positions.Count; v++)
            {
                var orgPoint = geometry.Positions[v];
                geometry.Positions[v] = new Point3D(orgPoint.X * size, orgPoint.Y * size, orgPoint.Z * size);
            }
        
            for (int fi = 0; fi < geometry.TriangleIndices.Count / 3; fi++)
            {
                var A = geometry.TriangleIndices[fi*3];
                var B = geometry.TriangleIndices[fi*3+1];
                var C = geometry.TriangleIndices[fi*3+2];

                Vector3 normal = Vector3.Cross(new Vector3((float)geometry.Positions[B].X, (float)geometry.Positions[B].Y, (float)geometry.Positions[B].Z) -
                    new Vector3((float)geometry.Positions[A].X, (float)geometry.Positions[A].Y, (float)geometry.Positions[A].Z),
                    new Vector3((float)geometry.Positions[C].X, (float)geometry.Positions[C].Y, (float)geometry.Positions[C].Z) -
                    new Vector3((float)geometry.Positions[A].X, (float)geometry.Positions[A].Y, (float)geometry.Positions[A].Z));
                
                if (!geometry.TempNormals.ContainsKey(A)) 
                    geometry.TempNormals.Add(A,new List<Vector3>());
                geometry.TempNormals[A].Add(normal);

                if (!geometry.TempNormals.ContainsKey(B))
                    geometry.TempNormals.Add(B, new List<Vector3>());
                geometry.TempNormals[B].Add(normal);

                if (!geometry.TempNormals.ContainsKey(C))
                    geometry.TempNormals.Add(C, new List<Vector3>());
                geometry.TempNormals[C].Add(normal);                
            }

            foreach(var v in geometry.TempNormals.Keys)
            {
                Vector3 sum = Vector3.Zero;
                foreach (var c in geometry.TempNormals[v])
                    sum = Vector3.Add(sum, c);
                geometry.Normals.Add(v, Vector3.Divide(sum, geometry.TempNormals[v].Count));
            }
                
            List<Vertex> vertices = new List<Vertex>();
            
            foreach (var f in geometry.TriangleIndices)
            {
                var p = geometry.Positions[f];
                var nor = geometry.Normals[f];
                color.A = (float)alpha;
                vertices.Add(new Vertex(new Vector4((float)p.X, (float)p.Y, (float)p.Z, 1.0f), color, nor));
            }
          
            return vertices.ToArray();
        }


        //public static Vertex[] CreateTransparentCube(float side, Color4 color, float alpha)
        //{
        //    color.A = alpha;
        //    side = side / 2f; // halv side - and other half +
        //    Vertex[] vertices =
        //    {
        //        new Vertex(new Vector4(-side, -side, -side, 1.0f), color),
        //        new Vertex(new Vector4(-side, side, -side, 1.0f),  color),
        //        new Vertex(new Vector4(-side, -side, side, 1.0f),  color),
        //        new Vertex(new Vector4(-side, -side, side, 1.0f),   color),
        //        new Vertex(new Vector4(-side, side, -side, 1.0f),   color),
        //        new Vertex(new Vector4(-side, side, side, 1.0f),   color),

        //        new Vertex(new Vector4(side, -side, -side, 1.0f),  color),
        //        new Vertex(new Vector4(side, side, -side, 1.0f),  color),
        //        new Vertex(new Vector4(side, -side, side, 1.0f),  color),
        //        new Vertex(new Vector4(side, -side, side, 1.0f),  color),
        //        new Vertex(new Vector4(side, side, -side, 1.0f),  color),
        //        new Vertex(new Vector4(side, side, side, 1.0f),  color),

        //        new Vertex(new Vector4(-side, -side, -side, 1.0f),  color),
        //        new Vertex(new Vector4(side, -side, -side, 1.0f),  color),
        //        new Vertex(new Vector4(-side, -side, side, 1.0f),  color),
        //        new Vertex(new Vector4(-side, -side, side, 1.0f), color),
        //        new Vertex(new Vector4(side, -side, -side, 1.0f),  color),
        //        new Vertex(new Vector4(side, -side, side, 1.0f),  color),

        //        new Vertex(new Vector4(-side, side, -side, 1.0f),  color),
        //        new Vertex(new Vector4(side, side, -side, 1.0f),  color),
        //        new Vertex(new Vector4(-side, side, side, 1.0f),  color),
        //        new Vertex(new Vector4(-side, side, side, 1.0f), color),
        //        new Vertex(new Vector4(side, side, -side, 1.0f),  color),
        //        new Vertex(new Vector4(side, side, side, 1.0f),  color),

        //        new Vertex(new Vector4(-side, -side, -side, 1.0f),  color),
        //        new Vertex(new Vector4(side, -side, -side, 1.0f),  color),
        //        new Vertex(new Vector4(-side, side, -side, 1.0f),  color),
        //        new Vertex(new Vector4(-side, side, -side, 1.0f),  color),
        //        new Vertex(new Vector4(side, -side, -side, 1.0f),  color),
        //        new Vertex(new Vector4(side, side, -side, 1.0f),  color),

        //        new Vertex(new Vector4(-side, -side, side, 1.0f), color),
        //        new Vertex(new Vector4(side, -side, side, 1.0f),  color),
        //        new Vertex(new Vector4(-side, side, side, 1.0f),   color),
        //        new Vertex(new Vector4(-side, side, side, 1.0f), color),
        //        new Vertex(new Vector4(side, -side, side, 1.0f),  color),
        //        new Vertex(new Vector4(side, side, side, 1.0f),  color),
        //    };
        //    return vertices;
        //}
        public static Vertex[] CreateSolidCube(float side, Color4 color)
        {
            side = side / 2f; // halv side - and other half +
            Vertex[] vertices =
            {
                  new Vertex(new Vector4(-side, -side, -side, 1.0f), Color4.Red, new Vector3( 0.0f,  0.0f, -1.0f)),
                new Vertex(new Vector4(side, -side, -side, 1.0f),  Color4.Red,new Vector3( 0.0f,  0.0f, -1.0f)),
                new Vertex(new Vector4(side, side, -side, 1.0f),  Color4.Red,new Vector3( 0.0f,  0.0f, -1.0f)),
                new Vertex(new Vector4(side, side, -side, 1.0f),  Color4.Red,new Vector3( 0.0f,  0.0f, -1.0f)),
                new Vertex(new Vector4(-side, side, -side,  1.0f),  Color4.Red,new Vector3( 0.0f,  0.0f, -1.0f)),
                new Vertex(new Vector4(- side, -side, -side, 1.0f),  Color4.Red,new Vector3( 0.0f,  0.0f, -1.0f)),

         
            // Back face
                 new Vertex(new Vector4(-side, -side, side, 1.0f), Color4.Red, new Vector3( 0.0f,  0.0f, 1.0f)),
                new Vertex(new Vector4(side, -side, side, 1.0f),  Color4.Red,new Vector3( 0.0f,  0.0f, 1.0f)),
                new Vertex(new Vector4(side, side, side, 1.0f),  Color4.Red,new Vector3( 0.0f,  0.0f, 1.0f)),
                new Vertex(new Vector4(side, side, side, 1.0f),  Color4.Red,new Vector3( 0.0f,  0.0f, 1.0f)),
                new Vertex(new Vector4(-side, side, side,  1.0f),  Color4.Red,new Vector3( 0.0f,  0.0f, 1.0f)),
                new Vertex(new Vector4(- side, -side, side, 1.0f),  Color4.Red,new Vector3( 0.0f,  0.0f, 1.0f)),

        
                // Left face
                new Vertex(new Vector4(-side, side, side, 1.0f), Color4.Red, new Vector3( -1.0f,  0.0f, 0.0f)),
                new Vertex(new Vector4(-side, side, -side, 1.0f),  Color4.Red,new Vector3( -1.0f,  0.0f, 0.0f)),
                new Vertex(new Vector4(-side, -side, -side, 1.0f),  Color4.Red,new Vector3( -1.0f,  0.0f, 0.0f)),
                new Vertex(new Vector4(-side, -side, -side, 1.0f),  Color4.Red,new Vector3( -1.0f,  0.0f, 0.0f)),
                new Vertex(new Vector4(-side, -side, side,  1.0f),  Color4.Red,new Vector3( -1.0f,  0.0f, 0.0f)),
                new Vertex(new Vector4(-side, side, side, 1.0f),  Color4.Red,new Vector3( -1.0f,  0.0f, 0.0f)),


                 // Right face
                new Vertex(new Vector4(side, side, side, 1.0f), Color4.Red, new Vector3( 1.0f,  0.0f, 0.0f)),
                new Vertex(new Vector4(side, side, -side, 1.0f),  Color4.Red,new Vector3( 1.0f,  0.0f, 0.0f)),
                new Vertex(new Vector4(side, -side, -side, 1.0f),  Color4.Red,new Vector3( 1.0f,  0.0f, 0.0f)),
                new Vertex(new Vector4(side, -side, -side, 1.0f),  Color4.Red,new Vector3( 1.0f,  0.0f, 0.0f)),
                new Vertex(new Vector4(side, -side, side,  1.0f),  Color4.Red,new Vector3( 1.0f,  0.0f, 0.0f)),
                new Vertex(new Vector4(side, side, side, 1.0f),  Color4.Red,new Vector3( 1.0f,  0.0f, 0.0f)),



             //  Bottom face
                new Vertex(new Vector4(-side, -side, -side, 1.0f), Color4.Red, new Vector3( 0.0f,  -1.0f, 0.0f)),
                new Vertex(new Vector4(side, -side, -side, 1.0f),  Color4.Red,new Vector3( 0.0f,  -1.0f, 0.0f)),
                new Vertex(new Vector4(side, -side, side, 1.0f),  Color4.Red,new Vector3( 0.0f,  -1.0f, 0.0f)),
                new Vertex(new Vector4(side, -side, side, 1.0f),  Color4.Red,new Vector3( 0.0f,  -1.0f, 0.0f)),
                new Vertex(new Vector4(-side, -side, side,  1.0f),  Color4.Red,new Vector3( 0.0f,  -1.0f, 0.0f)),
                new Vertex(new Vector4(-side, -side, -side, 1.0f),  Color4.Red,new Vector3( 0.0f,  -1.0f, 0.0f)),


            // Top face    
                new Vertex(new Vector4(-side, side, -side, 1.0f), Color4.Red, new Vector3( 0.0f,  1.0f, 0.0f)),
                new Vertex(new Vector4(side, side, -side, 1.0f),  Color4.Red,new Vector3( 0.0f,  1.0f, 0.0f)),
                new Vertex(new Vector4(side, side, side, 1.0f),  Color4.Red,new Vector3( 0.0f,  1.0f, 0.0f)),
                new Vertex(new Vector4(side, side, side, 1.0f),  Color4.Red,new Vector3( 0.0f,  1.0f, 0.0f)),
                new Vertex(new Vector4(-side, side, side,  1.0f),  Color4.Red,new Vector3( 0.0f,  1.0f, 0.0f)),
                new Vertex(new Vector4(-side, side, -side, 1.0f),  Color4.Red,new Vector3( 0.0f,  1.0f, 0.0f)),

            };
            
            return vertices;
        }
    }

    public class Camera     {
        public Matrix4 LookAtMatrix { get; }
        public Vector3 position;
        public Camera()
        {           
            position.X = 1;
            position.Y = 1;
            position.Z = 1.5f;
            LookAtMatrix = Matrix4.LookAt(position, Vector3.Zero, Vector3.UnitY);
        }
        public Camera(Vector3 position, Vector3 target)
        {
            LookAtMatrix = Matrix4.LookAt(position, target, Vector3.UnitY);
        }
        public void Update(double time, double delta)
        { }
    }
}
