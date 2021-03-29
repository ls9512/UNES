namespace Aya.UNes.Renderer
{
    public interface IRenderer
    {
       string RendererName { get; }

       void Draw();

       void InitRendering(UNes nes);

       void EndRendering();
    }
}
