using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.OpenGL;
using System.Drawing;
class Program
{
    private static IWindow _window;
    private static GL _opengl;
    private static uint _vertex_array_object;
    private static uint _vertex_buffer_object;
    private static uint _element_buffer_object;
    private static uint _program;
    static void Main()
    {
        WindowOptions options = WindowOptions.Default with
        {
            Size = new Vector2D<int>(800, 600),
            Title = "Engine"
        };
        _window = Window.Create(options);
        _window.Load += OnLoad;
        _window.Update += OnUpdate;
        _window.Render += OnRender;
        _window.Run();
    }
    private static unsafe void OnLoad() {
        IInputContext input = _window.CreateInput();
        for (int i = 0; i < input.Keyboards.Count; i++)
            input.Keyboards[i].KeyDown += KeyDown;
        _opengl = _window.CreateOpenGL();
        _opengl.ClearColor(Color.CornflowerBlue);
        _vertex_array_object = _opengl.GenVertexArray();
        _opengl.BindVertexArray(_vertex_array_object);
        float[] vertices =
        {
            0.5f,  0.5f, 0.0f,
            0.5f, -0.5f, 0.0f,
            -0.5f, -0.5f, 0.0f,
            -0.5f,  0.5f, 0.0f
        };
        _vertex_buffer_object = _opengl.GenBuffer();
        _opengl.BindBuffer(BufferTargetARB.ArrayBuffer, _vertex_buffer_object);
        fixed (float* buf = vertices)
            _opengl.BufferData(BufferTargetARB.ArrayBuffer, (nuint) (vertices.Length * sizeof(float)), buf, BufferUsageARB.StaticDraw);
        uint[] indices =
        {
            0u, 1u, 3u,
            1u, 2u, 3u
        };
        _element_buffer_object = _opengl.GenBuffer();
        _opengl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _element_buffer_object);
        fixed (uint* buf = indices)
            _opengl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint) (indices.Length * sizeof(uint)), buf, BufferUsageARB.StaticDraw);
        const string vertexCode = @"
        #version 330 core

        layout (location = 0) in vec3 aPosition;

        void main()
        {
            gl_Position = vec4(aPosition, 1.0);
        }";
        const string fragmentCode = @"
        #version 330 core

        out vec4 out_color;

        void main()
        {
            out_color = vec4(1.0, 0.5, 0.2, 1.0);
        }";
        uint vertexShader = _opengl.CreateShader(ShaderType.VertexShader);
        _opengl.ShaderSource(vertexShader, vertexCode);
        _opengl.CompileShader(vertexShader);
        _opengl.GetShader(vertexShader, ShaderParameterName.CompileStatus, out int vStatus);
        if (vStatus != (int) GLEnum.True)
            throw new Exception("Vertex shader failed to compile: " + _opengl.GetShaderInfoLog(vertexShader));
        uint fragmentShader = _opengl.CreateShader(ShaderType.FragmentShader);
        _opengl.ShaderSource(fragmentShader, fragmentCode);
        _opengl.CompileShader(fragmentShader);
        _opengl.GetShader(fragmentShader, ShaderParameterName.CompileStatus, out int fStatus);
        if (fStatus != (int) GLEnum.True)
            throw new Exception("Fragment shader failed to compile: " + _opengl.GetShaderInfoLog(fragmentShader));
        _program = _opengl.CreateProgram();
        _opengl.AttachShader(_program, vertexShader);
        _opengl.AttachShader(_program, fragmentShader);
        _opengl.LinkProgram(_program);
        _opengl.GetProgram(_program, ProgramPropertyARB.LinkStatus, out int lStatus);
        if (lStatus != (int) GLEnum.True)
            throw new Exception("Program failed to link: " + _opengl.GetProgramInfoLog(_program));
        _opengl.DetachShader(_program, vertexShader);
        _opengl.DetachShader(_program, fragmentShader);
        _opengl.DeleteShader(vertexShader);
        _opengl.DeleteShader(fragmentShader);
        const uint positionLoc = 0;
        _opengl.EnableVertexAttribArray(positionLoc);
        _opengl.VertexAttribPointer(positionLoc, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), (void*) 0);
        _opengl.BindVertexArray(0);
        _opengl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        _opengl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
    }
    private static void OnUpdate(double deltaTime) {
        //
    }
    private unsafe static void OnRender(double deltaTime) {
        _opengl.Clear(ClearBufferMask.ColorBufferBit);
        _opengl.BindVertexArray(_vertex_array_object);
        _opengl.UseProgram(_program);
        _opengl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, (void*) 0);
    }
    private static void KeyDown(IKeyboard keyboard, Key key, int keyCode) {
        if (key == Key.Escape)
            _window.Close();
    }
}